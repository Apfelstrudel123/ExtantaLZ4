using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.InputSystem;
using Core.GameSettings;
using Gameplay.Combat;

namespace Gameplay
{
    public class Cam : MonoBehaviour
    {
        public static Cam active;
        [HideInInspector] public Camera cam;
        [HideInInspector] public HDAdditionalCameraData camData;

        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Quaternion startRotation;

        private void Awake()
        {
            active = this;
            cam = GetComponent<Camera>();
            camData = GetComponent<HDAdditionalCameraData>();
        }

        public void Init()
        {
            
        }

        public void SetLoadingScreenPosition()
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
        }

        public void UpdateSettings()
        {
            camData.renderingPathCustomFrameSettings.lodBias = Settings.visuals.lod.Get() / 10f;
            camData.antialiasing = (HDAdditionalCameraData.AntialiasingMode)Settings.visuals.aaMethod.Get();
            camData.SMAAQuality = (HDAdditionalCameraData.SMAAQualityLevel)Settings.visuals.aaQuality.Get();
            camData.TAAQuality = (HDAdditionalCameraData.TAAQualityLevel)Settings.visuals.aaQuality.Get();
            camData.taaSharpenStrength = Settings.visuals.taaSharpen.Get();
            cam.fieldOfView = Settings.visuals.fov.Get();
            camData.allowDynamicResolution = Settings.visuals.dynamicRes.Get();
        }

		private float targetFOV;
		private Vector2 mouse;
		private Camera _camera;
		[Header("Mouse/Cam")]
		[SerializeField] private InputAction mouseMovement;
		[SerializeField] private float upwardPitchAngle = -90.0f;
		[SerializeField] private float downwardPitchAngle = 90.0f;

		public void UpdateCamera()
		{
			if (Core.Game.GameState != GameState.Active) { return; }
			if (_camera.fieldOfView != targetFOV)
			{
				if ((Weapons.State == WeaponState.Idle && targetedWeapon == WeaponState.Aiming) || (targetedWeapon == WeaponState.Idle && Weapons.State == WeaponState.Aiming))
				{
					float dif = _camera.fieldOfView - targetFOV;
					if (dif > 4f)
					{
						_camera.fieldOfView -= Weapons.Active.wpnData.fovSpeed * Time.deltaTime;
					}
					else if (dif < -4f)
					{
						_camera.fieldOfView += Weapons.Active.wpnData.fovSpeed * Time.deltaTime;
					}
					else
					{
						Weapons.State = targetedWeapon;
						_camera.fieldOfView = targetFOV;
					}
				}
			}

			mouse = mouseMovement.ReadValue<Vector2>();
			var yaw = mouse.x * Settings.controls.horizontalSensitivity.Get() / (Settings.visuals.fov.Get() / _camera.fieldOfView);
			var pitch = -mouse.y * Settings.controls.verticalSensitivity.Get() / (Settings.visuals.fov.Get() / _camera.fieldOfView);

			pitch = Mathf.Clamp(pitch, -180f, 180f);

			if (!Player.StateData.clampYaw)
			{
				transform.rotation *= Quaternion.Euler(0.0f, yaw, 0.0f);
				if (cam.transform.localRotation.eulerAngles.x <= upwardPitchAngle)
				{
					if (cam.transform.localRotation.eulerAngles.x + pitch > upwardPitchAngle)
					{
						pitch = 0;
					}
				}
				else if (cam.transform.localRotation.eulerAngles.x >= downwardPitchAngle)
				{
					if (cam.transform.localRotation.eulerAngles.x + pitch < downwardPitchAngle)
					{
						pitch = 0;
					}
				}
				cam.transform.RotateAround(cam.transform.position, transform.right, pitch);//.localRotation *= Quaternion.Euler(pitch, 0f, 0f);
			}
			else
			{
				float x;
				if (cam.transform.localRotation.eulerAngles.x > 180)
				{
					x = Mathf.Clamp(cam.transform.localRotation.eulerAngles.x + pitch, 360 + upwardPitchAngle, 360 + downwardPitchAngle);
				}
				else
				{
					x = Mathf.Clamp(cam.transform.localRotation.eulerAngles.x + pitch, upwardPitchAngle, downwardPitchAngle);
				}

				float y;
				if (cam.transform.localRotation.eulerAngles.y > 180)
				{
					y = Mathf.Clamp(cam.transform.localRotation.eulerAngles.y + yaw, 360 + Player.StateData.minYawAngle, 360 + Player.StateData.maxYawAngle);
				}
				else
				{
					y = Mathf.Clamp(cam.transform.localRotation.eulerAngles.y + yaw, Player.StateData.minYawAngle, Player.StateData.maxYawAngle);
				}

				cam.transform.localRotation = Quaternion.Euler(x, y, 0.0f);
			}
		}
	}
}
