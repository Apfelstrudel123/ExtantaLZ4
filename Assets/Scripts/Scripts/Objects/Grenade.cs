using UnityEngine;
namespace Gameplay
{
	public class Grenade : MonoBehaviour {

		private void OnEnable() {
			thrown = false;
			transform.GetChild(2).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(true);

			Invoke(nameof(Explode), grenadeTimer);
		}

		public void Hide() {
			CancelInvoke();
			gameObject.SetActive(false);
		}

		public void Throw() {
			thrown = true;
			throwForce = Random.Range(minimumForce, maximumForce);
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
			GetComponent<Rigidbody>().AddForce(Cam.active.transform.forward * throwForce);
			transform.GetChild(2).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
		}

		private void Explode() {
			GameWorld.InteractableManager.instance.CreateExplosion(transform.position, Quaternion.Euler(0f, 0f, 0f), 4000, 12, 100);
			Hide();
		}

		private void Update() {
			if (!thrown)
				transform.SetPositionAndRotation(Player.active.gadgetSpawn.position, Player.active.gadgetSpawn.rotation);
		}

		private void OnCollisionEnter(Collision collision) {
			impactSound.Play();
		}

		[SerializeField] private float minimumForce = 1500.0f;
		[SerializeField] private float maximumForce = 2500.0f;
		private float throwForce;
		
		[SerializeField] private float grenadeTimer = 5.0f;
		private bool thrown;
		[SerializeField] private AudioSource impactSound = null;
	}
}
