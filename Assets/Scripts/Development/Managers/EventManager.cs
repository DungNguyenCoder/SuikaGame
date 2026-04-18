using System;
using Development.Controllers;

namespace Development.Managers
{
    public static class EventManager
    {
        public static Action<Ball, Ball> SameIdCollision;
        public static Action OnLoseLevel;
    }
}
