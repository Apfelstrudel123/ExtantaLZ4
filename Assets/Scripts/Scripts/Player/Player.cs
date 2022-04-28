using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using ScriptableObjects;
using Core;
using Core.GameSettings;
using UI;
using Gameplay.Abilities;
using Gameplay.Combat;

namespace Gameplay
{ 
	public enum ControlType
	{
		Interact = 0,
		SecInteract = 1,
		Parkour = 2,
		Individual = 3,
	}
	public enum PlayerDetectionType
	{
		PickUp,
	}
	[RequireComponent(typeof(Input))]
	public class Player : MonoBehaviour
	{
		#region General 

		[Header("Testing")]
		public TMPro.TMP_Text stateText;

		public static Player active;
		public static PlayerState State { get; set; }
		public static Transform Transform { get => active.transform; }

		public static Cam Cam { get => active.cam; private set => active.cam = value; }
		
		public static Rigidbody Rigbody { get; private set; }
		public static CapsuleCollider WallCollider { get => active.wallCollider; private set => active.wallCollider = value; }
		public static CapsuleCollider GroundCollider { get => active.groundCollider; private set => active.groundCollider = value; }

		[Space()]
		[Header("Object References")]
		[SerializeField] private Cam cam;
		[SerializeField] private CapsuleCollider groundCollider;
		[SerializeField] private CapsuleCollider wallCollider;	

		private Transform spawnPoint;
		[HideInInspector] public GameObject[] points;

		private void Awake()
		{
			active = this;

			Rigbody = GetComponent<Rigidbody>();
			health = GetComponent<ObjectHealth>();

			_camera = cam.GetComponent<Camera>();

			//Into weapons
			List<WeaponData> wpns = new();
			Addressables.LoadAssetsAsync<WeaponData>(wpnLabel, obj =>
            {
				wpns.Add(obj);
            });
			weaponDatas = wpns.ToArray();
			for (int i = 0; i < weaponDatas.Length; i++)
			{
				weaponDatas[i].weaponID = i;
			}
			//

			//Into inventory
			PlayerItem[] p = Resources.LoadAll<PlayerItem>(itemPath);
			items = new PlayerItem[p.Length];
			for (int i = 0; i < p.Length; i++)
			{
				items[p[i].itemID] = p[i];
			}
			//

			PlayerStateData[] sd = Resources.LoadAll<PlayerStateData>(stateDataPath);
			for (int i = 0; i < sd.Length; i++)
			{
				stateData.Add(sd[i].name, sd[i]);
			}

			health.onDamage += TakeDamage;
			cam.Init();
			Movement.Init();
			Weapons.Init(wpnLabel);
			Inventory.Init(itemPath);
		}
		public void Setup()
		{
			SetState(new PlayerStanding(), true);

			mouseMovement.Enable();
			//muzzleLight.enabled = false;
			initialSwayPosition = weaponHolder.localPosition;

			if (Game.PlayerData.ammo[0] < 30)
			{
				Game.PlayerData.ammo[0] = 30;
			}

			//TODO: Into weapons
			LoadWeapon(Game.PlayerData.wpnKeys[0]);
			weaponObj = Instantiate(weaponAsset.prefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
			ammo = weaponDatas[GetWeaponIDByKey(Game.PlayerData.wpnKeys[0])].magSize;
			//

			health.ResetHealth();

            UI.GUI.UpdateAmmoBar();
		}
		public void Init()
		{
			Weapons.DrawWeapon();
			Input.Activate();

			Game.PlayerStatus = PlayerStatus.Alive;
			Game.GameState = GameState.Active;
		}

		public void CompleteReset()
		{
			Destroy(weaponObj);
			ResetPlayer(true);
		}

		private void Update()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active)
			{
				if (fxAudio.isPlaying)
				{
					fxAudio.Pause();
				}
				return;
			}

			if (!DebugConsole.instance.IsActive)
			{
				cam.UpdateCamera();
			}
		}

