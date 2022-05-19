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
using Gameplay.Interaction;

namespace Gameplay
{ 
	public enum PlayerDetectionType
	{
		PickUp
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
		
		public static Rigidbody Rigidbody { get; private set; }
		public static CapsuleCollider WallCollider { get => active.wallCollider; private set => active.wallCollider = value; }
		public static CapsuleCollider GroundCollider { get => active.groundCollider; private set => active.groundCollider = value; }

		public static bool Grounded { get; private set; }

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

			Audio.SetPlayerSources(fxAudio);

			Rigidbody = GetComponent<Rigidbody>();
			health = GetComponent<ObjectHealth>();

			PlayerStateData[] sd = Resources.LoadAll<PlayerStateData>(stateDataPath);
			for (int i = 0; i < sd.Length; i++)
			{
				stateData.Add(sd[i].name, sd[i]);
			}

			health.onDamage += TakeDamage;
			cam.Init();
			Movement.Init();
            Weapons.Init(wpnLabel, wpnassetPath, weaponHolder, throwableSpawn);
            Inventory.Init(itemPath);
		}
		public void Setup()
		{
			SetState(new PlayerStanding(), true);

			mouseMovement.Enable();
			//muzzleLight.enabled = false;

			if (Game.PlayerData.Ammo[0] < 30)
			{
				Game.PlayerData.Ammo[0] = 30;
			}

			Weapons.Setup();

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

		private void Update()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active)
			{
				if (fxAudio.isPlaying)
					fxAudio.Pause();
				return;
			}

