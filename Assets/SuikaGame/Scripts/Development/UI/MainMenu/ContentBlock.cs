using Cysharp.Threading.Tasks;
using JSAM;
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
            PlayClickSound();
            HandlePlayOrNewGameAsync(false).Forget();
        }

        public void OnClickNewGame()
        {
            PlayClickSound();
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

        private static void PlayClickSound()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
        }
    }
}
