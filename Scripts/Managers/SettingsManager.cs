using Godot;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Manages game settings (video, audio, controls).
    /// Singleton autoload node.
    /// </summary>
    public partial class SettingsManager : Node
    {
        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static SettingsManager Instance { get; private set; }

        [Signal]
        public delegate void SettingsChangedEventHandler();

        // Settings file path
        private const string SettingsPath = "user://settings.cfg";

        // Video settings
        private Vector2I _resolution = new(1920, 1080);

        // Audio settings

        /// <summary>
        /// Current screen resolution.
        /// </summary>
        public Vector2I Resolution => _resolution;

        /// <summary>
        /// Is fullscreen enabled.
        /// </summary>
        public bool Fullscreen { get; private set; } = true;

        /// <summary>
        /// Is VSync enabled.
        /// </summary>
        public bool VSync { get; private set; } = true;

        /// <summary>
        /// Maximum FPS limit (0 = unlimited).
        /// </summary>
        public int MaxFps { get; private set; } = 60;

        /// <summary>
        /// Master volume (0.0 to 1.0).
        /// </summary>
        public float MasterVolume { get; private set; } = 1.0f;

        /// <summary>
        /// Music volume (0.0 to 1.0).
        /// </summary>
        public float MusicVolume { get; private set; } = 0.7f;

        /// <summary>
        /// SFX volume (0.0 to 1.0).
        /// </summary>
        public float SfxVolume { get; private set; } = 0.8f;

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
        /// Sets screen resolution.
        /// </summary>
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
        /// Sets fullscreen mode.
        /// </summary>
        public void SetFullscreen(bool enabled, bool applyImmediately = true)
        {
            Fullscreen = enabled;

            if (applyImmediately)
            {
                ApplyVideoSettings();
            }

            GD.Print($"[SettingsManager] Fullscreen: {enabled}");
        }

        /// <summary>
        /// Sets VSync mode.
        /// </summary>
        public void SetVSync(bool enabled, bool applyImmediately = true)
        {
            VSync = enabled;

            if (applyImmediately)
            {
                ApplyVideoSettings();
            }

            GD.Print($"[SettingsManager] VSync: {enabled}");
        }

        /// <summary>
        /// Sets maximum FPS limit.
        /// </summary>
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
        /// Sets master volume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MasterVolume = MasterVolume;
            GD.Print($"[SettingsManager] Master volume: {MasterVolume:F2}");
        }

        /// <summary>
        /// Sets music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MusicVolume = MusicVolume;
            GD.Print($"[SettingsManager] Music volume: {MusicVolume:F2}");
        }

        /// <summary>
        /// Sets SFX volume.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.SfxVolume = SfxVolume;
            GD.Print($"[SettingsManager] SFX volume: {SfxVolume:F2}");
        }

        /// <summary>
        /// Applies all video settings to the window.
        /// </summary>
        public void ApplyVideoSettings()
        {
            Window window = GetWindow();

            // Set resolution
            window.Size = _resolution;

            // Set fullscreen mode
            if (Fullscreen)
            {
                window.Mode = Window.ModeEnum.Fullscreen;
            }
            else
            {
                window.Mode = Window.ModeEnum.Windowed;
                window.Position = (DisplayServer.ScreenGetSize() - _resolution) / 2;
            }

            // Set VSync
            DisplayServer.WindowSetVsyncMode(VSync
                ? DisplayServer.VSyncMode.Enabled
                : DisplayServer.VSyncMode.Disabled);

            // Set FPS limit
            Engine.MaxFps = MaxFps;

            _ = EmitSignal(SignalName.SettingsChanged);
            GD.Print("[SettingsManager] Video settings applied");
        }

        /// <summary>
        /// Applies all settings.
        /// </summary>
        private void ApplySettings()
        {
            ApplyVideoSettings();
            AudioManager.Instance.MasterVolume = MasterVolume;
            AudioManager.Instance.MusicVolume = MusicVolume;
            AudioManager.Instance.SfxVolume = SfxVolume;
        }

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        public void SaveSettings()
        {
            ConfigFile config = new();

            // Video
            config.SetValue("video", "resolution_x", _resolution.X);
            config.SetValue("video", "resolution_y", _resolution.Y);
            config.SetValue("video", "fullscreen", Fullscreen);
            config.SetValue("video", "vsync", VSync);
            config.SetValue("video", "max_fps", MaxFps);

            // Audio
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

        /// <summary>
        /// Loads settings from file.
        /// </summary>
        private void LoadSettings()
        {
            ConfigFile config = new();
            Error err = config.Load(SettingsPath);

            if (err != Error.Ok)
            {
                GD.Print("[SettingsManager] No settings file found, using defaults");
                return;
            }

            // Video
            _resolution = new Vector2I(
                (int)config.GetValue("video", "resolution_x", 1920),
                (int)config.GetValue("video", "resolution_y", 1080)
            );
            Fullscreen = (bool)config.GetValue("video", "fullscreen", true);
            VSync = (bool)config.GetValue("video", "vsync", true);
            MaxFps = (int)config.GetValue("video", "max_fps", 60);

            // Audio
            MasterVolume = (float)config.GetValue("audio", "master_volume", 1.0f);
            MusicVolume = (float)config.GetValue("audio", "music_volume", 0.7f);
            SfxVolume = (float)config.GetValue("audio", "sfx_volume", 0.8f);

            GD.Print("[SettingsManager] Settings loaded");
        }

        /// <summary>
        /// Resets all settings to default.
        /// </summary>
        public void ResetToDefaults()
        {
            _resolution = new Vector2I(1920, 1080);
            Fullscreen = true;
            VSync = true;
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
