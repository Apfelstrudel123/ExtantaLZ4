using UnityEngine;
public class CasingScript : MonoBehaviour 
{
	[SerializeField]private AudioClip[] casingSounds = null;
	[SerializeField]private AudioSource audioSource = null;

	private void OnCollisionEnter () 
	{
		audioSource.clip = casingSounds[Random.Range(0, casingSounds.Length)];
		audioSource.Play();
	}
}