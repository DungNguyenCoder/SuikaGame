using Cysharp.Threading.Tasks;
using Development.LoadSave;
using Development.Managers;
using Development.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Development.UI.MainMenu
{
    public class ContentBlock : MonoBehaviour
    {
        public void OnClickPlayGame()
        {
            HandlePlayOrNewGameAsync(false).Forget();
        }

        public void OnClickNewGame()
        {
            HandlePlayOrNewGameAsync(true).Forget();
        }

        private async UniTaskVoid HandlePlayOrNewGameAsync(bool startNewGame)
        {
            var playerSaveData = await JsonRepository.LoadPlayerProfile();
            if (!playerSaveData.HasSeenTutorial)
            {
                GameLaunchOptions.RequestTutorialFromMainMenuFirstPlay();
                PanelManager.Instance.OpenPanel(PanelConfig.TUTORIAL_PANEL);
                return;
            }

            if (startNewGame)
            {
                GameLaunchOptions.RequestNewGame();
            }
            else
            {
                GameLaunchOptions.RequestContinue();
            }

            SceneManager.LoadScene(GameConfig.GAMEPLAY_SCENE);
        }
    }
}
