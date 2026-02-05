using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace LWT.UnityWorkbench.Permission
{
    /// <summary>
    /// Manager for runtime permissions (Android/iOS).
    /// Provides unified interface for requesting and checking permissions.
    /// </summary>
    public class PermissionManager : MonoBehaviour
    {
        private static PermissionManager _instance;
        public static PermissionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PermissionManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[PermissionManager]");
                        _instance = go.AddComponent<PermissionManager>();
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        private readonly Dictionary<AppPermission, Action<bool>> _pendingCallbacks = new();

        /// <summary>Event fired when a permission is granted.</summary>
        public event Action<AppPermission> OnPermissionGranted;

        /// <summary>Event fired when a permission is denied.</summary>
        public event Action<AppPermission> OnPermissionDenied;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Check if a permission is granted.
        /// </summary>
        public bool HasPermission(AppPermission permission)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(GetAndroidPermission(permission));
#elif UNITY_IOS && !UNITY_EDITOR
            return HasIOSPermission(permission);
#else
            return true; // Editor/Standalone always has permission
#endif
        }

        /// <summary>
        /// Request a permission.
        /// </summary>
        public void RequestPermission(AppPermission permission, Action<bool> callback = null)
        {
            if (HasPermission(permission))
            {
                callback?.Invoke(true);
                OnPermissionGranted?.Invoke(permission);
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            RequestAndroidPermission(permission, callback);
#elif UNITY_IOS && !UNITY_EDITOR
            RequestIOSPermission(permission, callback);
#else
            // In editor/standalone, simulate permission grant
            bool granted = true; // Change to false to test denial flow
            callback?.Invoke(granted);
            if (granted)
                OnPermissionGranted?.Invoke(permission);
            else
                OnPermissionDenied?.Invoke(permission);
#endif
        }

        /// <summary>
        /// Request multiple permissions.
        /// </summary>
        public void RequestPermissions(AppPermission[] permissions, Action<Dictionary<AppPermission, bool>> callback)
        {
            var results = new Dictionary<AppPermission, bool>();
            var remaining = permissions.Length;

            if (remaining == 0)
            {
                callback?.Invoke(results);
                return;
            }

            foreach (var permission in permissions)
            {
                RequestPermission(permission, granted =>
                {
                    results[permission] = granted;
                    remaining--;

                    if (remaining == 0)
                    {
                        callback?.Invoke(results);
                    }
                });
            }
        }

        /// <summary>
        /// Check if should show permission rationale (Android).
        /// </summary>
        public bool ShouldShowRationale(AppPermission permission)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return UnityEngine.Android.Permission.ShouldShowRequestPermissionRationale(GetAndroidPermission(permission));
#else
            return false;
#endif
        }

        /// <summary>
        /// Open app settings (for manually enabling permissions).
        /// </summary>
        public void OpenAppSettings()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            OpenAndroidSettings();
#elif UNITY_IOS && !UNITY_EDITOR
            OpenIOSSettings();
#else
            Debug.Log("[Permission] Open settings not supported on this platform");
#endif
        }

        #region Android Implementation

