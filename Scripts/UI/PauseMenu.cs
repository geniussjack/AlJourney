using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI-компонент PauseMenu. Отвечает за отображение пользовательского интерфейса.
    /// </summary>
    public partial class PauseMenu : Control
    {
        private TextureButton _resumeButton;
        private TextureButton _mainMenuButton;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Stop;

            Panel overlay = new();
            overlay.SetAnchorsPreset(LayoutPreset.FullRect);
            StyleBoxFlat styleBox = new()
            {
                BgColor = new Color(0f, 0f, 0f, 0.7f)
            };
            overlay.AddThemeStyleboxOverride("panel", styleBox);
            overlay.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(overlay);

            CenterContainer centerContainer = new();
            centerContainer.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(centerContainer);

            PanelContainer menuPanel = new();
            centerContainer.AddChild(menuPanel);

            VBoxContainer vbox = new()
            {
                CustomMinimumSize = new Vector2(400, 0)
            };
            vbox.AddThemeConstantOverride("separation", 20);
            menuPanel.AddChild(vbox);

            MarginContainer margin = new();
            margin.AddThemeConstantOverride("margin_left", 30);
            margin.AddThemeConstantOverride("margin_right", 30);
            margin.AddThemeConstantOverride("margin_top", 30);
            margin.AddThemeConstantOverride("margin_bottom", 30);
            vbox.AddChild(margin);

            VBoxContainer innerVbox = new();
            innerVbox.AddThemeConstantOverride("separation", 24);
            margin.AddChild(innerVbox);

            Label titleLabel = new()
            {
                Text = "PAUSED",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 40);
            innerVbox.AddChild(titleLabel);

            HSeparator sep = new();
            innerVbox.AddChild(sep);

            _resumeButton = new TextureButton
            {
                TextureNormal = GD.Load<Texture2D>("res://Resources/Sprites/UI/atlas_btn_resume.tres"),
                IgnoreTextureSize = true,
                StretchMode = TextureButton.StretchModeEnum.Scale,
                CustomMinimumSize = new Vector2(369, 93)
            };
            innerVbox.AddChild(_resumeButton);

            _mainMenuButton = new TextureButton
            {
                TextureNormal = GD.Load<Texture2D>("res://Resources/Sprites/UI/atlas_btn_home.tres"),
                IgnoreTextureSize = true,
                StretchMode = TextureButton.StretchModeEnum.Scale,
                CustomMinimumSize = new Vector2(273, 93)
            };
            innerVbox.AddChild(_mainMenuButton);

            _resumeButton.Pressed += OnResumePressed;
            _mainMenuButton.Pressed += OnMainMenuPressed;

            Hide();

            ProcessMode = ProcessModeEnum.Always;

            GD.Print("[PauseMenu] Initialized");
        }

        /// <summary>
        /// Элемент _Input.
        /// </summary>
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

        /// <summary>
        /// Элемент Pause.
        /// </summary>
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

        /// <summary>
        /// Элемент Resume.
        /// </summary>
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
