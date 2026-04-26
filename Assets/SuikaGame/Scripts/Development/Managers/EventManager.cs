using System;
using Development.Controllers;
using UnityEngine;

namespace Development.Managers
{
    public static class EventManager
    {
        public static Action<Ball, Ball> SameIdCollision;
        public static Action<int, int> OnScoreChanged;
        public static Action OnProfileChanged;
        public static Action<Sprite> OnProfileAvatarChanged;
        public static Action OnLoseLevel;
        public static Action OnRequestPause;
        public static Action OnRequestResume;
    }
}
