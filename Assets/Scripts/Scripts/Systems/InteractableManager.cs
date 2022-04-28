using System.Collections;
using UnityEngine;
namespace GameWorld
{
	public class InteractableManager : MonoBehaviour
	{
		public static InteractableManager instance;

		[Header("Explosive Barrel")]
		//[SerializeField]private float minTime = 0.15f;
		//[SerializeField]private float maxTime = 0.25f;

		[SerializeField] private LayerMask explosionMask = new LayerMask();

		[Header("WeaponPickUp")]
		[SerializeField] private GameObject pickUpPrefab;

		private void Awake()
		{
			instance = this;
		}

		public void CreateExplosion(Vector3 pos, Quaternion rot, float force, float size, float damage)
		{
			Core.ObjectPool.Request("Explosion", pos, rot, null);
			Collider[] colliders = Physics.OverlapSphere(pos, size, explosionMask);

			foreach (Collider hit in colliders)
			{
				if (hit != null)
				{
					Rigidbody rb = hit.GetComponent<Rigidbody>();

					if (rb != null)
					{
						rb.AddExplosionForce(force, pos - new Vector3(0f, 0.2f, 0f), size);
					}

					if (hit.transform.CompareTag("ExplosiveBarrel"))
					{
						Core.ObjectPool.Request("DestroyedBarrel", hit.transform.position, hit.transform.rotation, null);
						//yield return new WaitForSeconds(UnityEngine.Random.Range(minTime, maxTime));
						InteractableManager.instance.CreateExplosion(hit.transform.position, Quaternion.Euler(0f, 0f, 0f), 4000, 12, 1000);
						Destroy(hit.transform.gameObject);
					}
					else if (hit.gameObject.layer == 14)
					{
						float dist = (pos - hit.transform.position).magnitude;
						if (dist < 1)
						{
							dist = 1;
						}

						hit.GetComponent<AI.NPC>().Health().SubtractHealth(damage * (1 - 1 / (size / dist)), ObjectHealth.DamageType.Explosion);
					}
					else if (hit.transform.CompareTag("Vehicle"))
					{
						float dist = ((pos - hit.transform.position)).magnitude;
						if (dist < 1)
						{
							dist = 1;
						}
						hit.GetComponent<Gameplay.Horse>().TakeDamage(damage * (1 - 1 / (size / dist)));
					}

					if (hit.gameObject.CompareTag("Player"))
					{
						float dist = ((pos - hit.transform.position)).magnitude;
						if (dist < 1)
						{
							dist = 1;
						}
						Gameplay.Player.health.SubtractHealth(damage * (1 - 1 / (size / dist)), ObjectHealth.DamageType.Explosion);
					}
				}
			}
		}
	}
}