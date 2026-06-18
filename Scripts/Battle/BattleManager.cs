using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.UI;
using AlJourney.Scripts.Utils;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Глобальный менеджер боевой системы. Управляет ходами,
    /// волнами противников, обработкой комбо-эффектов от доски Match-3,
    /// а также начислением урона и выдачей лута.
    /// </summary>
    public partial class BattleManager : Node, IBattleManager
    {
        /// <summary>
        /// Сигнал вызывается в начале новой битвы.
        /// </summary>
        [Signal]
        public delegate void BattleStartedEventHandler();

        /// <summary>
        /// Сигнал вызывается при смене текущей фазы боя.
        /// </summary>
        /// <param name="newPhase">Новая фаза из перечисления BattlePhase.</param>
        [Signal]
        public delegate void PhaseChangedEventHandler(BattlePhase newPhase);

        /// <summary>
        /// Сигнал вызывается, когда все враги в текущей волне побеждены.
        /// </summary>
        [Signal]
        public delegate void WaveCompletedEventHandler();

        /// <summary>
        /// Сигнал вызывается при окончании битвы.
        /// </summary>
        /// <param name="playerWon">True, если игрок победил.</param>
        [Signal]
        public delegate void BattleEndedEventHandler(bool playerWon);

        /// <summary>
        /// Сигнал вызывается каждый раз, когда погибает один из противников.
        /// </summary>
        /// <param name="enemy">Ссылка на побежденного врага.</param>
        [Signal]
        public delegate void EnemyDefeatedEventHandler(Enemy enemy);

        public int NecromancerTurnCount { get; private set; }
        public void IncrementNecromancerTurnCount()
        {
            NecromancerTurnCount++;
        }

        private GridManager _gridManager;
        private ComboSystem _comboSystem;
        private CameraShake _cameraShake;
        private GridUI _gridUI;
        private bool _isConnectedToGridManager = false;
        private bool _battleEndedSignaled;
        private bool _isWaveCompleted;
        private readonly List<ComboEffect> _accumulatedEffects = [];

        /// <summary>
        /// Текущая фаза битвы.
        /// </summary>
        public BattlePhase CurrentPhase { get; private set; }

        /// <summary>
        /// Номер текущей волны врагов.
        /// </summary>
        public int CurrentWave { get; private set; }

        /// <summary>
        /// Ссылка на систему двух героев.
        /// </summary>
        public DualHeroSystem HeroSystem { get; private set; }

        /// <summary>
        /// Список всех активных врагов на поле.
        /// </summary>
        public List<Enemy> Enemies { get; private set; }

        /// <summary>
        /// Инициализация менеджера и привязка к глобальным узлам.
        /// </summary>
        public override void _Ready()
        {
            Enemies = [];
            CurrentPhase = BattlePhase.PlayerSwap;
            NecromancerTurnCount = 0;
            _battleEndedSignaled = false;
            _isWaveCompleted = false;

            _gridManager = GetNode<GridManager>("/root/GridManager");
            _comboSystem = GetNode<ComboSystem>("/root/ComboSystem");

            if (!_isConnectedToGridManager)
            {
                _gridManager.SwapCompleted += OnSwapCompleted;
                _isConnectedToGridManager = true;
            }

            GD.Print("[BattleManager] Initialized for dual hero system");
        }

        /// <summary>
        /// Привязывает интерфейс сетки к боевому менеджеру.
        /// Необходимо для визуализации комбо-эффектов.
        /// </summary>
        /// <param name="gridUI">Экземпляр UI сетки.</param>
        public void Initialize(GridUI gridUI)
        {
            _gridUI = gridUI;
        }

        /// <summary>
        /// Запускает битву для указанной волны с переданной системой героев.
        /// </summary>
        /// <param name="heroSystem">Объект системы героев игрока.</param>
        /// <param name="waveNumber">Номер волны для генерации сложности.</param>
        /// <param name="cameraShake">Опциональный контроллер тряски камеры.</param>
        public void StartBattle(DualHeroSystem heroSystem, int waveNumber, CameraShake cameraShake = null)
        {
            HeroSystem = heroSystem;
            CurrentWave = waveNumber;
            NecromancerTurnCount = 0;
            _cameraShake = cameraShake;
            _battleEndedSignaled = false;
            _isWaveCompleted = false;

            HeroSystem.BothHeroesDied += OnBothHeroesDied;

            GenerateWaveEnemies();
            _gridManager.InitializeGrid();

            ChangePhase(BattlePhase.PlayerSwap);
            _ = EmitSignal(SignalName.BattleStarted);
        }

        private void ChangePhase(BattlePhase newPhase)
        {
            CurrentPhase = newPhase;
            _ = _gridUI?.CanInteract = CurrentPhase == BattlePhase.PlayerSwap;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
            GD.Print($"[BattleManager] Phase changed to {CurrentPhase}");
        }

        private void GenerateWaveEnemies()
        {
            Enemies.Clear();

            List<Enemy> newEnemies = EnemySpawner.GenerateWaveEnemies(CurrentWave);
            foreach (Enemy enemy in newEnemies)
            {
                enemy.CharacterDied += () => OnEnemyDied(enemy);
                Enemies.Add(enemy);
            }
        }

        private async void OnSwapCompleted(bool wasValid)
        {
            if (!wasValid)
            {
                return;
            }

            ChangePhase(BattlePhase.PlayerCombo);
            _accumulatedEffects.Clear();

            await ProcessMatchesRecursive();
        }

        private async void ProcessPlayerTurn()
        {
            ChangePhase(BattlePhase.PlayerCombo);
            _accumulatedEffects.Clear();
            _comboSystem.ResetCascade();

            await ProcessMatchesRecursive();
        }

        private async Task ProcessMatchesRecursive(bool isCascade = false)
        {
            List<MatchResult> matches = _gridManager.FindAllMatches();

            if (matches.Count == 0)
            {
                await ApplyAccumulatedEffects();
                return;
            }

            List<ComboEffect> comboEffects = _comboSystem.ProcessMatches(matches, isCascade);
            for (int i = 0; i < comboEffects.Count; i++)
            {
                PlayerCharacter activeHero = HeroSystem.GetHeroForElement(comboEffects[i].ElementType);
                if (activeHero?.IsAlive == false)
                {
                    comboEffects[i] = null;
                }
            }
            _accumulatedEffects.AddRange(comboEffects);

            _gridUI?.VisualizeMatchesAndEffects(matches, comboEffects);
            _gridManager.ProcessMatches(matches);

            _ = await ToSignal(GetTree().CreateTimer(0.6f), SceneTreeTimer.SignalName.Timeout);
            await ProcessMatchesRecursive(true);
        }

        private async Task ApplyAccumulatedEffects()
        {
            if (_accumulatedEffects.Count == 0)
            {
                await HandleEndOfPlayerTurn();
                return;
            }

            foreach (ComboEffect effect in _accumulatedEffects)
            {
                if (effect == null)
                {
                    continue;
                }
                
                ApplyComboEffect(effect);
                _ = await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
            }

            _accumulatedEffects.Clear();
            HeroSystem.ProcessStatusEffects();

            await HandleEndOfPlayerTurn();
        }

        private async Task HandleEndOfPlayerTurn()
        {
            if (_gridManager.RemainingSwaps > 0)
            {
                ChangePhase(BattlePhase.PlayerSwap);
            }
            else
            {
                await StartEnemyTurn();
            }
        }

        private void ApplyComboEffect(ComboEffect effect)
        {
            PlayerCharacter activeHero = HeroSystem.GetHeroForElement(effect.ElementType);
            if (activeHero?.IsAlive != true)
            {
                return;
            }

            if (effect.ElementType is ElementType.Fire or ElementType.Sword)
            {
                CombatEffectProcessor.ApplyDamageEffect(effect, activeHero, this, _cameraShake);
            }
            else if (effect.ElementType == ElementType.Heal)
            {
                CombatEffectProcessor.ApplyHealEffect(effect, HeroSystem, this, _cameraShake);
            }
            else if (effect.ElementType == ElementType.Shield)
            {
                CombatEffectProcessor.ApplyShieldEffect(effect, HeroSystem, this, _cameraShake);
            }
        }
        private async Task StartEnemyTurn()
        {
            ChangePhase(BattlePhase.EnemyTurn);

            List<Enemy> activeEnemies = [.. Enemies.Where(e => e.IsAlive)];
            foreach (Enemy enemy in activeEnemies)
            {
                enemy.ProcessStatusEffects();
            }

            foreach (Enemy enemy in activeEnemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                EnemyAIController.PerformEnemyAction(enemy, this, _cameraShake);
                _ = await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
            }

            if (!HeroSystem.IsAnyAlive)
            {
                return;
            }

            if (Enemies.All(e => !e.IsAlive))
            {
                OnWaveCompleted();
                return;
            }

            StartNextTurn();
        }

        private void StartNextTurn()
        {
            _gridManager.ResetSwaps();
            ChangePhase(BattlePhase.PlayerSwap);
        }

        internal void OnEnemyDied(Enemy enemy)
        {
            _ = EmitSignal(SignalName.EnemyDefeated, enemy);

            GameStateManager.Instance.AddCoins(enemy.CoinReward);

            if (enemy.IsBoss || enemy.IsMiniboss)
            {
                GenerateBossLoot();
            }
            else if (GD.Randf() <= 0.20f)
            {
                EquipmentData item = LootSystem.Instance.GenerateNormalLoot(CurrentWave);
                if (item != null)
                {
                    InventoryManager.Instance.AddItems([item]);
                }
            }

            if (Enemies.All(e => !e.IsAlive))
            {
                _ = CallDeferred(MethodName.OnWaveCompleted);
            }
        }

        private void OnBothHeroesDied()
        {
            if (_battleEndedSignaled)
            {
                return;
            }

            _battleEndedSignaled = true;
            _ = EmitSignal(SignalName.BattleEnded, false);
        }

        private void GenerateBossLoot()
        {
            List<EquipmentData> loot = LootSystem.Instance.GenerateBossLoot(CurrentWave);
            InventoryManager.Instance.AddItems(loot);
        }

        private void OnWaveCompleted()
        {
            if (_isWaveCompleted)
            {
                return;
            }

            _isWaveCompleted = true;

            ChangePhase(BattlePhase.WaveTransition);
            _ = EmitSignal(SignalName.WaveCompleted);

            (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) = HeroSystem.GetCombinedStats();
            GameStateManager.Instance.UpdateHeroStats(
                mageHealth, mageMaxHealth, mageDamage, mageDefense,
                warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense
            );

            GameStateManager.Instance.NextWave();
            SaveSystem.Instance.AutoSave();
        }

        /// <summary>
        /// Очищает состояние битвы, отписывается от сигналов и удаляет врагов.
        /// Вызывается при переходе на экран результатов или меню.
        /// </summary>
        public void EndBattle()
        {
            if (_gridManager != null && _isConnectedToGridManager)
            {
                _gridManager.SwapCompleted -= OnSwapCompleted;
                _isConnectedToGridManager = false;
            }

            HeroSystem?.BothHeroesDied -= OnBothHeroesDied;

            foreach (Enemy enemy in Enemies)
            {
                enemy.QueueFree();
            }
            Enemies.Clear();
        }
    }
}
