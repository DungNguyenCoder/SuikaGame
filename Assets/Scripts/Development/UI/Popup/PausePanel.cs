using Development.Managers;
using Development.Utils;
using UnityEngine.SceneManagement;

namespace Development.UI.Popup
{
    public class PausePanel : Panel
    {
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
            PanelManager.Instance.OpenPanel(PanelConfig.TUTORIAL_PANEL);
        }

        public void OnClickQuit()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}