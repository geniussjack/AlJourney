using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Пользовательский интерфейс главного меню игры. Управляет навигацией между разделами: продолжение/новая игра, настройки, титры и выход из игры.
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

        /// <summary>
        /// Вызывается при готовности узла. Инициализирует ссылки на кнопки и панели, подписывается на события нажатия и отображает основной экран меню.
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
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.SaveFileExists();
            _continueButton.Disabled = !hasSave;
            _continueButton.Modulate = hasSave ? Colors.White : new Color(1, 1, 1, 0.45f);
        }

        private void OnNewGamePressed()
        {
            AudioManager.Instance?.PlayNewGameSound();
            GD.Print("[MainMenuUI] New game pressed");
            _ = SaveSystem.Instance.DeleteSave();
            GameStateManager.Instance.StartNewGame();
            SceneManager.Instance.LoadScene(GameState.Battle);
        }

        private void OnContinuePressed()
        {
            if (SaveSystem.Instance.SaveFileExists())
            {
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
        /// Скрывает все дополнительные панели и возвращает пользователя на основной экран главного меню.
        /// </summary>
        public void OnBackToMainMenu()
        {
            GD.Print("[MainMenuUI] Back to main menu");
            ShowMainMenu();
        }
    }
}
