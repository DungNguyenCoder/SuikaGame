using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Development.LoadSave.Data
{
    [Serializable]
    public class ProgressSaveData
    {
        public int CurrentScore;
        public bool IsGameOver;
        public List<BallSaveData> BoardBalls = new List<BallSaveData>();
        [FormerlySerializedAs("cloud")] public CloudSaveData Cloud = new CloudSaveData();
    }
}
