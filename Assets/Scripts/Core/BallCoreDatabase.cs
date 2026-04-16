using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName = "SuikaGame/Data/Ball Core")]
    public class BallCoreDatabase : ScriptableObject
    {
        public List<BallData> ballDatas = new List<BallData>();

        public BallData GetBallData(int ballID)
        {
            return ballDatas.Find(b => b.ID == ballID);
        }
    }
}
