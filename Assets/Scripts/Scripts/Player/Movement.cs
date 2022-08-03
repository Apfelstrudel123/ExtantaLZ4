using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Core.GameSettings;
using GameWorld;
using Gameplay.Combat;
using Gameplay.Interaction;

namespace Gameplay
{
	public enum InputType
	{
		Crouch,
		Prone,
		Jump
	}
	public enum LeaningState
	{
		Left,
		None,
		Right
	}

	public static class Movement
    {
		//References

		private static float airTime;
		private static bool grounded;
		private static bool inWater;

		[Header("Movement")]
		public static float speed = 1f;
		public static float runningSpeed = 2f;
		public static float crouchSpeed = 0.5f;
		public static float proneSpeed = 0.25f;
		public static float jumpHeight = 100f;
		public static float ladderSpeed = 1f;
		public static float ladderFastSpeed = 1f;
		public static float ladderSlideSpeed = 1f;
		[SerializeField] private static float groundDistance = 0.3f;
		[SerializeField] private static float headDistance = 0.4f;
		[SerializeField] private static LayerMask headLayer = new LayerMask();
		[SerializeField] private static LayerMask groundMask = new LayerMask();

		public static void Init()
        {
			
        }

		public static bool CanChangeState(PlayerState newState, float newHead)
		{
			if (Player.State == newState || Physics.CheckCapsule(Player.StateData.headCheck, new Vector3(0f, newHead, 0f), headDistance, headLayer))
			{
				return false;
			}
			return true;
		}

		#region Input Handling

		private static bool InputAllowed()
		{
			return Core.Game.PlayerStatus == PlayerStatus.Alive && Core.Game.GameState == GameState.Active && !UI.DebugConsole.instance.IsActive;
		}

		public static void OnCrouch()
		{
			if (InputAllowed())
				Player.State.GetInput(InputType.Crouch);
		}
		public static void OnProne()
		{
			if (InputAllowed())
				Player.State.GetInput(InputType.Prone);
		}
		public static void OnJump()
		{
			if (!InputAllowed())
				return;

			if (Interact.Active != null && Interact.Active.Type() == InteractionType.Parkour)
				Interact.InteractWithActive();
			else
				Player.State.GetInput(InputType.Jump);
		}

		#endregion

		#region Riding

		private static readonly Control unmountControl = new ("Unmount", false, InteractionType.Primary);
		public static Horse ActiveHorse { get; private set; }
		public static void MountHorse(Horse h)
		{
			Weapons.Reset();	
			Cam.Camera.fieldOfView = Settings.visuals.fov.Get();
			//Input.Drive = Vector2.zero;
			ActiveHorse = h;
			ActiveHorse.isUsed = true;
			//fxAudio.Stop(); TODO
			Player.Rigidbody.velocity = Vector3.zero;
			Player.WallCollider.enabled = false;
			Player.GroundCollider.enabled = false;
			Player.Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			Player.Transform.SetPositionAndRotation(ActiveHorse.playerPos.position, ActiveHorse.playerPos.rotation);
			Cam.active.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Player.SetState(new HorseStanding(), false);
		}
		public static void UnmountHorse()
		{
			Interact.RemoveControl(unmountControl);
			unmountControl.active = false;
			ActiveHorse.isUsed = false;
			Player.Transform.position = ActiveHorse.exitPos.position;
			Player.WallCollider.enabled = true;
			Player.GroundCollider.enabled = true;
			Player.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			Player.Transform.rotation = Quaternion.Euler(0, Player.Transform.rotation.eulerAngles.y, 0);
			Cam.Camera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			ActiveHorse.OnLeave();
			ActiveHorse = null;
			Player.Reset(true);
		}

		#endregion

		#region Ladder Interaction

