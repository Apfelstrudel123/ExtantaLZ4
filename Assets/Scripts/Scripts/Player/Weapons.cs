using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ScriptableObjects;
using Core;

namespace Gameplay.Combat
{
	public enum WeaponState {
		Idle,
		Reloading,
		Aiming,
		Throwing
	}

	public struct Weapon {
		public UnityEngine.GameObject wpnObject;
		public UnityEngine.Animator wpnAnimator;
		public WeaponData wpnData;
        public WeaponAssets wpnAssets;
		public AsyncOperationHandle<WeaponAssets> wpnHandle;
	}

    public static class Weapons {
        #region CFG & Data

        private static WeaponData[] wpnDatas;
		private static string assetPath;
		private static Transform holder;
		private static Transform gadgetSpawn;

        public readonly static List<Weapon> weapons = new();
		public static int Slot { get; private set; }
		public static Weapon Active { get => weapons[Slot]; private set => weapons[Slot] = value; }

		public static WeaponState State { get; private set; } = WeaponState.Idle;
		public static WeaponState Target { get; private set; } = WeaponState.Idle;
		private static WeaponState prevWeaponState = WeaponState.Idle;

		#endregion

		#region General

		public static void Init(AssetLabelReference _wpndataLabel, string wpnassetPath, Transform _wpnHolder, Transform throwSpawn)
        {
			assetPath = wpnassetPath;
			holder = _wpnHolder;
			gadgetSpawn = throwSpawn;
			Slot = 0;

			List<WeaponData> wpns = new();
			Addressables.LoadAssetsAsync<WeaponData>(_wpndataLabel, obj => {
				wpns.Add(obj);
			});
			
			wpnDatas = wpns.ToArray();
			for (int i = 0; i < wpnDatas.Length; i++)
			{
				wpnDatas[i].weaponID = i;
			}
		}

		public static void Setup()
        {
			Slot = Game.PlayerData.slot;

			foreach (string k in Game.PlayerData.wpnKeys)
			{
				Weapon w = CreateWeapon(k);
				weapons.Add(w);
			}

			initialSway = holder.localPosition;
		}

        public static void SetActive(bool value)
        {
            holder.gameObject.SetActive(value);
        }

		private static Vector3 initialSway;
		private static float lastFired;
		public static void Update()
		{
			if ((State != WeaponState.Idle && State != WeaponState.Aiming) || (Target != WeaponState.Idle && Target != WeaponState.Aiming)
				|| Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active)
				return;

			if (Target != WeaponState.Aiming && Input.Aim)
			{
				if (!Player.StateData.stayOnAim)
				{
					Player.SetState(new PlayerStanding(), true);
				}
				AimIn();
			}
			else if (Target == WeaponState.Aiming && !Input.Aim)
			{
				AimOut();
			}

			if (Game.PlayerData.Ammo[0] > 0 && Input.Shoot && Active.wpnData.automatic && Time.time - lastFired > 60 / Active.wpnData.fireRate)
			{
				Debug.Log("Auto Shot");
				Shoot();
			}
		}

		#endregion

		#region State

		public static void SetTarget(WeaponState state)
		{
			Target = state;
		}
		public static void SetStateToTarget()
		{
			State = Target;
		}

		public static void Reset() {
			State = WeaponState.Idle;
			Target = WeaponState.Idle;
			prevWeaponState = WeaponState.Idle;
		}

		#endregion

		#region Weapon Core

		private static AsyncOperationHandle<WeaponAssets> LoadWeapon(string key)
		{
			return Addressables.LoadAssetAsync<WeaponAssets>(assetPath + key + ".asset");
		}
		private static Weapon CreateWeapon(string key)
		{
			Weapon w = new();
			w.wpnData = wpnDatas[GetWeaponIDByKey(key)];
			w.wpnHandle = LoadWeapon(key);
			w.wpnHandle.WaitForCompletion();
			w.wpnAssets = w.wpnHandle.Result;

			w.wpnObject = Object.Instantiate(w.wpnAssets.prefab, holder.position, holder.rotation, holder);
			//TODO: Get Animator
			return w;
		}
		public static void PickUpWeapon(string key)
		{
			//TODO: Throw old weapon
			//Rigidbody r = Instantiate(weaponOptics[selectedIndex], weapons[selectedIndex].position, weapons[selectedIndex].rotation).GetComponent<Rigidbody>();
			//r.AddForce(transform.forward, ForceMode.VelocityChange);

			Weapon w = CreateWeapon(key);

			if (weapons.Count < Game.PlayerData.holsterSize)
			{
				weapons.Add(w);
				List<string> temp = new();
				temp.AddRange(Game.PlayerData.wpnKeys);
				temp.Add(key);
				Game.PlayerData.wpnKeys = temp.ToArray();
			}
			else
			{
				Addressables.Release(Active.wpnHandle);
				weapons[Slot] = w;
				Game.PlayerData.wpnKeys[Slot] = key;
			}
		}
		private static int GetWeaponIDByKey(string key)
		{
			for (int i = 0; i < wpnDatas.Length; i++)
			{
				if (key + ".asset" == wpnDatas[i].name)
				{
					return i;
				}
			}
			return -1;
		}
		public static void DrawWeapon()
		{
			hideTask.Cancel();
			Active.wpnObject.SetActive(true);
			Audio.PlayerSFX(Active.wpnAssets.equipSound);
			//muzzleParticles.transform.localPosition = sideData.muzzlePos;

			Active.wpnAnimator.Play("Equip", 0);
			UI.GUI.UpdateAmmoBar();
		}
		private static CancellationTokenSource hideTask;
		public static void HideWeapon()
		{
			Audio.PlayerSFX(Active.wpnAssets.holsterSound);

			Active.wpnAnimator.SetTrigger("Holster");

			hideTask = new CancellationTokenSource(Active.wpnData.holsterTime);
			Task.Run(DisableWeapon, hideTask.Token);
		}
		private static void DisableWeapon()
		{
			Active.wpnObject.SetActive(false);
		}
		public static bool HasWeapon(string weaponName)
		{
			for (int i = 0; i < Game.PlayerData.holsterSize; ++i)
			{
				if (Game.PlayerData.wpnKeys[i] == weaponName)
				{
					return true;
				}
			}
			return false;
		}
		public static bool HasWeapon(int weaponID)
		{
			for (int i = 0; i < Game.PlayerData.holsterSize; ++i)
			{
				if (GetWeaponIDByKey(Game.PlayerData.wpnKeys[i]) == weaponID)
				{
					return true;
				}
			}
			return false;
		}
		
