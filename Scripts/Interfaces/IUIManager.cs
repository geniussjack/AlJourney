using Godot;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления глобальным UI.
    /// </summary>
    /// <summary>
    /// Менеджер IUIManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface IUIManager
    {
        void OpenMenu(Control menu);
        void CloseCurrentMenu();
        void CloseAllMenus();
    }
}
