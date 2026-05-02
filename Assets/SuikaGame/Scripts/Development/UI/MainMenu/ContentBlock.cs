using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Development.LoadSave;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuikaGame.Scripts.Development.UI.MainMenu
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