		private static Ladder activeLadder;
		public static void UseLadder(Ladder ladder)
		{
			if (Weapons.State == WeaponState.Aiming || Weapons.Target == WeaponState.Aiming)
				// TODO Weapons.AimOut();
			Weapons.Reset();

			Cam.SetFoV(Settings.visuals.fov.Get(), true);
			// fxAudio.Stop(); TODO
			Player.Rigidbody.velocity = Vector3.zero;
			Player.WallCollider.enabled = false;
			Player.GroundCollider.enabled = false;
			activeLadder = ladder;

			Player.Transform.SetPositionAndRotation(ClosestPointOnLadder(ladder.start.position, ladder.end.position),
				Quaternion.Euler(0f, ladder.transform.rotation.y - 90f, 0f));
			Cam.active.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

			Player.SetState(new PlayerLadder(ladder), false);
			Weapons.HideWeapon();
		}
		private static Vector3 ClosestPointOnLadder(Vector3 start, Vector3 end)
		{
			Vector3 v = Player.Transform.position - start;
			Vector3 dir = (end - start).normalized;

			float d = Vector3.Distance(start, end);
			float t = Vector3.Dot(dir, v);

			if (t <= 0)
				return start + dir * 0.05f;
			if (t >= d)
				return end - dir * 0.05f;

			return start + dir * t;
		}
		public static void ExitLadder()
		{
			Player.WallCollider.enabled = true;
			Player.GroundCollider.enabled = true;
			Player.Rigidbody.velocity = Vector3.zero;
			Player.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

			if ((Player.Transform.position - activeLadder.start.position).sqrMagnitude < (Player.Transform.position - activeLadder.end.position).sqrMagnitude)
				Player.Transform.position = activeLadder.enter.position;
			else
				Player.Transform.position = activeLadder.exit.position;

			Player.Transform.rotation = Quaternion.Euler(0, Player.Transform.rotation.eulerAngles.y, 0);
			Cam.active.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

			activeLadder.Exit();
			activeLadder = null;
			Player.Reset(true);
			Weapons.DrawWeapon();
		}

		#endregion

		#region Vaulting

		public static Vector3 vaultAngle {get; private set;}

		public static void FinishVault()
		{
			Player.WallCollider.enabled = true;
			Player.GroundCollider.enabled = true;
			Player.Rigidbody.velocity = Vector3.zero;
			Player.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			Player.Reset(true);
			Cam.active.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}

        #endregion

        #region Water Interaction

        private static void Dive() {
			if (!inWater) {
				Audio.EnterWater();
				inWater = true;
				Audio.mixer.SetFloat("FX_Reverb_Mix", 0f);
				Audio.EnterWater();
			}
			else {
				Audio.LeaveWater();
				inWater = false;
				Audio.mixer.SetFloat("FX_Reverb_Mix", -80f);
				Audio.LeaveWater();
			}
		}

        #endregion

        #region Leaning

        public static LeaningState Lean { get; private set; } = LeaningState.None;

		public static void OnLean(InputValue value)
		{
			if (Player.StateData.canLean)
			{
				if (value.Get<float>() > 0.5f)
				{
					SetLeaningState(LeaningState.Right);
				}
				else if (value.Get<float>() < -0.5f)
				{
					SetLeaningState(LeaningState.Left);
				}
			}
		}
		private static void SetLeaningState(LeaningState to)
		{
			if (!InputAllowed())
                return;

			if (Lean == to && Lean != LeaningState.None)
			{
				to = LeaningState.None;
			}

			float rot = 0f;
			Vector3 p = Cam.active.transform.localPosition;
			switch (to)
			{
				case LeaningState.Left:
					rot = 10f;
					p.x = -0.2f;
					break;
				case LeaningState.None:
					rot = 0f;
					p.x = 0f;
					break;
				case LeaningState.Right:
					rot = -10f;
					p.x = 0.2f;
					break;
				default:
					Debug.Log("LeaningState to invalid!");
					break;
			}

			Cam.active.transform.localPosition = p;
			Cam.active.transform.localRotation *= Quaternion.Euler(0, 0, rot - Cam.active.transform.localRotation.eulerAngles.z);
			Lean = to;
		}

        #endregion
    }
}
