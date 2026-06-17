using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер пользовательского интерфейса. Отвечает за открытие, закрытие и управление стеком экранов меню.
    /// </summary>
    public partial class UIManager : Node, IUIManager
    {
        /// <summary>
        /// Глобальный экземпляр менеджера интерфейса.
        /// </summary>
        public static UIManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Событие, вызываемое при открытии нового меню.
        /// </summary>
        /// <param name="menuName">Имя открытого меню.</param>
        public delegate void MenuOpenedEventHandler(string menuName);

        [Signal]
        /// <summary>
        /// Событие, вызываемое при закрытии меню.
        /// </summary>
        /// <param name="menuName">Имя закрытого меню.</param>
        public delegate void MenuClosedEventHandler(string menuName);

        private readonly Stack<Control> _menuStack = new();
        private Control _currentMenu;

        /// <summary>
        /// Инициализирует менеджер интерфейса при добавлении в дерево сцены.
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
        /// Открывает указанное меню, скрывая текущее и добавляя его в стек возврата.
        /// </summary>
        /// <param name="menu">Контрол меню, которое нужно открыть.</param>
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
        /// Закрывает текущее активное меню и возвращает на экран предыдущее меню из стека.
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
        /// Закрывает все открытые меню и очищает стек возврата.
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
