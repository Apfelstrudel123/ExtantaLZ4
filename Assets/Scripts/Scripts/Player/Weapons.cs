using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ScriptableObjects;
namespace Gameplay
{
    namespace Combat
    {
		public enum WeaponState
		{
			Idle,
			Reloading,
			Aiming,
			Throwing
		}
		public struct Weapon
        {
			public UnityEngine.GameObject wpnObject;
			public UnityEngine.Animator wpnAnimator;
			public WeaponData wpnData;
            public WeaponAssets wpnAssets;
			public AsyncOperationHandle<WeaponAssets> wpnHandle;
		}
        public static class Weapons
        {
			//CONST
			private static WeaponData[] weaponDatas;
			private static AssetLabelReference wpnLabel;
			private static string wpnassetPath;
			private static Transform weaponHolder;

			public static int Slot { get; private set; }
            public readonly static List<Weapon> weapons = new();
			public static Weapon Active { get => weapons[Slot]; private set => weapons[Slot] = value; }

			public static void Init()
            {
				Slot = 0;
            }

            public static void SetActive(bool value)
            {
                weaponHolder.gameObject.SetActive(value);
            }

			public static WeaponState State { get; private set; } = WeaponState.Idle;
			public static WeaponState targetedWeapon = WeaponState.Idle;
			private static WeaponState prevWeaponState = WeaponState.Idle;

			private static Vector3 initialSwayPosition;	
			private static float lastFired;

			public static void Update()
			{
				if ((State == WeaponState.Idle || State == WeaponState.Aiming) && (targetedWeapon == WeaponState.Idle || targetedWeapon == WeaponState.Aiming)
					&& Core.Game.PlayerStatus == PlayerStatus.Alive && Core.Game.GameState == GameState.Active)
				{
					if (targetedWeapon != WeaponState.Aiming && Input.Aim)
					{
						if (!Player.StateData.stayOnAim)
						{
							Player.SetState(new PlayerStanding(), true);
						}
						AimIn();
					}
					else if (targetedWeapon == WeaponState.Aiming && !Input.Aim)
					{
						AimOut();
					}

					if (ammo > 0 && Input.Shoot && Active.wpnData.automatic && Time.time - lastFired > 60 / Active.wpnData.fireRate)
					{
						Debug.Log("Auto Shot");
						Shoot();
					}
				}
			}	

			private static AsyncOperationHandle<WeaponAssets> LoadWeapon(string key)
			{
				return Addressables.LoadAssetAsync<WeaponAssets>(wpnassetPath + key + ".asset");
			}

