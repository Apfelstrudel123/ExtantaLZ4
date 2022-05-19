using System.Collections.Generic;
using UnityEngine;
using Gameplay.Combat;

namespace Gameplay.Interaction
{
    public static class Interact
    {
        public static Interactable Active { get; private set; }

        public static void InteractWithActive()
        {
            /*if (currentInteractName == "Vault")
				{
					col.enabled = false;
					rig.velocity = Vector3.zero;
					cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					vaultAngle = transform.forward;
					SetState(typeof(PlayerVaulting), false);
				}*/

            Active.onInteract.Invoke();
            if (Active.disableOnInteract)
            {
                RemoveControl(Active);
                Active.active = false;
                Active = null;
            }
        }

		private static bool lightOn = false;
		[Space()]
		[Header("Interact")]
		private static float pickUpDist;
		private static GameObject fovLight;
		private static LayerMask interactLayer = new LayerMask();

		public static void Update()
		{
			if (Weapons.State != Weapons.Target || Weapons.State != WeaponState.Idle)
				return;

			if (Physics.Raycast(Cam.Transform.position, Cam.Transform.forward, out RaycastHit hit, pickUpDist, interactLayer, QueryTriggerInteraction.Collide))
			{
				Interactable interactable = GetInteractable(hit.transform);

				if (interactable != null)
				{		
					if (interactable != Active)
					{
						if (Active != null)
						{
							RemoveControl(Active);
							Active = null;
						}

						if (interactable.onInteract != null && interactable.active)
						{
							NewControl(interactable);
							Active = interactable;
						}
					}
					return;
				}
			}
			if (Active != null)
			{
				RemoveControl(Active);
				Active = null;
			}

		}
		public static void OnInteract()
		{
			if (Player.State == Horse)
			{
				Movement.UnmountHorse();

			}
			if (Active == null || Active.Type() != InteractionType.Primary)
				return;

			Weapons.StopReload();
			Active.onInteract.Invoke();
			if (Active.disableOnInteract)
			{
				RemoveControl(Active);
				Active.active = false;
				Active = null;
			}
		}
		private static Interactable GetInteractable(Transform t)
		{
			if (t.TryGetComponent<Interactable>(out Interactable i))
			{
				return i;
			}
			else if (t.transform.gameObject.layer == 19)
			{
				return t.transform.GetComponent<AI.AIHitbox>().parent.GetComponentInChildren<Interactable>();
			}
			return null;
		}

		private static readonly List<IInteractable> interactions = new();
		

		public static void NewControl(IInteractable interactable)
		{
			if (!interactions.Contains(interactable))
			{
				interactions.Add(interactable);
				UI.GUI.UpdateControlPanel(interactions.ToArray());
			}
		}
		public static void RemoveControl(IInteractable interactable)
		{
			//TODO: Get index + check index
			if (!interactions.Contains(interactable))
				return;
			int i = interactions.IndexOf(interactable);
			interactions.RemoveAt(i);
			UI.GUI.UpdateControlPanel(interactions.ToArray());
		}

		public static void Clear()
        {
			interactions.Clear();
			UI.GUI.UpdateControlPanel(null);
		}

		private static void SwitchLight()
		{
			lightOn = !lightOn;
			fovLight.SetActive(lightOn);
		}
	}
}
