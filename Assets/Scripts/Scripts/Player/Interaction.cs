using UnityEngine;
using Gameplay.Combat;

namespace Gameplay
{
    public static class Interaction
    {
        public static Interactable Active { get; private set; }
        public static string ActiveName { get; private set; }

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
                Player.RemoveControl(ActiveName);
                Active.active = false;
                Active = null;
                ActiveName = string.Empty;
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
			if (Weapons.State == targetedWeapon && (Weapons.State == WeaponState.Idle))
			{
				if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, pickUpDist, interactLayer, QueryTriggerInteraction.Collide))
				{
					Interactable i = GetInteractableType(hit.transform);

					if (i != Active)
					{
						if (Active != null)
						{
							RemoveControl(ActiveName);
							ActiveName = string.Empty;
						}
						if (i != null && i.onInteract != null && i.active)
						{
							string s = i.onInteract.GetPersistentMethodName(0);
							NewControl(s, (ControlType)i.interactionType);
							Active = i;
							ActiveName = s;
						}
					}
					else if (i != null)
					{
						string s = i.onInteract.GetPersistentMethodName(0);
						if (s != ActiveName)
						{
							if (ActiveName != string.Empty)
							{
								RemoveControl(ActiveName);
								ActiveName = string.Empty;
							}
							if (i.onInteract != null && i.active)
							{
								NewControl(s, ControlType.Interact);
								Active = i;
								ActiveName = s;
							}
						}
						else if (!i.active)
						{
							if (ActiveName != string.Empty)
							{
								RemoveControl(ActiveName);
								ActiveName = string.Empty;
							}
						}
					}
				}
				else if (Active != null)
				{
					RemoveControl(ActiveName);
					ActiveName = string.Empty;
					Active = null;
				}
			}
		}
		public static void OnInteract()
		{
			if (currentHorse != null)
			{
				UnmountHorse();

			}
			if (ActiveName == string.Empty || Active == null || Active.onInteract.GetPersistentMethodName(0) != ActiveName
				|| Active.interactionType != Interactable.InteractionType.Primary)
			{
				return;
			}

			StopReload();
			Active.onInteract.Invoke();
			if (Active.disableOnInteract)
			{
				RemoveControl(ActiveName);
				Active.active = false;
				Active = null;
				ActiveName = string.Empty;
			}
		}
		private static Interactable GetInteractableType(Transform t)
		{
			if (t.transform.gameObject.layer == 19)
			{
				return t.transform.GetComponent<AI.AIHitbox>().parent.GetComponentInChildren<Interactable>();
			}
			else if (t.TryGetComponent<Interactable>(out Interactable i))
			{
				return i;
			}
			return null;
		}

		private static void SwitchLight()
		{
			lightOn = !lightOn;
			fovLight.SetActive(lightOn);
		}
	}
}
