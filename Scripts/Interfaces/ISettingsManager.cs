using Godot;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления настройками игры.
    /// Предоставляет доступ к параметрам графики и звука, а также методы для их изменения и сохранения.
    /// </summary>
    public interface ISettingsManager
    {
        /// <summary>
        /// Текущее разрешение экрана.
        /// </summary>
        Vector2I Resolution { get; }

        /// <summary>
        /// Флаг полноэкранного режима. Значение true означает, что игра работает на весь экран.
        /// </summary>
        bool Fullscreen { get; }

        /// <summary>
        /// Текущий язык игры (en или ru).
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Максимальное ограничение кадров в секунду.
        /// </summary>
        int MaxFps { get; }

        /// <summary>
        /// Общая громкость всех звуков в игре.
        /// </summary>
        float MasterVolume { get; }

        /// <summary>
        /// Уровень громкости фоновой музыки.
        /// </summary>
        float MusicVolume { get; }

        /// <summary>
        /// Уровень громкости звуковых эффектов.
        /// </summary>
        float SfxVolume { get; }

        /// <summary>
        /// Устанавливает новое разрешение экрана.
        /// </summary>
        /// <param name="resolution">Новое разрешение экрана.</param>
        /// <param name="applyImmediately">Применить настройки немедленно.</param>
        void SetResolution(Vector2I resolution, bool applyImmediately = true);

        /// <summary>
        /// Включает или отключает полноэкранный режим.
        /// </summary>
        /// <param name="enabled">True - включить полный экран, False - оконный режим.</param>
        /// <param name="applyImmediately">Применить настройки немедленно.</param>
        void SetFullscreen(bool enabled, bool applyImmediately = true);

        /// <summary>
        /// Изменяет язык игры.
        /// </summary>
        /// <param name="lang">Код языка (например, "en" или "ru").</param>
        /// <param name="applyImmediately">Применить настройки немедленно.</param>
        void SetLanguage(string lang, bool applyImmediately = true);

        /// <summary>
        /// Устанавливает лимит максимального количества кадров в секунду.
        /// </summary>
        /// <param name="fps">Максимальный FPS.</param>
        /// <param name="applyImmediately">Применить настройки немедленно.</param>
        void SetMaxFps(int fps, bool applyImmediately = true);

        /// <summary>
        /// Устанавливает уровень общей громкости.
        /// </summary>
        void SetMasterVolume(float volume);

        /// <summary>
        /// Устанавливает уровень громкости фоновой музыки.
        /// </summary>
        void SetMusicVolume(float volume);

        /// <summary>
        /// Устанавливает уровень громкости звуковых эффектов.
        /// </summary>
        void SetSfxVolume(float volume);

        /// <summary>
        /// Применяет все изменения видео-настроек в движке Godot.
        /// </summary>
        void ApplyVideoSettings();

        /// <summary>
        /// Сохраняет текущие конфигурации настроек в файл.
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Сбрасывает все настройки игры до их значений по умолчанию.
        /// </summary>
        void ResetToDefaults();
    }
}
