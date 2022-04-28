using UnityEngine;
public class ImpactScript : MonoBehaviour 
{
	[Header("Audio")]
	public AudioClip[] impactSounds;
	[SerializeField] private Rigidbody[] pebbles = null;
	[SerializeField] private float minForce = 0;
	[SerializeField] private float maxForce = 1;
	[SerializeField] private float randomForce = 1;
	[SerializeField] private ParticleSystem[] particles = null;

	private void Recycle() 
	{
		foreach (Rigidbody rig in pebbles)
		{
			rig.transform.localPosition = Vector3.zero;
			rig.AddForce((transform.forward + transform.TransformDirection(Random.Range(-randomForce, randomForce), Random.Range(-randomForce, randomForce), 0f)) * Random.Range(minForce, maxForce), ForceMode.Impulse);
		}

		GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);

		foreach (ParticleSystem p in particles)
		{
			p.Play(true);
		}
	}
}