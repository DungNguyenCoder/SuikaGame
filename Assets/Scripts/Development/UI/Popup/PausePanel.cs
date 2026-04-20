using Development.Managers;
using Development.Utils;

namespace Development.UI.Popup
{
    public class PausePanel : Panel
    {
        public void OnClickContinue()
        {
            
        }
        
        public void OnClickSettings()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.SETTING_PANEL);
        }

        public void OnClickTutorial()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.TUTORIAL_PANEL);
        }
    }
}