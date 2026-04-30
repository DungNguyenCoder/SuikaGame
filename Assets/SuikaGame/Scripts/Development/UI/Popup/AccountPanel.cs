using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Cysharp.Threading.Tasks;
using Development;
using Development.LoadSave;
using Development.LoadSave.Data;
using Development.Managers;
using Development.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuikaGame.Scripts.Development.UI.Popup
{
    public class AccountPanel : Panel
    {
        [SerializeField] private TMP_InputField accountInputField;
        [SerializeField] private FacebookAuth facebookAuth;
        [SerializeField] private RectTransform avatarContent;
        [SerializeField] private RectTransform selectedBorder;
        [SerializeField] private Image selectedBorderImage;
        [SerializeField] private ScrollRect avatarScrollRect;
        [SerializeField] private Vector2 selectedBorderPadding = new Vector2(10f, 10f);
        [SerializeField] private float selectedHideOffset = 6f;
        [SerializeField] private Transform panel;
        [SerializeField] private TMP_Text connectText;

        private const string DefaultUserName = "Guest";
        private const string FacebookLoginButtonText = "Connect";
        private const string FacebookLogoutButtonText = "Logout";
        private const float AvatarScrollTopPosition = 1f;
        private const float ViewportEdgePadding = 0.01f;

        private readonly List<AvatarOption> _avatarOptions = new();
        private readonly Vector3[] _targetWorldCorners = new Vector3[4];
        private readonly Vector3[] _viewportWorldCorners = new Vector3[4];
        private PlayerSaveData _playerData;
        private string _initialUserName = DefaultUserName;
        private string _initialAvatarPath = string.Empty;
        private string _workingAvatarPath = string.Empty;
        private int _enableVersion;
        private bool _isResettingAvatarScroll;
        private bool _isFacebookActionRunning;

        private void OnEnable()
        {
            _enableVersion++;
            panel.localScale = Vector3.zero;
            selectedBorderImage.raycastTarget = false;
            RefreshFacebookButtonText();
            CacheAvatarOptions();
            ResetAvatarScrollToTop();
            avatarScrollRect.onValueChanged.AddListener(HandleScrollValueChanged);
            InitializeFromProfileAsync(_enableVersion).Forget();
        }

        private void OnDisable()
        {
            _enableVersion++;
            _isFacebookActionRunning = false;
            avatarScrollRect.onValueChanged.RemoveListener(HandleScrollValueChanged);
            avatarScrollRect.StopMovement();
            avatarScrollRect.velocity = Vector2.zero;
        }

        public void OnClickSaveChange()
        {
            SaveChangesAsync().Forget();
        }

        public void OnClickFacebookLogin()
        {
            ToggleFacebookConnectionAsync(_enableVersion).Forget();
        }
        
        public void OnClickClose()
        {
            RevertWorkingChanges();
            Close();
        }

        public bool TryGetAvatarSprite(string avatarPath, out Sprite avatarSprite)
        {
            if (_avatarOptions.Count == 0)
            {
                CacheAvatarOptions();
            }

            AvatarOption option = FindAvatarOption(avatarPath);
            if (option != null && option.Image.sprite != null)
            {
                avatarSprite = option.Image.sprite;
                return true;
            }

            avatarSprite = null;
            return false;
        }

        private async UniTaskVoid InitializeFromProfileAsync(int enableVersion)
        {
            _playerData = SaveRuntimeData.Player ?? await JsonRepository.LoadPlayerProfile();
            if (!CanApplyPanelRefresh(enableVersion))
            {
                return;
            }

            SaveRuntimeData.SetPlayer(_playerData);

            _initialUserName = NormalizeUserName(_playerData.UserName);
            _initialAvatarPath = _playerData.AvatarId ?? string.Empty;
            _workingAvatarPath = _initialAvatarPath;
            accountInputField.text = _initialUserName;

            RefreshAvatarLayout();
            SelectAvatarByPath(_workingAvatarPath, true);
            RefreshSelectionAfterLayoutAsync(enableVersion).Forget();
        }

        private async UniTaskVoid SaveChangesAsync()
        {
            _playerData = SaveRuntimeData.Player ?? await JsonRepository.LoadPlayerProfile();
            _playerData.UserName = NormalizeUserName(accountInputField.text);
            _playerData.AvatarId = ResolveValidAvatarPath(_workingAvatarPath);

            _initialUserName = _playerData.UserName;
            _initialAvatarPath = _playerData.AvatarId;
            _workingAvatarPath = _playerData.AvatarId;

            SaveRuntimeData.SetPlayer(_playerData);
            await JsonRepository.SavePlayerProfile(_playerData);
            EventManager.OnProfileAvatarChanged?.Invoke(GetSelectedAvatarSprite());
            EventManager.OnProfileChanged?.Invoke();

            Close();
        }

        private void RevertWorkingChanges()
        {
            _workingAvatarPath = _initialAvatarPath;
            accountInputField.text = _initialUserName;
            SelectAvatarByPath(_workingAvatarPath, true);
        }

        private async UniTaskVoid ToggleFacebookConnectionAsync(int enableVersion)
        {
            if (_isFacebookActionRunning)
            {
                return;
            }

            _isFacebookActionRunning = true;
            try
            {
                if (facebookAuth.IsLoggedIn)
                {
                    SetFacebookButtonText(false);
                    accountInputField.text = DefaultUserName;
                    facebookAuth.Logout();
                    return;
                }

                SetFacebookButtonText(true);
                (bool success, string userName) = await facebookAuth.TryGetFacebookUserNameAsync();
                if (success && CanApplyPanelRefresh(enableVersion))
                {
                    accountInputField.text = NormalizeUserName(userName);
                }
            }
            finally
            {
                _isFacebookActionRunning = false;
                if (CanApplyPanelRefresh(enableVersion))
                {
                    RefreshFacebookButtonText();
                }
            }
        }

        private void CacheAvatarOptions()
        {
            _avatarOptions.Clear();

            for (int i = 0; i < avatarContent.childCount; i++)
            {
                Transform child = avatarContent.GetChild(i);
                if (!(child is RectTransform avatarRectTransform))
                {
                    continue;
                }

                if (!child.TryGetComponent(out Image avatarImage))
                {
                    continue;
                }

                Button avatarButton = child.GetComponent<Button>();
                if (avatarButton == null)
                {
                    avatarButton = child.gameObject.AddComponent<Button>();
                }

                avatarButton.targetGraphic = avatarImage;
                avatarButton.onClick.RemoveAllListeners();

                string avatarPath = BuildPathFromContent(avatarRectTransform);
                var option = new AvatarOption(avatarPath, avatarRectTransform, avatarImage);
                avatarButton.onClick.AddListener(() => OnAvatarClicked(option));
                _avatarOptions.Add(option);
            }
        }

        private void OnAvatarClicked(AvatarOption option)
        {
            _workingAvatarPath = option.Path;
            RefreshSelectedBorder(option.RectTransform);
        }

        private void SelectAvatarByPath(string avatarPath, bool fallbackToFirst)
        {
            AvatarOption selectedOption = FindAvatarOption(avatarPath);
            if (selectedOption == null && fallbackToFirst && _avatarOptions.Count > 0)
            {
                selectedOption = _avatarOptions[0];
            }

            if (selectedOption == null)
            {
                selectedBorder.gameObject.SetActive(false);
                _workingAvatarPath = string.Empty;
                return;
            }

            _workingAvatarPath = selectedOption.Path;
            RefreshSelectedBorder(selectedOption.RectTransform);
        }

        private AvatarOption FindAvatarOption(string avatarPath)
        {
            if (string.IsNullOrEmpty(avatarPath))
            {
                return null;
            }

            for (int i = 0; i < _avatarOptions.Count; i++)
            {
                AvatarOption option = _avatarOptions[i];
                if (option.Path == avatarPath)
                {
                    return option;
                }
            }

            return null;
        }

        private string ResolveValidAvatarPath(string avatarPath)
        {
            AvatarOption option = FindAvatarOption(avatarPath);
            if (option != null)
            {
                return option.Path;
            }

            if (_avatarOptions.Count == 0)
            {
                return string.Empty;
            }

            return _avatarOptions[0].Path;
        }

        private Sprite GetSelectedAvatarSprite()
        {
            AvatarOption option = FindAvatarOption(_workingAvatarPath);
            if (option != null)
            {
                return option.Image.sprite;
            }

            if (_avatarOptions.Count == 0)
            {
                return null;
            }

            return _avatarOptions[0].Image.sprite;
        }

        private void MoveSelectedBorder(RectTransform target)
        {
            RectTransform borderParent = (RectTransform)selectedBorder.parent;
            Vector3 worldCenter = target.TransformPoint(target.rect.center);
            Vector3 localCenter = borderParent.InverseTransformPoint(worldCenter);
            selectedBorder.anchoredPosition = new Vector2(localCenter.x, localCenter.y);
            selectedBorder.sizeDelta = target.rect.size + selectedBorderPadding;
            selectedBorder.gameObject.SetActive(true);
            selectedBorder.SetAsLastSibling();
        }

        private void HandleScrollValueChanged(Vector2 _)
        {
            if (_isResettingAvatarScroll)
            {
                return;
            }

            AvatarOption selectedOption = FindAvatarOption(_workingAvatarPath);
            if (selectedOption != null)
            {
                RefreshSelectedBorder(selectedOption.RectTransform);
            }
        }

        private async UniTaskVoid RefreshSelectionAfterLayoutAsync(int enableVersion)
        {
            await UniTask.NextFrame();
            if (!CanApplyPanelRefresh(enableVersion))
            {
                return;
            }

            RefreshAvatarLayout();
            SelectAvatarByPath(_workingAvatarPath, true);
        }

        private bool CanApplyPanelRefresh(int enableVersion)
        {
            return isActiveAndEnabled && enableVersion == _enableVersion;
        }

        private void RefreshAvatarLayout()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(avatarContent);
            ResetAvatarScrollToTop();
        }

        private void ResetAvatarScrollToTop()
        {
            _isResettingAvatarScroll = true;
            avatarScrollRect.StopMovement();
            avatarScrollRect.verticalNormalizedPosition = AvatarScrollTopPosition;
            avatarContent.anchoredPosition = new Vector2(avatarContent.anchoredPosition.x, 0f);
            avatarScrollRect.velocity = Vector2.zero;
            _isResettingAvatarScroll = false;
        }

        private void RefreshFacebookButtonText()
        {
            SetFacebookButtonText(facebookAuth.IsLoggedIn);
        }

        private void SetFacebookButtonText(bool isConnected)
        {
            connectText.text = isConnected ? FacebookLogoutButtonText : FacebookLoginButtonText;
        }

        private void RefreshSelectedBorder(RectTransform target)
        {
            if (!IsVisibleInViewport(target))
            {
                selectedBorder.gameObject.SetActive(false);
                return;
            }

            MoveSelectedBorder(target);
        }

        private bool IsVisibleInViewport(RectTransform target)
        {
            RectTransform viewport = avatarScrollRect.viewport;
            if (viewport == null)
            {
                return true;
            }

            target.GetWorldCorners(_targetWorldCorners);
            viewport.GetWorldCorners(_viewportWorldCorners);

            float targetMinX = Mathf.Min(_targetWorldCorners[0].x, _targetWorldCorners[2].x);
            float targetMaxX = Mathf.Max(_targetWorldCorners[0].x, _targetWorldCorners[2].x);
            float targetMinY = Mathf.Min(_targetWorldCorners[0].y, _targetWorldCorners[2].y);
            float targetMaxY = Mathf.Max(_targetWorldCorners[0].y, _targetWorldCorners[2].y);

            float viewportMinX = Mathf.Min(_viewportWorldCorners[0].x, _viewportWorldCorners[2].x);
            float viewportMaxX = Mathf.Max(_viewportWorldCorners[0].x, _viewportWorldCorners[2].x);
            float viewportMinY = Mathf.Min(_viewportWorldCorners[0].y, _viewportWorldCorners[2].y);
            float viewportMaxY = Mathf.Max(_viewportWorldCorners[0].y, _viewportWorldCorners[2].y);

            float minX = viewportMinX + ViewportEdgePadding - selectedHideOffset;
            float maxX = viewportMaxX - ViewportEdgePadding + selectedHideOffset;
            float minY = viewportMinY + ViewportEdgePadding - selectedHideOffset;
            float maxY = viewportMaxY - ViewportEdgePadding + selectedHideOffset;

            bool insideX = targetMinX > minX && targetMaxX < maxX;
            bool insideY = targetMinY > minY && targetMaxY < maxY;
            return insideX && insideY;
        }

        private string BuildPathFromContent(Transform target)
        {
            var nodeNames = new Stack<string>();
            Transform current = target;
            while (current != avatarContent)
            {
                nodeNames.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", nodeNames);
        }

        private static string NormalizeUserName(string rawName)
        {
            string trimmed = string.IsNullOrWhiteSpace(rawName) ? string.Empty : rawName.Trim();
            string normalizedName = RemoveDiacritics(trimmed);
            return string.IsNullOrEmpty(normalizedName) ? DefaultUserName : normalizedName;
        }

        private static string RemoveDiacritics(string value)
        {
            string normalizedValue = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalizedValue.Length);
            for (int i = 0; i < normalizedValue.Length; i++)
            {
                char character = normalizedValue[i];
                if (character == '\u0111')
                {
                    builder.Append('d');
                    continue;
                }

                if (character == '\u0110')
                {
                    builder.Append('D');
                    continue;
                }

                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private sealed class AvatarOption
        {
            public AvatarOption(string path, RectTransform rectTransform, Image image)
            {
                Path = path;
                RectTransform = rectTransform;
                Image = image;
            }

            public string Path { get; }
            public RectTransform RectTransform { get; }
            public Image Image { get; }
        }
    }
}
