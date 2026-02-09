using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "SuikaGame/Data/Ball")]
public class BallDatabase : ScriptableObject
{
    public List<BallData> ballDatas = new List<BallData>();
    public List<SkinSeries> skinSeries = new List<SkinSeries>();

    // Trả về Sprite tương ứng với seriesID và skinID (hoặc null nếu không tìm thấy)
    public Sprite GetSkinSprite(int seriesID, int skinID)
    {
        var series = skinSeries.Find(s => s.ID == seriesID);
        if (series == null) return null;
        var skin = series.skinDatas.Find(s => s.ID == skinID);
        return skin != null ? skin.Sprite : null;
    }
}