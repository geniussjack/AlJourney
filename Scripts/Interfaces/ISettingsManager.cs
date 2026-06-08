using Godot;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления настройками игры.
    /// </summary>
    /// <summary>
    /// Менеджер ISettingsManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface ISettingsManager
    {
        Vector2I Resolution { get; }
        bool Fullscreen { get; }
        bool VSync { get; }
        int MaxFps { get; }
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }

        void SetResolution(Vector2I resolution, bool applyImmediately = true);
        void SetFullscreen(bool enabled, bool applyImmediately = true);
        void SetVSync(bool enabled, bool applyImmediately = true);
        void SetMaxFps(int fps, bool applyImmediately = true);
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
        void ApplyVideoSettings();
        void SaveSettings();
        void ResetToDefaults();
    }
}
