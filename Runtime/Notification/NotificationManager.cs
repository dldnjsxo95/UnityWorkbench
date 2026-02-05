using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Notification
{
    /// <summary>
    /// Manager for local notifications.
    /// Provides a unified interface for scheduling and managing notifications.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        private static NotificationManager _instance;
        public static NotificationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NotificationManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[NotificationManager]");
                        _instance = go.AddComponent<NotificationManager>();
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        [Header("Settings")]
        [SerializeField] private string _defaultChannelId = "default";
        [SerializeField] private string _defaultChannelName = "Default";
        [SerializeField] private string _defaultSmallIcon = "icon_small";
        [SerializeField] private string _defaultLargeIcon = "icon_large";

        /// <summary>Default notification channel ID.</summary>
        public string DefaultChannelId => _defaultChannelId;
        /// <summary>Default notification channel name.</summary>
        public string DefaultChannelName => _defaultChannelName;
        /// <summary>Default small icon name.</summary>
        public string DefaultSmallIcon => _defaultSmallIcon;
        /// <summary>Default large icon name.</summary>
        public string DefaultLargeIcon => _defaultLargeIcon;

        private readonly Dictionary<string, NotificationData> _scheduledNotifications = new();
        private int _notificationIdCounter;

        /// <summary>Whether notifications are enabled.</summary>
        public bool NotificationsEnabled { get; private set; } = true;

        /// <summary>Event fired when a notification is received while app is active.</summary>
        public event Action<NotificationData> OnNotificationReceived;

        /// <summary>Event fired when user interacts with a notification.</summary>
        public event Action<NotificationData> OnNotificationOpened;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            _notificationIdCounter = PlayerPrefs.GetInt("NotificationIdCounter", 0);

#if UNITY_ANDROID
            InitializeAndroid();
#elif UNITY_IOS
            InitializeIOS();
#endif
        }

        private void InitializeAndroid()
        {
            // Create default notification channel for Android 8.0+
            Debug.Log("[Notification] Android notification channel initialized");
        }

        private void InitializeIOS()
        {
            // Request notification permissions on iOS
            Debug.Log("[Notification] iOS notifications initialized");
        }

        /// <summary>
        /// Request notification permissions (required on iOS).
        /// </summary>
        public void RequestPermission(Action<bool> callback)
        {
#if UNITY_IOS
            // Request iOS notification permission
            callback?.Invoke(true); // Placeholder
#else
            callback?.Invoke(true);
#endif
        }

        /// <summary>
        /// Schedule a local notification.
        /// </summary>
        public int Schedule(string title, string body, DateTime fireTime,
            string data = null, string channelId = null)
        {
            var notification = new NotificationData
            {
                Id = GetNextId(),
                Title = title,
                Body = body,
                FireTime = fireTime,
                Data = data,
                ChannelId = channelId ?? _defaultChannelId
            };

            return Schedule(notification);
        }

        /// <summary>
        /// Schedule a local notification with delay.
        /// </summary>
        public int ScheduleWithDelay(string title, string body, TimeSpan delay,
            string data = null, string channelId = null)
        {
            return Schedule(title, body, DateTime.Now + delay, data, channelId);
        }

        /// <summary>
        /// Schedule a notification from NotificationData.
        /// </summary>
        public int Schedule(NotificationData notification)
        {
            if (notification.Id == 0)
            {
                notification.Id = GetNextId();
            }

            _scheduledNotifications[notification.Id.ToString()] = notification;

            // Platform-specific scheduling
#if UNITY_ANDROID
            ScheduleAndroid(notification);
#elif UNITY_IOS
            ScheduleIOS(notification);
#else
            Debug.Log($"[Notification] Scheduled: {notification.Title} at {notification.FireTime}");
#endif

            return notification.Id;
        }

        /// <summary>
        /// Cancel a scheduled notification.
        /// </summary>
        public void Cancel(int notificationId)
        {
            _scheduledNotifications.Remove(notificationId.ToString());

#if UNITY_ANDROID
            CancelAndroid(notificationId);
#elif UNITY_IOS
            CancelIOS(notificationId);
#endif

            Debug.Log($"[Notification] Cancelled: {notificationId}");
        }

        /// <summary>
        /// Cancel all scheduled notifications.
        /// </summary>
        public void CancelAll()
        {
            _scheduledNotifications.Clear();

#if UNITY_ANDROID
            CancelAllAndroid();
#elif UNITY_IOS
            CancelAllIOS();
#endif

            Debug.Log("[Notification] All notifications cancelled");
        }

        /// <summary>
        /// Get all scheduled notifications.
        /// </summary>
        public IEnumerable<NotificationData> GetScheduledNotifications()
        {
            return _scheduledNotifications.Values;
        }

        /// <summary>
        /// Check if a notification is scheduled.
        /// </summary>
        public bool IsScheduled(int notificationId)
        {
            return _scheduledNotifications.ContainsKey(notificationId.ToString());
        }

        /// <summary>
        /// Enable or disable notifications.
        /// </summary>
        public void SetNotificationsEnabled(bool enabled)
        {
            NotificationsEnabled = enabled;
            PlayerPrefs.SetInt("NotificationsEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (!enabled)
            {
                CancelAll();
            }
        }

        /// <summary>
        /// Set badge count (iOS only).
        /// </summary>
        public void SetBadgeCount(int count)
        {
#if UNITY_IOS
            // Set iOS badge
            Debug.Log($"[Notification] Badge set to: {count}");
#endif
        }

        /// <summary>
        /// Clear badge (iOS only).
        /// </summary>
        public void ClearBadge()
        {
            SetBadgeCount(0);
        }

        private int GetNextId()
        {
            _notificationIdCounter++;
            PlayerPrefs.SetInt("NotificationIdCounter", _notificationIdCounter);
            return _notificationIdCounter;
        }

        #region Platform-Specific Implementations

        private void ScheduleAndroid(NotificationData notification)
        {
            // Android-specific notification scheduling
            // Would use Unity's AndroidJavaClass/AndroidJavaObject
            Debug.Log($"[Notification] Android scheduled: {notification.Title}");
        }

        private void ScheduleIOS(NotificationData notification)
        {
            // iOS-specific notification scheduling
            Debug.Log($"[Notification] iOS scheduled: {notification.Title}");
        }

        private void CancelAndroid(int id)
        {
            Debug.Log($"[Notification] Android cancelled: {id}");
        }

        private void CancelIOS(int id)
        {
            Debug.Log($"[Notification] iOS cancelled: {id}");
        }

        private void CancelAllAndroid()
        {
            Debug.Log("[Notification] Android all cancelled");
        }

        private void CancelAllIOS()
        {
            Debug.Log("[Notification] iOS all cancelled");
        }

        #endregion

        /// <summary>
        /// Called when a notification is received.
        /// </summary>
        internal void HandleNotificationReceived(NotificationData notification)
        {
            OnNotificationReceived?.Invoke(notification);
        }

        /// <summary>
        /// Called when user opens a notification.
        /// </summary>
        internal void HandleNotificationOpened(NotificationData notification)
        {
            OnNotificationOpened?.Invoke(notification);
        }
    }

    /// <summary>
    /// Notification data structure.
    /// </summary>
    [Serializable]
    public class NotificationData
    {
        public int Id;
        public string Title;
        public string Body;
        public DateTime FireTime;
        public string Data;
        public string ChannelId;
        public string SmallIcon;
        public string LargeIcon;
        public bool AutoCancel = true;
        public NotificationRepeat Repeat = NotificationRepeat.None;
    }

    public enum NotificationRepeat
    {
        None,
        Daily,
        Weekly,
        Monthly
    }
}
