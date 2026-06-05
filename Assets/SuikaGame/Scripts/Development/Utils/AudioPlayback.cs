using JSAM;

namespace SuikaGame.Scripts.Development.Utils
{
    public static class AudioPlayback
    {
        public static void PlayExclusiveMusic(AudioLibraryMusic music)
        {
            AudioManager.StopAllMusic();
            AudioManager.PlayMusic(music);
        }
    }
}