			public static void PickUpWeapon(string key)
			{
				//TODO: Throw old weapon
				//Rigidbody r = Instantiate(weaponOptics[gunKeys[selectedIndex]], weapons[selectedIndex].position, weapons[selectedIndex].rotation).GetComponent<Rigidbody>();
				//r.AddForce(transform.forward, ForceMode.VelocityChange);

				Weapon w = new();
				w.wpnData = weaponDatas[GetWeaponIDByKey(key)];
				w.wpnHandle = LoadWeapon(key);
				w.wpnHandle.WaitForCompletion();
				w.wpnAssets = w.wpnHandle.Result;

				w.wpnObject = Object.Instantiate(w.wpnAssets.prefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
				//TODO: Get Animator

				if (weapons.Count < Core.Game.PlayerData.holsterSize)
                {	
					weapons.Add(w);
					List<string> temp = new();
					temp.AddRange(Core.Game.PlayerData.wpnKeys);
					temp.Add(key);
					Core.Game.PlayerData.wpnKeys = temp.ToArray();
				}
				else
                {
					Addressables.Release(Active.wpnHandle);
					weapons[Slot] = w;
					Core.Game.PlayerData.wpnKeys[Slot] = key;
				}	
			}
			private static int GetWeaponIDByKey(string key)
			{
				for (int i = 0; i < weaponDatas.Length; i++)
				{
					if (key + ".asset" == weaponDatas[i].name)
					{
						return i;
					}
				}
				return -1;
			}
			private static void DrawWeapon()
			{
				CancelInvoke(nameof(DisableWeapon));
				Active.wpnObject.SetActive(true);
				fxAudio.PlayOneShot(Active.wpnAssets.equipSound);
				//muzzleParticles.transform.localPosition = sideData.muzzlePos;

				Active.wpnAnimator.Play("Equip", 0);
				UI.GUI.UpdateAmmoBar();
			}
			private static void HideWeapon()
			{
				fxAudio.PlayOneShot(Active.wpnAssets.holsterSound);

				Active.wpnAnimator.SetTrigger("Holster");
				Invoke(nameof(DisableWeapon), Active.wpnData.holsterTime);
			}
			private static void DisableWeapon()
			{
				Active.wpnObject.SetActive(false);
			}
			public static bool HasWeapon(string weaponName)
			{
				for (int i = 0; i < Core.Game.PlayerData.holsterSize; ++i)
				{
					if (Core.Game.PlayerData.wpnKeys[i] == weaponName)
					{
						return true;
					}
				}
				return false;
			}
			public static bool HasWeapon(int weaponID)
			{
				for(int i = 0; i < Core.Game.PlayerData.holsterSize; ++i)
                {
					if (GetWeaponIDByKey(Core.Game.PlayerData.wpnKeys[i]) == weaponID)
					{
						return true;
					}
				}	
				return false;
			}
			private static void MuzzleFlashLight()
			{
				muzzleLight.enabled = true;
				Invoke(DisableMuzzleLight, muzzleDuration);
			}
			private static void DisableMuzzleLight()
            {
				muzzleLight.enabled = false;
			}
			public static void ShootSemi()
			{
				if (Core.Game.PlayerStatus != PlayerStatus.Alive || Core.Game.GameState != GameState.Active || ammo <= 0 || UI.DebugConsole.instance.IsActive)
					return;
				if (Player.State == PlayerRunning || Player.State == PlayerLadder)
					return;
				if ((State != WeaponState.Idle && State != WeaponState.Aiming) || (targetedWeapon != WeaponState.Idle && targetedWeapon != WeaponState.Aiming))
					return;

				if (!Active.wpnData.automatic)
				{
					if (Time.time - lastFired > 60 / Active.wpnData.fireRate)
					{
						Shoot();
					}
				}
			}
			private static void AimIn()
			{
				targetedWeapon = WeaponState.Aiming;
				State = WeaponState.Idle;

				Active.wpnAnimator.SetBool("Aim", true);
				fxAudio.PlayOneShot(Active.wpnAssets.aimSound);

				targetFOV = Active.wpnData.aimFov;
			}
			private static void AimOut()
			{
				targetedWeapon = WeaponState.Idle;
				State = WeaponState.Aiming;
				Active.wpnAnimator.SetBool("Aim", false);
				targetFOV = Core.GameSettings.Settings.visuals.fov.Get();
			}
			private static void Shoot()
			{
				Vector3 pos = cam.position + transform.forward * 0.1f;
				Quaternion dir = cam.transform.rotation;
				lastFired = Time.time;

				ammo--;
				fxAudio.PlayOneShot(Active.wpnAssets.shootSound);
				Active.wpnAnimator.SetTrigger("Fire");
				Core.ObjectPool.Request("Casing", Active.wpnObject.transform.GetChild(0).transform.position, Active.wpnObject.transform.GetChild(0).transform.rotation, InitCasing);

				if (State == WeaponState.Idle)
				{
					dir.eulerAngles.Set(dir.eulerAngles.x + Random.Range(-Active.wpnData.xSpread, Active.wpnData.xSpread), dir.eulerAngles.y +
															Random.Range(-Active.wpnData.ySpread, Active.wpnData.ySpread), dir.eulerAngles.z);
				}
				else if (State == WeaponState.Aiming)
				{
					cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,
												Quaternion.Euler(cam.transform.rotation.eulerAngles.x - Active.wpnData.yRecoil, cam.transform.rotation.eulerAngles.y +
												Random.Range(-Active.wpnData.xRecoil, Active.wpnData.xRecoil), cam.transform.rotation.eulerAngles.z), 0.5f);
				}

				Core.ObjectPool.Request("Bullet", pos, dir, InitBullet);
			}
			private static void InitCasing(GameObject obj)
			{
				Rigidbody r = obj.GetComponent<Rigidbody>();
				r.velocity = Vector3.zero;
				//r.angularVelocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
				r.AddForce((transform.right / 10) + new Vector3(UnityEngine.Random.Range(0f, 0f), UnityEngine.Random.Range(3f, 8f), UnityEngine.Random.Range(0f, 0f)), ForceMode.Impulse);
			}
			private static void InitBullet(GameObject obj)
			{
				obj.GetComponent<Rigidbody>().velocity = obj.transform.forward * Active.wpnData.bulletForce;
				obj.GetComponent<Bullet>().dmg = Active.wpnData.damage;

				//muzzleParticles.Emit(1);
				StartCoroutine(MuzzleFlashLight());

				UI.GUI.UpdateAmmoBar();
			}
			private static void OnReload()
			{
				if (Core.Game.PlayerStatus != PlayerStatus.Alive || Core.Game.GameState != GameState.Active || UI.DebugConsole.instance.IsActive)
				{ return; }
				if (Player.State == PlayerLadder)
				{ return; }

				if ((ammo >= Active.wpnData.magSize || Core.Game.PlayerData.ammo < 1))
				{
					return;
				}

				Reload();
			}
			private static void Reload()
			{
				if (State == WeaponState.Idle && targetedWeapon == WeaponState.Idle)
				{
					Active.wpnAnimator.SetTrigger("Reload");
					State = WeaponState.Reloading;


					fxAudio.PlayOneShot(Active.wpnAssets.reloadSound);

					Invoke(FinishReload, Active.wpnData.reloadTime);
				}
			}
			private static void FinishReload()
            {
				if (Core.Game.PlayerData.ammo > Active.wpnData.magSize - ammo)
				{
					Core.Game.PlayerData.ammo -= Active.wpnData.magSize - ammo;
					ammo = Active.wpnData.magSize;
				}
				else
				{
					ammo += Core.Game.PlayerData.ammo;
					Core.Game.PlayerData.ammo[Slot] = 0;
				}

				State = WeaponState.Idle;
				UI.GUI.UpdateAmmoBar();
			}
			public static void StopReload()
			{
				//TODO: Cancel invoke reload
				//CancelInvoke();
			}
		}
    }
}
