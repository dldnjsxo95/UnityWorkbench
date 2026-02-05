using System;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Interface for popup windows.
    /// </summary>
    public interface IPopup
    {
        /// <summary>
        /// Unique identifier for this popup instance.
        /// </summary>
        string PopupId { get; }

        /// <summary>
        /// Whether this popup blocks input to underlying UI (modal).
        /// </summary>
        bool IsModal { get; }

        /// <summary>
        /// Display priority. Higher values display on top.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Whether the popup is currently visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Show the popup.
        /// </summary>
        void Show();

        /// <summary>
        /// Hide the popup.
        /// </summary>
        void Hide();

        /// <summary>
        /// Close and destroy the popup.
        /// </summary>
        void Close();

        /// <summary>
        /// Event fired when popup is closed.
        /// </summary>
        event Action<IPopup> OnClosed;
    }
}
