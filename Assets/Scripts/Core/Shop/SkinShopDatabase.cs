using System.Collections.Generic;
using Core.Skin;
using UnityEngine;

namespace Core.Shop
{
    [CreateAssetMenu(menuName = "SuikaGame/Data/Skin Shop")]
    public class SkinShopDatabase : ScriptableObject
    {
        public SkinDatabase skinDatabase;
        public List<SkinShopItemData> items = new List<SkinShopItemData>();

        public SkinShopItemData GetItemBySeriesID(int skinSeriesID)
        {
            return items.Find(item => item != null && item.SkinSeriesID == skinSeriesID);
        }
    }
}
