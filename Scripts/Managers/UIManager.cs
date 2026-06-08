using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер UIManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class UIManager : Node, IUIManager
    {
        public static UIManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Элемент MenuOpenedEventHandler.
        /// </summary>
        public delegate void MenuOpenedEventHandler(string menuName);

        [Signal]
        /// <summary>
        /// Элемент MenuClosedEventHandler.
        /// </summary>
        public delegate void MenuClosedEventHandler(string menuName);

        private readonly Stack<Control> _menuStack = new();
        private Control _currentMenu;

        /// <summary>
        /// Элемент _Ready.
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
        /// Открывает Menu.
        /// </summary>
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
        /// Закрывает CurrentMenu.
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
        /// Закрывает AllMenus.
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
