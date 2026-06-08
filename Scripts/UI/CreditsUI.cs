using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI-компонент CreditsUI. Отвечает за отображение пользовательского интерфейса.
    /// </summary>
    public partial class CreditsUI : Control
    {
        private TextureButton _backButton;
        private RichTextLabel _creditsText;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            _backButton  = GetNode<TextureButton>("CreditsMenu/Panel/VBoxContainer/BackButton");
            _creditsText = GetNode<RichTextLabel>("CreditsMenu/Panel/VBoxContainer/ScrollContainer/CreditsText");

            _backButton.Pressed += OnBackPressed;

            SetupCreditsContent();

            GD.Print("[CreditsUI] Initialized");
        }

        private void SetupCreditsContent()
        {
            _creditsText.BbcodeEnabled = true;
            _creditsText.Text = @"[center][b][font_size=32]AlJourney[/font_size][/b]

[font_size=20]A Match-3 RPG Roguelike[/font_size]

[font_size=16]-------------------------[/font_size]

[b]Development Team[/b]

[b]Game Design & Programming[/b]
Your Name Here

[b]Art & Graphics[/b]
Placeholder Assets

[b]Audio[/b]
Placeholder Sounds

[font_size=16]-------------------------[/font_size]

[b]Special Thanks[/b]
Godot Engine Team
Community Contributors

[font_size=16]-------------------------[/font_size]

[b]Built with[/b]
Godot Engine 4.5.1
C# / .NET 10.0

[font_size=14](C) 2026 All Rights Reserved[/font_size][/center]";
        }

        private void OnBackPressed()
        {
            GD.Print("[CreditsUI] Back pressed");

            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");

            MainMenuUI mainMenu = GetParent() as MainMenuUI;
            mainMenu?.OnBackToMainMenu();
        }
    }
}


