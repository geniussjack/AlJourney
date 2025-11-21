using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Settings menu UI controller.
    /// Manages video and audio settings.
    /// </summary>
    public partial class SettingsMenuUI : Control
    {
        // Video settings controls
        private OptionButton _resolutionDropdown;
        private CheckButton _fullscreenToggle;
        private CheckButton _vsyncToggle;
        private OptionButton _fpsLimitDropdown;

        // Audio settings controls
        private HSlider _masterVolumeSlider;
        private Label _masterVolumeLabel;
        private HSlider _musicVolumeSlider;
        private Label _musicVolumeLabel;
        private HSlider _sfxVolumeSlider;
        private Label _sfxVolumeLabel;

        // Action buttons
        private Button _applyButton;
        private Button _resetButton;
        private Button _backButton;

        // Available resolutions
        private readonly Vector2I[] _resolutions =
        [
            new(1280, 720),   // HD
            new(1920, 1080),  // Full HD
            new(2560, 1440),  // 2K
            new(3840, 2160)   // 4K
        ];

        // Available FPS limits
        private readonly int[] _fpsLimits = [30, 60, 120, 144, 240, 0]; // 0 = unlimited

        public override void _Ready()
        {
            // Get video controls
            _resolutionDropdown = GetNode<OptionButton>("VBoxContainer/VideoSettings/ResolutionDropdown");
            _fullscreenToggle = GetNode<CheckButton>("VBoxContainer/VideoSettings/FullscreenToggle");
            _vsyncToggle = GetNode<CheckButton>("VBoxContainer/VideoSettings/VsyncToggle");
            _fpsLimitDropdown = GetNode<OptionButton>("VBoxContainer/VideoSettings/FpsLimitDropdown");

            // Get audio controls
            _masterVolumeSlider = GetNode<HSlider>("VBoxContainer/AudioSettings/MasterVolumeSlider");
            _masterVolumeLabel = GetNode<Label>("VBoxContainer/AudioSettings/MasterVolumeLabel");
            _musicVolumeSlider = GetNode<HSlider>("VBoxContainer/AudioSettings/MusicVolumeSlider");
            _musicVolumeLabel = GetNode<Label>("VBoxContainer/AudioSettings/MusicVolumeLabel");
            _sfxVolumeSlider = GetNode<HSlider>("VBoxContainer/AudioSettings/SfxVolumeSlider");
            _sfxVolumeLabel = GetNode<Label>("VBoxContainer/AudioSettings/SfxVolumeLabel");

            // Get action buttons
            _applyButton = GetNode<Button>("VBoxContainer/ButtonsContainer/ApplyButton");
            _resetButton = GetNode<Button>("VBoxContainer/ButtonsContainer/ResetButton");
            _backButton = GetNode<Button>("VBoxContainer/ButtonsContainer/BackButton");

            // Setup dropdowns
            SetupResolutionDropdown();
            SetupFpsDropdown();

            // Connect signals
            _fullscreenToggle.Toggled += OnFullscreenToggled;
            _vsyncToggle.Toggled += OnVsyncToggled;
            _resolutionDropdown.ItemSelected += OnResolutionSelected;
            _fpsLimitDropdown.ItemSelected += OnFpsLimitSelected;

            _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
            _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
            _sfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;

            _applyButton.Pressed += OnApplyPressed;
            _resetButton.Pressed += OnResetPressed;
            _backButton.Pressed += OnBackPressed;

            // Load current settings
            LoadCurrentSettings();

            GD.Print("[SettingsMenuUI] Initialized");
        }

        /// <summary>
        /// Populates resolution dropdown with available options.
        /// </summary>
        private void SetupResolutionDropdown()
        {
            _resolutionDropdown.Clear();
            for (int i = 0; i < _resolutions.Length; i++)
            {
                Vector2I res = _resolutions[i];
                _resolutionDropdown.AddItem($"{res.X} x {res.Y}", i);
            }
        }

        /// <summary>
        /// Populates FPS limit dropdown with available options.
        /// </summary>
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

        /// <summary>
        /// Loads current settings from SettingsManager.
        /// </summary>
        private void LoadCurrentSettings()
        {
            SettingsManager settings = SettingsManager.Instance;

            // Video settings
            _fullscreenToggle.ButtonPressed = settings.Fullscreen;
            _vsyncToggle.ButtonPressed = settings.VSync;

            // Find matching resolution
            Vector2I currentRes = settings.Resolution;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                if (_resolutions[i] == currentRes)
                {
                    _resolutionDropdown.Selected = i;
                    break;
                }
            }

            // Find matching FPS limit
            int currentFps = settings.MaxFps;
            for (int i = 0; i < _fpsLimits.Length; i++)
            {
                if (_fpsLimits[i] == currentFps)
                {
                    _fpsLimitDropdown.Selected = i;
                    break;
                }
            }

            // Audio settings
            _masterVolumeSlider.Value = settings.MasterVolume;
            _musicVolumeSlider.Value = settings.MusicVolume;
            _sfxVolumeSlider.Value = settings.SfxVolume;

            UpdateVolumeLabels();
        }

        /// <summary>
        /// Updates volume percentage labels.
        /// </summary>
        private void UpdateVolumeLabels()
        {
            _masterVolumeLabel.Text = $"Master: {_masterVolumeSlider.Value * 100:F0}%";
            _musicVolumeLabel.Text = $"Music: {_musicVolumeSlider.Value * 100:F0}%";
            _sfxVolumeLabel.Text = $"SFX: {_sfxVolumeSlider.Value * 100:F0}%";
        }

        /// <summary>
        /// Called when fullscreen toggle changes.
        /// </summary>
        private void OnFullscreenToggled(bool toggled)
        {
            SettingsManager.Instance.SetFullscreen(toggled, false);
        }

        /// <summary>
        /// Called when VSync toggle changes.
        /// </summary>
        private void OnVsyncToggled(bool toggled)
        {
            SettingsManager.Instance.SetVSync(toggled, false);
        }

        /// <summary>
        /// Called when resolution dropdown selection changes.
        /// </summary>
        private void OnResolutionSelected(long index)
        {
            Vector2I resolution = _resolutions[index];
            SettingsManager.Instance.SetResolution(resolution, false);
        }

        /// <summary>
        /// Called when FPS limit dropdown selection changes.
        /// </summary>
        private void OnFpsLimitSelected(long index)
        {
            int fpsLimit = _fpsLimits[index];
            SettingsManager.Instance.SetMaxFps(fpsLimit, false);
        }

        /// <summary>
        /// Called when master volume slider changes.
        /// </summary>
        private void OnMasterVolumeChanged(double value)
        {
            SettingsManager.Instance.SetMasterVolume((float)value);
            UpdateVolumeLabels();
        }

        /// <summary>
        /// Called when music volume slider changes.
        /// </summary>
        private void OnMusicVolumeChanged(double value)
        {
            SettingsManager.Instance.SetMusicVolume((float)value);
            UpdateVolumeLabels();
        }

        /// <summary>
        /// Called when SFX volume slider changes.
        /// </summary>
        private void OnSfxVolumeChanged(double value)
        {
            SettingsManager.Instance.SetSfxVolume((float)value);
            UpdateVolumeLabels();

            // Play test sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");
        }

        /// <summary>
        /// Called when Apply button is pressed.
        /// </summary>
        private void OnApplyPressed()
        {
            GD.Print("[SettingsMenuUI] Apply pressed");

            // Apply all pending video settings
            SettingsManager.Instance.ApplyVideoSettings();

            // Save settings to file
            SettingsManager.Instance.SaveSettings();

            // Play confirmation sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");
        }

        /// <summary>
        /// Called when Reset button is pressed.
        /// </summary>
        private void OnResetPressed()
        {
            GD.Print("[SettingsMenuUI] Reset pressed");

            // Reset to defaults
            SettingsManager.Instance.ResetToDefaults();

            // Reload UI
            LoadCurrentSettings();

            // Play sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");
        }

        /// <summary>
        /// Called when Back button is pressed.
        /// </summary>
        private void OnBackPressed()
        {
            GD.Print("[SettingsMenuUI] Back pressed");

            // Get parent main menu and call back method
            MainMenuUI mainMenu = GetParent().GetNode<MainMenuUI>("../");
            mainMenu?.OnBackToMainMenu();
        }
    }
}
