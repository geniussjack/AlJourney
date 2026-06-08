using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер состояния игры. Отвечает за управление глобальным состоянием, сохранением данных, волнами и ресурсами.
    /// </summary>
    public partial class GameStateManager : Node, IGameStateManager
    {
        /// <summary>
        /// Глобальный экземпляр менеджера состояния игры (паттерн Singleton).
        /// </summary>
        public static GameStateManager Instance { get; private set; } = null!;

        [Signal]
        /// <summary>
        /// Событие, вызываемое при изменении глобального состояния игры.
        /// </summary>
        /// <param name="newState">Новое состояние игры.</param>
        public delegate void StateChangedEventHandler(GameState newState);

        [Signal]
        /// <summary>
        /// Событие, вызываемое при смене текущей волны.
        /// </summary>
        /// <param name="waveNumber">Номер новой волны.</param>
        public delegate void WaveChangedEventHandler(int waveNumber);

        [Signal]
        /// <summary>
        /// Событие, вызываемое при изменении количества монет у игрока.
        /// </summary>
        /// <param name="newAmount">Новое количество монет.</param>
        public delegate void CoinsChangedEventHandler(int newAmount);

        [Signal]
        /// <summary>
        /// Событие, вызываемое при обновлении характеристик героев.
        /// </summary>
        public delegate void HeroStatsChangedEventHandler();

        private GameState _currentState;

        /// <summary>
        /// Текущее глобальное состояние игры.
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
        /// Текущие данные сохранения игры.
        /// </summary>
        public SaveData CurrentSave { get; private set; }

        /// <summary>
        /// Номер текущей волны врагов.
        /// </summary>
        public int CurrentWave => CurrentSave?.CurrentWave ?? 1;

        /// <summary>
        /// Текущее количество монет у игрока.
        /// </summary>
        public int Coins => CurrentSave?.Coins ?? 0;

        /// <summary>
        /// Указывает, активна ли игра в данный момент (идет ли битва/прохождение).
        /// </summary>
        public bool IsGameActive { get; private set; }

        /// <summary>
        /// Инициализирует узел менеджера состояния при добавлении в дерево сцены.
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
        /// Запускает новую игру, сбрасывая прогресс и устанавливая начальные значения.
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
        /// Загружает состояние игры из предоставленных данных сохранения.
        /// </summary>
        /// <param name="saveData">Данные сохранения для загрузки.</param>
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
        /// Переходит к следующей волне, обновляя номер текущей волны и рекорд.
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
        /// Добавляет указанное количество монет в текущее сохранение.
        /// </summary>
        /// <param name="amount">Количество добавляемых монет.</param>
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
        /// Списывает указанное количество монет, если их достаточно на счету.
        /// </summary>
        /// <param name="amount">Количество монет для списания.</param>
        /// <returns><c>true</c>, если списание прошло успешно; иначе <c>false</c>.</returns>
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
        /// Обновляет базовые характеристики обоих героев в данных сохранения.
        /// </summary>
        /// <param name="mageHealth">Текущее здоровье мага.</param>
        /// <param name="mageMaxHealth">Максимальное здоровье мага.</param>
        /// <param name="mageDamage">Урон мага.</param>
        /// <param name="mageDefense">Защита мага.</param>
        /// <param name="warriorHealth">Текущее здоровье воина.</param>
        /// <param name="warriorMaxHealth">Максимальное здоровье воина.</param>
        /// <param name="warriorDamage">Урон воина.</param>
        /// <param name="warriorDefense">Защита воина.</param>
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
        /// Изменяет текущее глобальное состояние игры.
        /// </summary>
        /// <param name="newState">Новое состояние игры, в которое нужно перейти.</param>
        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            GD.Print($"[GameStateManager] State changed to {newState}");
        }

        /// <summary>
        /// Завершает текущую игру, переводя её в состояние победы или поражения.
        /// </summary>
        /// <param name="isVictory">Значение <c>true</c>, если игра завершилась победой; иначе <c>false</c>.</param>
        public void EndGame(bool isVictory)
        {
            IsGameActive = false;
            CurrentState = isVictory ? GameState.Victory : GameState.GameOver;

            GD.Print($"[GameStateManager] Game ended - {(isVictory ? "Victory" : "Defeat")}");
        }

        /// <summary>
        /// Возвращает игру в главное меню, сбрасывая активную сессию.
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
