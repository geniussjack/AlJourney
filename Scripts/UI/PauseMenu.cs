using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Pause menu controller for battle scene.
    /// Provides options to resume, restart, or return to main menu.
    /// </summary>
    public partial class PauseMenu : Control
    {
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        private Panel _overlay;

        public override void _Ready()
        {
            // Create semi-transparent overlay
            _overlay = new Panel
            {
                Name = "Overlay"
            };
            _overlay.SetAnchorsPreset(LayoutPreset.FullRect);
            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0, 0, 0, 0.7f)
            };
            _overlay.AddThemeStyleboxOverride("panel", styleBox);
            AddChild(_overlay);

            // Create center container for menu
            var centerContainer = new CenterContainer
            {
                Name = "CenterContainer"
            };
            centerContainer.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(centerContainer);

            // Create menu panel
            var menuPanel = new PanelContainer();
            centerContainer.AddChild(menuPanel);

            // Create VBoxContainer for buttons
            var vbox = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(300, 0)
            };
            vbox.AddThemeConstantOverride("separation", 15);
            menuPanel.AddChild(vbox);

            // Title
            var titleLabel = new Label
            {
                Text = "PAUSED",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 32);
            vbox.AddChild(titleLabel);

            // Spacer
            var spacer1 = new Control { CustomMinimumSize = new Vector2(0, 20) };
            vbox.AddChild(spacer1);

            // Resume button
            _resumeButton = new Button
            {
                Text = "Resume",
                CustomMinimumSize = new Vector2(0, 50)
            };
            vbox.AddChild(_resumeButton);

            // Settings button
            _settingsButton = new Button
            {
                Text = "Settings",
                CustomMinimumSize = new Vector2(0, 50)
            };
            vbox.AddChild(_settingsButton);

            // Main Menu button
            _mainMenuButton = new Button
            {
                Text = "Main Menu",
                CustomMinimumSize = new Vector2(0, 50)
            };
            vbox.AddChild(_mainMenuButton);

            // Connect signals
            _resumeButton.Pressed += OnResumePressed;
            _settingsButton.Pressed += OnSettingsPressed;
            _mainMenuButton.Pressed += OnMainMenuPressed;

            // Initially hidden
            Hide();

            // Set process mode to always (works even when paused)
            ProcessMode = ProcessModeEnum.Always;

            GD.Print("[PauseMenu] Initialized");
        }

        public override void _Input(InputEvent @event)
        {
            // Toggle pause with ESC key
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (Visible)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
                GetViewport().SetInputAsHandled();
            }
        }

        /// <summary>
        /// Shows pause menu and pauses game.
        /// </summary>
        public void Pause()
        {
            Show();
            GetTree().Paused = true;

            // Animate fade in
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);

            GD.Print("[PauseMenu] Game paused");
        }

        /// <summary>
        /// Hides pause menu and resumes game.
        /// </summary>
        public void Resume()
        {
            // Animate fade out
            var tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            tween.TweenCallback(Callable.From(() =>
            {
                Hide();
                GetTree().Paused = false;
                GD.Print("[PauseMenu] Game resumed");
            }));
        }

        /// <summary>
        /// Called when Resume button is pressed.
        /// </summary>
        private void OnResumePressed()
        {
            AudioManager.Instance?.PlaySfx("res://Resources/Audio/SFX/button_click.wav");
            Resume();
        }

        /// <summary>
        /// Called when Settings button is pressed.
        /// </summary>
        private void OnSettingsPressed()
        {
            AudioManager.Instance?.PlaySfx("res://Resources/Audio/SFX/button_click.wav");
            GD.Print("[PauseMenu] Settings button pressed (not implemented yet)");
            // TODO: Show settings overlay
        }

        /// <summary>
        /// Called when Main Menu button is pressed.
        /// </summary>
        private void OnMainMenuPressed()
        {
            AudioManager.Instance?.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Unpause before scene transition
            GetTree().Paused = false;

            // Return to main menu
            SceneManager.GoToMainMenu();
        }
    }
}
