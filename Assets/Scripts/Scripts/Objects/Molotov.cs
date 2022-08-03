using UnityEngine;

namespace Gameplay
{
	public class Molotov : MonoBehaviour {

		[SerializeField] private float explodeAfter = 5.0f;
		[SerializeField] private float minimumForce = 1500.0f;
		[SerializeField] private float maximumForce = 2500.0f;

		private float throwForce;
		private bool thrown;
		private AudioSource impactSound = null;
		private Rigidbody rig;
		private bool exploded = false;

		[SerializeField] private GameObject model;
		[SerializeField] private ParticleSystem[] particles;

		private void Awake() {
			impactSound = GetComponent<AudioSource>();
			rig = GetComponent<Rigidbody>();
		}

		private void OnEnable() {
			rig.constraints = RigidbodyConstraints.FreezeAll;
			rig.velocity = Vector3.zero;

			model.SetActive(true);
			thrown = false;
			exploded = false;
			Invoke(nameof(Explode), explodeAfter);
		}

		public void Hide() {
			foreach (ParticleSystem p in particles)
			{ p.Stop(); }
			CancelInvoke();
			gameObject.SetActive(false);
		}

		public void Throw() {
			thrown = true;
			throwForce = Random.Range(minimumForce, maximumForce);
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
			GetComponent<Rigidbody>().AddForce(Cam.active.transform.forward * throwForce);
		}

		private void Explode() {
			rig.constraints = RigidbodyConstraints.FreezeAll;
			rig.velocity = Vector3.zero;

			exploded = true;
			impactSound.Play();
			model.SetActive(false);

			foreach (ParticleSystem p in particles)
			{ p.Play(); }
			//InteractableManager.instance.CreateFire(transform.position);

			FireSystem.CreateFire(transform.position);

			Invoke(nameof(Hide), 2f);
		}

		private void Update() {
			if (!thrown) {
				transform.SetPositionAndRotation(Player.active.gadgetSpawn.position, Player.active.gadgetSpawn.rotation);
			}
		}

		private void OnCollisionEnter(Collision collision) {
			if (thrown && !exploded) {
				Explode();
			}
		}
	}
}