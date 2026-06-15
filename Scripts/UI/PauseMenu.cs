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
        private Button _resumeButton;
        private Button _saveButton;
        private Button _mainMenuButton;

        /// <summary>
        /// Вызывается при инициализации узла. Настраивает кнопки продолжения, сохранения и выхода,
        /// скрывает меню по умолчанию и устанавливает режим обработки Always.
        /// </summary>
        public override void _Ready()
        {
            _resumeButton = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ResumeButton");
            _saveButton = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/SaveButton");
            _mainMenuButton = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MainMenuButton");

            _resumeButton.Pressed += OnResumePressed;
            _saveButton.Pressed += OnSavePressed;
            _mainMenuButton.Pressed += OnMainMenuPressed;

            Hide();

            ProcessMode = ProcessModeEnum.Always;

            GD.Print("[PauseMenu] Initialized");
        }

        /// <summary>
        /// Обрабатывает пользовательский ввод. При нажатии Esc переключает состояние паузы.
        /// </summary>
        /// <param name="event">Событие пользовательского ввода.</param>
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
        /// Ставит игру на паузу и отображает меню с анимацией появления.
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
        /// Снимает игру с паузы с анимацией исчезновения.
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
