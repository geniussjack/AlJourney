using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI-компонент MainMenuUI. Отвечает за отображение пользовательского интерфейса.
    /// </summary>
    public partial class MainMenuUI : Control
    {
        private TextureButton _continueButton;
        private TextureButton _settingsButton;
        private TextureButton _creditsButton;
        private TextureButton _quitButton;

        private Control _mainMenuPanel;
        private Control _settingsPanel;
        private Control _creditsPanel;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            _mainMenuPanel = GetNode<Control>("MainMenuPanel");

            _continueButton = GetNode<TextureButton>("MainMenuPanel/VBoxContainer/ContinueButton");
            _settingsButton = GetNode<TextureButton>("MainMenuPanel/VBoxContainer/SettingsButton");
            _creditsButton  = GetNode<TextureButton>("MainMenuPanel/VBoxContainer/CreditsButton");
            _quitButton     = GetNode<TextureButton>("MainMenuPanel/VBoxContainer/QuitButton");

            _settingsPanel = GetNode<Control>("SettingsPanel");
            _creditsPanel  = GetNode<Control>("CreditsPanel");

            _continueButton.Pressed += OnPlayPressed;
            _settingsButton.Pressed += OnSettingsPressed;
            _creditsButton.Pressed  += OnCreditsPressed;
            _quitButton.Pressed     += OnQuitPressed;

            ShowMainMenu();

            GD.Print("[MainMenuUI] Initialized");
        }

        private void ShowMainMenu()
        {
            _mainMenuPanel.Show();
            _settingsPanel.Hide();
            _creditsPanel.Hide();
        }

        private void OnPlayPressed()
        {
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");

            if (SaveSystem.Instance.SaveFileExists())
            {
                GD.Print("[MainMenuUI] Save found - continuing game");
                SceneManager.ContinueGame();
            }
            else
            {
                GD.Print("[MainMenuUI] No save - starting new game");
                GameStateManager.Instance.StartNewGame();
                SceneManager.Instance.LoadScene(GameState.Battle);
            }
        }

        private void OnSettingsPressed()
        {
            GD.Print("[MainMenuUI] Settings pressed");
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            _mainMenuPanel.Hide();
            _settingsPanel.Show();
        }

        private void OnCreditsPressed()
        {
            GD.Print("[MainMenuUI] Credits pressed");
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            _mainMenuPanel.Hide();
            _creditsPanel.Show();
        }

        private void OnQuitPressed()
        {
            GD.Print("[MainMenuUI] Quit pressed");
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            GetTree().Quit();
        }

        /// <summary>
        /// Элемент OnBackToMainMenu.
        /// </summary>
        public void OnBackToMainMenu()
        {
            GD.Print("[MainMenuUI] Back to main menu");
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            ShowMainMenu();
        }
    }
}
