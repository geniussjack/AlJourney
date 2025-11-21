using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Central manager for game state, progression, and runtime data.
    /// Singleton autoload node.
    /// </summary>
    public partial class GameStateManager : Node
    {
        private static GameStateManager _instance;

        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static GameStateManager Instance => _instance;

        // Signals
        [Signal]
        public delegate void StateChangedEventHandler(GameState newState);

        [Signal]
        public delegate void WaveChangedEventHandler(int waveNumber);

        [Signal]
        public delegate void CoinsChangedEventHandler(int newAmount);

        [Signal]
        public delegate void HeroStatsChangedEventHandler();

        // Current game state
        private GameState _currentState;
        private SaveData _currentSaveData;
        private bool _isGameActive;

        /// <summary>
        /// Current game flow state.
        /// </summary>
        public GameState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    EmitSignal(SignalName.StateChanged, (int)value);
                }
            }
        }

        /// <summary>
        /// Active save data reference.
        /// </summary>
        public SaveData CurrentSave => _currentSaveData;

        /// <summary>
        /// Current wave number.
        /// </summary>
        public int CurrentWave => _currentSaveData?.CurrentWave ?? 1;

        /// <summary>
        /// Player's total coins.
        /// </summary>
        public int Coins => _currentSaveData?.Coins ?? 0;

        /// <summary>
        /// Is a game session currently active.
        /// </summary>
        public bool IsGameActive => _isGameActive;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }

            _instance = this;
            _currentState = GameState.MainMenu;
            _isGameActive = false;

            GD.Print("[GameStateManager] Initialized");
        }

        /// <summary>
        /// Starts a new game with both heroes.
        /// </summary>
        public void StartNewGame()
        {
            _currentSaveData = SaveData.CreateNew();
            _isGameActive = true;
            CurrentState = GameState.Battle;

            EmitSignal(SignalName.WaveChanged, _currentSaveData.CurrentWave);
            EmitSignal(SignalName.CoinsChanged, _currentSaveData.Coins);
            EmitSignal(SignalName.HeroStatsChanged);

            GD.Print("[GameStateManager] New game started with dual heroes - Wave 1");
        }

        /// <summary>
        /// Loads existing save data.
        /// </summary>
        public void LoadGame(SaveData saveData)
        {
            _currentSaveData = saveData;
            _isGameActive = true;
            CurrentState = GameState.Battle;

            EmitSignal(SignalName.WaveChanged, _currentSaveData.CurrentWave);
            EmitSignal(SignalName.CoinsChanged, _currentSaveData.Coins);
            EmitSignal(SignalName.HeroStatsChanged);

            GD.Print($"[GameStateManager] Game loaded - Wave {_currentSaveData.CurrentWave}");
        }

        /// <summary>
        /// Advances to the next wave.
        /// </summary>
        public void NextWave()
        {
            if (_currentSaveData == null) return;

            _currentSaveData.CurrentWave++;
            EmitSignal(SignalName.WaveChanged, _currentSaveData.CurrentWave);

            GD.Print($"[GameStateManager] Advanced to wave {_currentSaveData.CurrentWave}");
        }

        /// <summary>
        /// Adds coins to player's total.
        /// </summary>
        public void AddCoins(int amount)
        {
            if (_currentSaveData == null || amount <= 0) return;

            _currentSaveData.Coins += amount;
            EmitSignal(SignalName.CoinsChanged, _currentSaveData.Coins);

            GD.Print($"[GameStateManager] Added {amount} coins. Total: {_currentSaveData.Coins}");
        }

        /// <summary>
        /// Removes coins from player's total. Returns true if successful.
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (_currentSaveData == null || amount <= 0 || _currentSaveData.Coins < amount)
                return false;

            _currentSaveData.Coins -= amount;
            EmitSignal(SignalName.CoinsChanged, _currentSaveData.Coins);

            GD.Print($"[GameStateManager] Spent {amount} coins. Remaining: {_currentSaveData.Coins}");
            return true;
        }

        /// <summary>
        /// Updates hero stats in save data.
        /// </summary>
        public void UpdateHeroStats(
            int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
            int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            if (_currentSaveData == null) return;

            _currentSaveData.MageHealth = mageHealth;
            _currentSaveData.MageMaxHealth = mageMaxHealth;
            _currentSaveData.MageDamage = mageDamage;
            _currentSaveData.MageDefense = mageDefense;

            _currentSaveData.WarriorHealth = warriorHealth;
            _currentSaveData.WarriorMaxHealth = warriorMaxHealth;
            _currentSaveData.WarriorDamage = warriorDamage;
            _currentSaveData.WarriorDefense = warriorDefense;

            EmitSignal(SignalName.HeroStatsChanged);
        }

        /// <summary>
        /// Transitions to a new game state.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            GD.Print($"[GameStateManager] State changed to {newState}");
        }

        /// <summary>
        /// Ends current game session (permadeath or victory).
        /// </summary>
        public void EndGame(bool isVictory)
        {
            _isGameActive = false;
            CurrentState = isVictory ? GameState.Victory : GameState.GameOver;

            GD.Print($"[GameStateManager] Game ended - {(isVictory ? "Victory" : "Defeat")}");
        }

        /// <summary>
        /// Returns to main menu and clears active session.
        /// </summary>
        public void ReturnToMainMenu()
        {
            _isGameActive = false;
            _currentSaveData = null;
            CurrentState = GameState.MainMenu;

            GD.Print("[GameStateManager] Returned to main menu");
        }
    }
}