		#endregion

		#region Shooting

		public static void ShootSemi()
		{
			if (Game.PlayerStatus != PlayerStatus.Alive || Game.GameState != GameState.Active || Game.PlayerData.Ammo[0] <= 0 || UI.DebugConsole.instance.IsActive)
				return;
			//if (Player.State == PlayerRunning || Player.State == PlayerLadder)
			//	return; TODO
			if ((State != WeaponState.Idle && State != WeaponState.Aiming) || (Target != WeaponState.Idle && Target != WeaponState.Aiming))
				return;

			if (!Active.wpnData.automatic)
			{
				if (Time.time - lastFired > 60 / Active.wpnData.fireRate)
				{
					Shoot();
				}
			}
		}
		private static void Shoot()
		{
			Vector3 pos = Cam.Transform.position + Cam.Transform.forward * 0.1f;
			Quaternion dir = Cam.Transform.rotation;
			lastFired = Time.time;

			--Game.PlayerData.Ammo[0];
			Audio.PlayerSFX(Active.wpnAssets.shootSound);
			Active.wpnAnimator.SetTrigger("Fire");
			ObjectPool.Request("Casing", Active.wpnObject.transform.GetChild(0).transform.position, Active.wpnObject.transform.GetChild(0).transform.rotation, InitCasing);

			if (State == WeaponState.Idle)
			{
				dir.eulerAngles.Set(dir.eulerAngles.x + Random.Range(-Active.wpnData.xSpread, Active.wpnData.xSpread), dir.eulerAngles.y +
														Random.Range(-Active.wpnData.ySpread, Active.wpnData.ySpread), dir.eulerAngles.z);
			}
			else if (State == WeaponState.Aiming)
			{
				Cam.Transform.rotation = Quaternion.Slerp(Cam.Transform.rotation,
											Quaternion.Euler(Cam.Transform.rotation.eulerAngles.x - Active.wpnData.yRecoil, Cam.Transform.rotation.eulerAngles.y +
											Random.Range(-Active.wpnData.xRecoil, Active.wpnData.xRecoil), Cam.Transform.rotation.eulerAngles.z), 0.5f);
			}

			ObjectPool.Request("Bullet", pos, dir, InitBullet);
		}
		private static void InitCasing(GameObject obj)
		{
			Rigidbody r = obj.GetComponent<Rigidbody>();
			r.velocity = Vector3.zero;
			//r.angularVelocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
			r.AddForce((Player.Transform.right / 10) + new Vector3(Random.Range(0f, 0f), Random.Range(3f, 8f), Random.Range(0f, 0f)), ForceMode.Impulse);
		}
		private static void InitBullet(GameObject obj)
		{
			obj.GetComponent<Rigidbody>().velocity = obj.transform.forward * Active.wpnData.bulletForce;
			obj.GetComponent<Bullet>().dmg = Active.wpnData.damage;

			//muzzleParticles.Emit(1);
			MuzzleFlashLight();

			UI.GUI.UpdateAmmoBar();
		}

		#endregion

		#region Aiming

		private static void AimIn()
		{
			Target = WeaponState.Aiming;
			State = WeaponState.Idle;

			Active.wpnAnimator.SetBool("Aim", true);
			Audio.PlayerSFX(Active.wpnAssets.aimSound);

			Cam.SetFoV(Active.wpnData.aimFov, true);
		}
		private static void AimOut()
		{
			Target = WeaponState.Idle;
			State = WeaponState.Aiming;
			Active.wpnAnimator.SetBool("Aim", false);
			Cam.SetFoV(Core.GameSettings.Settings.visuals.fov.Get(), true);
		}

