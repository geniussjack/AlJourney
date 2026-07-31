using Godot;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for managing game settings.
    /// Provides access to video and audio parameters, as well as methods to change and save them.
    /// </summary>
    public interface ISettingsManager
    {
        /// <summary>
        /// The current screen resolution.
        /// </summary>
        Vector2I Resolution { get; }

        /// <summary>
        /// Window mode (0 = Fullscreen, 1 = Borderless, 2 = Windowed).
        /// </summary>
        int WindowMode { get; }

        /// <summary>
        /// The game's current language (en or ru).
        /// </summary>
        string Language { get; }

        /// <summary>
        /// The maximum frames-per-second cap.
        /// </summary>
        int MaxFps { get; }

        /// <summary>
        /// The overall volume of all sounds in the game.
        /// </summary>
        float MasterVolume { get; }

        /// <summary>
        /// The background music volume level.
        /// </summary>
        float MusicVolume { get; }

        /// <summary>
        /// The sound effects volume level.
        /// </summary>
        float SfxVolume { get; }

        /// <summary>
        /// Sets a new screen resolution.
        /// </summary>
        /// <param name="resolution">The new screen resolution.</param>
        /// <param name="applyImmediately">Whether to apply the setting immediately.</param>
        void SetResolution(Vector2I resolution, bool applyImmediately = true);

        /// <summary>
        /// Sets the window mode.
        /// </summary>
        /// <param name="mode">0 - Fullscreen, 1 - Borderless, 2 - Windowed.</param>
        /// <param name="applyImmediately">Whether to apply the setting immediately.</param>
        void SetWindowMode(int mode, bool applyImmediately = true);

        /// <summary>
        /// Changes the game's language.
        /// </summary>
        /// <param name="lang">The language code (e.g. "en" or "ru").</param>
        /// <param name="applyImmediately">Whether to apply the setting immediately.</param>
        void SetLanguage(string lang, bool applyImmediately = true);

        /// <summary>
        /// Sets the maximum frames-per-second limit.
        /// </summary>
        /// <param name="fps">The maximum FPS.</param>
        /// <param name="applyImmediately">Whether to apply the setting immediately.</param>
        void SetMaxFps(int fps, bool applyImmediately = true);

        /// <summary>
        /// Sets the overall (master) volume level.
        /// </summary>
        void SetMasterVolume(float volume);

        /// <summary>
        /// Sets the background music volume level.
        /// </summary>
        void SetMusicVolume(float volume);

        /// <summary>
        /// Sets the sound effects volume level.
        /// </summary>
        void SetSfxVolume(float volume);

        /// <summary>
        /// Applies all pending video setting changes in the Godot engine.
        /// </summary>
        void ApplyVideoSettings();

        /// <summary>
        /// Saves the current settings configuration to a file.
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Loads settings from a file.
        /// </summary>
        void LoadSettings();

        /// <summary>
        /// Resets all game settings to their default values.
        /// </summary>
        void ResetToDefaults();
    }
}
