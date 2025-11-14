using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Credits screen UI controller.
    /// Displays development team information.
    /// </summary>
    public partial class CreditsUI : Control
    {
        private Button _backButton;
        private RichTextLabel _creditsText;

        public override void _Ready()
        {
            _backButton = GetNode<Button>("VBoxContainer/BackButton");
            _creditsText = GetNode<RichTextLabel>("VBoxContainer/ScrollContainer/CreditsText");

            _backButton.Pressed += OnBackPressed;

            // Set credits content
            SetupCreditsContent();

            GD.Print("[CreditsUI] Initialized");
        }

        /// <summary>
        /// Sets up the credits text content.
        /// </summary>
        private void SetupCreditsContent()
        {
            _creditsText.BbcodeEnabled = true;
            _creditsText.Text = @"[center][b][font_size=32]AlJourney[/font_size][/b]

[font_size=20]A Match-3 RPG Roguelike[/font_size]

[font_size=16]─────────────────────────[/font_size]

[b]Development Team[/b]

[b]Game Design & Programming[/b]
Your Name Here

[b]Art & Graphics[/b]
Placeholder Assets

[b]Audio[/b]
Placeholder Sounds

[font_size=16]─────────────────────────[/font_size]

[b]Special Thanks[/b]
Godot Engine Team
Community Contributors

[font_size=16]─────────────────────────[/font_size]

[b]Built with[/b]
Godot Engine 4.5.1
C# / .NET 8.0

[font_size=14]© 2025 All Rights Reserved[/font_size][/center]";
        }

        /// <summary>
        /// Called when Back button is pressed.
        /// </summary>
        private void OnBackPressed()
        {
            GD.Print("[CreditsUI] Back pressed");

            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Get parent main menu and call back method
            var mainMenu = GetParent().GetNode<MainMenuUI>("../");
            mainMenu?.OnBackToMainMenu();
        }
    }
}