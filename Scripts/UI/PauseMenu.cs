using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI component for the pause menu. Handles pause menu logic, pausing the game, and returning to the main menu.
    /// Works together with PauseMenu.tscn.
    /// </summary>
    public partial class PauseMenu : Control
    {
        private Label _titleLabel;
        private Button _resumeButton;
        private Button _saveButton;
        private Button _mainMenuButton;

        /// <summary>
        /// Called when the node is initialized. Sets up the resume, save and quit buttons,
        /// hides the menu by default, and sets the process mode to Always.
        /// </summary>
        public override void _Ready()
        {
            _titleLabel = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/TitleLabel");
            _resumeButton = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ResumeButton");
            _saveButton = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/SaveButton");
            _mainMenuButton = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MainMenuButton");

            _titleLabel.Text = "UI_PAUSE_TITLE";
            _resumeButton.Text = "UI_PAUSE_RESUME";
            _saveButton.Text = "UI_PAUSE_SAVE";
            _mainMenuButton.Text = "UI_PAUSE_MAIN_MENU";

            _resumeButton.Pressed += OnResumePressed;
            _saveButton.Pressed += OnSavePressed;
            _mainMenuButton.Pressed += OnMainMenuPressed;

            Hide();

            ProcessMode = ProcessModeEnum.Always;

            GD.Print("[PauseMenu] Initialized");
        }

        /// <summary>
        /// Handles user input. Pressing Esc toggles the pause state.
        /// </summary>
        /// <param name="event">The input event.</param>
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("ui_accept"))
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
        /// Pauses the game and shows the menu with a fade-in animation.
        /// </summary>
        public void Pause()
        {
            Show();
            GetTree().Paused = true;

            Modulate = new Color(1, 1, 1, 0);
            Tween tween = CreateTween();
            _ = tween.SetPauseMode(Tween.TweenPauseMode.Process);
            _ = tween.TweenProperty(this, "modulate:a", 1.0f, 0.15f);

            GD.Print("[PauseMenu] Paused");
        }

        /// <summary>
        /// Unpauses the game with a fade-out animation.
        /// </summary>
        public void Resume()
        {
            Tween tween = CreateTween();
            _ = tween.SetPauseMode(Tween.TweenPauseMode.Process);
            _ = tween.TweenProperty(this, "modulate:a", 0.0f, 0.15f);
            _ = tween.TweenCallback(Callable.From(() =>
            {
                Hide();
                GetTree().Paused = false;
                GD.Print("[PauseMenu] Resumed");
            }));
        }

        private void OnResumePressed()
        {
            Resume();
        }

        private void OnSavePressed()
        {
            _ = SaveSystem.Instance.SaveGame();
            GD.Print("[PauseMenu] Game saved");
        }

        private void OnMainMenuPressed()
        {
            GetTree().Paused = false;
            SceneManager.GoToMainMenu();
        }
    }
}
