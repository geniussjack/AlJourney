using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для системы сохранений.
    /// Предоставляет функционал для сохранения и загрузки прогресса игрока в файл, а также автосохранения.
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// Сохраняет текущий прогресс игры в файл.
        /// Возвращает true в случае успешного сохранения.
        /// </summary>
        bool SaveGame();

        /// <summary>
        /// Загружает прогресс игрока из файла сохранения.
        /// Возвращает загруженные данные, либо null, если сохранение не найдено или повреждено.
        /// </summary>
        SaveData LoadGame();

        /// <summary>
        /// Удаляет текущий файл сохранения.
        /// Возвращает true в случае успешного удаления.
        /// </summary>
        bool DeleteSave();

        /// <summary>
        /// Проверяет, существует ли файл сохранения на устройстве.
        /// </summary>
        bool SaveFileExists();

        /// <summary>
        /// Выполняет автоматическое сохранение игры в фоновом режиме.
        /// </summary>
        void AutoSave();
    }
}
