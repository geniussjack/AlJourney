using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI-компонент VictoryUI. Отвечает за отображение пользовательского интерфейса.
    /// </summary>
    public partial class VictoryUI : Control
    {
        private Label _finalWaveLabel;
        private Label _totalCoinsLabel;
        private Label _totalEnemiesLabel;
        private Label _survivalTimeLabel;
        private TextureButton _mainMenuButton;
        private TextureButton _newGameButton;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            _finalWaveLabel    = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/WaveLabel");
            _totalCoinsLabel   = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/CoinsLabel");
            _totalEnemiesLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/EnemiesLabel");
            _survivalTimeLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/TimeLabel");
            _mainMenuButton    = GetNode<TextureButton>("CenterContainer/VBoxContainer/ButtonsContainer/MainMenuButton");
            _newGameButton     = GetNode<TextureButton>("CenterContainer/VBoxContainer/ButtonsContainer/NewGameButton");

            _mainMenuButton.Pressed += OnMainMenuPressed;
            _newGameButton.Pressed  += OnNewGamePressed;

            DisplayStats();

            GD.Print("[VictoryUI] Initialized");
        }

        private void DisplayStats()
        {
            SaveData saveData = GameStateManager.Instance.CurrentSave;

            if (saveData != null)
            {
                int finalWave       = saveData.CurrentWave;
                int totalCoins      = saveData.Coins;
                int enemiesDefeated = CalculateEnemiesDefeated(finalWave);

                _finalWaveLabel.Text    = $"Final Wave: {finalWave}";
                _totalCoinsLabel.Text   = $"Coins: {totalCoins}";
                _totalEnemiesLabel.Text = $"Enemies Defeated: {enemiesDefeated}";
                _survivalTimeLabel.Text = "Victory Achieved!";

                GD.Print($"[VictoryUI] Victory! Wave: {finalWave}, Coins: {totalCoins}, Enemies: {enemiesDefeated}");
            }
            else
            {
                _finalWaveLabel.Text    = "Final Wave: 1";
                _totalCoinsLabel.Text   = "Coins: 0";
                _totalEnemiesLabel.Text = "Enemies Defeated: 0";
                _survivalTimeLabel.Text = "Victory!";
            }
        }

        private static int CalculateEnemiesDefeated(int wave)
        {
            return wave * 4;
        }

        private void OnMainMenuPressed()
        {
            GD.Print("[VictoryUI] Returning to main menu");
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            _ = SaveSystem.Instance.DeleteSave();
            SceneManager.GoToMainMenu();
        }

        private void OnNewGamePressed()
        {
            GD.Print("[VictoryUI] Starting new game");
            AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav");
            _ = SaveSystem.Instance.DeleteSave();
            GameStateManager.Instance.StartNewGame();
            SceneManager.Instance.LoadScene(Core.GameState.Battle);
        }
    }
}
