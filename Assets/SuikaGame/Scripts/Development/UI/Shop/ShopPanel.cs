using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JSAM;
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
        [SerializeField] private BoosterShopDatabase boosterShopDatabase;

        [Header("UI")]
        [SerializeField] private TMP_Text coinAmount;
        [SerializeField] private ObjectContainer objectContainer;
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private BoosterContainer boosterContainer;
        [SerializeField] private RectTransform selectedChecker;

        private enum ShopTab
        {
            Object,
            Background,
            Item
        }

        private PlayerSaveData _playerData;
        private ShopTab _currentTab = ShopTab.Object;

        public override void Open()
        {
            base.Open();
            AudioPlayback.PlayExclusiveMusic(AudioLibraryMusic.Shop);
            AudioManager.PlaySound(AudioLibrarySounds._Popup);
            LoadAndBuildAsync().Forget();
        }

        public override void Close()
        {
            base.Close();
            AudioPlayback.PlayExclusiveMusic(AudioLibraryMusic.MainMenu);
        }

        public void OnClickObjectTab()
        {
            PlayClickSound();
            SetContainerVisible(true, false, false);
            _currentTab = ShopTab.Object;
            MoveCheckerToCurrentSelection();
        }

        public void OnClickBackgroundTab()
        {
            PlayClickSound();
            SetContainerVisible(false, true, false);
            _currentTab = ShopTab.Background;
            MoveCheckerToCurrentSelection();
        }

        public void OnClickItemTab()
        {
            PlayClickSound();
            SetContainerVisible(false, false, true);
            _currentTab = ShopTab.Item;
            DetachSelectedChecker();
        }

        public void OnClickClose()
        {
            PlayClickSound();
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
            boosterContainer.Build(boosterShopDatabase, _playerData, HandleBoosterPurchased);

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
            boosterContainer.Refresh(_playerData);
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

        private void HandleBoosterPurchased()
        {
            SaveAndRefreshAsync().Forget();
        }

        private async UniTaskVoid SaveAndRefreshAsync()
        {
            NormalizePlayerData(_playerData);
            SaveRuntimeData.SetPlayer(_playerData);
            await JsonRepository.SavePlayerProfile(_playerData);
            EventManager.OnProfileChanged?.Invoke();
            Refresh();
        }

        private void SetContainerVisible(bool showObject, bool showBackground, bool showItem)
        {
            objectContainer.SetVisible(showObject);
            backgroundContainer.SetVisible(showBackground);
            boosterContainer.SetVisible(showItem);
        }

        private void MoveCheckerToCurrentSelection()
        {
            if (_currentTab == ShopTab.Object)
            {
                objectContainer.MoveCheckerToSelectedBlock(selectedChecker, _playerData.SelectedSkinSeriesId);
                return;
            }

            if (_currentTab == ShopTab.Background)
            {
                backgroundContainer.MoveCheckerToSelectedBlock(selectedChecker, _playerData.SelectedBackgroundId);
                return;
            }

            DetachSelectedChecker();
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
            changed |= playerData.BoosterInventory == null;
            BoosterInventoryService.EnsureInitialized(playerData);
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

        private static void PlayClickSound()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
        }
    }
}
