using System;
using System.Collections.Generic;
using SuikaGame.Scripts.Core.Shop;
using SuikaGame.Scripts.Development.LoadSave.Data;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.Shop
{
    public class BackgroundContainer : MonoBehaviour
    {
        [SerializeField] private Transform blockRoot;
        [SerializeField] private BackgroundBlock blockPrefab;

        private readonly List<BackgroundShopItemData> _items = new();
        private readonly List<BackgroundBlock> _blocks = new();
        private Action<int> _onBackgroundSelected;

        public void Build(BackgroundShopDatabase database, PlayerSaveData playerData, Action<int> onBackgroundSelected)
        {
            ClearGeneratedBlocks();
            HideAuthoredBlocks();
            _items.Clear();
            _blocks.Clear();
            _onBackgroundSelected = onBackgroundSelected;

            foreach (var item in database.items)
            {
                BackgroundBlock block = Instantiate(blockPrefab, blockRoot);
                block.name = $"BackgroundBlock_{item.BackgroundID}";

                _items.Add(item);
                _blocks.Add(block);
                SetupBlock(block, item, playerData);
            }
        }

        public void Refresh(PlayerSaveData playerData, Action<int> onBackgroundSelected)
        {
            _onBackgroundSelected = onBackgroundSelected;
            for (int i = 0; i < _blocks.Count; i++)
            {
                SetupBlock(_blocks[i], _items[i], playerData);
            }
        }

        public bool TrySelect(PlayerSaveData playerData, int backgroundId)
        {
            BackgroundShopItemData item = FindItem(backgroundId);

            bool wasPurchased = playerData.PurchasedBackgroundIds.Contains(backgroundId);
            int price = Mathf.Max(0, item.Price);
            if (!wasPurchased && playerData.Coin < price)
            {
                return false;
            }

            if (!wasPurchased)
            {
                playerData.Coin -= price;
                playerData.PurchasedBackgroundIds.Add(backgroundId);
            }

            bool changed = !wasPurchased || playerData.SelectedBackgroundId != backgroundId;
            playerData.SelectedBackgroundId = backgroundId;
            return changed;
        }

        public bool EnsureFreeItemsPurchased(PlayerSaveData playerData)
        {
            bool changed = false;
            foreach (BackgroundShopItemData item in _items)
            {
                if (item.Price <= 0 && !playerData.PurchasedBackgroundIds.Contains(item.BackgroundID))
                {
                    playerData.PurchasedBackgroundIds.Add(item.BackgroundID);
                    changed = true;
                }
            }

            return changed;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void MoveCheckerToSelectedBlock(RectTransform checker, int selectedBackgroundId)
        {
            BackgroundBlock selectedBlock = FindBlock(selectedBackgroundId);
            MoveCheckerToBlock(checker, selectedBlock.transform);
        }

        private void SetupBlock(BackgroundBlock block, BackgroundShopItemData item, PlayerSaveData playerData)
        {
            bool isPurchased = playerData.PurchasedBackgroundIds.Contains(item.BackgroundID);
            block.Setup(item, isPurchased, _onBackgroundSelected);
        }
        
        private BackgroundShopItemData FindItem(int backgroundId)
        {
            return _items.Find(item => item.BackgroundID == backgroundId);
        }

        private BackgroundBlock FindBlock(int backgroundId)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].BackgroundID == backgroundId)
                {
                    return _blocks[i];
                }
            }

            return null;
        }

        private static void MoveCheckerToBlock(RectTransform checker, Transform block)
        {
            checker.SetParent(block, false);
            checker.anchorMin = Vector2.one;
            checker.anchorMax = Vector2.one;
            checker.pivot = Vector2.one;
            checker.anchoredPosition = Vector2.zero;
            checker.SetAsLastSibling();
            checker.gameObject.SetActive(true);
        }

        private void ClearGeneratedBlocks()
        {
            foreach (BackgroundBlock block in _blocks)
            {
                if (block != null)
                {
                    Destroy(block.gameObject);
                }
            }
        }

        private void HideAuthoredBlocks()
        {
            for (int i = 0; i < blockRoot.childCount; i++)
            {
                blockRoot.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
