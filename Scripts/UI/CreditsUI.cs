using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI-компонент экрана титров. Отвечает за отображение информации о разработчиках и используемых технологиях.
    /// </summary>
    public partial class CreditsUI : Control
    {
        private Button _backButton;
        private RichTextLabel _creditsText;

        /// <summary>
        /// Вызывается при готовности узла. Инициализирует элементы интерфейса, подписывается на нажатие кнопки "Назад" и заполняет текст титров.
        /// </summary>
        public override void _Ready()
        {
            _backButton = GetNode<Button>("CreditsMenu/Panel/VBoxContainer/BackButton");
            _creditsText = GetNode<RichTextLabel>("CreditsMenu/Panel/VBoxContainer/ScrollContainer/CreditsText");

            _backButton.Pressed += OnBackPressed;


            SetupCreditsContent();

            GD.Print("[CreditsUI] Initialized");
        }

        private void SetupCreditsContent()
        {
            _creditsText.BbcodeEnabled = true;
            _creditsText.Text = $@"[center][b][font_size=32]{Tr("UI_CREDITS_GAME_TITLE")}[/font_size][/b]

[font_size=20]{Tr("UI_CREDITS_SUBTITLE")}[/font_size]

[font_size=16]-------------------------[/font_size]

[b]{Tr("UI_CREDITS_TEAM_TITLE")}[/b]

[b]{Tr("UI_CREDITS_PROGRAMMING")}[/b]
{Tr("AUTHOR_NAME")}

[b]{Tr("UI_CREDITS_ART")}[/b]
{Tr("AUTHOR_NAME")}

[b]{Tr("UI_CREDITS_AUDIO")}[/b]
{Tr("AUTHOR_NAME")}

[font_size=16]-------------------------[/font_size]

[b]{Tr("UI_CREDITS_THANKS")}[/b]
Godot Engine Team
Community Contributors

[font_size=16]-------------------------[/font_size]

[b]{Tr("UI_CREDITS_BUILT_WITH")}[/b]
Godot Engine 4.5.1
C# / .NET 10.0

[font_size=14]{Tr("UI_CREDITS_COPYRIGHT")}[/font_size][/center]";
        }

        private void OnBackPressed()
        {
            GD.Print("[CreditsUI] Back pressed");

            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));

            if (GetParent() is MainMenuUI mainMenu)
            {
                mainMenu.OnBackToMainMenu();
                return;
            }

            SceneManager.GoToMainMenu();
        }
    }
}


