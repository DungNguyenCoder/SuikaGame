using Development.Managers;
using Development.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Development.UI.Popup
{
    public class PausePanel : Panel
    {
        [SerializeField] private Transform panelTransform;

        private void OnEnable()
        {
            panelTransform.localScale = Vector3.zero;
        }
        
        public void OnClickContinue()
        {
            EventManager.OnRequestResume?.Invoke();
        }
        
        public void OnClickSettings()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.SETTING_PANEL);
        }

        public void OnClickTutorial()
        {
            GameLaunchOptions.RequestTutorialFromPauseMenu();
            PanelManager.Instance.OpenPanel(PanelConfig.TUTORIAL_PANEL);
        }

        public void OnClickQuit()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
