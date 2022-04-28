using UnityEngine;
using UnityEngine.InputSystem;
using GameWorld;
using Core.GameSettings;
using Gameplay.Combat;

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
		private static Player player;
		private static Transform transform;

		private static float airTime;
		private static bool grounded;

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
			player = Player.active;
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

			if (Interaction.Active != null && Interaction.Active.interactionType == Interactable.InteractionType.Parkour)
				Interaction.InteractWithActive();
			else
				Player.State.GetInput(InputType.Jump);
		}

		#endregion

		#region Riding

		private static Horse activeHorse;
		public static void MountHorse(Horse h)
		{
			Weapons.State = WeaponState.Idle;
			targetedWeapon = WeaponState.Idle;
			prevWeaponState = WeaponState.Idle;
			_camera.fieldOfView = Settings.visuals.fov.Get();
			Input.Drive = Vector2.zero;
			activeHorse = h;
			activeHorse.isUsed = true;
			fxAudio.Stop();
			Player.Rigbody.velocity = Vector3.zero;
			Player.WallCollider.enabled = false;
			Player.GroundCollider.enabled = false;
			Player.Rigbody.constraints = RigidbodyConstraints.FreezeAll;
			Player.Transform.SetPositionAndRotation(activeHorse.playerPos.position, activeHorse.playerPos.rotation);
			Cam.active.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Player.SetState(typeof(HorseStanding), false);
		}
		private static void UnmountHorse()
		{
			RemoveControl(currentInteractName);
			currentInteractable = null;
			currentInteractName = string.Empty;
			activeHorse.isUsed = false;
			transform.position = activeHorse.exitPos.position;
			wallCollider.enabled = true;
			groundCollider.enabled = true;
			rigbody.constraints = RigidbodyConstraints.FreezeRotation;
			transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
			cam.localRotation = Quaternion.Euler(0f, 0f, 0f);
			activeHorse.OnLeave();
			activeHorse = null;
			ResetPlayer(true);
		}

		#endregion

		#region Ladder Interaction

		private static Ladder activeLadder;
		public static void UseLadder(Ladder ladder)
		{
			if (Weapons.State == WeaponState.Aiming || targetedWeapon == WeaponState.Aiming)
				AimOut();
			Weapons.State = WeaponState.Idle;
			targetedWeapon = WeaponState.Idle;
			prevWeaponState = WeaponState.Idle;

			targetFOV = Settings.visuals.fov.Get();
			fxAudio.Stop();
			Player.Rigbody.velocity = Vector3.zero;
			Player.WallCollider.enabled = false;
			Player.GroundCollider.enabled = false;
			currentLadder = ladder;

			transform.SetPositionAndRotation(ClosestPointOnLadder(ladder.start.position, ladder.end.position),
				Quaternion.Euler(0f, ladder.transform.rotation.y - 90f, 0f));
			Cam.active.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

			Player.SetState(new PlayerLadder(ladder), false);
			HideWeapon();
		}
		private static Vector3 ClosestPointOnLadder(Vector3 start, Vector3 end)
		{
			Vector3 v = transform.position - start;
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
			wallCollider.enabled = true;
			groundCollider.enabled = true;
			rigbody.velocity = Vector3.zero;
			rigbody.constraints = RigidbodyConstraints.FreezeRotation;

			if ((transform.position - activeLadder.start.position).sqrMagnitude < (transform.position - activeLadder.end.position).sqrMagnitude)
				transform.position = activeLadder.enter.position;
			else
				transform.position = activeLadder.exit.position;

			transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
			cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

			currentLadder.Exit();
			currentLadder = null;
			ResetPlayer(true);
			DrawWeapon();
		}

		#endregion

		#region Vaulting

		private static Vector3 vaultAngle;
		public static void Unvault()
		{
			Player.WallCollider.enabled = true;
			Player.GroundCollider.enabled = true;
			Player.Rigbody.velocity = Vector3.zero;
			Player.Rigbody.constraints = RigidbodyConstraints.FreezeRotation;
			Player.ResetPlayer(true);
			Cam.active.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}

        #endregion

        #region Water Interaction

        private static void Dive()
		{
			if (!inWater)
			{
				inWater = true;
				mixer.SetFloat("FX_Reverb_Mix", 0f);
				fxAudio.PlayOneShot(diveIn);
				ambientAudio.clip = waterIdle;
				ambientAudio.Play();
			}
			else if (inWater)
			{
				inWater = false;
				mixer.SetFloat("FX_Reverb_Mix", -80f);
				ambientAudio.Stop();
				fxAudio.PlayOneShot(diveOut);
			}
		}

        #endregion

        #region Leaning

        private static LeaningState leaningState = LeaningState.None;

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

			if (leaningState == to && leaningState != LeaningState.None)
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
			leaningState = to;
		}

        #endregion
    }
}
