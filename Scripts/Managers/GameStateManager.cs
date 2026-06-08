using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер GameStateManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class GameStateManager : Node, IGameStateManager
    {
        /// <summary>
        /// Элемент Instance.
        /// </summary>
        public static GameStateManager Instance { get; private set; } = null!;

        [Signal]
        /// <summary>
        /// Элемент StateChangedEventHandler.
        /// </summary>
        public delegate void StateChangedEventHandler(GameState newState);

        [Signal]
        /// <summary>
        /// Элемент WaveChangedEventHandler.
        /// </summary>
        public delegate void WaveChangedEventHandler(int waveNumber);

        [Signal]
        /// <summary>
        /// Элемент CoinsChangedEventHandler.
        /// </summary>
        public delegate void CoinsChangedEventHandler(int newAmount);

        [Signal]
        /// <summary>
        /// Элемент HeroStatsChangedEventHandler.
        /// </summary>
        public delegate void HeroStatsChangedEventHandler();

        private GameState _currentState;

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

        public SaveData CurrentSave { get; private set; }

        /// <summary>
        /// Элемент CurrentWave.
        /// </summary>
        public int CurrentWave => CurrentSave?.CurrentWave ?? 1;

        /// <summary>
        /// Элемент Coins.
        /// </summary>
        public int Coins => CurrentSave?.Coins ?? 0;

        public bool IsGameActive { get; private set; }

        /// <summary>
        /// Элемент _Ready.
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
        /// Запускает NewGame.
        /// </summary>
        public void StartNewGame()
        {
            CurrentSave = SaveData.CreateNew();
            IsGameActive = true;
            CurrentState = GameState.Battle;

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);
            _ = EmitSignal(SignalName.HeroStatsChanged);

            InventoryManager.Instance?.LoadFromData(CurrentSave);

            GD.Print("[GameStateManager] New game started with dual heroes - Wave 1");
        }

        /// <summary>
        /// Загружает Game.
        /// </summary>
        public void LoadGame(SaveData saveData)
        {
            CurrentSave = saveData;
            IsGameActive = true;
            CurrentState = GameState.Battle;

            _ = EmitSignal(SignalName.WaveChanged, CurrentSave.CurrentWave);
            _ = EmitSignal(SignalName.CoinsChanged, CurrentSave.Coins);
            _ = EmitSignal(SignalName.HeroStatsChanged);

            InventoryManager.Instance?.LoadFromData(CurrentSave);

            GD.Print($"[GameStateManager] Game loaded - Wave {CurrentSave.CurrentWave}");
        }

        /// <summary>
        /// Элемент NextWave.
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
        /// Добавляет Coins.
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
        /// Элемент SpendCoins.
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
        /// Обновляет HeroStats.
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
        /// Элемент ChangeState.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            GD.Print($"[GameStateManager] State changed to {newState}");
        }

        /// <summary>
        /// Элемент EndGame.
        /// </summary>
        public void EndGame(bool isVictory)
        {
            IsGameActive = false;
            CurrentState = isVictory ? GameState.Victory : GameState.GameOver;

            GD.Print($"[GameStateManager] Game ended - {(isVictory ? "Victory" : "Defeat")}");
        }

        /// <summary>
        /// Элемент ReturnToMainMenu.
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
