using System;
using Core.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuikaGame.Scripts.Development.UI.Shop
{
    public class ObjectBlock : MonoBehaviour
    {
        [SerializeField] private Image previewImage;
        [SerializeField] private GameObject lockObject;
        [SerializeField] private TMP_Text priceText;

        private int _skinSeriesId;
        private Action<int> _onClick;

        public void Setup(SkinShopItemData item, bool isPurchased, Action<int> onClick)
        {
            _skinSeriesId = item.SkinSeriesID;
            _onClick = onClick;
            gameObject.SetActive(true);
            previewImage.sprite = item.Banner;
            priceText.text = item.Price.ToString();
            lockObject.SetActive(!isPurchased);
        }

        public void OnClick()
        {
            _onClick?.Invoke(_skinSeriesId);
        }
    }
}
