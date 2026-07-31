using AlJourney.Scripts.Interfaces;
using Godot;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Settings manager. Responsible for saving, loading and applying the game's video and audio settings.
    /// </summary>
    public partial class SettingsManager : Node, ISettingsManager
    {
        /// <summary>
        /// Global instance of the settings manager.
        /// </summary>
        public static SettingsManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Raised whenever any settings are changed and applied.
        /// </summary>
        public delegate void SettingsChangedEventHandler();

        private const string SettingsPath = "user://settings.cfg";

        private Vector2I _resolution = new(1920, 1080);


        /// <summary>
        /// The current screen resolution.
        /// </summary>
        public Vector2I Resolution => _resolution;

        /// <summary>
        /// Window mode (0 = Fullscreen, 1 = Borderless, 2 = Windowed).
        /// </summary>
        public int WindowMode { get; private set; } = 0;

        /// <summary>
        /// The game's current language.
        /// </summary>
        public string Language { get; private set; } = OS.GetLocaleLanguage() == "ru" ? "ru" : "en";

        /// <summary>
        /// The maximum frames per second.
        /// </summary>
        public int MaxFps { get; private set; } = 60;

        /// <summary>
        /// The overall sound volume.
        /// </summary>
        public float MasterVolume { get; private set; } = 1.0f;

        /// <summary>
        /// The background music volume.
        /// </summary>
        public float MusicVolume { get; private set; } = 0.7f;

        /// <summary>
        /// The sound effects volume.
        /// </summary>
        public float SfxVolume { get; private set; } = 0.8f;

        /// <summary>
        /// Initializes the settings manager when added to the scene tree.
        /// Loads and applies the saved settings.
        /// </summary>
        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }

            Instance = this;
            LoadSettings();
            ApplySettings();

            GD.Print("[SettingsManager] Initialized");
        }

        /// <summary>
        /// Sets a new screen resolution.
        /// </summary>
        /// <param name="resolution">The new resolution.</param>
        /// <param name="applyImmediately">If <c>true</c>, video settings are applied immediately.</param>
        public void SetResolution(Vector2I resolution, bool applyImmediately = true)
        {
            _resolution = resolution;

            if (applyImmediately)
            {
                ApplyVideoSettings();
            }

            GD.Print($"[SettingsManager] Resolution set to {resolution}");
        }

        /// <summary>
        /// Sets the window mode.
        /// </summary>
        /// <param name="mode">0 - Fullscreen, 1 - Borderless, 2 - Windowed.</param>
        /// <param name="applyImmediately">If <c>true</c>, video settings are applied immediately.</param>
        public void SetWindowMode(int mode, bool applyImmediately = true)
        {
            WindowMode = mode;

            if (applyImmediately)
            {
                ApplyVideoSettings();
            }

            GD.Print($"[SettingsManager] WindowMode: {mode}");
        }

        /// <summary>
        /// Changes the game's language.
        /// </summary>
        /// <param name="lang">The language code.</param>
        public void SetLanguage(string lang, bool applyImmediately = true)
        {
            Language = lang;
            if (applyImmediately)
            {
                TranslationServer.SetLocale(lang);
            }
            GD.Print($"[SettingsManager] Language: {lang}");
        }

        /// <summary>
        /// Sets the maximum frame rate.
        /// </summary>
        /// <param name="fps">The maximum frames per second.</param>
        /// <param name="applyImmediately">If <c>true</c>, video settings are applied immediately.</param>
        public void SetMaxFps(int fps, bool applyImmediately = true)
        {
            MaxFps = fps;

            if (applyImmediately)
            {
                ApplyVideoSettings();
            }

            GD.Print($"[SettingsManager] Max FPS: {fps}");
        }

        /// <summary>
        /// Sets the overall volume and forwards it to AudioManager.
        /// </summary>
        /// <param name="volume">The volume level.</param>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MasterVolume = MasterVolume;
            GD.Print($"[SettingsManager] Master volume: {MasterVolume:F2}");
        }

        /// <summary>
        /// Sets the background music volume and forwards it to AudioManager.
        /// </summary>
        /// <param name="volume">The volume level.</param>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MusicVolume = MusicVolume;
            GD.Print($"[SettingsManager] Music volume: {MusicVolume:F2}");
        }

        /// <summary>
        /// Sets the sound effects volume and forwards it to AudioManager.
        /// </summary>
        /// <param name="volume">The volume level.</param>
        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.SfxVolume = SfxVolume;
            GD.Print($"[SettingsManager] SFX volume: {SfxVolume:F2}");
        }

        /// <summary>
        /// Applies the current video settings to the engine and application window.
        /// </summary>
        public void ApplyVideoSettings()
        {
            Window window = GetWindow();

            if (WindowMode == 0) // Fullscreen
            {
                window.Mode = Window.ModeEnum.ExclusiveFullscreen;
                window.Borderless = false;
            }
            else if (WindowMode == 1) // Borderless
            {
                int screenId = window.CurrentScreen;
                Vector2I screenPos = DisplayServer.ScreenGetPosition(screenId);
                Vector2I screenSize = DisplayServer.ScreenGetSize(screenId);
                window.Mode = Window.ModeEnum.Windowed;
                window.Borderless = true;
                window.Size = _resolution;
                window.Position = screenPos + ((screenSize - _resolution) / 2);
            }
            else // Windowed
            {
                int screenId = window.CurrentScreen;
                Vector2I screenPos = DisplayServer.ScreenGetPosition(screenId);
                Vector2I screenSize = DisplayServer.ScreenGetSize(screenId);
                window.Mode = Window.ModeEnum.Windowed;
                window.Borderless = false;

                Vector2I newSize = _resolution;
                if (newSize == screenSize)
                {
                    newSize.Y -= 40; // Prevent Windows from auto-maximizing
                }
                window.Size = newSize;

                Vector2I centered = screenPos + ((screenSize - newSize) / 2);
                if (centered.Y <= screenPos.Y)
                {
                    centered.Y = screenPos.Y + 40;
                }

                window.Position = centered;
            }

            Engine.MaxFps = MaxFps;

            _ = EmitSignal(SignalName.SettingsChanged);
            GD.Print("[SettingsManager] Video settings applied");
        }

        private void ApplySettings()
        {
            ApplyVideoSettings();
            TranslationServer.SetLocale(Language);
            AudioManager.Instance.MasterVolume = MasterVolume;
            AudioManager.Instance.MusicVolume = MusicVolume;
            AudioManager.Instance.SfxVolume = SfxVolume;
        }

        /// <summary>
        /// Saves the current settings to a configuration file on disk.
        /// </summary>
        public void SaveSettings()
        {
            ConfigFile config = new();

            config.SetValue("video", "resolution_x", _resolution.X);
            config.SetValue("video", "resolution_y", _resolution.Y);
            config.SetValue("video", "window_mode", WindowMode);
            config.SetValue("video", "language", Language);
            config.SetValue("video", "max_fps", MaxFps);

            config.SetValue("audio", "master_volume", MasterVolume);
            config.SetValue("audio", "music_volume", MusicVolume);
            config.SetValue("audio", "sfx_volume", SfxVolume);

            Error err = config.Save(SettingsPath);
            if (err != Error.Ok)
            {
                GD.PrintErr($"[SettingsManager] Failed to save settings: {err}");
            }
            else
            {
                GD.Print("[SettingsManager] Settings saved");
            }
        }

        public void LoadSettings()
        {
            ConfigFile config = new();
            Error err = config.Load(SettingsPath);

            if (err != Error.Ok)
            {
                GD.Print("[SettingsManager] No settings file found, using defaults");
                return;
            }

            _resolution = new Vector2I(
                (int)config.GetValue("video", "resolution_x", 1920),
                (int)config.GetValue("video", "resolution_y", 1080)
            );
            WindowMode = (int)config.GetValue("video", "window_mode", 0);
            Language = (string)config.GetValue("video", "language", OS.GetLocaleLanguage() == "ru" ? "ru" : "en");
            MaxFps = (int)config.GetValue("video", "max_fps", 60);

            MasterVolume = (float)config.GetValue("audio", "master_volume", 1.0f);
            MusicVolume = (float)config.GetValue("audio", "music_volume", 0.7f);
            SfxVolume = (float)config.GetValue("audio", "sfx_volume", 0.8f);

            GD.Print("[SettingsManager] Settings loaded");
        }

        /// <summary>
        /// Resets all settings to their default values, applies and saves them.
        /// </summary>
        public void ResetToDefaults()
        {
            _resolution = new Vector2I(1920, 1080);
            WindowMode = 0;
            Language = OS.GetLocaleLanguage() == "ru" ? "ru" : "en";
            MaxFps = 60;
            MasterVolume = 1.0f;
            MusicVolume = 0.7f;
            SfxVolume = 0.8f;

            ApplySettings();
            SaveSettings();

            GD.Print("[SettingsManager] Settings reset to defaults");
        }
    }
}