			if (!DebugConsole.instance.IsActive)
				cam.UpdateCamera();
		}

		private void FixedUpdate()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active)
				return;

			Grounded = Physics.CheckSphere(transform.position + StateData.groundCheck, groundDistance, groundMask, QueryTriggerInteraction.Ignore);

			PlayerCollider.instance.lastPos = PlayerCollider.instance.transform.position;
			PlayerCollider.instance.CheckCol();

			if (!DebugConsole.instance.IsActive)
				State.OnFixedUpdate();
		}
		private void LateUpdate()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active)
				return;

			Weapons.UpdateSway();
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

		[Header("Gadget Settings")]
		public float grenadeSpawnDelay = 0.35f;
		[SerializeField] private Transform throwableSpawn;

		#region Health

		public static ObjectHealth health;
		private float lastDamageSound = 0f;
		private readonly Control healControl = new ("Heal", false, InteractionType.Individual);
		private bool healOffered = false;
		[Header("Health & Economy")]
		[SerializeField] private float deathCamTime;
		[SerializeField] private float respawnTime;

		private void OnHeal()
		{
			if (Weapons.State != WeaponState.Reloading && Weapons.State != WeaponState.Aiming && Weapons.State != WeaponState.Throwing
				&& Weapons.Target == Weapons.State && health.currentHealth < health.maxHealth && Game.PlayerStatus == PlayerStatus.Alive
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
				Interact.RemoveControl(healControl);
			}
		}
		public void TakeDamage(ObjectHealth.DamageType origin)
		{
			if (Game.PlayerStatus != PlayerStatus.Alive)
			{ return; }

			GameWorld.Enviroment.instance.UpdateHealth(health.currentHealth);
			if (health.currentHealth < 25 && !healOffered)
			{
				Interact.NewControl(healControl);
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

			Interact.Clear();
		}
		public void Respawn()
		{
			Reset(true);
			Rigidbody.velocity = Vector3.zero;
			Game.PlayerStatus = PlayerStatus.Alive;
            UI.GUI.UnhideHUD();
			Weapons.Active.wpnAnimator.SetBool("Aim", false);
			Weapons.Active.wpnAnimator.SetBool("Running", false);
			Weapons.DrawWeapon();
			Cam.Transform.localRotation = Quaternion.Euler(0, 0, 0);
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

			active.stateData.TryGetValue(newState.ToString, out StateData);

			if (Weapons.LightOn && !StateData.canTurnOnLight)
			{
				Weapons.SwitchLight();
			}

			if (!StateData.canLean && Movement.Lean != LeaningState.None)
			{
				Movement.SetLeaningState(LeaningState.None);
			}

			if (immediately)
			{
				State = (PlayerState)System.Activator.CreateInstance(newState);
				State.OnEnter();

				Cam.Transform.position = Transform.position + StateData.camPosition;
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

				Cam.Transform.position = Transform.position + StateData.camPosition;
				GroundCollider.height = StateData.height;
				GroundCollider.radius = StateData.radius;
				WallCollider.height = StateData.wallHeight;
				WallCollider.radius = StateData.wallRadius;
				GroundCollider.direction = StateData.colliderDirection;
				WallCollider.direction = StateData.colliderDirection;

				Transform.position = Transform.position + new Vector3(0f, -StateData.groundCheck.y + lastY, 0f);
			}
		}
		public static void Reset(bool immediately)
		{
			if (Weapons.LightOn) 
				Weapons.SwitchLight();
			Weapons.Reset();
			SetState(new PlayerStanding(), immediately);
			Cam.Camera.fieldOfView = Settings.visuals.fov.Get();
			Cam.SetFoV(Settings.visuals.fov.Get(), true);
		}

		public void CompleteReset()
		{
			Destroy(weaponObj);
			Reset(true);
		}

		#endregion

		[Space()][Header("Inventory")]
		[SerializeField] private string itemPath = null;

		public static void FastTravel(Vector3 position, Quaternion rotation)
		{
			if (Weapons.LightOn)
				Weapons.SwitchLight();
			Transform.SetPositionAndRotation(position, rotation);
			Reset(false);
            Rigidbody.velocity = Vector3.zero;
		}
	}

	public abstract class PlayerState
	{
		public abstract void OnEnter();
		public abstract void OnFixedUpdate();
		public abstract void GetInput(InputType inputType);
		public abstract void OnLeave();

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override bool Equals(object obj)
        {
            return GetType() == obj.GetType();
        }
		public static bool operator ==(PlayerState lhs, PlayerState rhs)
        {
			return lhs.Equals(rhs);
        }
		public static bool operator !=(PlayerState lhs, PlayerState rhs)
		{
			return !lhs.Equals(rhs);
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
			else if (Input.Run && Input.Move.y > 0 && Weapons.State != WeaponState.Aiming && Weapons.Target != WeaponState.Aiming)
			{
				Player.SetState(new PlayerRunning(), false);
			}
			else
			{
				Interact.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigidbody.MovePosition(Player.Transform.position + v * Player.instance.speed);

				Player.Rigidbody.AddForce(Physics.gravity, ForceMode.Acceleration);
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
					Player.Rigidbody.velocity = Vector3.zero;
					Vector3 v = Player.Transform.right * Input.Move.x * Player.instance.speed 
						+ Player.Transform.forward * Input.Move.y * Player.instance.speed;
					v.y = Player.instance.jumpHeight;
					Player.Rigidbody.AddForce(v, ForceMode.VelocityChange);
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
			Player.Rigidbody.MovePosition(Player.Transform.position + new Vector3(Player.instance.vaultAngle.x, 0f, Player.instance.vaultAngle.z) * Time.fixedDeltaTime);
		}

		public override void OnLeave()
		{
			Movement.FinishVault();
		}
	}

	public class PlayerRunning : PlayerState
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
					Player.Rigidbody.velocity = Vector3.zero;
					Vector3 v = Player.Transform.right * Input.Move.x * Player.instance.speed +	Player.Transform.forward * Input.Move.y * Player.instance.runningSpeed;
					v.y = Player.instance.jumpHeight;
					Player.Rigidbody.AddForce(v, ForceMode.VelocityChange);
					break;
			}
		}

		public override void OnEnter()
		{
			Weapons.Active.wpnAnimator.SetBool("Running", true);
		}
		public override void OnFixedUpdate()
		{
			if (!Player.Grounded)
			{
				Player.SetState(new PlayerFalling(), false);
			}
			if (!Input.Run || Input.Move.y <= 0)
			{
				Player.SetState(new PlayerStanding(), false);
			}
			else
			{
				Interact.Update();
				Weapons.Update();
				float x = Input.Move.x;
				float z = Input.Move.y;

				/*float add = x + z;
				if (add > 1)
				{
					z /= add;
				}*/
				Vector3 v = (Player.Transform.right * x * Player.instance.speed + Player.Transform.forward * z * Player.instance.runningSpeed) * Time.fixedDeltaTime;

				Player.Rigidbody.MovePosition(Player.Transform.position + v);

				Player.Rigidbody.AddForce(Physics.gravity, ForceMode.Acceleration);
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
				Player.SetState(new PlayerStanding(), false);
			}
			else
			{
				Interact.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigidbody.MovePosition(Player.Transform.position + v);

				Player.Rigidbody.AddForce(Physics.gravity, ForceMode.Acceleration);
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
				Player.SetState(new PlayerFalling(), false);
			}
			else if (Input.Run && Input.Move.y > 0 && Weapons.State != WeaponState.Aiming)
			{
				Player.SetState(new PlayerRunning(), false);
			}
			else
			{
				Interact.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigidbody.MovePosition(Player.Transform.position + v * Player.instance.proneSpeed);

				Player.Rigidbody.AddForce(Physics.gravity, ForceMode.Acceleration);
			}
		}
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					if (Player.CanChangeState(typeof(PlayerCrouching), 0.55f))
					{
						Player.SetState(new PlayerCrouching(), false);
					}
					break;
				case InputType.Prone:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(new PlayerStanding(), false);
					}
					break;
				case InputType.Jump:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(new PlayerStanding(), false);
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
				Player.SetState(new PlayerFalling(), false);
			}
			else if (Input.Run && Input.Move.y > 0 && Weapons.State != WeaponState.Aiming)
			{
				Player.SetState(new PlayerRunning(), false);
			}
			else
			{
				Interact.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward 
					* Input.Move.y).normalized * Time.fixedDeltaTime;
				Player.Rigidbody.MovePosition(Player.Transform.position + v * Player.instance.crouchSpeed);

				Player.Rigidbody.AddForce(Physics.gravity, ForceMode.Acceleration);
			}
		}
		public override void GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.Crouch:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(new PlayerStanding(), false);
					}
					break;
				case InputType.Prone:
					if (Player.CanChangeState(typeof(PlayerProning), 0f))
					{
						Player.SetState(new PlayerProning(), false);
					}
					break;
				case InputType.Jump:
					if (Player.CanChangeState(typeof(PlayerStanding), 0.54f))
					{
						Player.SetState(new PlayerStanding(), false);
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
				Interact.Update();
				Weapons.Update();
				Vector3 v = (Player.Transform.right * Input.Move.x + Player.Transform.forward * Input.Move.y).normalized * Time.fixedDeltaTime;
				Movement.ActiveHorse.rig.MovePosition(Movement.ActiveHorse.transform.position + v * 2.5f);

				Movement.ActiveHorse.rig.AddForce(Physics.gravity, ForceMode.Acceleration);

				float rot = Input.Move.x * Time.fixedDeltaTime * 50f;
				if (Input.Move.y < 0)
				{ rot *= -1; }
				Movement.ActiveHorse.transform.Rotate(0f, rot, 0f);
				Player.Transform.SetPositionAndRotation(Movement.ActiveHorse.playerPos.position, Movement.ActiveHorse.playerPos.rotation);
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
					Movement.ActiveHorse.rig.velocity = Vector3.zero;
					Vector3 v = Movement.ActiveHorse.transform.right * Input.Move.x + 2f * Input.Move.y * Movement.ActiveHorse.transform.forward;
					v.y = Player.instance.jumpHeight;
					Movement.ActiveHorse.rig.AddForce(v, ForceMode.VelocityChange);
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
					Movement.ActiveHorse.rig.velocity = Vector3.zero;
					Vector3 v = 2f * Input.Move.x * Movement.ActiveHorse.transform.right + Movement.ActiveHorse.transform.forward * Input.Move.y * 6f;
					v.y = Player.instance.jumpHeight;
					Movement.ActiveHorse.rig.AddForce(v, ForceMode.VelocityChange);
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
			if (!Input.Run || Input.Move.y <= 0)
			{
				Player.SetState(new HorseStanding(), false);
			}
			else
			{
				Interaction.Interact.Update();
				Weapons.Update();
				float x = Input.Move.x;
				float z = Input.Move.y;

				Vector2 n = new Vector2(x, z);
				//x = n.normalized.x;
				//z = n.normalized.y;

				Vector3 v = (2.5f * x * Player.Transform.right + 8f * z * Player.Transform.forward) * Time.fixedDeltaTime;

				Movement.ActiveHorse.rig.MovePosition(Movement.ActiveHorse.transform.position + v);

				float rot = x * Time.fixedDeltaTime * 50f;
				if (z < 0)
				{ rot *= -1; }
				Movement.ActiveHorse.rig.AddForce(Physics.gravity, ForceMode.Acceleration);
				Movement.ActiveHorse.transform.Rotate(0f, rot, 0f);
				Player.active.transform.SetPositionAndRotation(Movement.ActiveHorse.playerPos.position, Movement.ActiveHorse.playerPos.rotation);
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
				Interaction.Interact.Update();
				Weapons.Update();

				Movement.ActiveHorse.rig.AddForce(Physics.gravity, ForceMode.Acceleration);
				Player.active.transform.SetPositionAndRotation(Movement.ActiveHorse.playerPos.position,
					Movement.ActiveHorse.playerPos.rotation);
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

			Player.Rigidbody.MovePosition(Player.Transform.position + v);
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