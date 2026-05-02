using System;
using SuikaGame.Scripts.Core.Enums;
using SuikaGame.Scripts.Development.LoadSave.Data;

namespace SuikaGame.Scripts.Development.Utils
{
    public static class BoosterInventoryService
    {
        public static void EnsureInitialized(PlayerSaveData playerData)
        {
            BoosterInventoryData inventory = GetInventory(playerData);
            ClampInventory(inventory);
        }

        public static int GetRemainingUses(PlayerSaveData playerData, BoosterType type)
        {
            BoosterInventoryData inventory = GetInventory(playerData);
            ClampInventory(inventory);
            return GetRemainingUses(inventory, type);
        }

        public static void AddUses(PlayerSaveData playerData, BoosterType type, int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Booster amount must be greater than zero.");
            }

            BoosterInventoryData inventory = GetInventory(playerData);
            int nextUses = GetRemainingUses(inventory, type) + amount;
            SetRemainingUses(inventory, type, nextUses);
        }

        public static bool TryConsumeUse(PlayerSaveData playerData, BoosterType type)
        {
            BoosterInventoryData inventory = GetInventory(playerData);
            int currentUses = GetRemainingUses(inventory, type);
            if (currentUses <= 0)
            {
                return false;
            }

            SetRemainingUses(inventory, type, currentUses - 1);
            return true;
        }

        private static BoosterInventoryData GetInventory(PlayerSaveData playerData)
        {
            if (playerData == null)
            {
                throw new ArgumentNullException(nameof(playerData));
            }

            playerData.BoosterInventory ??= new BoosterInventoryData();
            return playerData.BoosterInventory;
        }

        private static int GetRemainingUses(BoosterInventoryData inventory, BoosterType type)
        {
            return type switch
            {
                BoosterType.Destruction => inventory.DestructionUses,
                BoosterType.Promotion => inventory.PromotionUses,
                BoosterType.Biggest => inventory.BiggestUses,
                BoosterType.Shuffle => inventory.ShuffleUses,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static void SetRemainingUses(BoosterInventoryData inventory, BoosterType type, int value)
        {
            int clampedValue = Math.Max(0, value);
            switch (type)
            {
                case BoosterType.Destruction:
                    inventory.DestructionUses = clampedValue;
                    break;
                case BoosterType.Promotion:
                    inventory.PromotionUses = clampedValue;
                    break;
                case BoosterType.Biggest:
                    inventory.BiggestUses = clampedValue;
                    break;
                case BoosterType.Shuffle:
                    inventory.ShuffleUses = clampedValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static void ClampInventory(BoosterInventoryData inventory)
        {
            inventory.DestructionUses = Math.Max(0, inventory.DestructionUses);
            inventory.PromotionUses = Math.Max(0, inventory.PromotionUses);
            inventory.BiggestUses = Math.Max(0, inventory.BiggestUses);
            inventory.ShuffleUses = Math.Max(0, inventory.ShuffleUses);
        }
    }
}
