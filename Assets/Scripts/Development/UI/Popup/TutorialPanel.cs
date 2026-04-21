using Development.Managers;
using Development.Utils;
using UnityEngine.SceneManagement;

namespace Development.UI.Popup
{
    public class TutorialPanel : Panel
    {
        public void OnClickStartGame()
        {
            SceneManager.LoadScene(GameConfig.GAMEPLAY_SCENE);
        }

        public void OnClickGoBack()
        {
            PanelManager.Instance.ClosePanel(PanelConfig.TUTORIAL_PANEL);
        }
    }
}