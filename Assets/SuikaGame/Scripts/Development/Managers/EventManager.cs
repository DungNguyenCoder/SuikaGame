using System;
using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Core.Enums;
using SuikaGame.Scripts.Development.Controllers;
using UnityEngine;

namespace SuikaGame.Scripts.Development.Managers
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
        public static Func<BoosterType, UniTask<bool>> OnRequestUseBooster;
    }
}
