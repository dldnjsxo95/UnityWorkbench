using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Groups menu buttons for navigation.
    /// Supports vertical, horizontal, and grid layouts.
    /// </summary>
    public class MenuGroup : MonoBehaviour
    {
        /// <summary>
        /// Navigation mode for the menu group.
        /// </summary>
        public enum NavigationMode
        {
            Vertical,
            Horizontal,
            Grid
        }

        [Header("Navigation Settings")]
        [SerializeField] private NavigationMode _navigationMode = NavigationMode.Vertical;
        [SerializeField] private int _gridColumns = 4;
        [SerializeField] private bool _wrapAround = true;

        [Header("Buttons")]
        [SerializeField] private List<MenuButton> _buttons = new List<MenuButton>();
        [SerializeField] private bool _autoCollectButtons = true;

        private int _currentIndex = 0;

        /// <summary>
        /// Navigation mode for this group.
        /// </summary>
        public NavigationMode Mode => _navigationMode;

        /// <summary>
        /// All buttons in this group.
        /// </summary>
        public IReadOnlyList<MenuButton> Buttons => _buttons;

        /// <summary>
        /// Currently selected button index.
        /// </summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>
        /// Currently selected button.
        /// </summary>
        public MenuButton CurrentButton => _currentIndex >= 0 && _currentIndex < _buttons.Count
            ? _buttons[_currentIndex]
            : null;

        /// <summary>
        /// Number of navigable buttons.
        /// </summary>
        public int NavigableCount
        {
            get
            {
                int count = 0;
                foreach (var button in _buttons)
                {
                    if (button != null && button.IsNavigable)
                        count++;
                }
                return count;
            }
        }

        private void Awake()
        {
            if (_autoCollectButtons)
            {
                CollectButtons();
            }
        }

        private void OnEnable()
        {
            // Select first navigable button
            SelectFirstNavigable();
        }

        /// <summary>
        /// Collect all MenuButton children.
        /// </summary>
        public void CollectButtons()
        {
            _buttons.Clear();
            GetComponentsInChildren(true, _buttons);
        }

        /// <summary>
        /// Add a button to the group.
        /// </summary>
        public void AddButton(MenuButton button)
        {
            if (button != null && !_buttons.Contains(button))
            {
                _buttons.Add(button);
            }
        }

        /// <summary>
        /// Remove a button from the group.
        /// </summary>
        public void RemoveButton(MenuButton button)
        {
            _buttons.Remove(button);
        }

        /// <summary>
        /// Get the neighbor button in the specified direction.
        /// </summary>
        public MenuButton GetNeighbor(MenuButton current, Vector2 direction)
        {
            if (current == null || _buttons.Count == 0)
                return null;

            int currentIndex = _buttons.IndexOf(current);
            if (currentIndex < 0)
                return null;

            int targetIndex = GetTargetIndex(currentIndex, direction);
            return targetIndex >= 0 ? _buttons[targetIndex] : null;
        }

        /// <summary>
        /// Navigate in the specified direction.
        /// Returns the new selected button.
        /// </summary>
        public MenuButton Navigate(Vector2 direction)
        {
            if (_buttons.Count == 0)
                return null;

            int targetIndex = GetTargetIndex(_currentIndex, direction);

            if (targetIndex >= 0 && targetIndex != _currentIndex)
            {
                // Deselect current
                if (_currentIndex >= 0 && _currentIndex < _buttons.Count)
                {
                    _buttons[_currentIndex]?.OnNavigateFrom();
                }

                // Select new
                _currentIndex = targetIndex;
                _buttons[_currentIndex]?.OnNavigateTo();
            }

            return CurrentButton;
        }

        /// <summary>
        /// Select a specific button.
        /// </summary>
        public void SelectButton(MenuButton button)
        {
            int index = _buttons.IndexOf(button);
            if (index < 0) return;

            // Deselect current
            if (_currentIndex >= 0 && _currentIndex < _buttons.Count && _currentIndex != index)
            {
                _buttons[_currentIndex]?.OnNavigateFrom();
            }

            _currentIndex = index;
            _buttons[_currentIndex]?.OnNavigateTo();
        }

        /// <summary>
        /// Select the first navigable button.
        /// </summary>
        public void SelectFirstNavigable()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i] != null && _buttons[i].IsNavigable)
                {
                    SelectButton(_buttons[i]);
                    return;
                }
            }
        }

        /// <summary>
        /// Clear selection.
        /// </summary>
        public void ClearSelection()
        {
            if (_currentIndex >= 0 && _currentIndex < _buttons.Count)
            {
                _buttons[_currentIndex]?.OnNavigateFrom();
            }
            _currentIndex = -1;
        }

        private int GetTargetIndex(int fromIndex, Vector2 direction)
        {
            if (_buttons.Count == 0) return -1;

            // Normalize direction
            float horizontal = direction.x;
            float vertical = direction.y;

            int targetIndex = fromIndex;

            switch (_navigationMode)
            {
                case NavigationMode.Vertical:
                    if (vertical > 0.5f) // Up
                        targetIndex = FindNextNavigable(fromIndex, -1);
                    else if (vertical < -0.5f) // Down
                        targetIndex = FindNextNavigable(fromIndex, 1);
                    break;

                case NavigationMode.Horizontal:
                    if (horizontal > 0.5f) // Right
                        targetIndex = FindNextNavigable(fromIndex, 1);
                    else if (horizontal < -0.5f) // Left
                        targetIndex = FindNextNavigable(fromIndex, -1);
                    break;

                case NavigationMode.Grid:
                    if (vertical > 0.5f) // Up
                        targetIndex = FindNextNavigableGrid(fromIndex, -_gridColumns);
                    else if (vertical < -0.5f) // Down
                        targetIndex = FindNextNavigableGrid(fromIndex, _gridColumns);
                    else if (horizontal > 0.5f) // Right
                        targetIndex = FindNextNavigable(fromIndex, 1);
                    else if (horizontal < -0.5f) // Left
                        targetIndex = FindNextNavigable(fromIndex, -1);
                    break;
            }

            return targetIndex;
        }

        private int FindNextNavigable(int fromIndex, int direction)
        {
            int count = _buttons.Count;
            int index = fromIndex;

            for (int i = 0; i < count; i++)
            {
                index += direction;

                if (_wrapAround)
                {
                    if (index < 0) index = count - 1;
                    else if (index >= count) index = 0;
                }
                else
                {
                    if (index < 0 || index >= count)
                        return fromIndex;
                }

                if (_buttons[index] != null && _buttons[index].IsNavigable)
                    return index;
            }

            return fromIndex;
        }

        private int FindNextNavigableGrid(int fromIndex, int offset)
        {
            int targetIndex = fromIndex + offset;

            if (_wrapAround)
            {
                while (targetIndex < 0) targetIndex += _buttons.Count;
                while (targetIndex >= _buttons.Count) targetIndex -= _buttons.Count;
            }
            else
            {
                if (targetIndex < 0 || targetIndex >= _buttons.Count)
                    return fromIndex;
            }

            if (_buttons[targetIndex] != null && _buttons[targetIndex].IsNavigable)
                return targetIndex;

            return fromIndex;
        }
    }
}
