using UnityEngine;
namespace Core
{
    public static class Music
    {
        public static void Init(AudioSource _menuMusic, AudioSource _ingameMusic, AudioClip[] _menuClips)
        {
            menuMusic = _menuMusic;
            ingameMusic = _ingameMusic;
            menuClips = _menuClips;
        }

        private static AudioSource ingameMusic;
        public static void OutpostWin()
        {
            ingameMusic.Stop();
            ingameMusic.clip = null;
            ingameMusic.PlayOneShot(null);
        }

        public enum MenuClips
        {
            TitleTheme,
            PauseTheme,
            LoadingScreen
        }
        private static AudioSource menuMusic;
        private static AudioClip[] menuClips;
        public static void PlayMenuMusic(MenuClips menu)
        {
            menuMusic.clip = menuClips[(int)menu];
            menuMusic.Play();
        }
        public static void StopMenuMusic()
        {
            menuMusic.Stop();
        }
    }
}
