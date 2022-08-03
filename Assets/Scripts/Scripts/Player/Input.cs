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
        private static bool InputAllowed()
        {
            return Core.Game.PlayerStatus == PlayerStatus.Alive && Core.Game.GameState == GameState.Active && !UI.DebugConsole.instance.IsActive;
        }

        public static Vector2 Mouse()// { get; private set; }
        {
            return new Vector2(); // TODO mouseMovement.ReadValue<Vector2>();
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
            if (InputAllowed())
                Movement.OnCrouch();
        }
        private void OnProne()
        {
            if (InputAllowed())
                Movement.OnProne();
        }
        private void OnJump()
        {
            if (InputAllowed())
                Movement.OnJump();
        }
        private void OnHandbrake()
        {
            Handbrake = !Handbrake;
        }
        private void OnLean(InputValue value)
        {
            if (InputAllowed())
                Movement.OnLean(value);
        }
        private void OnDrive(InputValue value)
        {
            Drive = value.Get<Vector2>();
        }
        private void OnMove(InputValue value)
        {
            Move = value.Get<Vector2>();
        }
        private void OnRun()
        {
            Run = !Run;
        }
        private void OnShoot()
        {
            Shoot = !Shoot;
            if (Shoot && InputAllowed())
                Combat.Weapons.ShootSemi();
        }
        private void OnAim()
        {
            Aim = !Aim;
        }

        private void OnInteract()
        {
            if (InputAllowed())
                Interaction.Interact.OnInteract();
        }

        private void OnHotkeyThrowable()
        {
        }
        private void OnHotkeyExtra()
        {
        }
        private void OnSecInteract()
        {
            SecondaryInteract = !SecondaryInteract;
        }
        private void OnSwitchLight()
        {
            if (InputAllowed())
                Combat.Weapons.SwitchLight();
        }

        private void OnSwitchThrowable(InputValue value)
        {
            if (InputAllowed())
                Combat.Weapons.OnSwitchThrowable((int)(value.Get<float>() / 120f));
        }

        private void OnThrow(InputValue value)
        {
            if (InputAllowed())
                Combat.Weapons.OnThrow(value.isPressed);
        }
        private static void OnReload()
        {
            if (InputAllowed())
                Combat.Weapons.Reload();
        }
    }
}
