using Development.Managers;
using Development.Utils;
using Development.Animations;
using JSAM;
using UnityEngine;

namespace Development.UI.Popup
{
    public class SettingPanel : Panel
    {
        [SerializeField] private Transform panelTransform;
        
        [SerializeField] private ToggleKnobAnimation musicToggleAnimation;
        [SerializeField] private ToggleKnobAnimation sfxToggleAnimation;
        [SerializeField] private ToggleKnobAnimation vibrationToggleAnimation;

        private bool _isVibrationEnabled;

        private bool IsVibrationEnabled =>
            PlayerPrefs.GetInt(GameConfig.VIBRATION_KEY, 1) == 1;
        
        private void OnEnable()
        {
            panelTransform.localScale = Vector3.zero;
            _isVibrationEnabled = IsVibrationEnabled;
            RefreshToggleVisualState(true);
        }

        private void OnDisable()
        {
            musicToggleAnimation.Cancel();
            sfxToggleAnimation.Cancel();
            vibrationToggleAnimation.Cancel();
        }

        public void OnClickSFX()
        {
            var willEnableSound = AudioManager.SoundMuted;
            AudioManager.SoundMuted = !AudioManager.SoundMuted;
            if (willEnableSound)
            {
                AudioManager.PlaySound(AudioLibrarySounds._Click);
            }

            RefreshToggleVisualState(false);
        }

        public void OnClickMusic()
        {
            AudioManager.MusicMuted = !AudioManager.MusicMuted;
            PlayToggleSound();
            RefreshToggleVisualState(false);
        }

        public void OnClickVibration()
        {
            _isVibrationEnabled = !_isVibrationEnabled;
            PlayerPrefs.SetInt(GameConfig.VIBRATION_KEY, _isVibrationEnabled ? 1 : 0);
            PlayerPrefs.Save();

            PlayToggleSound();
            RefreshToggleVisualState(false);
        }

        public void OnClickClose()
        {
            PlayToggleSound();
            PanelManager.Instance.ClosePanel(PanelConfig.SETTING_PANEL);
        }

        private static void PlayToggleSound()
        {
            if (!AudioManager.SoundMuted)
            {
                AudioManager.PlaySound(AudioLibrarySounds._Click);
            }
        }

        private void RefreshToggleVisualState(bool instant)
        {
            musicToggleAnimation.SetState(!AudioManager.MusicMuted, instant);
            sfxToggleAnimation.SetState(!AudioManager.SoundMuted, instant);
            vibrationToggleAnimation.SetState(_isVibrationEnabled, instant);
        }
    }
}
