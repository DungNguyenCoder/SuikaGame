using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Core.Shop;
using SuikaGame.Scripts.Development.LoadSave;
using SuikaGame.Scripts.Development.LoadSave.Data;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using TMPro;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.Shop
{
    public class ShopPanel : Panel
    {
        [Header("Data")]
        [SerializeField] private SkinShopDatabase skinShopDatabase;
        [SerializeField] private BackgroundShopDatabase backgroundShopDatabase;

        [Header("UI")]
        [SerializeField] private TMP_Text coinAmount;
        [SerializeField] private ObjectContainer objectContainer;
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private RectTransform selectedChecker;

        private PlayerSaveData _playerData;
        private bool _isShowingObjectContainer = true;

        public override void Open()
        {
            base.Open();
            LoadAndBuildAsync().Forget();
        }

        public void OnClickObjectTab()
        {
            SetContainerVisible(true, false);
            _isShowingObjectContainer = true;
            MoveCheckerToCurrentSelection();
        }

        public void OnClickBackgroundTab()
        {
            SetContainerVisible(false, true);
            _isShowingObjectContainer = false;
            MoveCheckerToCurrentSelection();
        }

        public void OnClickClose()
        {
            PanelManager.Instance.ClosePanel(PanelConfig.SHOP_PANEL);
        }

        private async UniTaskVoid LoadAndBuildAsync()
        {
            _playerData = SaveRuntimeData.Player ?? await JsonRepository.LoadPlayerProfile();
            _playerData ??= new PlayerSaveData();
            
            bool changed = NormalizePlayerData(_playerData);
            SaveRuntimeData.SetPlayer(_playerData);

            DetachSelectedChecker();
            objectContainer.Build(skinShopDatabase, _playerData, HandleObjectSelected);
            backgroundContainer.Build(backgroundShopDatabase, _playerData, HandleBackgroundSelected);

            changed |= objectContainer.EnsureFreeItemsPurchased(_playerData);
            changed |= backgroundContainer.EnsureFreeItemsPurchased(_playerData);

            Refresh();
            OnClickObjectTab();

            if (changed)
            {
                await JsonRepository.SavePlayerProfile(_playerData);
            }
        }

        private void Refresh()
        {
            coinAmount.text = _playerData.Coin.ToString();
            objectContainer.Refresh(_playerData, HandleObjectSelected);
            backgroundContainer.Refresh(_playerData, HandleBackgroundSelected);
            MoveCheckerToCurrentSelection();
        }

        private void HandleObjectSelected(int skinSeriesId)
        {
            if (objectContainer.TrySelect(_playerData, skinSeriesId))
            {
                SaveAndRefreshAsync().Forget();
            }
        }

        private void HandleBackgroundSelected(int backgroundId)
        {
            if (backgroundContainer.TrySelect(_playerData, backgroundId))
            {
                SaveAndRefreshAsync().Forget();
            }
        }

        private async UniTaskVoid SaveAndRefreshAsync()
        {
            NormalizePlayerData(_playerData);
            SaveRuntimeData.SetPlayer(_playerData);
            await JsonRepository.SavePlayerProfile(_playerData);
            EventManager.OnProfileChanged?.Invoke();
            Refresh();
        }

        private void SetContainerVisible(bool showObject, bool showBackground)
        {
            objectContainer.SetVisible(showObject);
            backgroundContainer.SetVisible(showBackground);
        }

        private void MoveCheckerToCurrentSelection()
        {
            if (_isShowingObjectContainer)
            {
                objectContainer.MoveCheckerToSelectedBlock(selectedChecker, _playerData.SelectedSkinSeriesId);
                return;
            }

            backgroundContainer.MoveCheckerToSelectedBlock(selectedChecker, _playerData.SelectedBackgroundId);
        }

        private void DetachSelectedChecker()
        {
            selectedChecker.SetParent(transform, false);
            selectedChecker.gameObject.SetActive(false);
        }
        
        private static bool NormalizePlayerData(PlayerSaveData playerData)
        {
            bool changed = false;

            changed |= AddIfMissing(playerData.PurchasedSkinSeriesIds, playerData.SelectedSkinSeriesId);
            changed |= AddIfMissing(playerData.PurchasedBackgroundIds, playerData.SelectedBackgroundId);
            return changed;
        }

        private static bool AddIfMissing(List<int> ids, int id)
        {
            if (ids.Contains(id))
            {
                return false;
            }

            ids.Add(id);
            return true;
        }
    }
}
