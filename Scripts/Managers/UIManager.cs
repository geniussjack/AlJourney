using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// User interface manager. Responsible for opening, closing and managing the stack of menu screens.
    /// </summary>
    public partial class UIManager : Node, IUIManager
    {
        /// <summary>
        /// Global instance of the UI manager.
        /// </summary>
        public static UIManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Raised when a new menu is opened.
        /// </summary>
        /// <param name="menuName">The name of the opened menu.</param>
        public delegate void MenuOpenedEventHandler(string menuName);

        [Signal]
        /// <summary>
        /// Raised when a menu is closed.
        /// </summary>
        /// <param name="menuName">The name of the closed menu.</param>
        public delegate void MenuClosedEventHandler(string menuName);

        private readonly Stack<Control> _menuStack = new();
        private Control _currentMenu;

        /// <summary>
        /// Initializes the UI manager when added to the scene tree.
        /// </summary>
        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }

            Instance = this;
            GD.Print("[UIManager] Initialized");
        }

        /// <summary>
        /// Opens the given menu, hiding the current one and pushing it onto the back stack.
        /// </summary>
        /// <param name="menu">The menu control to open.</param>
        public void OpenMenu(Control menu)
        {
            if (menu == null)
            {
                GD.PrintErr("[UIManager] Cannot open null menu");
                return;
            }

            if (_currentMenu != null)
            {
                _menuStack.Push(_currentMenu);
                _currentMenu.Hide();
            }

            _currentMenu = menu;
            _currentMenu.Show();

            _ = EmitSignal(SignalName.MenuOpened, menu.Name);
            GD.Print($"[UIManager] Opened menu: {menu.Name}");
        }

        /// <summary>
        /// Closes the currently active menu and returns to the previous menu on the stack.
        /// </summary>
        public void CloseCurrentMenu()
        {
            if (_currentMenu == null)
            {
                GD.PrintErr("[UIManager] No menu to close");
                return;
            }

            string menuName = _currentMenu.Name;
            _currentMenu.Hide();

            _ = EmitSignal(SignalName.MenuClosed, menuName);
            GD.Print($"[UIManager] Closed menu: {menuName}");

            if (_menuStack.Count > 0)
            {
                _currentMenu = _menuStack.Pop();
                _currentMenu.Show();
            }
            else
            {
                _currentMenu = null;
            }
        }

        /// <summary>
        /// Closes every open menu and clears the back stack.
        /// </summary>
        public void CloseAllMenus()
        {
            _currentMenu?.Hide();
            _currentMenu = null;

            while (_menuStack.Count > 0)
            {
                Control menu = _menuStack.Pop();
                menu.Hide();
            }

            GD.Print("[UIManager] All menus closed");
        }
    }
}
