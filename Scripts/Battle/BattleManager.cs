using AlJourney.Scripts.Battle.Rules;
using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Utils;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Глобальный менеджер пошаговой боевой системы. Управляет очередью хода отряда игрока
    /// (игрок сам выбирает, кто из живых бойцов ходит следующим, затем его способность и цель),
    /// ходом врагов, волнами противников, а также начислением урона и выдачей лута.
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
        /// Сигнал вызывается при любом изменении состояния выбора хода игрока
        /// (выбор бойца, выбор способности, разрешение цели). Используется UI выбора цели для обновления.
        /// </summary>
        [Signal]
        public delegate void TurnStateChangedEventHandler();

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

        /// <summary>
        /// Сигнал вызывается при изменении общего заряда ульты отряда.
        /// </summary>
        /// <param name="charge">Текущее значение заряда.</param>
        /// <param name="maxCharge">Максимальное значение заряда.</param>
        [Signal]
        public delegate void UltimateChargeChangedEventHandler(int charge, int maxCharge);

        /// <summary>
        /// Максимальное значение общего заряда ульты отряда.
        /// </summary>
        public const int MaxUltimateCharge = 100;

        /// <summary>
        /// Фиксированный прирост заряда ульты за одно результативное действие
        /// (атака отряда, попавшая по врагу, либо атака врага, попавшая по отряду).
        /// </summary>
        public const int UltimateChargePerAction = 25;

        public int NecromancerTurnCount { get; private set; }
        public void IncrementNecromancerTurnCount()
        {
            NecromancerTurnCount++;
        }

        private CameraShake _cameraShake;
        private bool _battleEndedSignaled;
        private bool _isWaveCompleted;

        private List<PlayerCharacter> _pendingActors = [];

        /// <summary>
        /// Текущее значение общего заряда ульты отряда (0..<see cref="MaxUltimateCharge"/>).
        /// </summary>
        public int UltimateCharge { get; private set; }

        /// <summary>
        /// Истина, если заряд ульты полон и она доступна к применению.
        /// </summary>
        public bool IsUltimateReady => UltimateCharge >= MaxUltimateCharge;

        /// <summary>
        /// Текущая фаза битвы.
        /// </summary>
        public BattlePhase CurrentPhase { get; private set; }

        /// <summary>
        /// Номер текущей волны врагов.
        /// </summary>
        public int CurrentWave { get; private set; }

        /// <summary>
        /// Ссылка на систему отряда героев.
        /// </summary>
        public DualHeroSystem HeroSystem { get; private set; }

        /// <summary>
        /// Список всех активных врагов на поле.
        /// </summary>
        public List<Enemy> Enemies { get; private set; }

        /// <summary>
        /// Участники отряда, которые ещё не совершили ход в текущем раунде.
        /// </summary>
        public IReadOnlyList<PlayerCharacter> PendingActors => _pendingActors;

        /// <summary>
        /// Боец, выбранный игроком для текущего хода.
        /// </summary>
        public PlayerCharacter SelectedActor { get; private set; }

        /// <summary>
        /// Способность, выбранная для текущего хода.
        /// </summary>
        public AbilityData SelectedAbility { get; private set; }

        /// <summary>
        /// Инициализация менеджера боя.
        /// </summary>
        public override void _Ready()
        {
            Enemies = [];
            CurrentPhase = BattlePhase.PlayerTurn;
            NecromancerTurnCount = 0;
            _battleEndedSignaled = false;
            _isWaveCompleted = false;
            UltimateCharge = 0;

            GD.Print("[BattleManager] Initialized for party-based turn combat");
        }

        /// <summary>
        /// Запускает битву для указанной волны с переданной системой отряда.
        /// </summary>
        /// <param name="heroSystem">Объект системы отряда игрока.</param>
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
            UltimateCharge = 0;

            HeroSystem.PartyDefeated += OnPartyDefeated;

            GenerateWaveEnemies();
            StartPlayerTurn();

            _ = EmitSignal(SignalName.BattleStarted);
        }

        private void StartPlayerTurn()
        {
            HeroSystem.ProcessStatusEffects();

            _pendingActors = [.. HeroSystem.GetAliveMembers()];
            SelectedActor = null;
            SelectedAbility = null;

            ChangePhase(BattlePhase.PlayerTurn);
        }

        private void ChangePhase(BattlePhase newPhase)
        {
            CurrentPhase = newPhase;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        }

        /// <summary>
        /// Выбирает бойца, который совершит ход следующим. Игрок сам определяет порядок хода
        /// среди ещё не действовавших в этом раунде живых участников отряда.
        /// </summary>
        public void SelectActor(PlayerCharacter actor)
        {
            if (CurrentPhase != BattlePhase.PlayerTurn || actor is null || !_pendingActors.Contains(actor))
            {
                return;
            }

            SelectedActor = actor;
            SelectedAbility = null;
            _ = EmitSignal(SignalName.TurnStateChanged);
        }

        /// <summary>
        /// Выбирает способность, которую применит выбранный боец: атака, защита/поддержка,
        /// либо (при полном заряде) ультимативная способность. Ультимата разрешается немедленно,
        /// без отдельного подтверждения цели — она либо бьёт по площади, либо сама выбирает цель
        /// по своим правилам (см. <see cref="ResolveUltimate"/>).
        /// </summary>
        public void SelectAbility(AbilityData ability)
        {
            if (CurrentPhase != BattlePhase.PlayerTurn || SelectedActor is null || ability is null)
            {
                return;
            }

            if (ability.IsUltimate)
            {
                if (!IsUltimateReady)
                {
                    return;
                }

                ResolveUltimate(SelectedActor, ability);
                return;
            }

            SelectedAbility = ability;
            _ = EmitSignal(SignalName.TurnStateChanged);
        }

        /// <summary>
        /// Возвращает список допустимых целей для наведения выбранной способности.
        /// </summary>
        public IReadOnlyList<Character> GetValidTargets()
        {
            if (SelectedAbility is null)
            {
                return [];
            }

            IReadOnlyList<Character> allies = [.. HeroSystem.GetAliveMembers()];
            IReadOnlyList<Character> enemies = [.. Enemies.Where(e => e.IsAlive)];

            return AbilityTargetingRules.GetValidTargets(SelectedAbility.TargetType, allies, enemies, static c => c.IsAlive);
        }

        /// <summary>
        /// Подтверждает цель и немедленно разрешает эффект выбранной способности.
        /// Если это был последний ещё не походивший боец отряда — начинается ход врагов.
        /// </summary>
        public void ConfirmTarget(Character target)
        {
            if (CurrentPhase != BattlePhase.PlayerTurn || SelectedActor is null || SelectedAbility is null)
            {
                return;
            }

            if (!GetValidTargets().Contains(target))
            {
                return;
            }

            ResolveAbility(SelectedActor, SelectedAbility, target);
            AdvanceTurnAfterAction(SelectedActor);
        }

        private void ResolveAbility(PlayerCharacter caster, AbilityData ability, Character primaryTarget)
        {
            IReadOnlyList<Character> allies = [.. HeroSystem.GetAliveMembers()];
            IReadOnlyList<Character> enemies = [.. Enemies.Where(e => e.IsAlive)];
            IReadOnlyList<Character> targets = AbilityTargetingRules.ResolveEffectTargets(
                ability.TargetType, ability.IsAoE, primaryTarget, allies, enemies, static c => c.IsAlive);

            if (ability.IsAttackAbility)
            {
                CombatEffectProcessor.ApplyAttackAbility(ability, caster, targets, this, _cameraShake);
                if (targets.Count > 0)
                {
                    AddUltimateCharge(UltimateChargePerAction);
                }
            }
            else
            {
                CombatEffectProcessor.ApplySupportAbility(ability, targets, HeroSystem, this, _cameraShake);
            }
        }

        /// <summary>
        /// Немедленно разрешает ультимативную способность выбранного бойца и обнуляет общий заряд отряда.
        /// AoE-ульты бьют по всем живым врагам; одиночные — по врагу с наибольшим текущим HP (автовыбор,
        /// без участия игрока).
        /// </summary>
        private void ResolveUltimate(PlayerCharacter caster, AbilityData ultimate)
        {
            IReadOnlyList<Character> aliveEnemies = [.. Enemies.Where(e => e.IsAlive)];
            IReadOnlyList<Character> targets;

            if (ultimate.IsAoE)
            {
                targets = aliveEnemies;
            }
            else
            {
                Character highestHealthEnemy = AbilityTargetingRules.SelectHighestHealthTarget(
                    aliveEnemies, static c => c.CurrentHealth, static c => c.IsAlive);
                targets = highestHealthEnemy is null ? [] : [highestHealthEnemy];
            }

            CombatEffectProcessor.ApplyAttackAbility(ultimate, caster, targets, this, _cameraShake);

            UltimateCharge = 0;
            _ = EmitSignal(SignalName.UltimateChargeChanged, UltimateCharge, MaxUltimateCharge);

            AdvanceTurnAfterAction(caster);
        }

        /// <summary>
        /// Убирает бойца из очереди ещё не походивших участников раунда и либо передаёт ход дальше
        /// внутри отряда, либо (если это был последний боец) запускает ход врагов.
        /// </summary>
        private void AdvanceTurnAfterAction(PlayerCharacter actor)
        {
            _ = _pendingActors.Remove(actor);
            SelectedActor = null;
            SelectedAbility = null;

            if (_pendingActors.Count == 0)
            {
                StartEnemyTurn();
            }
            else
            {
                _ = EmitSignal(SignalName.TurnStateChanged);
            }
        }

        /// <summary>
        /// Увеличивает (с ограничением сверху и снизу) общий заряд ульты отряда и оповещает подписчиков.
        /// Вызывается как при результативных атаках отряда, так и при попаданиях атак врагов по отряду.
        /// </summary>
        /// <param name="amount">Величина прироста заряда.</param>
        internal void AddUltimateCharge(int amount)
        {
            int clamped = Mathf.Clamp(UltimateCharge + amount, 0, MaxUltimateCharge);
            if (clamped == UltimateCharge)
            {
                return;
            }

            UltimateCharge = clamped;
            _ = EmitSignal(SignalName.UltimateChargeChanged, UltimateCharge, MaxUltimateCharge);
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

        private async void StartEnemyTurn()
        {
            ChangePhase(BattlePhase.EnemyTurn);

            try
            {
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

                if (HeroSystem.GetAliveMembers().Count == 0)
                {
                    return;
                }

                if (Enemies.All(e => !e.IsAlive))
                {
                    OnWaveCompleted();
                    return;
                }
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[BattleManager] Error during enemy turn: {ex}");
            }

            StartPlayerTurn();
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
                Callable.From(OnWaveCompleted).CallDeferred();
            }
        }

        private void OnPartyDefeated()
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
            HeroSystem?.PartyDefeated -= OnPartyDefeated;

            foreach (Enemy enemy in Enemies)
            {
                enemy.QueueFree();
            }
            Enemies.Clear();
        }
    }
}
