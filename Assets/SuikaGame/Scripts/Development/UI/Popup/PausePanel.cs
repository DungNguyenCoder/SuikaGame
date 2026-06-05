using JSAM;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuikaGame.Scripts.Development.UI.Popup
{
    public class PausePanel : Panel
    {
        [SerializeField] private Transform panelTransform;

        public override void Open()
        {
            base.Open();
            AudioManager.PlaySound(AudioLibrarySounds._Popup);
        }

        private void OnEnable()
        {
            panelTransform.localScale = Vector3.zero;
        }
        
        public void OnClickContinue()
        {
            PlayClickSound();
            EventManager.OnRequestResume?.Invoke();
        }
        
        public void OnClickSettings()
        {
            PlayClickSound();
            PanelManager.Instance.OpenPanel(PanelConfig.SETTING_PANEL);
        }

        public void OnClickTutorial()
        {
            PlayClickSound();
            GameLaunchOptions.RequestTutorialFromPauseMenu();
            PanelManager.Instance.OpenPanel(PanelConfig.TUTORIAL_PANEL);
        }

        public void OnClickQuit()
        {
            PlayClickSound();
            SceneManager.LoadScene("MainMenu");
        }

        private static void PlayClickSound()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
        }
    }
}
