namespace Development.Utils
{
    public class GameConfig
    {
        public static bool ENABLE_DEBUG_LOG =
        #if ENABLE_DEBUG
                    true;
        #else
                false;
        #endif
    }
}