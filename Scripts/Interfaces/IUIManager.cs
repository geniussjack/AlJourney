using Godot;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for managing the global user interface.
    /// Responsible for opening, closing and managing the menus currently active on screen.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// Opens the given menu, making it active on screen.
        /// </summary>
        /// <param name="menu">The UI node to open.</param>
        void OpenMenu(Control menu);

        /// <summary>
        /// Closes the topmost menu.
        /// </summary>
        void CloseCurrentMenu();

        /// <summary>
        /// Closes every currently open menu, fully clearing the screen of UI windows.
        /// </summary>
        void CloseAllMenus();
    }
}
