namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Interface for loading screen implementations.
    /// </summary>
    public interface ILoadingScreen
    {
        /// <summary>
        /// Whether the loading screen is currently visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Show the loading screen.
        /// </summary>
        void Show();

        /// <summary>
        /// Hide the loading screen.
        /// </summary>
        void Hide();

        /// <summary>
        /// Set the loading progress (0-1).
        /// </summary>
        void SetProgress(float progress);

        /// <summary>
        /// Set the loading message/status text.
        /// </summary>
        void SetMessage(string message);

        /// <summary>
        /// Set both progress and message.
        /// </summary>
        void SetStatus(float progress, string message);
    }
}
