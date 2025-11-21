using AlJourney.Scripts.Battle;
using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.UI;
using Godot;

namespace AlJourney.Scripts.Scenes
{
    /// <summary>
    /// Main battle scene controller.
    /// Orchestrates all battle components: HUD, Grid, Heroes, Enemies, and BattleManager.
    /// </summary>
    public partial class BattleScene : Node
    {
        // Scene components
        private BattleHUD _battleHUD;
        private GridUI _gridUI;
        private BattleManager _battleManager;
        private DualHeroSystem _heroSystem;

        // Managers
        private GridManager _gridManager;
        private GameStateManager _gameStateManager;

        public override void _Ready()
        {
            // Get managers
            _gridManager = GetNode<GridManager>("/root/GridManager");
            _gameStateManager = GameStateManager.Instance;

            // Get scene components
            _battleHUD = GetNode<BattleHUD>("CanvasLayer/BattleHUD");
            _gridUI = GetNode<GridUI>("CanvasLayer/CenterContainer/GridUI");
            _battleManager = GetNode<BattleManager>("BattleManager");

            // Create hero system
            _heroSystem = new DualHeroSystem();
            AddChild(_heroSystem);

            // Initialize heroes from save or new game
            InitializeHeroes();

            // Initialize HUD
            _battleHUD.Initialize(_heroSystem);

            // Connect battle manager signals
            _battleManager.WaveCompleted += OnWaveCompleted;
            _battleManager.BattleEnded += OnBattleEnded;
            _battleManager.EnemyDefeated += OnEnemyDefeated;

            // Connect grid manager signals for HUD updates
            _gridManager.SwapCompleted += OnSwapCompleted;

            // Start battle
            int currentWave = _gameStateManager.CurrentWave;
            _battleManager.StartBattle(_heroSystem, currentWave);

            // Update HUD with enemies
            _battleHUD.SetupEnemies(_battleManager.Enemies);

            // Initialize grid
            _gridManager.InitializeGrid();

            GD.Print($"[BattleScene] Battle started - Wave {currentWave}");
        }

        /// <summary>
        /// Initializes heroes from save data or creates new heroes.
        /// </summary>
        private void InitializeHeroes()
        {
            SaveData saveData = _gameStateManager.CurrentSave;

            if (saveData != null)
            {
                // Load from save
                _heroSystem.LoadFromSave(
                    saveData.MageHealth, saveData.MageMaxHealth, saveData.MageDamage, saveData.MageDefense,
                    saveData.WarriorHealth, saveData.WarriorMaxHealth, saveData.WarriorDamage, saveData.WarriorDefense
                );

                GD.Print("[BattleScene] Heroes loaded from save");
            }
            else
            {
                // New game - heroes already initialized with base stats in DualHeroSystem._Ready()
                GD.Print("[BattleScene] New heroes created with base stats");
            }
        }

        /// <summary>
        /// Called when a swap is completed.
        /// </summary>
        private void OnSwapCompleted(bool wasValid)
        {
            if (wasValid)
            {
                // Update remaining swaps in HUD
                _battleHUD.UpdateSwaps(_gridManager.RemainingSwaps);
            }
        }

        /// <summary>
        /// Called when an enemy is defeated.
        /// </summary>
        private void OnEnemyDefeated(Enemy enemy)
        {
            GD.Print($"[BattleScene] Enemy defeated: {enemy.CharacterName}");
            // HUD will automatically update through enemy signals
        }

        /// <summary>
        /// Called when wave is completed.
        /// </summary>
        private void OnWaveCompleted()
        {
            GD.Print("[BattleScene] Wave completed! Transitioning to shop...");

            // Save hero stats before going to shop
            SaveHeroStats();

            // Transition to shop
            GetTree().CreateTimer(1.0f).Timeout += SceneManager.GoToShop;
        }

        /// <summary>
        /// Called when battle ends (player defeat).
        /// </summary>
        private void OnBattleEnded(bool playerWon)
        {
            if (!playerWon)
            {
                GD.Print("[BattleScene] Battle lost - transitioning to Game Over...");

                // Transition to game over screen
                GetTree().CreateTimer(1.5f).Timeout += SceneManager.GameOver;
            }
        }

        /// <summary>
        /// Saves hero stats to game state.
        /// </summary>
        private void SaveHeroStats()
        {
            (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) = _heroSystem.GetCombinedStats();
            _gameStateManager.UpdateHeroStats(
                mageHealth, mageMaxHealth, mageDamage, mageDefense,
                warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense
            );

            GD.Print("[BattleScene] Hero stats saved");
        }

        public override void _ExitTree()
        {
            // Disconnect signals
            if (_battleManager != null)
            {
                _battleManager.WaveCompleted -= OnWaveCompleted;
                _battleManager.BattleEnded -= OnBattleEnded;
                _battleManager.EnemyDefeated -= OnEnemyDefeated;
            }

            if (_gridManager != null)
            {
                _gridManager.SwapCompleted -= OnSwapCompleted;
            }

            // Cleanup battle
            _battleManager?.EndBattle();
        }
    }
}
