using System;
using JSAM;
using SuikaGame.Scripts.Core.Enums;
using SuikaGame.Scripts.Core.Shop;
using SuikaGame.Scripts.Development.LoadSave.Data;
using SuikaGame.Scripts.Development.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuikaGame.Scripts.Development.UI.Shop
{
    public class BoosterContainer : MonoBehaviour
    {
        [Header("Destruction")]
        [SerializeField] private Button destructionButton;
        [SerializeField] private TMP_Text destructionPrice;
        [SerializeField] private TMP_Text destructionAmount;

        [Header("Promotion")]
        [SerializeField] private Button promotionButton;
        [SerializeField] private TMP_Text promotionPrice;
        [SerializeField] private TMP_Text promotionAmount;

        [Header("Biggest")]
        [SerializeField] private Button biggestButton;
        [SerializeField] private TMP_Text biggestPrice;
        [SerializeField] private TMP_Text biggestAmount;

        [Header("Shuffle")]
        [SerializeField] private Button shuffleButton;
        [SerializeField] private TMP_Text shufflePrice;
        [SerializeField] private TMP_Text shuffleAmount;

        private BoosterShopDatabase _database;
        private PlayerSaveData _playerData;
        private Action _onPurchased;

        public void Build(BoosterShopDatabase database, PlayerSaveData playerData, Action onPurchased)
        {
            _database = database;
            _playerData = playerData;
            _onPurchased = onPurchased;
            BindButtons();
            Refresh(playerData);
        }

        public void Refresh(PlayerSaveData playerData)
        {
            _playerData = playerData;

            RefreshItem(BoosterType.Destruction, destructionButton, destructionPrice, destructionAmount);
            RefreshItem(BoosterType.Promotion, promotionButton, promotionPrice, promotionAmount);
            RefreshItem(BoosterType.Biggest, biggestButton, biggestPrice, biggestAmount);
            RefreshItem(BoosterType.Shuffle, shuffleButton, shufflePrice, shuffleAmount);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void OnClickBuyDestruction()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            TryBuy(BoosterType.Destruction);
        }

        public void OnClickBuyPromotion()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            TryBuy(BoosterType.Promotion);
        }

        public void OnClickBuyBiggest()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            TryBuy(BoosterType.Biggest);
        }

        public void OnClickBuyShuffle()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            TryBuy(BoosterType.Shuffle);
        }

        private void TryBuy(BoosterType type)
        {
            BoosterShopItemData item = GetItem(type);
            int price = Mathf.Max(0, item.Price);
            if (_playerData.Coin < price)
            {
                return;
            }

            _playerData.Coin -= price;
            BoosterInventoryService.AddUses(_playerData, type, Mathf.Max(1, item.UsesPerPurchase));
            AudioManager.PlaySound(AudioLibrarySounds._CoinSound);
            _onPurchased?.Invoke();
        }

        private void RefreshItem(BoosterType type, Button button, TMP_Text priceText, TMP_Text amountText)
        {
            BoosterShopItemData item = GetItem(type);
            int price = Mathf.Max(0, item.Price);
            priceText.text = price.ToString();
            amountText.text = $"X{Mathf.Max(1, item.UsesPerPurchase)}";
            button.interactable = _playerData.Coin >= price;
        }

        private void BindButtons()
        {
            destructionButton.onClick.RemoveListener(OnClickBuyDestruction);
            destructionButton.onClick.AddListener(OnClickBuyDestruction);
            promotionButton.onClick.RemoveListener(OnClickBuyPromotion);
            promotionButton.onClick.AddListener(OnClickBuyPromotion);
            biggestButton.onClick.RemoveListener(OnClickBuyBiggest);
            biggestButton.onClick.AddListener(OnClickBuyBiggest);
            shuffleButton.onClick.RemoveListener(OnClickBuyShuffle);
            shuffleButton.onClick.AddListener(OnClickBuyShuffle);
        }

        private BoosterShopItemData GetItem(BoosterType type)
        {
            if (_database == null)
            {
                throw new InvalidOperationException($"{nameof(BoosterContainer)} requires a booster shop database.");
            }

            BoosterShopItemData item = _database.GetItemByType(type);
            if (item == null)
            {
                throw new InvalidOperationException($"{nameof(BoosterContainer)} missing shop data for booster type {type}.");
            }

            return item;
        }
    }
}
