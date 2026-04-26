using System.Collections.Generic;
using Core.Background;
using UnityEngine;

namespace Core.Shop
{
    [CreateAssetMenu(menuName = "SuikaGame/Data/Background Shop")]
    public class BackgroundShopDatabase : ScriptableObject
    {
        public BackgroundDatabase backgroundDatabase;
        public List<BackgroundShopItemData> items = new List<BackgroundShopItemData>();

        public BackgroundShopItemData GetItemByBackgroundID(int backgroundID)
        {
            return items.Find(item => item != null && item.BackgroundID == backgroundID);
        }
    }
}
