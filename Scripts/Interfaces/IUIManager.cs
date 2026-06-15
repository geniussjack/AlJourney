using Godot;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления глобальным пользовательским интерфейсом.
    /// Отвечает за открытие, закрытие и управление активными меню на экране.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// Открывает указанное меню, делая его активным на экране.
        /// </summary>
        /// <param name="menu">Узел интерфейса, который необходимо открыть.</param>
        void OpenMenu(Control menu);

        /// <summary>
        /// Закрывает самое верхнее меню.
        /// </summary>
        void CloseCurrentMenu();

        /// <summary>
        /// Закрывает все открытые в данный момент меню, полностью очищая экран от окон интерфейса.
        /// </summary>
        void CloseAllMenus();
    }
}
