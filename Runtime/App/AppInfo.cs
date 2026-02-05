using System;
using UnityEngine;

namespace LWT.UnityWorkbench.App
{
    /// <summary>
    /// Static utility class for app information and device details.
    /// </summary>
    public static class AppInfo
    {
        /// <summary>App version from PlayerSettings.</summary>
        public static string Version => Application.version;

        /// <summary>Unity version.</summary>
        public static string UnityVersion => Application.unityVersion;

        /// <summary>Bundle identifier / package name.</summary>
        public static string BundleId => Application.identifier;

        /// <summary>Product name.</summary>
        public static string ProductName => Application.productName;

        /// <summary>Company name.</summary>
        public static string CompanyName => Application.companyName;

        /// <summary>Platform the app is running on.</summary>
        public static RuntimePlatform Platform => Application.platform;

        /// <summary>Whether running on mobile device.</summary>
        public static bool IsMobile => Application.isMobilePlatform;

        /// <summary>Whether running in editor.</summary>
        public static bool IsEditor => Application.isEditor;

        /// <summary>Whether this is a debug/development build.</summary>
        public static bool IsDebugBuild => Debug.isDebugBuild;

        /// <summary>System language.</summary>
        public static SystemLanguage SystemLanguage => Application.systemLanguage;

        /// <summary>Internet reachability status.</summary>
        public static NetworkReachability NetworkStatus => Application.internetReachability;

        /// <summary>Whether internet is available.</summary>
        public static bool HasInternet => Application.internetReachability != NetworkReachability.NotReachable;

        /// <summary>Whether on WiFi.</summary>
        public static bool IsOnWiFi => Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;

        /// <summary>Whether on mobile data.</summary>
        public static bool IsOnMobileData => Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;

        /// <summary>Device unique identifier.</summary>
        public static string DeviceId => SystemInfo.deviceUniqueIdentifier;

        /// <summary>Device model.</summary>
        public static string DeviceModel => SystemInfo.deviceModel;

        /// <summary>Device name.</summary>
        public static string DeviceName => SystemInfo.deviceName;

        /// <summary>Operating system.</summary>
        public static string OperatingSystem => SystemInfo.operatingSystem;

        /// <summary>Screen resolution.</summary>
        public static Resolution ScreenResolution => Screen.currentResolution;

        /// <summary>Screen DPI.</summary>
        public static float ScreenDPI => Screen.dpi;

        /// <summary>System memory in MB.</summary>
        public static int SystemMemoryMB => SystemInfo.systemMemorySize;

        /// <summary>Graphics memory in MB.</summary>
        public static int GraphicsMemoryMB => SystemInfo.graphicsMemorySize;

        /// <summary>Processor type.</summary>
        public static string Processor => SystemInfo.processorType;

        /// <summary>Processor count.</summary>
        public static int ProcessorCount => SystemInfo.processorCount;

        /// <summary>Graphics device name.</summary>
        public static string GraphicsDevice => SystemInfo.graphicsDeviceName;

        /// <summary>Battery level (0-1, -1 if not available).</summary>
        public static float BatteryLevel => SystemInfo.batteryLevel;

        /// <summary>Battery status.</summary>
        public static BatteryStatus BatteryStatus => SystemInfo.batteryStatus;

        /// <summary>
        /// Get a formatted version string including build info.
        /// </summary>
        public static string GetFullVersionString()
        {
            var buildType = IsDebugBuild ? "Debug" : "Release";
            return $"v{Version} ({buildType})";
        }

        /// <summary>
        /// Get device info as a formatted string (useful for bug reports).
        /// </summary>
        public static string GetDeviceInfoString()
        {
            return $@"Device: {DeviceModel}
OS: {OperatingSystem}
CPU: {Processor} ({ProcessorCount} cores)
RAM: {SystemMemoryMB} MB
GPU: {GraphicsDevice} ({GraphicsMemoryMB} MB)
Screen: {Screen.width}x{Screen.height} @ {ScreenDPI} DPI
Battery: {(BatteryLevel >= 0 ? $"{BatteryLevel:P0}" : "N/A")} ({BatteryStatus})";
        }

        /// <summary>
        /// Get app info as a formatted string.
        /// </summary>
        public static string GetAppInfoString()
        {
            return $@"App: {ProductName}
Version: {GetFullVersionString()}
Bundle: {BundleId}
Platform: {Platform}
Unity: {UnityVersion}";
        }

        /// <summary>
        /// Check if device meets minimum requirements.
        /// </summary>
        public static bool MeetsRequirements(int minMemoryMB = 1024, int minProcessorCount = 2)
        {
            return SystemMemoryMB >= minMemoryMB && ProcessorCount >= minProcessorCount;
        }

        /// <summary>
        /// Get the persistent data path for this app.
        /// </summary>
        public static string PersistentDataPath => Application.persistentDataPath;

        /// <summary>
        /// Get the streaming assets path.
        /// </summary>
        public static string StreamingAssetsPath => Application.streamingAssetsPath;

        /// <summary>
        /// Get the temporary cache path.
        /// </summary>
        public static string TemporaryCachePath => Application.temporaryCachePath;
    }
}
