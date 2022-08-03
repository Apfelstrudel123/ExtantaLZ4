using UnityEngine;
using UnityEngine.Audio;

namespace Core
{
    public static class Audio {
        public static AudioMixer mixer;

        public static void Init(AudioMixer _mixer, AudioSource _menuSounds) {
            mixer = _mixer;
            mixer.SetFloat("FXVol", -80);
            menuSounds = _menuSounds;      
        }

        public static void SetPlayerSources(AudioSource sfx, AudioSource _ambience) {
            playerSFX = sfx;
            ambience = _ambience;
        }

        private static AudioSource menuSounds;

        public static void PlayMenuSound(AudioClip clip) {
            menuSounds.PlayOneShot(clip);
        }

        private static AudioSource playerSFX;

        public static void PlayerSFX(AudioClip clip) {
            playerSFX.PlayOneShot(clip);
        }

        private static AudioSource ambience;


        public static void PlayAmbience(AudioClip clip) {
            ambience.clip = clip;
            ambience.Play();
        }

        public static void StopAmbience()
        {
            ambience.Stop();
        }

        private static AudioClip diveIn;
        private static AudioClip diveOut;
        private static AudioClip waterIdle;

        public static void EnterWater() {
			mixer.SetFloat("FX_Reverb_Mix", 0f);
			PlayerSFX(diveIn);
			PlayAmbience(waterIdle);
		}

        public static void LeaveWater() {
			mixer.SetFloat("FX_Reverb_Mix", -80f);
			StopAmbience();
			PlayerSFX(diveOut);
		}
    }
}
