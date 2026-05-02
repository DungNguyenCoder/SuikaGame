using System;
using SuikaGame.Scripts.Core.Enums;
using UnityEngine;

namespace SuikaGame.Scripts.Core.Shop
{
    [Serializable]
    public class BoosterShopItemData
    {
        public BoosterType Type;
        public int Price;
        public int UsesPerPurchase = 1;
    }
}
