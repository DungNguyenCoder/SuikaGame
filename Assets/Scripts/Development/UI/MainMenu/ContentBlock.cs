using Development;
using Development.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Development.UI.MainMenu
{
    public class ContentBlock : MonoBehaviour
    {
        public void OnClickPlayGame()
        {
            GameLaunchOptions.RequestContinue();
            SceneManager.LoadScene(GameConfig.GAMEPLAY_SCENE);
        }

        public void OnClickNewGame()
        {
            GameLaunchOptions.RequestNewGame();
            SceneManager.LoadScene(GameConfig.GAMEPLAY_SCENE);
        }
    }
}
