using System;
using Development.Controllers;

namespace Development.Managers
{
    public static class EventManager
    {
        public static Action<Ball, Ball> SameIdCollision;
        public static Action<int, int> OnScoreChanged;
        public static Action OnLoseLevel;
        public static Action OnRequestPause;
        public static Action OnRequestResume;
    }
}
