using System;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Interface for UI screen management.
    /// Provides stack-based screen navigation.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// Currently active screen on top of the stack.
        /// </summary>
        UIScreen CurrentScreen { get; }

        /// <summary>
        /// Number of screens in the stack.
        /// </summary>
        int StackCount { get; }

        /// <summary>
        /// Whether navigation can go back (stack has more than one screen).
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Show a screen by type. Pushes to stack.
        /// </summary>
        void ShowScreen<T>(bool hideCurrentScreen = true) where T : UIScreen;

        /// <summary>
        /// Show a screen by name. Pushes to stack.
        /// </summary>
        void ShowScreen(string screenName, bool hideCurrentScreen = true);

        /// <summary>
        /// Hide the current screen without removing from stack.
        /// </summary>
        void HideCurrentScreen();

        /// <summary>
        /// Go back to previous screen. Pops current screen from stack.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Clear all screens from stack except the bottom one.
        /// </summary>
        void ClearToRoot();

        /// <summary>
        /// Clear all screens from stack.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Event fired when a screen is shown.
        /// </summary>
        event Action<UIScreen> OnScreenShown;

        /// <summary>
        /// Event fired when a screen is hidden.
        /// </summary>
        event Action<UIScreen> OnScreenHidden;
    }
}
