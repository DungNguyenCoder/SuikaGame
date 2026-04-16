using System;

namespace Core
{
    [Serializable]
    public class BallData
    {
        public int ID;
        public float ColliderRadius;
        public float PixelsPerUnit = 100f;
    }
}