		#endregion

		#region Reload

		private static CancellationTokenSource reloadTask;
		public static void Reload()
		{
			//if (Player.State == PlayerLadder || Game.PlayerData.Ammo[0] >= Active.wpnData.magSize || Game.PlayerData.Ammo[0] < 1)
			//	return; TODO

			if (State == WeaponState.Idle && Target == WeaponState.Idle)
			{
				Active.wpnAnimator.SetTrigger("Reload");
				Target = State = WeaponState.Reloading;

				Audio.PlayerSFX(Active.wpnAssets.reloadSound);

				reloadTask = new CancellationTokenSource(Active.wpnData.reloadTime);
				
				Task.Run(FinishReload, reloadTask.Token);
			}
		}
		private static void FinishReload()
        {
			if (Game.PlayerData.Ammo[Slot] > Active.wpnData.magSize - Game.PlayerData.Ammo[Slot])
			{
				Game.PlayerData.Ammo[Slot] -= Active.wpnData.magSize - Game.PlayerData.Ammo[Slot];
				Game.PlayerData.Ammo[Slot] = Active.wpnData.magSize;
			}
			else
			{
				Game.PlayerData.Ammo[Slot] += Game.PlayerData.Ammo[0];
				Game.PlayerData.Ammo[Slot] = 0;
			}

			State = WeaponState.Idle;
			UI.GUI.UpdateAmmoBar();
		}
		public static void StopReload()
		{
			reloadTask.Cancel();
		}

		#endregion

		#region Weapon Extras

		private static Light muzzleLight;
		private static int muzzleDuration = 300;
		private static void MuzzleFlashLight()
		{
			muzzleLight.enabled = true;
			Task.Delay(muzzleDuration).ContinueWith(t => DisableMuzzleLight());
		}
		private static void DisableMuzzleLight()
		{
			muzzleLight.enabled = false;
		}
		public static void UpdateSway()
		{
			Vector2 m = -Input.Mouse() * 0.002f;

			if (State == WeaponState.Aiming)
				m /= 6f;

			m.x = Mathf.Clamp(m.x, -0.006f, 0.006f);
			m.y = Mathf.Clamp(-m.y * 5, -0.06f, 0.06f);
			Vector3 sway = new(m.x, m.y / 5, 0);

			holder.localPosition = Vector3.Lerp(holder.localPosition, sway + initialSway, Time.deltaTime * 4);
		}
		public static bool LightOn { get; private set; } = false;
		public static void SwitchLight()
        {
			if (!Player.StateData.canTurnOnLight)
				return;

			LightOn = !LightOn;
        }

		#endregion

		#region Gadget

		private static Animator gadgetAnimator;
		private static bool throwOnLoaded = false;
		private static bool throwing = false;
		public static int activeGadget = 0;
		private static readonly string[] gadgetKeys = { "HEGrenade", "Molotov" };
		//Gadget Settings
		private static float grenadeSpawnDelay = 0.35f;

		public static void OnSwitchThrowable(int value)
		{
			activeGadget += value;

			if (activeGadget >= gadgetKeys.Length)
				activeGadget %= gadgetKeys.Length;
			else if (activeGadget < 0)
				activeGadget += gadgetKeys.Length;

			UI.GUI.UpdateAmmoBar();
		}

		public static void OnThrow(bool pressed)
		{
			//if (State == PlayerLadder) TODO
			//	return;
			if (State == WeaponState.Reloading || State == WeaponState.Aiming || Target == WeaponState.Reloading || Target == WeaponState.Aiming)
				return;

			if (pressed && !throwing)
			{
				if (Target != WeaponState.Throwing && Game.PlayerData.gadgets[activeGadget] > 0)
				{
					DisableWeapon();
					throwing = true;
					Target = WeaponState.Throwing;
					ObjectPool.Request(gadgetKeys[activeGadget], gadgetSpawn.position, gadgetSpawn.rotation, InitThrowable);
				}
			}
			else if (throwing)
			{
				throwing = false;
				if (State == WeaponState.Throwing)
				{
					Game.PlayerData.gadgets[activeGadget]--;
					UI.GUI.UpdateAmmoBar();
					gadgetAnimator.SetTrigger("Throw");
					Task.Delay(200).ContinueWith(t=> ThrowNade());
				}
				else if (Target == WeaponState.Throwing)
                    throwOnLoaded = true;
			}
		}
		private static void InitThrowable(GameObject obj)
		{
			gadgetAnimator = obj.GetComponent<Animator>();
			gadgetAnimator.Play("Equip", 0);
			prevWeaponState = State;
			State = Target;
			if (throwOnLoaded)
			{
				--Game.PlayerData.gadgets[activeGadget];
				UI.GUI.UpdateAmmoBar();
				gadgetAnimator.SetTrigger("Throw");
				Task.Delay(200).ContinueWith(t => ThrowNade());
			}
		}
		private static void ThrowNade()
		{
			throwOnLoaded = false;
			gadgetAnimator.SendMessage("Throw");
			Reset();
			DrawWeapon();
		}

		#endregion
	}
}
