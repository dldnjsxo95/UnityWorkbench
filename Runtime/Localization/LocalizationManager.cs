using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Localization
{
    /// <summary>
    /// Localization manager for multi-language support.
    /// Supports JSON-based language files and runtime language switching.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LocalizationManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[LocalizationManager]");
                        _instance = go.AddComponent<LocalizationManager>();
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        [Header("Settings")]
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private bool _useSystemLanguage = true;
        [SerializeField] private string _localizationFolder = "Localization";

        [Header("Languages")]
        [SerializeField] private List<LanguageData> _availableLanguages = new();

        private string _currentLanguage;
        private Dictionary<string, string> _localizedStrings = new();
        private Dictionary<string, Dictionary<string, string>> _cachedLanguages = new();

        /// <summary>Current active language code.</summary>
        public string CurrentLanguage => _currentLanguage;

        /// <summary>Available languages.</summary>
        public IReadOnlyList<LanguageData> AvailableLanguages => _availableLanguages;

        /// <summary>Event fired when language changes.</summary>
        public event Action<string> OnLanguageChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLanguage();
        }

        private void InitializeLanguage()
        {
            // Try to load saved language preference
            var savedLanguage = PlayerPrefs.GetString("Language", string.Empty);

            if (!string.IsNullOrEmpty(savedLanguage) && IsLanguageAvailable(savedLanguage))
            {
                SetLanguage(savedLanguage);
            }
            else if (_useSystemLanguage)
            {
                var systemLang = GetSystemLanguageCode();
                if (IsLanguageAvailable(systemLang))
                {
                    SetLanguage(systemLang);
                }
                else
                {
                    SetLanguage(_defaultLanguage);
                }
            }
            else
            {
                SetLanguage(_defaultLanguage);
            }
        }

        /// <summary>
        /// Set the current language.
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            if (_currentLanguage == languageCode) return;

            if (LoadLanguage(languageCode))
            {
                _currentLanguage = languageCode;
                PlayerPrefs.SetString("Language", languageCode);
                PlayerPrefs.Save();

                OnLanguageChanged?.Invoke(languageCode);
                Debug.Log($"[Localization] Language set to: {languageCode}");
            }
            else
            {
                Debug.LogWarning($"[Localization] Failed to load language: {languageCode}");
            }
        }

        /// <summary>
        /// Get a localized string by key.
        /// </summary>
        public string Get(string key)
        {
            if (_localizedStrings.TryGetValue(key, out var value))
            {
                return value;
            }

            Debug.LogWarning($"[Localization] Missing key: {key}");
            return $"[{key}]";
        }

        /// <summary>
        /// Get a localized string with format arguments.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            var format = Get(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }

        /// <summary>
        /// Check if a key exists.
        /// </summary>
        public bool HasKey(string key)
        {
            return _localizedStrings.ContainsKey(key);
        }

        /// <summary>
        /// Check if a language is available.
        /// </summary>
        public bool IsLanguageAvailable(string languageCode)
        {
            foreach (var lang in _availableLanguages)
            {
                if (lang.Code == languageCode) return true;
            }
            return false;
        }

        /// <summary>
        /// Get language display name.
        /// </summary>
        public string GetLanguageDisplayName(string languageCode)
        {
            foreach (var lang in _availableLanguages)
            {
                if (lang.Code == languageCode) return lang.DisplayName;
            }
            return languageCode;
        }

        private bool LoadLanguage(string languageCode)
        {
            // Check cache first
            if (_cachedLanguages.TryGetValue(languageCode, out var cached))
            {
                _localizedStrings = new Dictionary<string, string>(cached);
                return true;
            }

            // Load from Resources
            var path = $"{_localizationFolder}/{languageCode}";
            var textAsset = Resources.Load<TextAsset>(path);

            if (textAsset == null)
            {
                Debug.LogWarning($"[Localization] Language file not found: {path}");
                return false;
            }

            try
            {
                var data = JsonUtility.FromJson<LocalizationData>(textAsset.text);
                _localizedStrings.Clear();

                foreach (var entry in data.entries)
                {
                    _localizedStrings[entry.key] = entry.value;
                }

                // Cache the loaded language
                _cachedLanguages[languageCode] = new Dictionary<string, string>(_localizedStrings);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Localization] Failed to parse language file: {e.Message}");
                return false;
            }
        }

        private string GetSystemLanguageCode()
        {
            return Application.systemLanguage switch
            {
                SystemLanguage.English => "en",
                SystemLanguage.Korean => "ko",
                SystemLanguage.Japanese => "ja",
                SystemLanguage.Chinese => "zh",
                SystemLanguage.ChineseSimplified => "zh-CN",
                SystemLanguage.ChineseTraditional => "zh-TW",
                SystemLanguage.Spanish => "es",
                SystemLanguage.French => "fr",
                SystemLanguage.German => "de",
                SystemLanguage.Italian => "it",
                SystemLanguage.Portuguese => "pt",
                SystemLanguage.Russian => "ru",
                SystemLanguage.Arabic => "ar",
                SystemLanguage.Thai => "th",
                SystemLanguage.Vietnamese => "vi",
                _ => _defaultLanguage
            };
        }

        /// <summary>
        /// Add or update a localized string at runtime.
        /// </summary>
        public void SetString(string key, string value)
        {
            _localizedStrings[key] = value;
        }

        /// <summary>
        /// Clear language cache.
        /// </summary>
        public void ClearCache()
        {
            _cachedLanguages.Clear();
        }
    }

    [Serializable]
    public class LanguageData
    {
        public string Code;
        public string DisplayName;
        public string NativeName;
        public bool IsRTL;
    }

    [Serializable]
    public class LocalizationData
    {
        public LocalizationEntry[] entries;
    }

    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string value;
    }
}
