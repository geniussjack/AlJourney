using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для системы сохранений.
    /// </summary>
    /// <summary>
    /// Менеджер ISaveSystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface ISaveSystem
    {
        bool SaveGame();
        SaveData LoadGame();
        bool DeleteSave();
        bool SaveFileExists();
        void AutoSave();
    }
}
