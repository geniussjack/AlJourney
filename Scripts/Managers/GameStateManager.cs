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
        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static GameStateManager Instance { get; private set; }

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
                    _ = EmitSignal(SignalName.StateChanged, (int)value);
                }
            }
        }

        /// <summary>
        /// Active save data reference.
        /// </summary>
        public SaveData CurrentSave { get; private set; }

        /// <summary>
        /// Current wave number.
        /// </summary>
        public int CurrentWave => CurrentSave?.CurrentWave ?? 1;

        /// <summary>
        /// Player's total coins.
        /// </summary>
        public int Coins => CurrentSave?.Coins ?? 0;

        /// <summary>
        /// Is a game session currently active.
        /// </summary>
        public bool IsGameActive { get; private set; }

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }

            Instance = this;
            _currentState = GameState.MainMenu;
            IsGameActive = false;

            GD.Print("[GameStateManager] Initialized");
        }

        /// <summary>
        /// Starts a new game with both heroes.
        /// </summary>
        public void StartNewGame()
        {
            CurrentSave = SaveData.CreateNew();
            IsGameActive = true;
            CurrentState = GameState.Battle;

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);
            _ = EmitSignal(SignalName.HeroStatsChanged);

            GD.Print("[GameStateManager] New game started with dual heroes - Wave 1");
        }

        /// <summary>
        /// Loads existing save data.
        /// </summary>
        public void LoadGame(SaveData saveData)
        {
            CurrentSave = saveData;
            IsGameActive = true;
            CurrentState = GameState.Battle;

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);
            _ = EmitSignal(SignalName.HeroStatsChanged);

            GD.Print($"[GameStateManager] Game loaded - Wave {CurrentSave.CurrentWave}");
        }

        /// <summary>
        /// Advances to the next wave.
        /// </summary>
        public void NextWave()
        {
            if (CurrentSave == null)
            {
                return;
            }

            CurrentSave.CurrentWave++;

            // Update highest wave if current wave is higher
            if (CurrentSave.CurrentWave > CurrentSave.HighestWave)
            {
                CurrentSave.HighestWave = CurrentSave.CurrentWave;
                GD.Print($"[GameStateManager] New highest wave record: {CurrentSave.HighestWave}");
            }

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);

            GD.Print($"[GameStateManager] Advanced to wave {CurrentSave.CurrentWave}");
        }

        /// <summary>
        /// Adds coins to player's total.
        /// </summary>
        public void AddCoins(int amount)
        {
            if (CurrentSave == null || amount <= 0)
            {
                return;
            }

            CurrentSave.Coins += amount;
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);

            GD.Print($"[GameStateManager] Added {amount} coins. Total: {CurrentSave.Coins}");
        }

        /// <summary>
        /// Removes coins from player's total. Returns true if successful.
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (CurrentSave == null || amount <= 0 || CurrentSave.Coins < amount)
            {
                return false;
            }

            CurrentSave.Coins -= amount;
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);

            GD.Print($"[GameStateManager] Spent {amount} coins. Remaining: {CurrentSave.Coins}");
            return true;
        }

        /// <summary>
        /// Updates hero stats in save data.
        /// </summary>
        public void UpdateHeroStats(
            int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
            int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense)
        {
            if (CurrentSave == null)
            {
                return;
            }

            CurrentSave.MageHealth = mageHealth;
            CurrentSave.MageMaxHealth = mageMaxHealth;
            CurrentSave.MageDamage = mageDamage;
            CurrentSave.MageDefense = mageDefense;

            CurrentSave.WarriorHealth = warriorHealth;
            CurrentSave.WarriorMaxHealth = warriorMaxHealth;
            CurrentSave.WarriorDamage = warriorDamage;
            CurrentSave.WarriorDefense = warriorDefense;

            _ = EmitSignal(SignalName.HeroStatsChanged);
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
            IsGameActive = false;
            CurrentState = isVictory ? GameState.Victory : GameState.GameOver;

            GD.Print($"[GameStateManager] Game ended - {(isVictory ? "Victory" : "Defeat")}");
        }

        /// <summary>
        /// Returns to main menu and clears active session.
        /// </summary>
        public void ReturnToMainMenu()
        {
            IsGameActive = false;
            CurrentSave = null;
            CurrentState = GameState.MainMenu;

            GD.Print("[GameStateManager] Returned to main menu");
        }
    }
}
