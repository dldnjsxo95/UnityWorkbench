namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Interface for menu items that can be navigated with keyboard/gamepad.
    /// </summary>
    public interface IMenuNavigable
    {
        /// <summary>
        /// Whether this item can currently be navigated to.
        /// </summary>
        bool IsNavigable { get; }

        /// <summary>
        /// Called when navigation focus moves to this item.
        /// </summary>
        void OnNavigateTo();

        /// <summary>
        /// Called when navigation focus leaves this item.
        /// </summary>
        void OnNavigateFrom();

        /// <summary>
        /// Called when submit/confirm is pressed on this item.
        /// </summary>
        void OnSubmit();

        /// <summary>
        /// Called when cancel/back is pressed on this item.
        /// Return true if handled, false to pass to navigator.
        /// </summary>
        bool OnCancel();
    }
}
