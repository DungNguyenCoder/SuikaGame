namespace Development.Utils
{
    public class GameLaunchOptions
    {
        public enum TutorialEntryPoint
        {
            None,
            MainMenuFirstPlay,
            PauseMenu
        }

        private static bool _startNewGameRequested;
        private static TutorialEntryPoint _tutorialEntryPoint;

        public static void RequestNewGame()
        {
            _startNewGameRequested = true;
            _tutorialEntryPoint = TutorialEntryPoint.None;
        }

        public static void RequestContinue()
        {
            _startNewGameRequested = false;
            _tutorialEntryPoint = TutorialEntryPoint.None;
        }

        public static bool ConsumeStartNewGameRequest()
        {
            bool requested = _startNewGameRequested;
            _startNewGameRequested = false;
            return requested;
        }

        public static void RequestTutorialFromMainMenuFirstPlay()
        {
            _tutorialEntryPoint = TutorialEntryPoint.MainMenuFirstPlay;
        }

        public static void RequestTutorialFromPauseMenu()
        {
            _tutorialEntryPoint = TutorialEntryPoint.PauseMenu;
        }

        public static TutorialEntryPoint ConsumeTutorialEntryPoint()
        {
            TutorialEntryPoint entryPoint = _tutorialEntryPoint;
            _tutorialEntryPoint = TutorialEntryPoint.None;
            return entryPoint;
        }

        public static TutorialEntryPoint GetTutorialEntryPoint()
        {
            return _tutorialEntryPoint;
        }
    }
}
