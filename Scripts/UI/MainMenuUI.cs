using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI for the game's main menu. Handles navigation between sections: continue/new game, settings, credits and quitting the game.
    /// </summary>
    public partial class MainMenuUI : Control
    {
        private Button _newGameButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _creditsButton;
        private Button _quitButton;

        private Control _mainMenuPanel;
        private Control _settingsPanel;
        private Control _creditsPanel;

        private static bool _hasPlayedGameStart = false;

        /// <summary>
        /// Called when the node is ready. Initializes references to buttons and panels, subscribes to press events, and shows the main menu screen.
        /// </summary>
        public override void _Ready()
        {
            _mainMenuPanel = GetNode<Control>("MainMenuPanel");

            _newGameButton = GetNode<Button>("MainMenuPanel/VBoxContainer/NewGameButton");
            _continueButton = GetNode<Button>("MainMenuPanel/VBoxContainer/ContinueButton");
            _settingsButton = GetNode<Button>("MainMenuPanel/VBoxContainer/SettingsButton");
            _creditsButton = GetNode<Button>("MainMenuPanel/VBoxContainer/CreditsButton");
            _quitButton = GetNode<Button>("MainMenuPanel/VBoxContainer/QuitButton");

            _settingsPanel = GetNode<Control>("SettingsPanel");
            _creditsPanel = GetNode<Control>("CreditsPanel");

            _newGameButton.Pressed += OnNewGamePressed;
            _continueButton.Pressed += OnContinuePressed;
            _settingsButton.Pressed += OnSettingsPressed;
            _creditsButton.Pressed += OnCreditsPressed;
            _quitButton.Pressed += OnQuitPressed;

            _newGameButton.Text = "UI_MAIN_MENU_NEW_GAME";
            _continueButton.Text = "UI_MAIN_MENU_CONTINUE";
            _settingsButton.Text = "UI_MAIN_MENU_SETTINGS";
            _creditsButton.Text = "UI_MAIN_MENU_CREDITS";
            _quitButton.Text = "UI_MAIN_MENU_QUIT";

            ShowMainMenu();

            if (!_hasPlayedGameStart)
            {
                AudioManager.Instance?.PlayMusic("res://Resources/Audio/Music/game_start.mp3", false);
                _hasPlayedGameStart = true;
            }
            else
            {
                AudioManager.Instance?.StopMusic();
            }

            GD.Print("[MainMenuUI] Initialized");
        }

        private void ShowMainMenu()
        {
            _mainMenuPanel.Show();
            _settingsPanel.Hide();
            _creditsPanel.Hide();
            UpdateContinueButtonState();
        }

        private void UpdateContinueButtonState()
        {
            bool hasSave = SaveSystem.Instance?.SaveFileExists() == true;
            _continueButton.Disabled = !hasSave;
            _continueButton.Modulate = hasSave ? Colors.White : new Color(1, 1, 1, 0.45f);
        }

        private void OnNewGamePressed()
        {
            AudioManager.Instance?.PlayNewGameSound();
            AudioManager.Instance?.PlayMusic("res://Resources/Audio/Music/main_theme.mp3", true);
            GD.Print("[MainMenuUI] New game pressed");
            _ = SaveSystem.Instance.DeleteSave();
            GameStateManager.Instance.StartNewGame();
            SceneManager.Instance.LoadScene(GameState.Map);
        }

        private void OnContinuePressed()
        {
            if (SaveSystem.Instance.SaveFileExists())
            {
                AudioManager.Instance?.PlayMusic("res://Resources/Audio/Music/main_theme.mp3", true);
                GD.Print("[MainMenuUI] Save found - continuing game");
                SceneManager.ContinueGame();
            }
            else
            {
                GD.PrintErr("[MainMenuUI] Continue button pressed but no save found!");
            }
        }

        private void OnSettingsPressed()
        {
            GD.Print("[MainMenuUI] Settings pressed");
            _mainMenuPanel.Hide();
            _settingsPanel.Show();
        }

        private void OnCreditsPressed()
        {
            GD.Print("[MainMenuUI] Credits pressed");
            _mainMenuPanel.Hide();
            _creditsPanel.Show();
        }

        private void OnQuitPressed()
        {
            GD.Print("[MainMenuUI] Quit pressed");
            GetTree().Quit();
        }

        /// <summary>
        /// Hides every secondary panel and returns the user to the main menu's home screen.
        /// </summary>
        public void OnBackToMainMenu()
        {
            GD.Print("[MainMenuUI] Back to main menu");
            ShowMainMenu();
        }
    }
}
