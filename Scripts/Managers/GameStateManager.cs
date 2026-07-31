using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Game state manager. Responsible for managing the global state, saving data, waves and resources.
    /// </summary>
    public partial class GameStateManager : Node, IGameStateManager
    {
        /// <summary>
        /// Global instance of the game state manager.
        /// </summary>
        public static GameStateManager Instance { get; private set; } = null!;

        [Signal]
        /// <summary>
        /// Raised when the global game state changes.
        /// </summary>
        /// <param name="newState">The new game state.</param>
        public delegate void StateChangedEventHandler(GameState newState);

        [Signal]
        /// <summary>
        /// Raised when the current wave changes.
        /// </summary>
        /// <param name="waveNumber">The new wave number.</param>
        public delegate void WaveChangedEventHandler(int waveNumber);

        [Signal]
        /// <summary>
        /// Raised when the player's coin count changes.
        /// </summary>
        /// <param name="newAmount">The new coin amount.</param>
        public delegate void CoinsChangedEventHandler(int newAmount);

        [Signal]
        /// <summary>
        /// Raised when the heroes' stats are updated.
        /// </summary>
        public delegate void HeroStatsChangedEventHandler();

        private GameState _currentState;

        /// <summary>
        /// The current global game state.
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
        /// The current game save data.
        /// </summary>
        public SaveData CurrentSave { get; private set; }

        /// <summary>
        /// The current enemy wave number. As of Stage 3, this no longer reflects an endless counter, but
        /// the <see cref="Data.LevelDefinition.DifficultyRating"/> of the campaign map level the player
        /// is currently on — used as-is by the existing scaling scale (rewards, shop prices, enemy stats).
        /// </summary>
        public int CurrentWave => CurrentSave?.CurrentWave ?? 1;

        /// <summary>
        /// Id of the campaign map level the player is currently on or should attempt next.
        /// </summary>
        public string CurrentLevelId => CurrentSave?.CurrentLevelId ?? CampaignDatabase.FirstLevelId;

        /// <summary>
        /// Ids of every campaign level already completed.
        /// </summary>
        public IReadOnlyCollection<string> CompletedLevelIds => CurrentSave?.CompletedLevelIds ?? (IReadOnlyCollection<string>)System.Array.Empty<string>();

        /// <summary>
        /// The current number of coins the player has.
        /// </summary>
        public int Coins => CurrentSave?.Coins ?? 0;

        /// <summary>
        /// Indicates whether a game session is currently active.
        /// </summary>
        public bool IsGameActive { get; private set; }

        /// <summary>
        /// Initializes the state manager node when it is added to the scene tree.
        /// </summary>
        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }

            Instance = this;
            _currentState = GameState.MainMenu;
            CurrentSave = new SaveData();

            GD.Print("[GameStateManager] Initialized");
        }

        /// <summary>
        /// Starts a new game, resetting progress and setting initial values.
        /// </summary>
        public void StartNewGame()
        {
            CurrentSave = SaveData.CreateNew();
            IsGameActive = true;
            CurrentState = GameState.Map;

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);
            _ = EmitSignal(SignalName.HeroStatsChanged);

            InventoryManager.Instance?.LoadFromData(CurrentSave);

            GD.Print("[GameStateManager] New game started with dual heroes - Wave 1");
        }

        /// <summary>
        /// Loads the game state from the provided save data.
        /// </summary>
        /// <param name="saveData">The save data to load.</param>
        public void LoadGame(SaveData saveData)
        {
            CurrentSave = saveData;
            IsGameActive = true;
            CurrentState = GameState.Map;

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);
            _ = EmitSignal(SignalName.HeroStatsChanged);

            InventoryManager.Instance?.LoadFromData(CurrentSave);

            GD.Print($"[GameStateManager] Game loaded - Wave {CurrentSave.CurrentWave}");
        }

        /// <summary>
        /// Advances to the next wave, updating the current wave number and the record.
        /// </summary>
        public void NextWave()
        {
            if (CurrentSave == null)
            {
                return;
            }

            CurrentSave.CurrentWave++;

            if (CurrentSave.CurrentWave > CurrentSave.HighestWave)
            {
                CurrentSave.HighestWave = CurrentSave.CurrentWave;
                GD.Print($"[GameStateManager] New highest wave record: {CurrentSave.HighestWave}");
            }

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);

            GD.Print($"[GameStateManager] Advanced to wave {CurrentSave.CurrentWave}");
        }

        /// <summary>
        /// Marks the level selected on the campaign map as the current one, without starting it right away
        /// (unlike <see cref="StartLevel"/>) — used by the map screen before transitioning to the battle
        /// scene, which will call <see cref="StartLevel"/> itself on start.
        /// </summary>
        /// <param name="levelId">Id of the level selected by the player on the map.</param>
        public void SelectLevel(string levelId)
        {
            if (CurrentSave == null || string.IsNullOrEmpty(levelId))
            {
                return;
            }

            CurrentSave.CurrentLevelId = levelId;
        }

        /// <summary>
        /// Begins an attempt at the given campaign map level: records it as the current one and carries
        /// its <see cref="LevelDefinition.DifficultyRating"/> over into <see cref="CurrentWave"/> for the
        /// existing scaling scale (enemy stats, rewards, shop prices).
        /// </summary>
        /// <param name="level">The level being entered.</param>
        public void StartLevel(LevelDefinition level)
        {
            if (CurrentSave == null || level == null)
            {
                return;
            }

            CurrentSave.CurrentLevelId = level.Id;
            CurrentSave.CurrentWave = level.DifficultyRating;

            if (CurrentSave.CurrentWave > CurrentSave.HighestWave)
            {
                CurrentSave.HighestWave = CurrentSave.CurrentWave;
            }

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);

            GD.Print($"[GameStateManager] Started level {level.Id} (difficulty {level.DifficultyRating})");
        }

        /// <summary>
        /// Marks a level as completed. For main-line levels, automatically advances progress to the
        /// next level on the line (see <see cref="CampaignDatabase.GetNextMainLevel"/>); completing a
        /// branch does not move main-line progress.
        /// </summary>
        /// <param name="levelId">Id of the completed level.</param>
        public void CompleteLevel(string levelId)
        {
            if (CurrentSave == null || string.IsNullOrEmpty(levelId))
            {
                return;
            }

            _ = CurrentSave.CompletedLevelIds.Add(levelId);

            LevelDefinition nextLevel = CampaignDatabase.GetNextMainLevel(levelId);
            if (nextLevel != null)
            {
                CurrentSave.CurrentLevelId = nextLevel.Id;
            }

            GD.Print($"[GameStateManager] Completed level {levelId}" + (nextLevel != null ? $", next: {nextLevel.Id}" : ""));
        }

        /// <summary>
        /// Adds the given number of coins to the current save.
        /// </summary>
        /// <param name="amount">The number of coins to add.</param>
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
        /// Spends the given number of coins, if there are enough available.
        /// </summary>
        /// <param name="amount">The number of coins to spend.</param>
        /// <returns><c>true</c> if the coins were successfully spent; otherwise <c>false</c>.</returns>
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
        /// Updates the base stats of both heroes in the save data.
        /// </summary>
        /// <param name="mageHealth">The mage's current health.</param>
        /// <param name="mageMaxHealth">The mage's maximum health.</param>
        /// <param name="mageDamage">The mage's damage.</param>
        /// <param name="mageDefense">The mage's defense.</param>
        /// <param name="warriorHealth">The warrior's current health.</param>
        /// <param name="warriorMaxHealth">The warrior's maximum health.</param>
        /// <param name="warriorDamage">The warrior's damage.</param>
        /// <param name="warriorDefense">The warrior's defense.</param>
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
        /// Changes the current global game state.
        /// </summary>
        /// <param name="newState">The new game state to transition to.</param>
        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            GD.Print($"[GameStateManager] State changed to {newState}");
        }

        /// <summary>
        /// Ends the current game, transitioning it into a victory or defeat state.
        /// </summary>
        /// <param name="isVictory"><c>true</c> if the game ended in victory; otherwise <c>false</c>.</param>
        public void EndGame(bool isVictory)
        {
            IsGameActive = false;
            CurrentState = isVictory ? GameState.Victory : GameState.GameOver;

            GD.Print($"[GameStateManager] Game ended - {(isVictory ? "Victory" : "Defeat")}");
        }

        /// <summary>
        /// Returns the game to the main menu, resetting the active session.
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
