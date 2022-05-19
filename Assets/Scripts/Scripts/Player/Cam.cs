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
		public static Transform Transform { get; private set; }
        public static Camera Camera { get; private set; }
		public static HDAdditionalCameraData CamData { get; private set; }

		[SerializeField] private InputAction mouseMovement;
		[SerializeField] private float upwardPitchAngle = -90.0f;
		[SerializeField] private float downwardPitchAngle = 90.0f;

        private void Awake()
        {
            active = this;
			Transform = transform;
            Camera = GetComponent<Camera>();
            CamData = GetComponent<HDAdditionalCameraData>();
        }

        public void Init()
        {
            
        }
		
		public void UpdateCamera()
		{
			if (Core.Game.GameState != GameState.Active) { return; }
			if (Camera.fieldOfView != targetFoV)
			{
				if ((Weapons.State == WeaponState.Idle && Weapons.Target == WeaponState.Aiming) || (Weapons.Target == WeaponState.Idle && Weapons.State == WeaponState.Aiming))
				{
					float dif = Camera.fieldOfView - targetFoV;
					if (dif > 4f)
					{
						Camera.fieldOfView -= Weapons.Active.wpnData.fovSpeed * Time.deltaTime;
					}
					else if (dif < -4f)
					{
						Camera.fieldOfView += Weapons.Active.wpnData.fovSpeed * Time.deltaTime;
					}
					else
					{
						Camera.fieldOfView = targetFoV;
						Weapons.SetStateToTarget();
					}
				}
			}

			Vector2 mouse = Input.Mouse(); 
			var yaw = mouse.x * Settings.controls.horizontalSensitivity.Get() / (Settings.visuals.fov.Get() / Camera.fieldOfView);
			var pitch = -mouse.y * Settings.controls.verticalSensitivity.Get() / (Settings.visuals.fov.Get() / Camera.fieldOfView);

			pitch = Mathf.Clamp(pitch, -180f, 180f);

			if (!Player.StateData.clampYaw)
			{
				transform.rotation *= Quaternion.Euler(0.0f, yaw, 0.0f);
				if (transform.localRotation.eulerAngles.x <= upwardPitchAngle)
				{
					if (transform.localRotation.eulerAngles.x + pitch > upwardPitchAngle)
					{
						pitch = 0;
					}
				}
				else if (transform.localRotation.eulerAngles.x >= downwardPitchAngle)
				{
					if (transform.localRotation.eulerAngles.x + pitch < downwardPitchAngle)
					{
						pitch = 0;
					}
				}
				transform.RotateAround(transform.position, transform.right, pitch);//.localRotation *= Quaternion.Euler(pitch, 0f, 0f);
			}
			else
			{
				float x;
				if (transform.localRotation.eulerAngles.x > 180)
				{
					x = Mathf.Clamp(transform.localRotation.eulerAngles.x + pitch, 360 + upwardPitchAngle, 360 + downwardPitchAngle);
				}
				else
				{
					x = Mathf.Clamp(transform.localRotation.eulerAngles.x + pitch, upwardPitchAngle, downwardPitchAngle);
				}

				float y;
				if (transform.localRotation.eulerAngles.y > 180)
				{
					y = Mathf.Clamp(transform.localRotation.eulerAngles.y + yaw, 360 + Player.StateData.minYawAngle, 360 + Player.StateData.maxYawAngle);
				}
				else
				{
					y = Mathf.Clamp(transform.localRotation.eulerAngles.y + yaw, Player.StateData.minYawAngle, Player.StateData.maxYawAngle);
				}

				transform.localRotation = Quaternion.Euler(x, y, 0.0f);
			}
		}

		public void UpdateSettings()
		{
			CamData.renderingPathCustomFrameSettings.lodBias = Settings.visuals.lod.Get() / 10f;
			CamData.antialiasing = (HDAdditionalCameraData.AntialiasingMode)Settings.visuals.aaMethod.Get();
			CamData.SMAAQuality = (HDAdditionalCameraData.SMAAQualityLevel)Settings.visuals.aaQuality.Get();
			CamData.TAAQuality = (HDAdditionalCameraData.TAAQualityLevel)Settings.visuals.aaQuality.Get();
			CamData.taaSharpenStrength = Settings.visuals.taaSharpen.Get();
			Camera.fieldOfView = Settings.visuals.fov.Get();
			CamData.allowDynamicResolution = Settings.visuals.dynamicRes.Get();
		}

		private float targetFoV;
		public static void SetFoV(float fov, bool lerp)
        {
			active.targetFoV = fov;
			if (!lerp)
				Camera.fieldOfView = fov;
        }

		[SerializeField] private Vector3 startPosition;
		[SerializeField] private Quaternion startRotation;
		public void SetLoadingScreenPosition()
		{
			transform.SetPositionAndRotation(startPosition, startRotation);
		}
	}
}
