using UnityEngine;

namespace SuikaGame.Scripts.Development.Utils
{
    public static class VibrationService
    {
        public static bool IsEnabled => PlayerPrefs.GetInt(GameConfig.VIBRATION_KEY, 1) == 1;

        public static void SetEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(GameConfig.VIBRATION_KEY, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void Vibrate()
        {
            if (!IsEnabled)
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
