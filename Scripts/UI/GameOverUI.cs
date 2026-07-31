using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI for the game over screen. Displays the run's stats and a single "Exit" button that returns to
    /// the campaign map — a defeat no longer wipes progress or ends the session (see
    /// <see cref="Managers.SceneManager.GameOver"/>).
    /// </summary>
    public partial class GameOverUI : Control
    {
        private Label _waveReachedLabel;
        private Label _coinsCollectedLabel;
        private Label _enemiesDefeatedLabel;
        private Button _exitButton;

        /// <summary>
        /// Called when the node is initialized. Sets up references to labels and buttons, subscribes to press events, and displays the stats.
        /// </summary>
        public override void _Ready()
        {
            _waveReachedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/WaveLabel");
            _coinsCollectedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/CoinsLabel");
            _enemiesDefeatedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/EnemiesLabel");
            _exitButton = GetNode<Button>("CenterContainer/VBoxContainer/ButtonsContainer/ExitButton");

            _exitButton.Pressed += OnExitPressed;

            _exitButton.Text = Tr("UI_GAMEOVER_EXIT");
            GetNode<Label>("CenterContainer/VBoxContainer/Title").Text = Tr("UI_GAMEOVER_TITLE");
            GetNode<Label>("CenterContainer/VBoxContainer/Subtitle").Text = Tr("UI_GAMEOVER_SUBTITLE");
            GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/StatsTitle").Text = Tr("UI_GAMEOVER_STATS_TITLE");

            DisplayStats();

            GD.Print("[GameOverUI] Initialized");
        }

        private void DisplayStats()
        {
            SaveData saveData = GameStateManager.Instance.CurrentSave;

            if (saveData != null)
            {
                int waveReached = saveData.CurrentWave;
                int coinsCollected = saveData.Coins;
                int enemiesDefeated = CalculateEnemiesDefeated(waveReached);

                _waveReachedLabel.Text = $"{Tr("UI_GAMEOVER_WAVE_REACHED")} {waveReached}";
                _coinsCollectedLabel.Text = $"{Tr("UI_GAMEOVER_COINS")} {coinsCollected}";
                _enemiesDefeatedLabel.Text = $"{Tr("UI_GAMEOVER_ENEMIES_DEFEATED")} {enemiesDefeated}";

                GD.Print($"[GameOverUI] Stats - Wave: {waveReached}, Coins: {coinsCollected}, Enemies: {enemiesDefeated}");
            }
            else
            {
                _waveReachedLabel.Text = $"{Tr("UI_GAMEOVER_WAVE_REACHED")} 1";
                _coinsCollectedLabel.Text = $"{Tr("UI_GAMEOVER_COINS")} 0";
                _enemiesDefeatedLabel.Text = $"{Tr("UI_GAMEOVER_ENEMIES_DEFEATED")} 0";
            }
        }

        private static int CalculateEnemiesDefeated(int wave)
        {
            return wave * 4;
        }

        /// <summary>
        /// Exits the defeated battle back to the campaign map. Progress is kept — no save is deleted —
        /// and the party is healed to full so the player can immediately retry or pick a different level.
        /// </summary>
        private void OnExitPressed()
        {
            GD.Print("[GameOverUI] Exiting to campaign map after defeat");
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
            GameStateManager.Instance.HealPartyToFull();
            SceneManager.GoToMap();
        }
    }
}
