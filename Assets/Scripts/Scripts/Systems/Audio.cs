using UnityEngine;
using UnityEngine.Audio;
namespace Core
{
    public static class Audio
    {
        public static AudioMixer audioMixer;
        public static void Init(AudioMixer mixer, AudioSource _menuSounds)
        {
            audioMixer = mixer;
            audioMixer.SetFloat("FXVol", -80);
            menuSounds = _menuSounds;      
        }

        private static AudioSource menuSounds;
        public static void PlayMenuSound(AudioClip clip)
        {
            menuSounds.PlayOneShot(clip);
        }
    }
}
