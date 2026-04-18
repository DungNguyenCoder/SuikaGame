using System.Collections.Generic;
using UnityEngine;

namespace Core.Ball
{
    [CreateAssetMenu(menuName = "SuikaGame/Data/Ball Core")]
    public class BallDatabase : ScriptableObject
    {
        public List<BallData> ballDatas = new List<BallData>();

        public BallData GetBallData(int ballID)
        {
            return ballDatas.Find(b => b.ID == ballID);
        }
    }
}
