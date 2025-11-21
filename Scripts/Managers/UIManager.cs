using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Manages UI panels, popups, and transitions.
    /// Singleton autoload node.
    /// </summary>
    public partial class UIManager : Node
    {
        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static UIManager Instance { get; private set; }

        [Signal]
        public delegate void MenuOpenedEventHandler(string menuName);

        [Signal]
        public delegate void MenuClosedEventHandler(string menuName);

        private readonly Stack<Control> _menuStack = new();
        private Control _currentMenu;

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
        /// Opens a menu and adds it to the stack.
        /// </summary>
        public void OpenMenu(Control menu)
        {
            if (menu == null)
            {
                GD.PrintErr("[UIManager] Cannot open null menu");
                return;
            }

            // Hide current menu
            if (_currentMenu != null)
            {
                _menuStack.Push(_currentMenu);
                _currentMenu.Hide();
            }

            // Show new menu
            _currentMenu = menu;
            _currentMenu.Show();

            _ = EmitSignal(SignalName.MenuOpened, menu.Name);
            GD.Print($"[UIManager] Opened menu: {menu.Name}");
        }

        /// <summary>
        /// Closes current menu and returns to previous.
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

            // Return to previous menu
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
        /// Closes all menus and clears stack.
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

        /// <summary>
        /// Shows a simple notification popup.
        /// </summary>
        public static void ShowNotification(string message, float duration = 3.0f)
        {
            GD.Print($"[UIManager] Notification: {message}");
            // TODO: Implement notification popup system
        }
    }
}
