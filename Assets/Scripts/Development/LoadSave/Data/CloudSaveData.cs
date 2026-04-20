using System;
using UnityEngine.Serialization;

namespace Development.LoadSave.Data
{
    [Serializable]
    public class CloudSaveData
    {
        public bool HasBall = true;
        public int BallId = 1;
        [FormerlySerializedAs("CloudX")] public float PositionX;
    }
}
