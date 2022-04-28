using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
public class PoolObjectRuntime : MonoBehaviour
{
	[SerializeField]private float hideTime = 1f;
	public string type;
	[HideInInspector] public UnityEvent onRecycled;

	[Header("Play On Enabale")]
	[SerializeField] private PoolAudio[] sounds;
	[SerializeField] private ParticleSystem[] particles;
	[SerializeField] private VisualEffect[] vfx;

	private void OnEnable()
	{
		if (hideTime > 0)
		{
			Invoke(nameof(Hide), hideTime);
		}
		if(onRecycled != null)
        {
			onRecycled.Invoke();
        }

		foreach (ParticleSystem p in particles)
		{
			p.Play();
		}
		foreach (VisualEffect v in vfx)
		{
			v.Play();
		}
		foreach (PoolAudio a in sounds)
		{
			a.source.PlayOneShot(a.clips[Random.Range(0, a.clips.Length)]);
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);

		foreach (ParticleSystem p in particles)
		{
			p.Stop();
		}
		foreach (VisualEffect v in vfx)
		{
			v.Stop();
		}
		foreach (PoolAudio a in sounds)
		{
			a.source.Stop();
		}
	}
}

[System.Serializable]
public class PoolAudio
{
	public AudioSource source;
	public AudioClip[] clips;
}