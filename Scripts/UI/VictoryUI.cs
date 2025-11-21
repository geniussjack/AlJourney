using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Victory screen UI controller.
    /// Displayed when player defeats the final boss (optional implementation).
    /// </summary>
    public partial class VictoryUI : Control
    {
        private Label _finalWaveLabel;
        private Label _totalCoinsLabel;
        private Label _totalEnemiesLabel;
        private Label _survivalTimeLabel;
        private Button _mainMenuButton;
        private Button _newGameButton;

        public override void _Ready()
        {
            // Get UI elements
            _finalWaveLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/WaveLabel");
            _totalCoinsLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/CoinsLabel");
            _totalEnemiesLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/EnemiesLabel");
            _survivalTimeLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/TimeLabel");
            _mainMenuButton = GetNode<Button>("CenterContainer/VBoxContainer/ButtonsContainer/MainMenuButton");
            _newGameButton = GetNode<Button>("CenterContainer/VBoxContainer/ButtonsContainer/NewGameButton");

            // Connect signals
            _mainMenuButton.Pressed += OnMainMenuPressed;
            _newGameButton.Pressed += OnNewGamePressed;

            // Display stats
            DisplayStats();

            GD.Print("[VictoryUI] Initialized");
        }

        /// <summary>
        /// Displays final victory statistics.
        /// </summary>
        private void DisplayStats()
        {
            SaveData saveData = GameStateManager.Instance.CurrentSave;

            if (saveData != null)
            {
                int finalWave = saveData.CurrentWave;
                int totalCoins = saveData.Coins;
                int enemiesDefeated = CalculateEnemiesDefeated(finalWave);

                _finalWaveLabel.Text = $"Final Wave: {finalWave}";
                _totalCoinsLabel.Text = $"Total Coins: 💰 {totalCoins}";
                _totalEnemiesLabel.Text = $"Enemies Defeated: ⚔️ {enemiesDefeated}";
                _survivalTimeLabel.Text = "🏆 Victory Achieved!";

                GD.Print($"[VictoryUI] Victory! Wave: {finalWave}, Coins: {totalCoins}, Enemies: {enemiesDefeated}");
            }
            else
            {
                _finalWaveLabel.Text = "Final Wave: 1";
                _totalCoinsLabel.Text = "Total Coins: 💰 0";
                _totalEnemiesLabel.Text = "Enemies Defeated: ⚔️ 0";
                _survivalTimeLabel.Text = "🏆 Victory!";
            }
        }

        /// <summary>
        /// Calculates total enemies defeated.
        /// </summary>
        private static int CalculateEnemiesDefeated(int wave)
        {
            return wave * 4;
        }

        /// <summary>
        /// Called when Main Menu button is pressed.
        /// </summary>
        private void OnMainMenuPressed()
        {
            GD.Print("[VictoryUI] Returning to main menu");

            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Clear save (optional)
            _ = SaveSystem.Instance.DeleteSave();

            // Return to main menu
            SceneManager.GoToMainMenu();
        }

        /// <summary>
        /// Called when New Game button is pressed.
        /// </summary>
        private void OnNewGamePressed()
        {
            GD.Print("[VictoryUI] Starting new game");

            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Delete old save
            _ = SaveSystem.Instance.DeleteSave();

            // Start new game
            GameStateManager.Instance.StartNewGame();
            SceneManager.Instance.LoadScene(Core.GameState.Battle);
        }
    }
}
