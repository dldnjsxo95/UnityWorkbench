using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Central manager for popup windows.
    /// Handles popup stacking, modal blocking, and common dialog shortcuts.
    /// </summary>
    public class PopupManager : PersistentMonoSingleton<PopupManager>
    {
        [Header("References")]
        [SerializeField] private Transform _popupContainer;
        [SerializeField] private Canvas _popupCanvas;

        [Header("Modal Blocker")]
        [SerializeField] private GameObject _modalBlockerPrefab;
        [SerializeField] private Color _modalBlockerColor = new Color(0, 0, 0, 0.5f);

        [Header("Prefabs")]
        [SerializeField] private ConfirmDialog _confirmDialogPrefab;
        [SerializeField] private ToastNotification _toastPrefab;

        [Header("Settings")]
        [SerializeField] private bool _handleEscapeKey = true;
        [SerializeField] private int _baseSortingOrder = 1000;

        private readonly List<IPopup> _activePopups = new List<IPopup>();
        private readonly Dictionary<string, PopupBase> _popupPool = new Dictionary<string, PopupBase>();
        private GameObject _modalBlocker;
        private Image _modalBlockerImage;

        public IReadOnlyList<IPopup> ActivePopups => _activePopups;
        public int PopupCount => _activePopups.Count;
        public bool HasActivePopups => _activePopups.Count > 0;

        public event Action<IPopup> OnPopupOpened;
        public event Action<IPopup> OnPopupClosed;

        protected override void OnSingletonAwake()
        {
            EnsureCanvas();
            CreateModalBlocker();
        }

        private void Update()
        {
            if (_handleEscapeKey && Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapePressed();
            }
        }

        private void EnsureCanvas()
        {
            if (_popupCanvas == null)
            {
                var canvasGo = new GameObject("[PopupCanvas]");
                canvasGo.transform.SetParent(transform);

                _popupCanvas = canvasGo.AddComponent<Canvas>();
                _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _popupCanvas.sortingOrder = _baseSortingOrder;

                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasGo.AddComponent<GraphicRaycaster>();
            }

            if (_popupContainer == null)
            {
                var containerGo = new GameObject("PopupContainer");
                containerGo.transform.SetParent(_popupCanvas.transform);

                var rect = containerGo.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                _popupContainer = containerGo.transform;
            }
        }

        private void CreateModalBlocker()
        {
            var blockerGo = new GameObject("ModalBlocker");
            blockerGo.transform.SetParent(_popupContainer);
            blockerGo.transform.SetAsFirstSibling();

            var rect = blockerGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _modalBlockerImage = blockerGo.AddComponent<Image>();
            _modalBlockerImage.color = _modalBlockerColor;

            var button = blockerGo.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(OnModalBlockerClicked);

            _modalBlocker = blockerGo;
            _modalBlocker.SetActive(false);
        }

        #region Public Methods

        /// <summary>
        /// Show a popup by type.
        /// </summary>
        public T ShowPopup<T>() where T : PopupBase
        {
            var prefab = GetPrefab<T>();
            if (prefab == null)
            {
                Debug.LogError($"[PopupManager] Popup prefab not found for type: {typeof(T).Name}");
                return null;
            }

            return ShowPopup(prefab) as T;
        }

        /// <summary>
        /// Show a popup from prefab.
        /// </summary>
        public PopupBase ShowPopup(PopupBase prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("[PopupManager] Popup prefab is null.");
                return null;
            }

            var popup = Instantiate(prefab, _popupContainer);
            popup.Initialize();

            RegisterPopup(popup);
            popup.Show();

            return popup;
        }

        /// <summary>
        /// Hide a specific popup.
        /// </summary>
        public void HidePopup(IPopup popup)
        {
            if (popup == null) return;

            popup.Close();
        }

        /// <summary>
        /// Hide all active popups.
        /// </summary>
        public void HideAllPopups()
        {
            var popupsToClose = new List<IPopup>(_activePopups);
            foreach (var popup in popupsToClose)
            {
                popup.Close();
            }
        }

        /// <summary>
        /// Register a popup with the manager.
        /// </summary>
        public void RegisterPopup(PopupBase popup)
        {
            if (popup == null) return;

            _activePopups.Add(popup);
            popup.OnClosed += OnPopupClosedHandler;

            SortPopups();
            UpdateModalBlocker();

            OnPopupOpened?.Invoke(popup);
            EventBus<PopupOpenedEvent>.Publish(new PopupOpenedEvent { PopupId = popup.PopupId });
        }

        /// <summary>
        /// Show a confirmation dialog.
        /// </summary>
        public ConfirmDialog ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            if (_confirmDialogPrefab == null)
            {
                Debug.LogError("[PopupManager] ConfirmDialog prefab not assigned.");
                return null;
            }

            var dialog = ShowPopup(_confirmDialogPrefab) as ConfirmDialog;
            dialog?.Setup(title, message, onConfirm, onCancel);
            return dialog;
        }

        /// <summary>
        /// Show a simple alert dialog.
        /// </summary>
        public ConfirmDialog ShowAlert(string title, string message, Action onClose = null)
        {
            if (_confirmDialogPrefab == null)
            {
                Debug.LogError("[PopupManager] ConfirmDialog prefab not assigned.");
                return null;
            }

            var dialog = ShowPopup(_confirmDialogPrefab) as ConfirmDialog;
            dialog?.SetupAlert(title, message, onClose);
            return dialog;
        }

        /// <summary>
        /// Show a toast notification.
        /// </summary>
        public ToastNotification ShowToast(string message, float duration = 2f)
        {
            if (_toastPrefab == null)
            {
                Debug.LogError("[PopupManager] Toast prefab not assigned.");
                return null;
            }

            var toast = Instantiate(_toastPrefab, _popupContainer);
            toast.Show(message, duration);
            return toast;
        }

        /// <summary>
        /// Check if a popup is currently showing.
        /// </summary>
        public bool IsPopupShowing(string popupId)
        {
            foreach (var popup in _activePopups)
            {
                if (popup.PopupId == popupId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the topmost popup.
        /// </summary>
        public IPopup GetTopPopup()
        {
            return _activePopups.Count > 0 ? _activePopups[_activePopups.Count - 1] : null;
        }

        #endregion

        #region Private Methods

        private T GetPrefab<T>() where T : PopupBase
        {
            if (typeof(T) == typeof(ConfirmDialog))
                return _confirmDialogPrefab as T;

            if (typeof(T) == typeof(ToastNotification))
                return _toastPrefab as T;

            return null;
        }

        private void OnPopupClosedHandler(IPopup popup)
        {
            _activePopups.Remove(popup);
            popup.OnClosed -= OnPopupClosedHandler;

            UpdateModalBlocker();

            OnPopupClosed?.Invoke(popup);
            EventBus<PopupClosedEvent>.Publish(new PopupClosedEvent { PopupId = popup.PopupId });

            // Destroy popup object after transition
            if (popup is MonoBehaviour mb)
            {
                Destroy(mb.gameObject, 0.5f);
            }
        }

        private void SortPopups()
        {
            _activePopups.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            for (int i = 0; i < _activePopups.Count; i++)
            {
                if (_activePopups[i] is MonoBehaviour mb)
                {
                    mb.transform.SetSiblingIndex(_modalBlocker.transform.GetSiblingIndex() + 1 + i);
                }
            }
        }

        private void UpdateModalBlocker()
        {
            bool hasModalPopup = false;
            int modalIndex = -1;

            for (int i = _activePopups.Count - 1; i >= 0; i--)
            {
                if (_activePopups[i].IsModal)
                {
                    hasModalPopup = true;
                    modalIndex = i;
                    break;
                }
            }

            _modalBlocker.SetActive(hasModalPopup);

            if (hasModalPopup && _activePopups[modalIndex] is MonoBehaviour mb)
            {
                _modalBlocker.transform.SetSiblingIndex(mb.transform.GetSiblingIndex());
            }
        }

        private void OnModalBlockerClicked()
        {
            var topPopup = GetTopPopup();
            if (topPopup is PopupBase popupBase && popupBase.CloseOnBackgroundClick)
            {
                topPopup.Close();
            }
        }

        private void HandleEscapePressed()
        {
            var topPopup = GetTopPopup();
            if (topPopup is PopupBase popupBase && popupBase.CloseOnEscape)
            {
                topPopup.Close();
            }
        }

        #endregion
    }

    #region Events

    /// <summary>
    /// Event fired when a popup is opened.
    /// </summary>
    public struct PopupOpenedEvent : IEvent
    {
        public string PopupId;
    }

    /// <summary>
    /// Event fired when a popup is closed.
    /// </summary>
    public struct PopupClosedEvent : IEvent
    {
        public string PopupId;
    }

    #endregion
}