		private void FixedUpdate()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active)
			{
				return;
			}

			Grounded = Physics.CheckSphere(transform.position + StateData.groundCheck, groundDistance, groundMask, QueryTriggerInteraction.Ignore);

			PlayerCollider.instance.lastPos = PlayerCollider.instance.transform.position;
			PlayerCollider.instance.CheckCol();

			if (!DebugConsole.instance.IsActive)
			{
				State.OnFixedUpdate();
			}
		}
		private void LateUpdate()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active)
			{
				return;
			}

			Vector2 m = -mouse * 0.002f;
			if (Weapons.State == WeaponState.Aiming)
			{
				m /= 6;
			}

			m.x = Mathf.Clamp(m.x, -0.006f, 0.006f);
			m.y = -Mathf.Clamp(m.y * 5, -0.06f, 0.06f);
			Vector3 finalSwayPosition = new Vector3(m.x, m.y / 5, 0);
			weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition, finalSwayPosition + initialSwayPosition, Time.deltaTime * 4);
		}

		#endregion

		[Space()][Header("Mouse/Cam")]
		[SerializeField] private InputAction mouseMovement;
		[SerializeField] private float upwardPitchAngle = -90.0f;
		[SerializeField] private float downwardPitchAngle = 90.0f;

		[Space()][Header("Movement")]
		public float speed = 1f;
		public float runningSpeed = 2f;
		public float crouchSpeed = 0.5f;
		public float proneSpeed = 0.25f;
		public float jumpHeight = 100f;
		public float ladderSpeed = 1f;
		public float ladderFastSpeed = 1f;
		public float ladderSlideSpeed = 1f;
		[SerializeField] private float groundDistance = 0.3f;
		[SerializeField] private float headDistance = 0.4f;
		[SerializeField] private LayerMask headLayer = new LayerMask();
		[SerializeField] private LayerMask groundMask = new LayerMask();

		[Space()][Header("Weapon")]
		[SerializeField] private AssetLabelReference wpnLabel;
		[SerializeField] private string wpnassetPath;
		public Transform weaponHolder;
		//[SerializeField] private ParticleSystem muzzleParticles = null;
		//[SerializeField] private Light muzzleflashLight = null;
		[SerializeField] private float lightDuration = 0.02f;

		#region Throwing

		private Animator throwableAnim;
		private bool throwOnLoaded = false;
		private bool throwing = false;
		public static int currentThrowable = 0;
		private string[] throwableKeys = new string[] { "HEGrenade", "Molotov" };
		[Header("Grenade Settings")]
		public float grenadeSpawnDelay = 0.35f;
		public Transform grenadeSpawnPoint;

		private void OnSwitchThrowable(InputValue value)
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active && !DebugConsole.instance.IsActive)
			{ return; }
			currentThrowable += (int)(value.Get<float>() / 120f);

			if (currentThrowable >= throwableKeys.Length)
			{
				currentThrowable %= throwableKeys.Length;
			}
			else if (currentThrowable < 0)
			{
				currentThrowable += throwableKeys.Length;
			}

            UI.GUI.UpdateAmmoBar();
		}

		private void OnThrow(InputValue value)
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active && !DebugConsole.instance.IsActive)
				return;
			if (State == PlayerLadder)
				return;
			if (Weapons.State == WeaponState.Reloading || Weapons.State == WeaponState.Aiming || targetedWeapon == WeaponState.Reloading 
				|| targetedWeapon == WeaponState.Aiming)
				return;

			if (value.isPressed && !throwing)
			{
				if (targetedWeapon != WeaponState.Throwing && Game.PlayerData.gadgets[currentThrowable] > 0)
				{
					DisableWeapon();
					throwing = true;
					targetedWeapon = WeaponState.Throwing;
					ObjectPool.Request(throwableKeys[currentThrowable], grenadeSpawnPoint.transform.position, grenadeSpawnPoint.transform.rotation, InitThrowable);
				}
			}
			else if (throwing)
			{
				throwing = false;
				if (Weapons.State == WeaponState.Throwing)
				{
					Game.PlayerData.gadgets[currentThrowable]--;
                    UI.GUI.UpdateAmmoBar();
					throwableAnim.SetTrigger("Throw");
					Invoke(nameof(ThrowNade), 0.2f);
				}
				else if (targetedWeapon == WeaponState.Throwing)
				{
					throwOnLoaded = true;
				}
			}
		}
		private void InitThrowable(GameObject obj)
		{
			throwableAnim = obj.GetComponent<Animator>();
			throwableAnim.Play("Equip", 0);
			prevWeaponState = Weapons.State;
			Weapons.State = WeaponState.Throwing;
			if (throwOnLoaded)
			{
				Game.PlayerData.gadgets[currentThrowable]--;
                UI.GUI.UpdateAmmoBar();
				throwableAnim.SetTrigger("Throw");
				Invoke(nameof(ThrowNade), 0.2f);
			}
		}
		private void ThrowNade()
		{
			throwOnLoaded = false;
			throwableAnim.SendMessage("Throw");
			targetedWeapon = WeaponState.Idle;
			Weapons.State = WeaponState.Idle;
			DrawWeapon();
		}

		#endregion

		#region Health

		public static ObjectHealth health;
		private float lastDamageSound = 0f;
		[Header("Health & Economy")]
		[SerializeField] private float deathCamTime;
		[SerializeField] private float respawnTime;

		private void OnHeal()
		{
			if (Weapons.State != WeaponState.Reloading && Weapons.State != WeaponState.Aiming && Weapons.State != WeaponState.Throwing
				&& targetedWeapon == Weapons.State && health.currentHealth < health.maxHealth && Game.PlayerStatus == PlayerStatus.Alive
				&& Game.GameState == GameState.Active && State != PlayerLadder && !DebugConsole.instance.IsActive)
			{
				Heal();
			}
		}
		public void Heal()
		{
			health.ResetHealth();
			GameWorld.Enviroment.instance.UpdateHealth(health.currentHealth);
			if (healOffered)
			{
				healOffered = false;
				RemoveControl("Heal");
			}
		}
		public void TakeDamage(ObjectHealth.DamageType origin)
		{
			if (Game.PlayerStatus != PlayerStatus.Alive)
			{ return; }

			GameWorld.Enviroment.instance.UpdateHealth(health.currentHealth);
			if (health.currentHealth < 25 && !healOffered)
			{
				NewControl("Heal", ControlType.Individual);
				healOffered = true;
			}

			if (Time.realtimeSinceStartup - lastDamageSound > 1.5f)
			{
				lastDamageSound = Time.realtimeSinceStartup;
				if (origin == ObjectHealth.DamageType.Bullet)
				{
					fxAudio.PlayOneShot(hitSounds[UnityEngine.Random.Range(0, hitSounds.Length - 1)]);
				}
				else
				{
					fxAudio.PlayOneShot(damageSounds[UnityEngine.Random.Range(0, damageSounds.Length - 1)]);
				}
			}
		}
		public void OnDie()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive)
			{ return; }
			StartCoroutine(Die());
		}

		private IEnumerator Die()
		{
			Game.PlayerStatus = PlayerStatus.Dead;

            UI.GUI.HideHUD();

			yield return new WaitForSecondsRealtime(deathCamTime);

			GameManager.instance.PlayerDie();

			healOffered = false;
			//transform.SetPositionAndRotation(spawn.position, spawn.rotation);

			health.ResetHealth();
			GameWorld.Enviroment.instance.UpdateHealth(health.currentHealth);

			controlOffers.Clear();
			controls.Clear();
            UI.GUI.UpdateControlPanel(controls.ToArray(), controlOffers.ToArray());
		}
		public void Respawn()
		{
			ResetPlayer(true);
			rigbody.velocity = Vector3.zero;
			Game.PlayerStatus = PlayerStatus.Alive;
            UI.GUI.UnhideHUD();
			weaponAnimator.SetBool("Aim", false);
			weaponAnimator.SetBool("Running", false);
			DrawWeapon();
			cam.localRotation = Quaternion.Euler(0, 0, 0);
		}

		#endregion

		#region UI

		private static readonly List<string> controlOffers = new ();
		private static readonly List<ControlType> controls = new ();
		private bool healOffered = false;

		private void NewControl(string name, ControlType c)
		{
			if (!controlOffers.Contains(name))
			{
				controls.Add(c);
				controlOffers.Add(name);
                UI.GUI.UpdateControlPanel(controls.ToArray(), controlOffers.ToArray());
			}
		}
		public static void RemoveControl(string name)
		{
			//TODO: Get index + check index
			if (!controlOffers.Contains(name))
				return;
			int i = controlOffers.IndexOf(name);
			controlOffers.RemoveAt(i);
			controls.RemoveAt(i);
            UI.GUI.UpdateControlPanel(controls.ToArray(), controlOffers.ToArray());
		}

		#endregion

		[Space()][Header("Interact")]
		public float pickUpDist;
		[SerializeField] private GameObject fovLight;
		[SerializeField] private LayerMask interactLayer = new LayerMask();

		#region Collision

		public void Collide(Collider col)
		{

		}

		#endregion

		#region Audio

		[Space()]
		[Header("Audio")]
		[SerializeField] private AudioSource fxAudio = null;
		[SerializeField] private AudioSource ambientAudio = null;
		[SerializeField] private AudioMixer mixer = null;
		[SerializeField] private AudioClip diveIn = null;
		[SerializeField] private AudioClip diveOut = null;
		[SerializeField] private AudioClip waterIdle = null;
		[SerializeField] private AudioClip pickUpSound = null;
		[SerializeField] private AudioClip[] damageSounds;
		[SerializeField] private AudioClip[] hitSounds;

		#endregion

		#region State

		public static PlayerStateData StateData { get; private set; }
		private Dictionary<string, PlayerStateData> stateData = new ();
		[Header("States")]
		[SerializeField] private string stateDataPath;

		public static void SetState(PlayerState newState, bool immediately)
		{
			float lastY = 0f;
			if (State != null)
			{
				if (State == newState)
					return;

				lastY = StateData.groundCheck.y;
				active.stateText.text = newState.ToString();
				State.OnLeave();
			}

			active.stateData.TryGetValue(newState.Name, out StateData);

			if (lightOn && !StateData.canTurnOnLight)
			{
				SwitchLight();
			}

			if (!StateData.canLean && leaningState != LeaningState.None)
			{
				SetLeaningState(LeaningState.None);
			}

			if (immediately)
			{
				State = (PlayerState)System.Activator.CreateInstance(newState);
				State.OnEnter();

				cam.position = Transform.position + StateData.camPosition;
				GroundCollider.height = StateData.height;
				GroundCollider.radius = StateData.radius;
				WallCollider.height = StateData.wallHeight;
				WallCollider.radius = StateData.wallRadius;
				GroundCollider.direction = StateData.colliderDirection;
				WallCollider.direction = StateData.colliderDirection;

				Transform.position = Transform.position + new Vector3(0f, -StateData.groundCheck.y + lastY, 0f);
			}
			else
			{
				//yield return new WaitForSeconds(newState.time);
				State = (PlayerState)System.Activator.CreateInstance(newState);
				State.OnEnter();

				cam.position = Transform.position + StateData.camPosition;
				GroundCollider.height = StateData.height;
				GroundCollider.radius = StateData.radius;
				WallCollider.height = StateData.wallHeight;
				WallCollider.radius = StateData.wallRadius;
				GroundCollider.direction = StateData.colliderDirection;
				WallCollider.direction = StateData.colliderDirection;

				Transform.position = Transform.position + new Vector3(0f, -StateData.groundCheck.y + lastY, 0f);
			}
		}
		public void ResetPlayer(bool immediately)
		{
			if (lightOn) 
				SwitchLight();
			Weapons.State = WeaponState.Idle;
			targetedWeapon = WeaponState.Idle;
			prevWeaponState = WeaponState.Idle;
			SetState(typeof(PlayerStanding), immediately);
			_camera.fieldOfView = Settings.visuals.fov.Get();
			targetFOV = Settings.visuals.fov.Get();
		}

		#endregion

		[Space()][Header("Inventory")]
		[SerializeField] private string itemPath = null;

		public void FastTravel(Vector3 position, Quaternion rotation)
		{
			if (lightOn)
				SwitchLight();
			transform.SetPositionAndRotation(position, rotation);
			ResetPlayer(false);
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}

	public abstract class PlayerState
	{
		public abstract void OnEnter();
		public abstract void OnFixedUpdate();
		public abstract void GetInput(InputType inputType);
		public abstract void OnLeave();

        public override bool Equals(object obj)
        {
            return GetType() == obj.GetType();
        }
    }

	public class PlayerStanding : PlayerState
	{
		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			if (!Player.Grounded)
			{
				Player.SetState(new PlayerFalling(), false);
			}
			else if (Input.Run && Input.Move.y > 0 && Weapons.State != WeaponState.Aiming && Player.instance.targetedWeapon != WeaponState.Aiming)
			{
				Player.SetState(new PlayerRunning(), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigbody.MovePosition(Player.Transform.position + v * Player.instance.speed);

				Player.Rigbody.AddForce(Physics.gravity, ForceMode.Acceleration);
			}
		}
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					Player.SetState(typeof(PlayerCrouching), false);
					break;
				case InputType.Prone:
					Player.SetState(typeof(PlayerProning), false);
					break;
				case InputType.Jump:
					Player.Rigbody.velocity = Vector3.zero;
					Vector3 v = Player.Transform.right * Input.Move.x * Player.instance.speed 
						+ Player.Transform.forward * Input.Move.y * Player.instance.speed;
					v.y = Player.instance.jumpHeight;
					Player.Rigbody.AddForce(v, ForceMode.VelocityChange);
					break;
			}
		}
		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}

	public class PlayerVaulting : PlayerState
	{
		public override void GetInput(InputType inputType)
		{
			throw new System.NotImplementedException();
		}

		public override void OnEnter()
		{

		}

		public override void OnFixedUpdate()
		{
			Weapons.Update();
			Player.Rigbody.MovePosition(Player.Transform.position + new UnityEngine.Vector3
				(Player.instance.vaultAngle.x, 0f, Player.instance.vaultAngle.z) * Time.fixedDeltaTime);
		}

		public override void OnLeave()
		{
			Player.instance.Unvault();
		}
	}

	public class PlayerRunning : PlayerState
	{
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					Player.SetState(typeof(PlayerCrouching), false);
					break;
				case InputType.Prone:
					Player.SetState(typeof(PlayerProning), false);
					break;
				case InputType.Jump:
					Player.Rigbody.velocity = Vector3.zero;
					Vector3 v = Player.Transform.right * Input.Move.x * Player.instance.speed +
						Player.Transform.forward * Input.Move.y * Player.instance.runningSpeed;
					v.y = Player.instance.jumpHeight;
					Player.Rigbody.AddForce(v, ForceMode.VelocityChange);
					break;
			}
		}

		public override void OnEnter()
		{
			Weapons.Active.wpnAnimator.SetBool("Running", true);
		}
		public override void OnFixedUpdate()
		{
			if (!Player.grounded)
			{
				Player.SetState(typeof(PlayerFalling), false);
			}
			if (!Input.Run || Input.Move.y <= 0)
			{
				Player.SetState(typeof(PlayerStanding), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();
				float x = Input.Move.x;
				float z = Input.Move.y;

				/*float add = x + z;
				if (add > 1)
				{
					z /= add;
				}*/
				Vector3 v = (Player.Transform.right * x * Player.instance.speed + Player.Transform.forward * z * Player.instance.runningSpeed) * Time.fixedDeltaTime;

				Player.Rigbody.MovePosition(Player.Transform.position + v);

				Player.Rigbody.AddForce(Physics.gravity, ForceMode.Acceleration);
				AbilitySystem.UpdateValue(AbilityCategory.Endurance, Time.fixedDeltaTime * 2);
			}
		}

		public override void OnLeave()
		{
			Weapons.Active.wpnAnimator.SetBool("Running", false);
		}
	}

	public class PlayerFalling : PlayerState
	{
		public override void GetInput(InputType inputType)
		{

		}

		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			if (Player.Grounded)
			{
				Player.SetState(typeof(PlayerStanding), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigbody.MovePosition(Player.Transform.position + v);

				Player.Rigbody.AddForce(Physics.gravity, ForceMode.Acceleration);
				AbilitySystem.UpdateValue(AbilityCategory.Endurance, Time.fixedDeltaTime * 2);
			}
		}

		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}

	public class PlayerProning : PlayerState
	{
		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			if (!Player.Grounded)
			{
				Player.SetState(typeof(PlayerFalling), false);
			}
			else if (Input.Run && Input.Move.y > 0 && Weapons.State != WeaponState.Aiming)
			{
				Player.SetState(typeof(PlayerRunning), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigbody.MovePosition(Player.Transform.position + v * Player.instance.proneSpeed);

				Player.Rigbody.AddForce(Physics.gravity, ForceMode.Acceleration);
			}
		}
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					if (Player.CanChangeState(typeof(PlayerCrouching), 0.55f))
					{
						Player.SetState(typeof(PlayerCrouching), false);
					}
					break;
				case InputType.Prone:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(typeof(PlayerStanding), false);
					}
					break;
				case InputType.Jump:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(typeof(PlayerStanding), false);
					}
					break;
			}
		}
		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}

	public class PlayerCrouching : PlayerState
	{
		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			if (!Player.Grounded)
			{
				Player.SetState(typeof(PlayerFalling), false);
			}
			else if (Input.Run && Input.Move.y > 0 && Weapons.State != WeaponState.Aiming)
			{
				Player.SetState(typeof(PlayerRunning), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward 
					* Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigbody.MovePosition(Player.Transform.position + v * Player.instance.crouchSpeed);

				Player.Rigbody.AddForce(Physics.gravity, ForceMode.Acceleration);
			}
		}
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(typeof(PlayerStanding), false);
					}
					break;
				case InputType.Prone:
					if (Player.CanChangeState(typeof(PlayerProning), 0f))
					{
						Player.SetState(typeof(PlayerProning), false);
					}
					break;
				case InputType.Jump:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(typeof(PlayerStanding), false);
					}
					break;
			}
		}
		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}

	public class HorseStanding : PlayerState
	{
		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			if (!Player.Grounded)
			{
				Player.SetState(new HorseFalling(), false);
			}
			else if (Input.Run && Input.Move.y > 0 && Weapons.State != WeaponState.Aiming)
			{
				Player.SetState(new HorseRunning(), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.instance.currentHorse.rig.MovePosition(Player.instance.currentHorse.transform.position + v * 2.5f);

				Player.instance.currentHorse.rig.AddForce(Physics.gravity, ForceMode.Acceleration);

				float rot = Input.Move.x * Time.fixedDeltaTime * 50f;
				if (Input.Move.y < 0)
				{ rot *= -1; }
				Player.instance.currentHorse.transform.Rotate(0f, rot, 0f);
				Player.Transform.SetPositionAndRotation(Player.instance.currentHorse.playerPos.position, 
					Player.instance.currentHorse.playerPos.rotation);
				AbilitySystem.UpdateValue(AbilityCategory.Riding, Time.fixedDeltaTime);
			}
		}
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					Player.SetState(new PlayerCrouching(), false);
					break;
				case InputType.Prone:
					Player.SetState(new PlayerProning(), false);
					break;
				case InputType.Jump:
					Player.instance.currentHorse.rig.velocity = Vector3.zero;
					Vector3 v = Player.instance.currentHorse.transform.right * Input.Move.x 
						+ Player.instance.currentHorse.transform.forward * Input.Move.y * 2f;
					v.y = Player.instance.jumpHeight;
					Player.instance.currentHorse.rig.AddForce(v, ForceMode.VelocityChange);
					break;
			}
		}
		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}

	public class HorseRunning : PlayerState
	{
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					Player.SetState(new PlayerCrouching(), false);
					break;
				case InputType.Prone:
					Player.SetState(new PlayerProning(), false);
					break;
				case InputType.Jump:
					Player.instance.currentHorse.rig.velocity = Vector3.zero;
					Vector3 v = Player.instance.currentHorse.transform.right * Input.Move.x * 2f 
						+ Player.instance.currentHorse.transform.forward * Input.Move.y * 6f;
					v.y = Player.instance.jumpHeight;
					Player.instance.currentHorse.rig.AddForce(v, ForceMode.VelocityChange);
					break;
			}
		}

		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			if (!Player.Grounded)
			{
				Player.SetState(new HorseFalling(), false);
			}
			if (!Input.Run || Player.instance.moveInput.y <= 0)
			{
				Player.SetState(new HorseStanding(), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();
				float x = Input.Move.x;
				float z = Input.Move.y;

				Vector2 n = new Vector2(x, z);
				//x = n.normalized.x;
				//z = n.normalized.y;

				Vector3 v = (Player.Transform.right * x * 2.5f + Player.Transform.forward * z * 8f) * Time.fixedDeltaTime;

				Movement.activeHorse.rig.MovePosition(Movement.activeHorse.transform.position + v);

				float rot = x * Time.fixedDeltaTime * 50f;
				if (z < 0)
				{ rot *= -1; }
				Movement.activeHorse.rig.AddForce(Physics.gravity, ForceMode.Acceleration);
				Movement.activeHorse.transform.Rotate(0f, rot, 0f);
				Player.active.transform.SetPositionAndRotation(Movement.activeHorse.playerPos.position, Movement.activeHorse.playerPos.rotation);
				AbilitySystem.UpdateValue(AbilityCategory.Riding, Time.fixedDeltaTime * 2);
			}
		}

		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}

	public class HorseFalling : PlayerState
	{
		public override void GetInput(InputType inputType)
		{

		}

		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			if (Player.Grounded)
			{
				Player.SetState(new HorseStanding(), false);
			}
			else
			{
				Interaction.Update();
				Weapons.Update();

				Movement.activeHorse.rig.AddForce(Physics.gravity, ForceMode.Acceleration);
				Player.active.transform.SetPositionAndRotation(Movement.activeHorse.currentHorse.playerPos.position,
					Movement.activeHorse.currentHorse.playerPos.rotation);
			}
		}

		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}
	public class PlayerLadder : PlayerState
	{
		private GameWorld.Ladder ladder;
		private float length;

		public PlayerLadder(GameWorld.Ladder _ladder)
        {
			ladder = _ladder;
			length = (ladder.start.position - ladder.end.position).sqrMagnitude;
        }

		public override void OnEnter()
		{

		}
		public override void OnFixedUpdate()
		{
			float de = (Player.Transform.position -ladder.end.position).sqrMagnitude;
			float ds = (Player.Transform.position - ladder.start.position).sqrMagnitude;

			if (de >= length || ds >= length)
			{
				Movement.ExitLadder();
				return;
			}

			Vector3 v = ((ladder.start.position - ladder.end.position) 
				* Input.Move.y).normalized * Time.fixedDeltaTime;

			if (Input.Run)
			{
				if (v.y > 0)
				{
					v *= Player.active.ladderSlideSpeed;
				}
				else
				{
					v *= Player.active.ladderFastSpeed;
				}
			}
			else
			{
				v *= Player.active.ladderSpeed;

			}

			Player.Rigbody.MovePosition(Player.Transform.position + v);
		}
		public override void GetInput(InputType inputType)
		{

		}
		public override void OnLeave()
		{
			//throw new System.NotImplementedException();
		}
	}
}