using UnityEngine;

namespace LWT.UnityWorkbench.Localization
{
    /// <summary>
    /// Static utility class for quick localization access.
    /// </summary>
    public static class L
    {
        /// <summary>
        /// Get a localized string by key.
        /// Shorthand for LocalizationManager.Instance.Get(key)
        /// </summary>
        public static string Get(string key)
        {
            if (LocalizationManager.HasInstance)
            {
                return LocalizationManager.Instance.Get(key);
            }
            return $"[{key}]";
        }

        /// <summary>
        /// Get a localized string with format arguments.
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            if (LocalizationManager.HasInstance)
            {
                return LocalizationManager.Instance.Get(key, args);
            }
            return $"[{key}]";
        }

        /// <summary>
        /// Check if a key exists.
        /// </summary>
        public static bool Has(string key)
        {
            return LocalizationManager.HasInstance && LocalizationManager.Instance.HasKey(key);
        }

        /// <summary>
        /// Get current language code.
        /// </summary>
        public static string CurrentLanguage =>
            LocalizationManager.HasInstance ? LocalizationManager.Instance.CurrentLanguage : "en";

        /// <summary>
        /// Set the current language.
        /// </summary>
        public static void SetLanguage(string languageCode)
        {
            if (LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.SetLanguage(languageCode);
            }
        }
    }

    /// <summary>
    /// Localization database ScriptableObject for editor workflow.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "UnityWorkbench/Localization/Database")]
    public class LocalizationDatabase : ScriptableObject
    {
        [Header("Languages")]
        public LanguageData[] Languages = new[]
        {
            new LanguageData { Code = "en", DisplayName = "English", NativeName = "English", IsRTL = false },
            new LanguageData { Code = "ko", DisplayName = "Korean", NativeName = "한국어", IsRTL = false },
            new LanguageData { Code = "ja", DisplayName = "Japanese", NativeName = "日本語", IsRTL = false },
            new LanguageData { Code = "zh-CN", DisplayName = "Chinese (Simplified)", NativeName = "简体中文", IsRTL = false },
            new LanguageData { Code = "es", DisplayName = "Spanish", NativeName = "Español", IsRTL = false },
            new LanguageData { Code = "fr", DisplayName = "French", NativeName = "Français", IsRTL = false },
            new LanguageData { Code = "de", DisplayName = "German", NativeName = "Deutsch", IsRTL = false },
        };

        [Header("Keys")]
        public LocalizationKeyEntry[] Keys;

        /// <summary>
        /// Export language data to JSON.
        /// </summary>
        public string ExportToJson(string languageCode)
        {
            var data = new LocalizationData();
            var entries = new System.Collections.Generic.List<LocalizationEntry>();

            foreach (var key in Keys)
            {
                var value = key.GetValue(languageCode);
                if (!string.IsNullOrEmpty(value))
                {
                    entries.Add(new LocalizationEntry { key = key.Key, value = value });
                }
            }

            data.entries = entries.ToArray();
            return JsonUtility.ToJson(data, true);
        }
    }

    [System.Serializable]
    public class LocalizationKeyEntry
    {
        public string Key;
        public string Description;
        public LocalizationValue[] Values;

        public string GetValue(string languageCode)
        {
            if (Values == null) return string.Empty;

            foreach (var val in Values)
            {
                if (val.LanguageCode == languageCode)
                {
                    return val.Value;
                }
            }

            // Fallback to English
            foreach (var val in Values)
            {
                if (val.LanguageCode == "en")
                {
                    return val.Value;
                }
            }

            return string.Empty;
        }
    }

    [System.Serializable]
    public class LocalizationValue
    {
        public string LanguageCode;
        public string Value;
    }
}
