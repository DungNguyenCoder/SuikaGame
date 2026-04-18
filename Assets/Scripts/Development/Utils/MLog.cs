using UnityEngine;

namespace Development.Utils
{
    public static class MLog
    {
        private static bool Enabled => GameConfig.ENABLE_DEBUG_LOG;

        public static void Log(object message)
        {
            if (Enabled)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(object message)
        {
            if (Enabled)
            {
                Debug.LogWarning(message);
            }
        }

        public static void LogError(object message)
        {
            if (Enabled)
            {
                Debug.LogError(message);
            }
        }

        public static void LogException(System.Exception exception)
        {
            if (Enabled)
            {
                Debug.LogException(exception);
            }
        }
    }
}
