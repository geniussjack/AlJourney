using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Пользовательский интерфейс экрана завершения игры. Отображает финальную статистику и предоставляет кнопки для начала новой игры или выхода в главное меню.
    /// </summary>
    public partial class GameOverUI : Control
    {
        private Label _waveReachedLabel;
        private Label _coinsCollectedLabel;
        private Label _enemiesDefeatedLabel;
        private Button _mainMenuButton;
        private Button _newGameButton;

        /// <summary>
        /// Вызывается при инициализации узла. Настраивает ссылки на текстовые метки и кнопки, подписывается на события нажатия и запускает отображение статистики.
        /// </summary>
        public override void _Ready()
        {
            _waveReachedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/WaveLabel");
            _coinsCollectedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/CoinsLabel");
            _enemiesDefeatedLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatsContainer/EnemiesLabel");
            _mainMenuButton = GetNode<Button>("CenterContainer/VBoxContainer/ButtonsContainer/MainMenuButton");
            _newGameButton = GetNode<Button>("CenterContainer/VBoxContainer/ButtonsContainer/NewGameButton");

            _mainMenuButton.Pressed += OnMainMenuPressed;
            _newGameButton.Pressed += OnNewGamePressed;

            _mainMenuButton.Text = Tr("UI_GAMEOVER_MAIN_MENU");
            _newGameButton.Text = Tr("UI_GAMEOVER_RETRY");
            GetNode<Label>("CenterContainer/VBoxContainer/TitleLabel").Text = Tr("UI_GAMEOVER_TITLE");

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

        private void OnMainMenuPressed()
        {
            GD.Print("[GameOverUI] Returning to main menu");
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
            _ = SaveSystem.Instance.DeleteSave();
            SceneManager.GoToMainMenu();
        }

        private void OnNewGamePressed()
        {
            GD.Print("[GameOverUI] Starting new game");
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
            _ = SaveSystem.Instance.DeleteSave();
            GameStateManager.Instance.StartNewGame();
            SceneManager.Instance.LoadScene(Core.GameState.Battle);
        }
    }
}