#if UNITY_ANDROID
        private void RequestAndroidPermission(AppPermission permission, Action<bool> callback)
        {
            var androidPermission = GetAndroidPermission(permission);

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ =>
            {
                callback?.Invoke(true);
                OnPermissionGranted?.Invoke(permission);
            };
            callbacks.PermissionDenied += _ =>
            {
                callback?.Invoke(false);
                OnPermissionDenied?.Invoke(permission);
            };
            callbacks.PermissionDeniedAndDontAskAgain += _ =>
            {
                callback?.Invoke(false);
                OnPermissionDenied?.Invoke(permission);
            };

            UnityEngine.Android.Permission.RequestUserPermission(androidPermission, callbacks);
        }

        private string GetAndroidPermission(AppPermission permission)
        {
            return permission switch
            {
                AppPermission.Camera => UnityEngine.Android.Permission.Camera,
                AppPermission.Microphone => UnityEngine.Android.Permission.Microphone,
                AppPermission.FineLocation => UnityEngine.Android.Permission.FineLocation,
                AppPermission.CoarseLocation => UnityEngine.Android.Permission.CoarseLocation,
                AppPermission.ExternalStorageRead => UnityEngine.Android.Permission.ExternalStorageRead,
                AppPermission.ExternalStorageWrite => UnityEngine.Android.Permission.ExternalStorageWrite,
                AppPermission.Contacts => "android.permission.READ_CONTACTS",
                AppPermission.Calendar => "android.permission.READ_CALENDAR",
                AppPermission.Phone => "android.permission.CALL_PHONE",
                AppPermission.SMS => "android.permission.SEND_SMS",
                AppPermission.Bluetooth => "android.permission.BLUETOOTH",
                AppPermission.BluetoothScan => "android.permission.BLUETOOTH_SCAN",
                AppPermission.BluetoothConnect => "android.permission.BLUETOOTH_CONNECT",
                AppPermission.Notifications => "android.permission.POST_NOTIFICATIONS",
                _ => string.Empty
            };
        }

        private void OpenAndroidSettings()
        {
            try
            {
                using var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                using var intent = new AndroidJavaObject("android.content.Intent",
                    "android.settings.APPLICATION_DETAILS_SETTINGS");

                var packageName = currentActivity.Call<string>("getPackageName");
                using var uri = new AndroidJavaClass("android.net.Uri")
                    .CallStatic<AndroidJavaObject>("parse", $"package:{packageName}");

                intent.Call<AndroidJavaObject>("setData", uri);
                currentActivity.Call("startActivity", intent);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Permission] Failed to open settings: {e.Message}");
            }
        }
#endif

        #endregion

        #region iOS Implementation

#if UNITY_IOS
        private bool HasIOSPermission(AppPermission permission)
        {
            // iOS permission checks would use native plugins
            // This is a placeholder
            return true;
        }

        private void RequestIOSPermission(AppPermission permission, Action<bool> callback)
        {
            // iOS permission requests would use native plugins
            // This is a placeholder
            callback?.Invoke(true);
            OnPermissionGranted?.Invoke(permission);
        }

        private void OpenIOSSettings()
        {
            Application.OpenURL("app-settings:");
        }
#endif

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check and request camera permission.
        /// </summary>
        public void RequestCameraPermission(Action<bool> callback)
        {
            RequestPermission(AppPermission.Camera, callback);
        }

        /// <summary>
        /// Check and request microphone permission.
        /// </summary>
        public void RequestMicrophonePermission(Action<bool> callback)
        {
            RequestPermission(AppPermission.Microphone, callback);
        }

        /// <summary>
        /// Check and request location permission.
        /// </summary>
        public void RequestLocationPermission(Action<bool> callback, bool finePrecision = true)
        {
            var permission = finePrecision ? AppPermission.FineLocation : AppPermission.CoarseLocation;
            RequestPermission(permission, callback);
        }

        /// <summary>
        /// Check and request storage permissions.
        /// </summary>
        public void RequestStoragePermission(Action<bool> callback)
        {
            RequestPermissions(new[]
            {
                AppPermission.ExternalStorageRead,
                AppPermission.ExternalStorageWrite
            }, results =>
            {
                bool allGranted = true;
                foreach (var result in results.Values)
                {
                    if (!result) allGranted = false;
                }
                callback?.Invoke(allGranted);
            });
        }

        #endregion
    }

    /// <summary>
    /// App permissions enum.
    /// </summary>
    public enum AppPermission
    {
        Camera,
        Microphone,
        FineLocation,
        CoarseLocation,
        ExternalStorageRead,
        ExternalStorageWrite,
        Contacts,
        Calendar,
        Phone,
        SMS,
        Bluetooth,
        BluetoothScan,
        BluetoothConnect,
        Notifications
    }
}
