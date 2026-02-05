using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Menu button that supports keyboard/gamepad navigation.
    /// Provides visual feedback for selection state.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MenuButton : MonoBehaviour, IMenuNavigable, IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [Header("Button Settings")]
        [SerializeField] private bool _interactable = true;
        [SerializeField] private bool _navigable = true;

        [Header("UI References")]
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _selectedIndicator;

        [Header("Visual Feedback")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = new Color(0.9f, 0.9f, 0.9f);
        [SerializeField] private Color _pressedColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Scale Animation")]
        [SerializeField] private bool _useScaleAnimation = true;
        [SerializeField] private float _normalScale = 1f;
        [SerializeField] private float _selectedScale = 1.05f;
        [SerializeField] private float _scaleTransitionSpeed = 10f;

        [Header("Events")]
        [SerializeField] private UnityEvent _onSelected;
        [SerializeField] private UnityEvent _onDeselected;
        [SerializeField] private UnityEvent _onSubmit;
        [SerializeField] private UnityEvent _onClick;

        private RectTransform _rectTransform;
        private bool _isSelected;
        private bool _isPointerOver;
        private float _currentScale;
        private float _targetScale;

        /// <summary>
        /// Whether this button is currently selected/focused.
        /// </summary>
        public bool IsSelected => _isSelected;

        /// <summary>
        /// Whether this button can be interacted with.
        /// </summary>
        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Whether this button can be navigated to.
        /// </summary>
        public bool IsNavigable => _navigable && _interactable && gameObject.activeInHierarchy;

        /// <summary>
        /// Button label text.
        /// </summary>
        public string Label
        {
            get => _labelText != null ? _labelText.text : string.Empty;
            set { if (_labelText != null) _labelText.text = value; }
        }

        /// <summary>
        /// Event fired when button is submitted/clicked.
        /// </summary>
        public event Action OnButtonSubmit;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _currentScale = _normalScale;
            _targetScale = _normalScale;
        }

        private void OnEnable()
        {
            UpdateVisuals();
        }

        private void Update()
        {
            if (_useScaleAnimation)
            {
                if (!Mathf.Approximately(_currentScale, _targetScale))
                {
                    _currentScale = Mathf.MoveTowards(_currentScale, _targetScale,
                        _scaleTransitionSpeed * Time.unscaledDeltaTime);
                    transform.localScale = Vector3.one * _currentScale;
                }
            }
        }

        #region IMenuNavigable Implementation

        public void OnNavigateTo()
        {
            if (!_interactable) return;

            _isSelected = true;
            _targetScale = _selectedScale;
            UpdateVisuals();

            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(true);

            _onSelected?.Invoke();
        }

        public void OnNavigateFrom()
        {
            _isSelected = false;
            _targetScale = _normalScale;
            UpdateVisuals();

            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(false);

            _onDeselected?.Invoke();
        }

        public void OnSubmit()
        {
            if (!_interactable) return;

            _onSubmit?.Invoke();
            _onClick?.Invoke();
            OnButtonSubmit?.Invoke();
        }

        public bool OnCancel()
        {
            // Default: don't handle cancel, pass to navigator
            return false;
        }

        #endregion

        #region Unity Event Handlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable) return;

            _isPointerOver = true;

            // Notify navigator if available
            var navigator = GetComponentInParent<MenuNavigator>();
            navigator?.SelectButton(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable) return;

            OnSubmit();
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnNavigateTo();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            OnNavigateFrom();
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            OnSubmit();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Programmatically select this button.
        /// </summary>
        public void Select()
        {
            OnNavigateTo();
        }

        /// <summary>
        /// Programmatically deselect this button.
        /// </summary>
        public void Deselect()
        {
            OnNavigateFrom();
        }

        /// <summary>
        /// Set the button icon.
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.gameObject.SetActive(icon != null);
            }
        }

        #endregion

        #region Private Methods

        private void UpdateVisuals()
        {
            if (_backgroundImage != null)
            {
                if (!_interactable)
                {
                    _backgroundImage.color = _disabledColor;
                }
                else if (_isSelected || _isPointerOver)
                {
                    _backgroundImage.color = _selectedColor;
                }
                else
                {
                    _backgroundImage.color = _normalColor;
                }
            }

            if (_labelText != null)
            {
                _labelText.color = _interactable ? Color.white : _disabledColor;
            }
        }

        #endregion
    }
}
