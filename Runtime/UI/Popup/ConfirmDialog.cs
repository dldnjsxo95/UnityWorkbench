using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Confirmation dialog popup with title, message, and confirm/cancel buttons.
    /// Can also be used as a simple alert dialog.
    /// </summary>
    public class ConfirmDialog : PopupBase
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TMP_Text _confirmButtonText;
        [SerializeField] private TMP_Text _cancelButtonText;

        [Header("Default Texts")]
        [SerializeField] private string _defaultConfirmText = "Confirm";
        [SerializeField] private string _defaultCancelText = "Cancel";
        [SerializeField] private string _defaultOkText = "OK";

        private Action _onConfirm;
        private Action _onCancel;
        private bool _isAlertMode;

        protected override void Awake()
        {
            base.Awake();

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        /// <summary>
        /// Setup the dialog with confirm/cancel callbacks.
        /// </summary>
        public void Setup(string title, string message, Action onConfirm, Action onCancel = null)
        {
            _isAlertMode = false;

            if (_titleText != null)
                _titleText.text = title;

            if (_messageText != null)
                _messageText.text = message;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            // Show both buttons
            if (_confirmButton != null)
            {
                _confirmButton.gameObject.SetActive(true);
                if (_confirmButtonText != null)
                    _confirmButtonText.text = _defaultConfirmText;
            }

            if (_cancelButton != null)
            {
                _cancelButton.gameObject.SetActive(true);
                if (_cancelButtonText != null)
                    _cancelButtonText.text = _defaultCancelText;
            }
        }

        /// <summary>
        /// Setup the dialog with custom button texts.
        /// </summary>
        public void Setup(string title, string message, string confirmText, string cancelText,
            Action onConfirm, Action onCancel = null)
        {
            Setup(title, message, onConfirm, onCancel);

            if (_confirmButtonText != null)
                _confirmButtonText.text = confirmText;

            if (_cancelButtonText != null)
                _cancelButtonText.text = cancelText;
        }

        /// <summary>
        /// Setup as a simple alert dialog with only an OK button.
        /// </summary>
        public void SetupAlert(string title, string message, Action onClose = null)
        {
            _isAlertMode = true;

            if (_titleText != null)
                _titleText.text = title;

            if (_messageText != null)
                _messageText.text = message;

            _onConfirm = onClose;
            _onCancel = null;

            // Show only confirm button as OK
            if (_confirmButton != null)
            {
                _confirmButton.gameObject.SetActive(true);
                if (_confirmButtonText != null)
                    _confirmButtonText.text = _defaultOkText;
            }

            if (_cancelButton != null)
            {
                _cancelButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Setup as alert with custom OK text.
        /// </summary>
        public void SetupAlert(string title, string message, string okText, Action onClose = null)
        {
            SetupAlert(title, message, onClose);

            if (_confirmButtonText != null)
                _confirmButtonText.text = okText;
        }

        private void OnConfirmClicked()
        {
            _onConfirm?.Invoke();
            Close();
        }

        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            Close();
        }

        public override void Close()
        {
            // If alert mode and closing via escape/background, treat as confirm
            if (_isAlertMode)
            {
                _onConfirm?.Invoke();
            }
            else
            {
                _onCancel?.Invoke();
            }

            _onConfirm = null;
            _onCancel = null;

            base.Close();
        }
    }
}
