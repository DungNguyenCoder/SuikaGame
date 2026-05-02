using System;

namespace SuikaGame.Scripts.Core.Ball
{
    [Serializable]
    public class BallData
    {
        public int ID;
        public float ColliderRadius;
        public float PixelsPerUnit = 100f;
    }
}
