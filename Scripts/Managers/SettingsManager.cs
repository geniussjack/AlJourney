using Godot;
using AlJourney.Scripts.Interfaces;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер SettingsManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class SettingsManager : Node, ISettingsManager
    {
        public static SettingsManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Устанавливает tingsChangedEventHandler.
        /// </summary>
        public delegate void SettingsChangedEventHandler();

        private const string SettingsPath = "user://settings.cfg";

        private Vector2I _resolution = new(1920, 1080);


        /// <summary>
        /// Элемент Resolution.
        /// </summary>
        public Vector2I Resolution => _resolution;

        /// <summary>
        /// Элемент Fullscreen.
        /// </summary>
        public bool Fullscreen { get; private set; } = true;

        /// <summary>
        /// Элемент VSync.
        /// </summary>
        public bool VSync { get; private set; } = true;

        /// <summary>
        /// Элемент MaxFps.
        /// </summary>
        public int MaxFps { get; private set; } = 60;

        /// <summary>
        /// Элемент MasterVolume.
        /// </summary>
        public float MasterVolume { get; private set; } = 1.0f;

        /// <summary>
        /// Элемент MusicVolume.
        /// </summary>
        public float MusicVolume { get; private set; } = 0.7f;

        /// <summary>
        /// Элемент SfxVolume.
        /// </summary>
        public float SfxVolume { get; private set; } = 0.8f;

        /// <summary>
        /// Элемент _Ready.
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
        /// Устанавливает Resolution.
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
        /// Устанавливает Fullscreen.
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
        /// Устанавливает VSync.
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
        /// Устанавливает MaxFps.
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
        /// Устанавливает MasterVolume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MasterVolume = MasterVolume;
            GD.Print($"[SettingsManager] Master volume: {MasterVolume:F2}");
        }

        /// <summary>
        /// Устанавливает MusicVolume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MusicVolume = MusicVolume;
            GD.Print($"[SettingsManager] Music volume: {MusicVolume:F2}");
        }

        /// <summary>
        /// Устанавливает SfxVolume.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.SfxVolume = SfxVolume;
            GD.Print($"[SettingsManager] SFX volume: {SfxVolume:F2}");
        }

        /// <summary>
        /// Применяет VideoSettings.
        /// </summary>
        public void ApplyVideoSettings()
        {
            Window window = GetWindow();

            window.Size = _resolution;

            if (Fullscreen)
            {
                window.Mode = Window.ModeEnum.Fullscreen;
            }
            else
            {
                window.Mode = Window.ModeEnum.Windowed;
                window.Position = (DisplayServer.ScreenGetSize() - _resolution) / 2;
            }

            DisplayServer.WindowSetVsyncMode(VSync
                ? DisplayServer.VSyncMode.Enabled
                : DisplayServer.VSyncMode.Disabled);

            Engine.MaxFps = MaxFps;

            _ = EmitSignal(SignalName.SettingsChanged);
            GD.Print("[SettingsManager] Video settings applied");
        }

        private void ApplySettings()
        {
            ApplyVideoSettings();
            AudioManager.Instance.MasterVolume = MasterVolume;
            AudioManager.Instance.MusicVolume = MusicVolume;
            AudioManager.Instance.SfxVolume = SfxVolume;
        }

        /// <summary>
        /// Сохраняет Settings.
        /// </summary>
        public void SaveSettings()
        {
            ConfigFile config = new();

            config.SetValue("video", "resolution_x", _resolution.X);
            config.SetValue("video", "resolution_y", _resolution.Y);
            config.SetValue("video", "fullscreen", Fullscreen);
            config.SetValue("video", "vsync", VSync);
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

        private void LoadSettings()
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
            Fullscreen = (bool)config.GetValue("video", "fullscreen", true);
            VSync = (bool)config.GetValue("video", "vsync", true);
            MaxFps = (int)config.GetValue("video", "max_fps", 60);

            MasterVolume = (float)config.GetValue("audio", "master_volume", 1.0f);
            MusicVolume = (float)config.GetValue("audio", "music_volume", 0.7f);
            SfxVolume = (float)config.GetValue("audio", "sfx_volume", 0.8f);

            GD.Print("[SettingsManager] Settings loaded");
        }

        /// <summary>
        /// Сбрасывает ToDefaults.
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
