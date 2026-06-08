using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI-компонент PauseMenu. Отвечает за логику меню паузы, приостановку игры и возврат в главное меню.
    /// Работает в связке с PauseMenu.tscn.
    /// </summary>
    public partial class PauseMenu : Control
    {
        private TextureButton _resumeButton;
        private TextureButton _mainMenuButton;

        public override void _Ready()
        {
            _resumeButton = GetNode<TextureButton>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ResumeButton");
            _mainMenuButton = GetNode<TextureButton>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MainMenuButton");

            _resumeButton.Pressed += OnResumePressed;
            _mainMenuButton.Pressed += OnMainMenuPressed;

            Hide();

            ProcessMode = ProcessModeEnum.Always;

            GD.Print("[PauseMenu] Initialized");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (Visible)
                    Resume();
                else
                    Pause();

                GetViewport().SetInputAsHandled();
            }
        }

        public void Pause()
        {
            Show();
            GetTree().Paused = true;

            Modulate = new Color(1, 1, 1, 0);
            Tween tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.15f);

            GD.Print("[PauseMenu] Paused");
        }

        public void Resume()
        {
            Tween tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.15f);
            tween.TweenCallback(Callable.From(() =>
            {
                Hide();
                GetTree().Paused = false;
                GD.Print("[PauseMenu] Resumed");
            }));
        }

        private void OnResumePressed()
        {
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            Resume();
        }

        private void OnMainMenuPressed()
        {
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            GetTree().Paused = false;
            SceneManager.GoToMainMenu();
        }
    }
}
