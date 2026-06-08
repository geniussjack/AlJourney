using Godot;
using AlJourney.Scripts.Interfaces;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер настроек. Отвечает за сохранение, загрузку и применение графических и звуковых настроек игры.
    /// </summary>
    public partial class SettingsManager : Node, ISettingsManager
    {
        /// <summary>
        /// Глобальный экземпляр менеджера настроек (паттерн Singleton).
        /// </summary>
        public static SettingsManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Событие, вызываемое при изменении и применении любых настроек.
        /// </summary>
        public delegate void SettingsChangedEventHandler();

        private const string SettingsPath = "user://settings.cfg";

        private Vector2I _resolution = new(1920, 1080);


        /// <summary>
        /// Текущее разрешение экрана.
        /// </summary>
        public Vector2I Resolution => _resolution;

        /// <summary>
        /// Указывает, включен ли полноэкранный режим.
        /// </summary>
        public bool Fullscreen { get; private set; } = true;

        /// <summary>
        /// Указывает, включена ли вертикальная синхронизация (VSync).
        /// </summary>
        public bool VSync { get; private set; } = true;

        /// <summary>
        /// Максимальное количество кадров в секунду (FPS).
        /// </summary>
        public int MaxFps { get; private set; } = 60;

        /// <summary>
        /// Общая (мастер) громкость звука.
        /// </summary>
        public float MasterVolume { get; private set; } = 1.0f;

        /// <summary>
        /// Громкость фоновой музыки.
        /// </summary>
        public float MusicVolume { get; private set; } = 0.7f;

        /// <summary>
        /// Громкость звуковых эффектов (SFX).
        /// </summary>
        public float SfxVolume { get; private set; } = 0.8f;

        /// <summary>
        /// Инициализирует менеджер настроек при добавлении в дерево сцены.
        /// Загружает сохраненные настройки и применяет их.
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
        /// Устанавливает новое разрешение экрана.
        /// </summary>
        /// <param name="resolution">Новое разрешение.</param>
        /// <param name="applyImmediately">Если <c>true</c>, видео-настройки применяются немедленно.</param>
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
        /// Включает или выключает полноэкранный режим.
        /// </summary>
        /// <param name="enabled">Состояние полноэкранного режима.</param>
        /// <param name="applyImmediately">Если <c>true</c>, видео-настройки применяются немедленно.</param>
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
        /// Включает или выключает вертикальную синхронизацию (VSync).
        /// </summary>
        /// <param name="enabled">Состояние вертикальной синхронизации.</param>
        /// <param name="applyImmediately">Если <c>true</c>, видео-настройки применяются немедленно.</param>
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
        /// Устанавливает максимальную частоту кадров.
        /// </summary>
        /// <param name="fps">Максимальное количество кадров в секунду.</param>
        /// <param name="applyImmediately">Если <c>true</c>, видео-настройки применяются немедленно.</param>
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
        /// Устанавливает общую громкость звука и передает её в AudioManager.
        /// </summary>
        /// <param name="volume">Уровень громкости (от 0.0 до 1.0).</param>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MasterVolume = MasterVolume;
            GD.Print($"[SettingsManager] Master volume: {MasterVolume:F2}");
        }

        /// <summary>
        /// Устанавливает громкость фоновой музыки и передает её в AudioManager.
        /// </summary>
        /// <param name="volume">Уровень громкости (от 0.0 до 1.0).</param>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.MusicVolume = MusicVolume;
            GD.Print($"[SettingsManager] Music volume: {MusicVolume:F2}");
        }

        /// <summary>
        /// Устанавливает громкость звуковых эффектов (SFX) и передает её в AudioManager.
        /// </summary>
        /// <param name="volume">Уровень громкости (от 0.0 до 1.0).</param>
        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
            AudioManager.Instance.SfxVolume = SfxVolume;
            GD.Print($"[SettingsManager] SFX volume: {SfxVolume:F2}");
        }

        /// <summary>
        /// Применяет текущие видео-настройки к движку и окну приложения.
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
        /// Сохраняет текущие настройки в конфигурационный файл на диске.
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
        /// Сбрасывает все настройки до значений по умолчанию, применяет их и сохраняет.
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
