using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    [RequireComponent(typeof(Player))]
    public class Input : MonoBehaviour
    {
        private static PlayerInput InputData { get; set; }
        private void Awake()
        {
            InputData = GetComponent<PlayerInput>();
            InputData.DeactivateInput();
        }

		public static void Activate()
        {
			InputData.ActivateInput();
        }
		public static void Deactivate()
		{
			InputData.DeactivateInput();
		}

		public static void LoadJSON(string json)
        {
			InputData.actions.LoadFromJson(json);
        }

		public static Vector2 Move { get; private set; }
		public static Vector2 Drive { get; private set; }

		public static bool SecondaryInteract { get; private set; }
		public static bool Shoot { get; private set; }
		public static bool Run { get; private set; }
		public static bool Aim { get; private set; }
		public static bool Handbrake { get; private set; }

		private void OnCrouch()
		{
			Movement.OnCrouch();
		}
		private void OnProne()
		{
			Movement.OnProne();
		}
		private void OnJump()
		{
			Movement.OnJump();
		}	
		private void OnHandbrake() { 
			Handbrake = !Handbrake; 
		}
		private void OnLean(InputValue value) {
			Movement.OnLean(value);
		}
		private void OnDrive(InputValue value) { 
			Drive = value.Get<Vector2>(); 
		}
		private void OnMove(InputValue value) {
			Move = value.Get<Vector2>();
		}
		private void OnRun() { 
			Run = !Run; 
		}
		private void OnShoot() {
			Shoot = !Shoot;
			if(Shoot)
				Combat.Weapons.ShootSemi();
        }
		private void OnAim() {
			Aim = !Aim; 
		}

        private void OnInteract() {
            Interaction.OnInteract();
        }

        private void OnHotkeyThrowable() {
		}
		private void OnHotkeyExtra() {
		}
		private void OnSecInteract() {
			SecondaryInteract = !SecondaryInteract;
		}
		private void OnSwitchLight() {
			if (Player.StateData.canTurnOnLight) 
				Player.SwitchLight();
		}
	}
}
