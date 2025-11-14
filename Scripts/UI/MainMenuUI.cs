using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles menu navigation and game start.
    /// </summary>
    public partial class MainMenuUI : Control
    {
        // Button references
        private Button _newGameButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _creditsButton;
        private Button _quitButton;

        // Panel references
        private Control _mainMenuPanel;
        private Control _settingsPanel;
        private Control _creditsPanel;

        public override void _Ready()
        {
            // Get main menu panel
            _mainMenuPanel = GetNode<Control>("MainMenuPanel");

            // Get buttons
            _newGameButton = GetNode<Button>("MainMenuPanel/VBoxContainer/NewGameButton");
            _continueButton = GetNode<Button>("MainMenuPanel/VBoxContainer/ContinueButton");
            _settingsButton = GetNode<Button>("MainMenuPanel/VBoxContainer/SettingsButton");
            _creditsButton = GetNode<Button>("MainMenuPanel/VBoxContainer/CreditsButton");
            _quitButton = GetNode<Button>("MainMenuPanel/VBoxContainer/QuitButton");

            // Get other panels
            _settingsPanel = GetNode<Control>("SettingsPanel");
            _creditsPanel = GetNode<Control>("CreditsPanel");

            // Connect button signals
            _newGameButton.Pressed += OnNewGamePressed;
            _continueButton.Pressed += OnContinuePressed;
            _settingsButton.Pressed += OnSettingsPressed;
            _creditsButton.Pressed += OnCreditsPressed;
            _quitButton.Pressed += OnQuitPressed;

            // Check if save exists and enable/disable continue button
            UpdateContinueButton();

            // Show main panel, hide others
            ShowMainMenu();

            GD.Print("[MainMenuUI] Initialized");
        }

        /// <summary>
        /// Updates continue button state based on save file existence.
        /// </summary>
        private void UpdateContinueButton()
        {
            bool saveExists = SaveSystem.Instance.SaveFileExists();
            _continueButton.Disabled = !saveExists;
            _continueButton.TooltipText = saveExists
                ? "Continue from last save"
                : "No save file found";
        }

        /// <summary>
        /// Shows main menu panel.
        /// </summary>
        private void ShowMainMenu()
        {
            _mainMenuPanel.Show();
            _settingsPanel.Hide();
            _creditsPanel.Hide();
        }

        /// <summary>
        /// Called when New Game button is pressed.
        /// </summary>
        private void OnNewGamePressed()
        {
            GD.Print("[MainMenuUI] New Game pressed");
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Start new game directly (no character selection)
            GameStateManager.Instance.StartNewGame();
            SceneManager.Instance.LoadScene(GameState.Battle);
        }

        /// <summary>
        /// Called when Continue button is pressed.
        /// </summary>
        private void OnContinuePressed()
        {
            GD.Print("[MainMenuUI] Continue pressed");

            // Play button sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Load game
            SceneManager.ContinueGame();
        }

        /// <summary>
        /// Called when Settings button is pressed.
        /// </summary>
        private void OnSettingsPressed()
        {
            GD.Print("[MainMenuUI] Settings pressed");

            // Play button sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Show settings panel
            _mainMenuPanel.Hide();
            _settingsPanel.Show();
        }

        /// <summary>
        /// Called when Credits button is pressed.
        /// </summary>
        private void OnCreditsPressed()
        {
            GD.Print("[MainMenuUI] Credits pressed");

            // Play button sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Show credits panel
            _mainMenuPanel.Hide();
            _creditsPanel.Show();
        }

        /// <summary>
        /// Called when Quit button is pressed.
        /// </summary>
        private void OnQuitPressed()
        {
            GD.Print("[MainMenuUI] Quit pressed");

            // Play button sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Quit game
            GetTree().Quit();
        }

        /// <summary>
        /// Returns to main menu from sub-panels.
        /// Called by back buttons in settings/credits.
        /// </summary>
        public void OnBackToMainMenu()
        {
            GD.Print("[MainMenuUI] Back to main menu");

            // Play button sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            ShowMainMenu();
        }
    }
}