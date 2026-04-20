namespace Development.Utils
{
    public class GameLaunchOptions
    {
        private static bool _startNewGameRequested;

        public static void RequestNewGame()
        {
            _startNewGameRequested = true;
        }

        public static void RequestContinue()
        {
            _startNewGameRequested = false;
        }

        public static bool ConsumeStartNewGameRequest()
        {
            bool requested = _startNewGameRequested;
            _startNewGameRequested = false;
            return requested;
        }
    }
}
