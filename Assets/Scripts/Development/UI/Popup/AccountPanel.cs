using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Development.LoadSave;
using Development.LoadSave.Data;
using Development.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Development.UI.Popup
{
    public class AccountPanel : Panel
    {
        [SerializeField] private TMP_InputField accountInputField;
        [SerializeField] private RectTransform avatarContent;
        [SerializeField] private RectTransform selectedBorder;
        [SerializeField] private Image selectedBorderImage;
        [SerializeField] private ScrollRect avatarScrollRect;
        [SerializeField] private Vector2 selectedBorderPadding = new Vector2(10f, 10f);
        [SerializeField] private float selectedHideOffset = 6f;

        private const string DefaultUserName = "Guest";
        private const float ViewportEdgePadding = 0.01f;

        private readonly List<AvatarOption> _avatarOptions = new();
        private readonly Vector3[] _targetWorldCorners = new Vector3[4];
        private readonly Vector3[] _viewportWorldCorners = new Vector3[4];
        private PlayerSaveData _playerData;
        private string _initialUserName = DefaultUserName;
        private string _initialAvatarPath = string.Empty;
        private string _workingAvatarPath = string.Empty;

        private void OnEnable()
        {
            selectedBorderImage.raycastTarget = false;
            CacheAvatarOptions();
            avatarScrollRect.onValueChanged.AddListener(HandleScrollValueChanged);
            InitializeFromProfileAsync().Forget();
        }

        private void OnDisable()
        {
            avatarScrollRect.onValueChanged.RemoveListener(HandleScrollValueChanged);
        }

        public void OnClickSaveChange()
        {
            SaveChangesAsync().Forget();
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

        private async UniTaskVoid InitializeFromProfileAsync()
        {
            _playerData = SaveRuntimeData.Player ?? await JsonRepository.LoadPlayerProfile();
            SaveRuntimeData.SetPlayer(_playerData);

            _initialUserName = NormalizeUserName(_playerData.UserName);
            _initialAvatarPath = _playerData.AvatarId ?? string.Empty;
            _workingAvatarPath = _initialAvatarPath;
            accountInputField.text = _initialUserName;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(avatarContent);
            SelectAvatarByPath(_workingAvatarPath, true);
            RefreshSelectionAfterLayoutAsync().Forget();
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
            AvatarOption selectedOption = FindAvatarOption(_workingAvatarPath);
            if (selectedOption != null)
            {
                RefreshSelectedBorder(selectedOption.RectTransform);
            }
        }

        private async UniTaskVoid RefreshSelectionAfterLayoutAsync()
        {
            await UniTask.NextFrame();
            if (!isActiveAndEnabled)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(avatarContent);
            SelectAvatarByPath(_workingAvatarPath, true);
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
            return string.IsNullOrEmpty(trimmed) ? DefaultUserName : trimmed;
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
