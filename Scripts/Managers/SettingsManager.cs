using Godot;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Manages game settings (video, audio, controls).
    /// Singleton autoload node.
    /// </summary>
    public partial class SettingsManager : Node
    {
        private static SettingsManager _instance;

        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static SettingsManager Instance => _instance;

        [Signal]
        public delegate void SettingsChangedEventHandler();

        // Settings file path
        private const string SettingsPath = "user://settings.cfg";

        // Video settings
        private Vector2I _resolution = new(1920, 1080);
        private bool _fullscreen = true;
        private bool _vsync = true;
        private int _maxFps = 60;

        // Audio settings
        private float _masterVolume = 1.0f;
        private float _musicVolume = 0.7f;
        private float _sfxVolume = 0.8f;

        /// <summary>
        /// Current screen resolution.
        /// </summary>
        public Vector2I Resolution => _resolution;

        /// <summary>
        /// Is fullscreen enabled.
        /// </summary>
        public bool Fullscreen => _fullscreen;

        /// <summary>
        /// Is VSync enabled.
        /// </summary>
        public bool VSync => _vsync;

        /// <summary>
        /// Maximum FPS limit (0 = unlimited).
        /// </summary>
        public int MaxFps => _maxFps;

        /// <summary>
        /// Master volume (0.0 to 1.0).
        /// </summary>
        public float MasterVolume => _masterVolume;

        /// <summary>
        /// Music volume (0.0 to 1.0).
        /// </summary>
        public float MusicVolume => _musicVolume;

        /// <summary>
        /// SFX volume (0.0 to 1.0).
        /// </summary>
        public float SfxVolume => _sfxVolume;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }

            _instance = this;
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
            _fullscreen = enabled;

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
            _vsync = enabled;

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
            _maxFps = fps;

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
            _masterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MasterVolume = _masterVolume;
            GD.Print($"[SettingsManager] Master volume: {_masterVolume:F2}");
        }

        /// <summary>
        /// Sets music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MusicVolume = _musicVolume;
            GD.Print($"[SettingsManager] Music volume: {_musicVolume:F2}");
        }

        /// <summary>
        /// Sets SFX volume.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.SfxVolume = _sfxVolume;
            GD.Print($"[SettingsManager] SFX volume: {_sfxVolume:F2}");
        }

        /// <summary>
        /// Applies all video settings to the window.
        /// </summary>
        public void ApplyVideoSettings()
        {
            var window = GetWindow();

            // Set resolution
            window.Size = _resolution;

            // Set fullscreen mode
            if (_fullscreen)
            {
                window.Mode = Window.ModeEnum.Fullscreen;
            }
            else
            {
                window.Mode = Window.ModeEnum.Windowed;
                window.Position = (DisplayServer.ScreenGetSize() - _resolution) / 2;
            }

            // Set VSync
            DisplayServer.WindowSetVsyncMode(_vsync
                ? DisplayServer.VSyncMode.Enabled
                : DisplayServer.VSyncMode.Disabled);

            // Set FPS limit
            Engine.MaxFps = _maxFps;

            EmitSignal(SignalName.SettingsChanged);
            GD.Print("[SettingsManager] Video settings applied");
        }

        /// <summary>
        /// Applies all settings.
        /// </summary>
        private void ApplySettings()
        {
            ApplyVideoSettings();
            AudioManager.Instance.MasterVolume = _masterVolume;
            AudioManager.Instance.MusicVolume = _musicVolume;
            AudioManager.Instance.SfxVolume = _sfxVolume;
        }

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        public void SaveSettings()
        {
            var config = new ConfigFile();

            // Video
            config.SetValue("video", "resolution_x", _resolution.X);
            config.SetValue("video", "resolution_y", _resolution.Y);
            config.SetValue("video", "fullscreen", _fullscreen);
            config.SetValue("video", "vsync", _vsync);
            config.SetValue("video", "max_fps", _maxFps);

            // Audio
            config.SetValue("audio", "master_volume", _masterVolume);
            config.SetValue("audio", "music_volume", _musicVolume);
            config.SetValue("audio", "sfx_volume", _sfxVolume);

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
            var config = new ConfigFile();
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
            _fullscreen = (bool)config.GetValue("video", "fullscreen", true);
            _vsync = (bool)config.GetValue("video", "vsync", true);
            _maxFps = (int)config.GetValue("video", "max_fps", 60);

            // Audio
            _masterVolume = (float)config.GetValue("audio", "master_volume", 1.0f);
            _musicVolume = (float)config.GetValue("audio", "music_volume", 0.7f);
            _sfxVolume = (float)config.GetValue("audio", "sfx_volume", 0.8f);

            GD.Print("[SettingsManager] Settings loaded");
        }

        /// <summary>
        /// Resets all settings to default.
        /// </summary>
        public void ResetToDefaults()
        {
            _resolution = new Vector2I(1920, 1080);
            _fullscreen = true;
            _vsync = true;
            _maxFps = 60;
            _masterVolume = 1.0f;
            _musicVolume = 0.7f;
            _sfxVolume = 0.8f;

            ApplySettings();
            SaveSettings();

            GD.Print("[SettingsManager] Settings reset to defaults");
        }
    }
}