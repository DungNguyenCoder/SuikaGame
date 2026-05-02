using System.Collections.Generic;
using SuikaGame.Scripts.Core.Enums;
using UnityEngine;

namespace SuikaGame.Scripts.Core.Shop
{
    [CreateAssetMenu(menuName = "SuikaGame/Data/Booster Shop")]
    public class BoosterShopDatabase : ScriptableObject
    {
        public List<BoosterShopItemData> items = new List<BoosterShopItemData>();

        public BoosterShopItemData GetItemByType(BoosterType type)
        {
            return items.Find(item => item != null && item.Type == type);
        }
    }
}
