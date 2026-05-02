using System;
using System.Collections.Generic;
using Core.Shop;
using Development.LoadSave.Data;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.Shop
{
    public class ObjectContainer : MonoBehaviour
    {
        [SerializeField] private Transform blockRoot;
        [SerializeField] private ObjectBlock blockPrefab;

        private readonly List<SkinShopItemData> _items = new();
        private readonly List<ObjectBlock> _blocks = new();
        private Action<int> _onObjectSelected;

        public void Build(SkinShopDatabase database, PlayerSaveData playerData, Action<int> onObjectSelected)
        {
            ClearGeneratedBlocks();
            HideAuthoredBlocks();
            _items.Clear();
            _blocks.Clear();
            _onObjectSelected = onObjectSelected;

            for (int i = 0; i < database.items.Count; i++)
            {
                SkinShopItemData item = database.items[i];
                ObjectBlock block = Instantiate(blockPrefab, blockRoot);
                block.name = $"ObjectBlock_{item.SkinSeriesID}";

                _items.Add(item);
                _blocks.Add(block);
                SetupBlock(block, item, playerData);
            }
        }

        public void Refresh(PlayerSaveData playerData, Action<int> onObjectSelected)
        {
            _onObjectSelected = onObjectSelected;
            for (int i = 0; i < _blocks.Count; i++)
            {
                SetupBlock(_blocks[i], _items[i], playerData);
            }
        }

        public bool TrySelect(PlayerSaveData playerData, int skinSeriesId)
        {
            SkinShopItemData item = FindItem(skinSeriesId);
            
            bool wasPurchased = playerData.PurchasedSkinSeriesIds.Contains(skinSeriesId);
            int price = Mathf.Max(0, item.Price);
            if (!wasPurchased && playerData.Coin < price)
            {
                return false;
            }

            if (!wasPurchased)
            {
                playerData.Coin -= price;
                playerData.PurchasedSkinSeriesIds.Add(skinSeriesId);
            }

            bool changed = !wasPurchased || playerData.SelectedSkinSeriesId != skinSeriesId;
            playerData.SelectedSkinSeriesId = skinSeriesId;
            return changed;
        }

        public bool EnsureFreeItemsPurchased(PlayerSaveData playerData)
        {
            bool changed = false;
            foreach (SkinShopItemData item in _items)
            {
                if (item.Price <= 0 && !playerData.PurchasedSkinSeriesIds.Contains(item.SkinSeriesID))
                {
                    playerData.PurchasedSkinSeriesIds.Add(item.SkinSeriesID);
                    changed = true;
                }
            }

            return changed;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void MoveCheckerToSelectedBlock(RectTransform checker, int selectedSkinSeriesId)
        {
            ObjectBlock selectedBlock = FindBlock(selectedSkinSeriesId);
            MoveCheckerToBlock(checker, selectedBlock.transform);
        }

        private void SetupBlock(ObjectBlock block, SkinShopItemData item, PlayerSaveData playerData)
        {
            bool isPurchased = playerData.PurchasedSkinSeriesIds.Contains(item.SkinSeriesID);
            block.Setup(item, isPurchased, _onObjectSelected);
        }
        
        private SkinShopItemData FindItem(int skinSeriesId)
        {
            return _items.Find(item => item.SkinSeriesID == skinSeriesId);
        }

        private ObjectBlock FindBlock(int skinSeriesId)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].SkinSeriesID == skinSeriesId)
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
            foreach (ObjectBlock block in _blocks)
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
