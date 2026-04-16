using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName = "SuikaGame/Data/Skin")]
    public class SkinDatabase : ScriptableObject
    {
        public List<SkinSeries> skinSeries = new List<SkinSeries>();

        public Sprite GetSkinSprite(int seriesID, int ballID)
        {
            var series = skinSeries.Find(s => s.ID == seriesID);
            if (series == null || series.skinDatas == null) return null;

            var skin = series.skinDatas.Find(s => s.BallID == ballID);
            return skin != null ? skin.Sprite : null;
        }
    }
}
