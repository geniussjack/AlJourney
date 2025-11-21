using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Game Over screen UI controller.
    /// Displayed when both heroes die (permadeath).
    /// </summary>
    public partial class GameOverUI : Control
    {
        private Label _waveReachedLabel;
        private Label _coinsCollectedLabel;
        private Label _enemiesDefeatedLabel;
        private Button _mainMenuButton;
        private Button _newGameButton;

        public override void _Ready()
        {
            // Get UI elements
            _waveReachedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/WaveLabel");
            _coinsCollectedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/CoinsLabel");
            _enemiesDefeatedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/EnemiesLabel");
            _mainMenuButton = GetNode<Button>("CenterContainer/VBoxContainer/ButtonsContainer/MainMenuButton");
            _newGameButton = GetNode<Button>("CenterContainer/VBoxContainer/ButtonsContainer/NewGameButton");

            // Connect signals
            _mainMenuButton.Pressed += OnMainMenuPressed;
            _newGameButton.Pressed += OnNewGamePressed;

            // Display stats
            DisplayStats();

            GD.Print("[GameOverUI] Initialized");
        }

        /// <summary>
        /// Displays final game statistics.
        /// </summary>
        private void DisplayStats()
        {
            var saveData = GameStateManager.Instance.CurrentSave;

            if (saveData != null)
            {
                int waveReached = saveData.CurrentWave;
                int coinsCollected = saveData.Coins;

                // Calculate enemies defeated (approximate based on wave)
                int enemiesDefeated = CalculateEnemiesDefeated(waveReached);

                _waveReachedLabel.Text = $"Wave Reached: {waveReached}";
                _coinsCollectedLabel.Text = $"Coins Collected: 💰 {coinsCollected}";
                _enemiesDefeatedLabel.Text = $"Enemies Defeated: ⚔️ {enemiesDefeated}";

                GD.Print($"[GameOverUI] Stats - Wave: {waveReached}, Coins: {coinsCollected}, Enemies: {enemiesDefeated}");
            }
            else
            {
                _waveReachedLabel.Text = "Wave Reached: 1";
                _coinsCollectedLabel.Text = "Coins Collected: 💰 0";
                _enemiesDefeatedLabel.Text = "Enemies Defeated: ⚔️ 0";
            }
        }

        /// <summary>
        /// Calculates approximate number of enemies defeated based on wave.
        /// </summary>
        private static int CalculateEnemiesDefeated(int wave)
        {
            // Approximate: 3-5 enemies per wave
            return wave * 4;
        }

        /// <summary>
        /// Called when Main Menu button is pressed.
        /// </summary>
        private void OnMainMenuPressed()
        {
            GD.Print("[GameOverUI] Returning to main menu");

            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Delete save file (permadeath)
            SaveSystem.Instance.DeleteSave();

            // Return to main menu
            SceneManager.GoToMainMenu();
        }

        /// <summary>
        /// Called when New Game button is pressed.
        /// </summary>
        private void OnNewGamePressed()
        {
            GD.Print("[GameOverUI] Starting new game");

            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Delete old save
            SaveSystem.Instance.DeleteSave();

            // Start new game
            GameStateManager.Instance.StartNewGame();
            SceneManager.Instance.LoadScene(Core.GameState.Battle);
        }
    }
}