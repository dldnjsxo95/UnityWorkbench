using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LWT.UnityWorkbench.Localization
{
    /// <summary>
    /// Component that automatically localizes UI text elements.
    /// Attach to GameObjects with Text or TMP_Text components.
    /// </summary>
    [AddComponentMenu("UnityWorkbench/Localization/Localized Text")]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization")]
        [SerializeField] private string _localizationKey;
        [SerializeField] private bool _updateOnLanguageChange = true;

        [Header("Formatting")]
        [SerializeField] private bool _useFormatting = false;
        [SerializeField] private string[] _formatArgs;

        private Text _legacyText;
        private TMP_Text _tmpText;

        /// <summary>Localization key for this text.</summary>
        public string LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                UpdateText();
            }
        }

        private void Awake()
        {
            _legacyText = GetComponent<Text>();
            _tmpText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (_updateOnLanguageChange && LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            UpdateText();
        }

        private void OnDisable()
        {
            if (LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(string language)
        {
            UpdateText();
        }

        /// <summary>
        /// Update the text with current localization.
        /// </summary>
        public void UpdateText()
        {
            if (string.IsNullOrEmpty(_localizationKey)) return;
            if (!LocalizationManager.HasInstance) return;

            string localizedText;

            if (_useFormatting && _formatArgs != null && _formatArgs.Length > 0)
            {
                localizedText = LocalizationManager.Instance.Get(_localizationKey, _formatArgs);
            }
            else
            {
                localizedText = LocalizationManager.Instance.Get(_localizationKey);
            }

            SetText(localizedText);
        }

        /// <summary>
        /// Update text with format arguments.
        /// </summary>
        public void UpdateText(params object[] args)
        {
            if (string.IsNullOrEmpty(_localizationKey)) return;
            if (!LocalizationManager.HasInstance) return;

            var localizedText = LocalizationManager.Instance.Get(_localizationKey, args);
            SetText(localizedText);
        }

        /// <summary>
        /// Set format arguments and update text.
        /// </summary>
        public void SetFormatArgs(params string[] args)
        {
            _formatArgs = args;
            _useFormatting = args != null && args.Length > 0;
            UpdateText();
        }

        private void SetText(string text)
        {
            if (_tmpText != null)
            {
                _tmpText.text = text;
            }
            else if (_legacyText != null)
            {
                _legacyText.text = text;
            }
        }
    }

    /// <summary>
    /// Component for localizing dropdown options.
    /// </summary>
    [AddComponentMenu("UnityWorkbench/Localization/Localized Dropdown")]
    public class LocalizedDropdown : MonoBehaviour
    {
        [SerializeField] private string[] _optionKeys;

        private TMP_Dropdown _tmpDropdown;
        private Dropdown _legacyDropdown;

        private void Awake()
        {
            _tmpDropdown = GetComponent<TMP_Dropdown>();
            _legacyDropdown = GetComponent<Dropdown>();
        }

        private void OnEnable()
        {
            if (LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            UpdateOptions();
        }

        private void OnDisable()
        {
            if (LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(string language)
        {
            UpdateOptions();
        }

        /// <summary>
        /// Update dropdown options with localized text.
        /// </summary>
        public void UpdateOptions()
        {
            if (_optionKeys == null || _optionKeys.Length == 0) return;
            if (!LocalizationManager.HasInstance) return;

            if (_tmpDropdown != null)
            {
                var currentValue = _tmpDropdown.value;
                _tmpDropdown.ClearOptions();

                var options = new System.Collections.Generic.List<string>();
                foreach (var key in _optionKeys)
                {
                    options.Add(LocalizationManager.Instance.Get(key));
                }

                _tmpDropdown.AddOptions(options);
                _tmpDropdown.value = currentValue;
            }
            else if (_legacyDropdown != null)
            {
                var currentValue = _legacyDropdown.value;
                _legacyDropdown.ClearOptions();

                var options = new System.Collections.Generic.List<string>();
                foreach (var key in _optionKeys)
                {
                    options.Add(LocalizationManager.Instance.Get(key));
                }

                _legacyDropdown.AddOptions(options);
                _legacyDropdown.value = currentValue;
            }
        }

        /// <summary>
        /// Set option keys and update.
        /// </summary>
        public void SetOptionKeys(string[] keys)
        {
            _optionKeys = keys;
            UpdateOptions();
        }
    }
}
