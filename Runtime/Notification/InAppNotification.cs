using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LWT.UnityWorkbench.Notification
{
    /// <summary>
    /// In-app notification system for displaying alerts, toasts, and banners.
    /// </summary>
    public class InAppNotification : MonoBehaviour
    {
        private static InAppNotification _instance;
        public static InAppNotification Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<InAppNotification>();
                }
                return _instance;
            }
        }

        [Header("Prefabs")]
        [SerializeField] private GameObject _toastPrefab;
        [SerializeField] private GameObject _bannerPrefab;
        [SerializeField] private GameObject _alertPrefab;

        [Header("Containers")]
        [SerializeField] private RectTransform _toastContainer;
        [SerializeField] private RectTransform _bannerContainer;
        [SerializeField] private RectTransform _alertContainer;

        [Header("Settings")]
        [SerializeField] private float _defaultToastDuration = 3f;
        [SerializeField] private float _defaultBannerDuration = 5f;
        [SerializeField] private int _maxVisibleToasts = 3;
        [SerializeField] private float _toastSpacing = 10f;

        /// <summary>Spacing between stacked toasts.</summary>
        public float ToastSpacing => _toastSpacing;

        private Queue<InAppNotificationData> _toastQueue = new();
        private List<GameObject> _activeToasts = new();
        private GameObject _activeBanner;
        private bool _isShowingAlert;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        #region Toast Notifications

        /// <summary>
        /// Show a simple toast notification.
        /// </summary>
        public void ShowToast(string message, float duration = 0)
        {
            ShowToast(new InAppNotificationData
            {
                Message = message,
                Duration = duration > 0 ? duration : _defaultToastDuration,
                Type = NotificationType.Info
            });
        }

        /// <summary>
        /// Show a toast with type.
        /// </summary>
        public void ShowToast(string message, NotificationType type, float duration = 0)
        {
            ShowToast(new InAppNotificationData
            {
                Message = message,
                Duration = duration > 0 ? duration : _defaultToastDuration,
                Type = type
            });
        }

        /// <summary>
        /// Show a toast from notification data.
        /// </summary>
        public void ShowToast(InAppNotificationData data)
        {
            if (_activeToasts.Count >= _maxVisibleToasts)
            {
                _toastQueue.Enqueue(data);
                return;
            }

            CreateToast(data);
        }

        private void CreateToast(InAppNotificationData data)
        {
            if (_toastPrefab == null || _toastContainer == null) return;

            var toast = Instantiate(_toastPrefab, _toastContainer);
            _activeToasts.Add(toast);

            // Setup toast
            var textComponent = toast.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = data.Message;
            }

            // Set color based on type
            var image = toast.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetTypeColor(data.Type);
            }

            // Auto dismiss
            StartCoroutine(DismissToastAfterDelay(toast, data.Duration));
        }

        private IEnumerator DismissToastAfterDelay(GameObject toast, float delay)
        {
            yield return new WaitForSeconds(delay);
            DismissToast(toast);
        }

        private void DismissToast(GameObject toast)
        {
            if (toast == null) return;

            _activeToasts.Remove(toast);
            Destroy(toast);

            // Show queued toast
            if (_toastQueue.Count > 0)
            {
                var next = _toastQueue.Dequeue();
                ShowToast(next);
            }
        }

        #endregion

        #region Banner Notifications

        /// <summary>
        /// Show a banner notification.
        /// </summary>
        public void ShowBanner(string title, string message, float duration = 0, Action onClick = null)
        {
            ShowBanner(new InAppNotificationData
            {
                Title = title,
                Message = message,
                Duration = duration > 0 ? duration : _defaultBannerDuration,
                OnClick = onClick
            });
        }

        /// <summary>
        /// Show a banner from notification data.
        /// </summary>
        public void ShowBanner(InAppNotificationData data)
        {
            // Dismiss current banner
            if (_activeBanner != null)
            {
                DismissBanner();
            }

            if (_bannerPrefab == null || _bannerContainer == null) return;

            _activeBanner = Instantiate(_bannerPrefab, _bannerContainer);

            // Setup banner
            var titleText = _activeBanner.transform.Find("Title")?.GetComponent<TMP_Text>();
            var messageText = _activeBanner.transform.Find("Message")?.GetComponent<TMP_Text>();

            if (titleText != null) titleText.text = data.Title;
            if (messageText != null) messageText.text = data.Message;

            // Setup click handler
            var button = _activeBanner.GetComponent<Button>();
            if (button != null && data.OnClick != null)
            {
                button.onClick.AddListener(() =>
                {
                    data.OnClick?.Invoke();
                    DismissBanner();
                });
            }

            // Auto dismiss
            if (data.Duration > 0)
            {
                StartCoroutine(DismissBannerAfterDelay(data.Duration));
            }
        }

        private IEnumerator DismissBannerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            DismissBanner();
        }

        /// <summary>
        /// Dismiss the current banner.
        /// </summary>
        public void DismissBanner()
        {
            if (_activeBanner != null)
            {
                Destroy(_activeBanner);
                _activeBanner = null;
            }
        }

        #endregion

        #region Alert Dialogs

        /// <summary>
        /// Show an alert dialog.
        /// </summary>
        public void ShowAlert(string title, string message, string buttonText = "OK", Action onConfirm = null)
        {
            ShowAlert(new InAppNotificationData
            {
                Title = title,
                Message = message,
                ButtonText = buttonText,
                OnClick = onConfirm
            });
        }

        /// <summary>
        /// Show an alert with confirm/cancel buttons.
        /// </summary>
        public void ShowConfirmAlert(string title, string message,
            string confirmText = "Confirm", string cancelText = "Cancel",
            Action onConfirm = null, Action onCancel = null)
        {
            ShowAlert(new InAppNotificationData
            {
                Title = title,
                Message = message,
                ButtonText = confirmText,
                CancelText = cancelText,
                OnClick = onConfirm,
                OnCancel = onCancel,
                ShowCancel = true
            });
        }

        /// <summary>
        /// Show alert from notification data.
        /// </summary>
        public void ShowAlert(InAppNotificationData data)
        {
            if (_isShowingAlert) return;
            if (_alertPrefab == null || _alertContainer == null) return;

            _isShowingAlert = true;

            var alert = Instantiate(_alertPrefab, _alertContainer);

            // Setup alert
            var titleText = alert.transform.Find("Title")?.GetComponent<TMP_Text>();
            var messageText = alert.transform.Find("Message")?.GetComponent<TMP_Text>();
            var confirmButton = alert.transform.Find("ConfirmButton")?.GetComponent<Button>();
            var cancelButton = alert.transform.Find("CancelButton")?.GetComponent<Button>();

            if (titleText != null) titleText.text = data.Title;
            if (messageText != null) messageText.text = data.Message;

            if (confirmButton != null)
            {
                var confirmText = confirmButton.GetComponentInChildren<TMP_Text>();
                if (confirmText != null) confirmText.text = data.ButtonText ?? "OK";

                confirmButton.onClick.AddListener(() =>
                {
                    data.OnClick?.Invoke();
                    DismissAlert(alert);
                });
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(data.ShowCancel);

                if (data.ShowCancel)
                {
                    var cancelText = cancelButton.GetComponentInChildren<TMP_Text>();
                    if (cancelText != null) cancelText.text = data.CancelText ?? "Cancel";

                    cancelButton.onClick.AddListener(() =>
                    {
                        data.OnCancel?.Invoke();
                        DismissAlert(alert);
                    });
                }
            }
        }

        private void DismissAlert(GameObject alert)
        {
            if (alert != null)
            {
                Destroy(alert);
            }
            _isShowingAlert = false;
        }

        #endregion

        #region Helpers

        private Color GetTypeColor(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => new Color(0.2f, 0.8f, 0.2f),
                NotificationType.Warning => new Color(0.9f, 0.7f, 0.1f),
                NotificationType.Error => new Color(0.9f, 0.2f, 0.2f),
                _ => new Color(0.3f, 0.3f, 0.3f)
            };
        }

        /// <summary>
        /// Clear all notifications.
        /// </summary>
        public void ClearAll()
        {
            foreach (var toast in _activeToasts)
            {
                if (toast != null) Destroy(toast);
            }
            _activeToasts.Clear();
            _toastQueue.Clear();

            DismissBanner();
        }

        #endregion
    }

    /// <summary>
    /// In-app notification data.
    /// </summary>
    public class InAppNotificationData
    {
        public string Title;
        public string Message;
        public float Duration;
        public NotificationType Type = NotificationType.Info;
        public string ButtonText;
        public string CancelText;
        public bool ShowCancel;
        public Action OnClick;
        public Action OnCancel;
        public Sprite Icon;
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
