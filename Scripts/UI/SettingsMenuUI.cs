using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Пользовательский интерфейс меню настроек. Управляет изменениями графики и звука.
    /// </summary>
    public partial class SettingsMenuUI : Control
    {
        private OptionButton _resolutionDropdown;
        private OptionButton _windowModeDropdown;
        private OptionButton _languageDropdown;
        private OptionButton _fpsLimitDropdown;

        private HSlider _masterVolumeSlider;
        private Label _masterVolumeLabel;
        private HSlider _musicVolumeSlider;
        private Label _musicVolumeLabel;
        private HSlider _sfxVolumeSlider;
        private Label _sfxVolumeLabel;

        private Button _applyButton;
        private Button _resetButton;
        private Button _backButton;

        private readonly Vector2I[] _resolutions =
        [
            new(1280, 720),
            new(1920, 1080),
            new(2560, 1440),
            new(3840, 2160)
        ];

        private readonly int[] _fpsLimits = [30, 60, 120, 144, 240, 0];

        /// <summary>
        /// Вызывается при инициализации узла. Настраивает ссылки на элементы управления, подписывается на их события и загружает текущие настройки.
        /// </summary>
        public override void _Ready()
        {
            _resolutionDropdown = GetNode<OptionButton>("SettingsMenu/Panel/VBoxContainer/VideoSettings/ResolutionDropdown");
            _windowModeDropdown = GetNode<OptionButton>("SettingsMenu/Panel/VBoxContainer/VideoSettings/WindowModeDropdown");
            _languageDropdown = GetNode<OptionButton>("SettingsMenu/Panel/VBoxContainer/VideoSettings/LanguageDropdown");
            _fpsLimitDropdown = GetNode<OptionButton>("SettingsMenu/Panel/VBoxContainer/VideoSettings/FpsLimitDropdown");

            _masterVolumeSlider = GetNode<HSlider>("SettingsMenu/Panel/VBoxContainer/AudioSettings/MasterVolumeSlider");
            _masterVolumeLabel = GetNode<Label>("SettingsMenu/Panel/VBoxContainer/AudioSettings/MasterVolumeContainer/MasterVolumeLabel");
            _musicVolumeSlider = GetNode<HSlider>("SettingsMenu/Panel/VBoxContainer/AudioSettings/MusicVolumeSlider");
            _musicVolumeLabel = GetNode<Label>("SettingsMenu/Panel/VBoxContainer/AudioSettings/MusicVolumeContainer/MusicVolumeLabel");
            _sfxVolumeSlider = GetNode<HSlider>("SettingsMenu/Panel/VBoxContainer/AudioSettings/SfxVolumeSlider");
            _sfxVolumeLabel = GetNode<Label>("SettingsMenu/Panel/VBoxContainer/AudioSettings/SfxVolumeContainer/SfxVolumeLabel");

            _applyButton = GetNode<Button>("SettingsMenu/Panel/VBoxContainer/ButtonsContainer/ApplyButton");
            _resetButton = GetNode<Button>("SettingsMenu/Panel/VBoxContainer/ButtonsContainer/ResetButton");
            _backButton = GetNode<Button>("SettingsMenu/Panel/VBoxContainer/ButtonsContainer/BackButton");

            SetupResolutionDropdown();
            SetupFpsDropdown();
            SetupLanguageDropdown();
            SetupWindowModeDropdown();

            _windowModeDropdown.ItemSelected += OnWindowModeSelected;
            _languageDropdown.ItemSelected += OnLanguageSelected;
            _resolutionDropdown.ItemSelected += OnResolutionSelected;
            _fpsLimitDropdown.ItemSelected += OnFpsLimitSelected;

            _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
            _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
            _sfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;

            _applyButton.Pressed += OnApplyPressed;
            _resetButton.Pressed += OnResetPressed;
            _backButton.Pressed += OnBackPressed;

            LoadCurrentSettings();

            GD.Print("[SettingsMenuUI] Initialized");
        }

        public override void _Notification(int what)
        {
            if (what == NotificationTranslationChanged)
            {
                SetupLanguageDropdown();
                SetupWindowModeDropdown();
                LoadCurrentSettings();
            }
        }

        private void SetupLanguageDropdown()
        {
            _languageDropdown.Clear();
            _languageDropdown.AddItem(Tr("UI_ENGLISH"), 0);
            _languageDropdown.AddItem(Tr("UI_RUSSIAN"), 1);
        }

        private void SetupWindowModeDropdown()
        {
            _windowModeDropdown.Clear();
            _windowModeDropdown.AddItem(Tr("UI_FULLSCREEN"), 0);
            _windowModeDropdown.AddItem(Tr("UI_BORDERLESS"), 1);
            _windowModeDropdown.AddItem(Tr("UI_WINDOWED"), 2);
        }

        private void SetupResolutionDropdown()
        {
            _resolutionDropdown.Clear();
            for (int i = 0; i < _resolutions.Length; i++)
            {
                Vector2I res = _resolutions[i];
                _resolutionDropdown.AddItem($"{res.X} x {res.Y}", i);
            }
        }

        private void SetupFpsDropdown()
        {
            _fpsLimitDropdown.Clear();
            for (int i = 0; i < _fpsLimits.Length; i++)
            {
                int fps = _fpsLimits[i];
                string label = fps == 0 ? "Unlimited" : $"{fps} FPS";
                _fpsLimitDropdown.AddItem(label, i);
            }
        }

        private void LoadCurrentSettings()
        {
            SettingsManager settings = SettingsManager.Instance;

            _windowModeDropdown.Selected = settings.WindowMode;
            _languageDropdown.Selected = settings.Language == "ru" ? 1 : 0;

            Vector2I currentRes = settings.Resolution;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                if (_resolutions[i] == currentRes)
                {
                    _resolutionDropdown.Selected = i;
                    break;
                }
            }

            int currentFps = settings.MaxFps;
            for (int i = 0; i < _fpsLimits.Length; i++)
            {
                if (_fpsLimits[i] == currentFps)
                {
                    _fpsLimitDropdown.Selected = i;
                    break;
                }
            }

            _masterVolumeSlider.Value = settings.MasterVolume;
            _musicVolumeSlider.Value = settings.MusicVolume;
            _sfxVolumeSlider.Value = settings.SfxVolume;

            UpdateVolumeLabels();
        }

        private void UpdateVolumeLabels()
        {
            _masterVolumeLabel.Text = $"{Tr("UI_SETTINGS_MASTER")} {_masterVolumeSlider.Value * 100:F0}%";
            _musicVolumeLabel.Text = $"{Tr("UI_SETTINGS_MUSIC")} {_musicVolumeSlider.Value * 100:F0}%";
            _sfxVolumeLabel.Text = $"{Tr("UI_SETTINGS_SFX")} {_sfxVolumeSlider.Value * 100:F0}%";
        }

        private void OnWindowModeSelected(long index)
        {
            SettingsManager.Instance.SetWindowMode((int)index, false);
        }

        private void OnLanguageSelected(long index)
        {
            string lang = index == 1 ? "ru" : "en";
            SettingsManager.Instance.SetLanguage(lang);
        }

        private void OnResolutionSelected(long index)
        {
            Vector2I resolution = _resolutions[index];
            SettingsManager.Instance.SetResolution(resolution, false);
        }

        private void OnFpsLimitSelected(long index)
        {
            int fpsLimit = _fpsLimits[index];
            SettingsManager.Instance.SetMaxFps(fpsLimit, false);
        }

        private void OnMasterVolumeChanged(double value)
        {
            SettingsManager.Instance.SetMasterVolume((float)value);
            UpdateVolumeLabels();
        }

        private void OnMusicVolumeChanged(double value)
        {
            SettingsManager.Instance.SetMusicVolume((float)value);
            UpdateVolumeLabels();
        }

        private void OnSfxVolumeChanged(double value)
        {
            SettingsManager.Instance.SetSfxVolume((float)value);
            UpdateVolumeLabels();

            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
        }

        private void OnApplyPressed()
        {
            GD.Print("[SettingsMenuUI] Apply pressed");

            SettingsManager.Instance.ApplyVideoSettings();
            SettingsManager.Instance.SaveSettings();

            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));

            MainMenuUI mainMenu = GetParent() as MainMenuUI;
            mainMenu?.OnBackToMainMenu();
        }

        private void OnResetPressed()
        {
            GD.Print("[SettingsMenuUI] Reset pressed");

            SettingsManager.Instance.ResetToDefaults();

            LoadCurrentSettings();

            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
        }

        private void OnBackPressed()
        {
            GD.Print("[SettingsMenuUI] Back pressed");
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));

            ConfirmationDialog dialog = new()
            {
                Title = Tr("UI_SETTINGS_CANCEL_TITLE"),
                DialogText = Tr("UI_SETTINGS_CANCEL_PROMPT"),
                Theme = Theme
            };

            dialog.Confirmed += () =>
            {
                SettingsManager.Instance.LoadSettings();
                SettingsManager.Instance.ApplyVideoSettings();
                LoadCurrentSettings();

                MainMenuUI mainMenu = GetParent() as MainMenuUI;
                mainMenu?.OnBackToMainMenu();
                dialog.QueueFree();
            };

            dialog.Canceled += () => dialog.QueueFree();

            AddChild(dialog);
            dialog.PopupCentered();
        }
    }
}


