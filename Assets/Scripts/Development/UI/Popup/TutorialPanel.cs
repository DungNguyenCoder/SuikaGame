using Cysharp.Threading.Tasks;
using Development.LoadSave;
using Development.Managers;
using Development.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Development.UI.Popup
{
    public class TutorialPanel : Panel
    {
        [SerializeField] private Transform titleTransform;
        [SerializeField] private Transform tutorialTransform;
        [SerializeField] private Transform startButtonTransform;
        [SerializeField] private Transform goBackButtonTransform;

        private void OnEnable()
        {
            titleTransform.localScale = Vector3.zero;
            startButtonTransform.localScale = Vector3.zero;
            goBackButtonTransform.localScale = Vector3.zero;
            tutorialTransform.localScale = Vector3.zero;
        }
        
        public void OnClickStartGame()
        {
            HandleStartGameAsync().Forget();
        }

        public void OnClickGoBack()
        {
            if (GameLaunchOptions.GetTutorialEntryPoint() == GameLaunchOptions.TutorialEntryPoint.MainMenuFirstPlay)
            {
                GameLaunchOptions.ConsumeTutorialEntryPoint();
                Time.timeScale = 1f;
            }
            PanelManager.Instance.ClosePanel(PanelConfig.TUTORIAL_PANEL);
        }

        private async UniTaskVoid HandleStartGameAsync()
        {
            var tutorialEntryPoint = GameLaunchOptions.ConsumeTutorialEntryPoint();
            if (tutorialEntryPoint == GameLaunchOptions.TutorialEntryPoint.PauseMenu)
            {
                PanelManager.Instance.ClosePanel(PanelConfig.TUTORIAL_PANEL);
                EventManager.OnRequestResume?.Invoke();
                return;
            }

            if (tutorialEntryPoint == GameLaunchOptions.TutorialEntryPoint.MainMenuFirstPlay)
            {
                var playerSaveData = await JsonRepository.LoadPlayerProfile();
                playerSaveData.HasSeenTutorial = true;
                await JsonRepository.SavePlayerProfile(playerSaveData);
                SaveRuntimeData.SetPlayer(playerSaveData);
                GameLaunchOptions.RequestNewGame();
                Time.timeScale = 1f;
                SceneManager.LoadScene(GameConfig.GAMEPLAY_SCENE);
                return;                
            }

            Time.timeScale = 1f;
            PanelManager.Instance.ClosePanel(PanelConfig.TUTORIAL_PANEL);
        }
    }
}
