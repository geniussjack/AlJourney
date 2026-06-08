using System.Threading.Tasks;
using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.UI;
using AlJourney.Scripts.Utils;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Глобальный менеджер боевой системы. Управляет ходами (игрока и врагов),
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
        /// Сигнал вызывается при смене текущей фазы боя (например, с хода игрока на ход врага).
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
        /// Сигнал вызывается при окончании битвы (поражение или победа во всех волнах).
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

        private int _necromancerTurnCount;
        private GridManager _gridManager;
        private ComboSystem _comboSystem;
        private CameraShake _cameraShake;
        private GridUI _gridUI;
        private bool _isConnectedToGridManager = false;
        private bool _battleEndedSignaled;
        private readonly List<ComboEffect> _accumulatedEffects = new List<ComboEffect>();

        /// <summary>
        /// Текущая фаза битвы.
        /// </summary>
        public BattlePhase CurrentPhase { get; private set; }

        /// <summary>
        /// Номер текущей волны врагов.
        /// </summary>
        public int CurrentWave { get; private set; }

        /// <summary>
        /// Ссылка на систему двух героев (Маг и Воин).
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
            Enemies = new List<Enemy>();
            CurrentPhase = BattlePhase.PlayerSwap;
            _necromancerTurnCount = 0;
            _battleEndedSignaled = false;

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
        /// Привязывает интерфейс сетки (UI) к боевому менеджеру.
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
            _necromancerTurnCount = 0;
            _cameraShake = cameraShake;
            _battleEndedSignaled = false;

            HeroSystem.BothHeroesDied += OnBothHeroesDied;

            GenerateWaveEnemies();
            _gridManager.InitializeGrid();

            ChangePhase(BattlePhase.PlayerSwap);
            _ = EmitSignal(SignalName.BattleStarted);
        }

        private void ChangePhase(BattlePhase newPhase)
        {
            CurrentPhase = newPhase;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        }

        private void GenerateWaveEnemies()
        {
            Enemies.Clear();

            int enemyCount = ScalingSystem.GetEnemyCount(CurrentWave);
            bool skeletonUnlocked = ScalingSystem.IsSkeletonUnlocked(CurrentWave);

            for (int i = 0; i < enemyCount; i++)
            {
                EnemyType type = GetEnemyTypeForWave(skeletonUnlocked);
                Enemy enemy = Enemy.Create(type, CurrentWave);
                enemy.CharacterDied += () => OnEnemyDied(enemy);
                Enemies.Add(enemy);
            }

            GD.Print($"[BattleManager] Wave {CurrentWave}: {enemyCount} enemies");
        }

        private static EnemyType GetEnemyTypeForWave(bool skeletonUnlocked)
        {
            if (!skeletonUnlocked) return EnemyType.Slime;
            return GD.Randf() < 0.5f ? EnemyType.Slime : EnemyType.SkeletonWarrior;
        }

        private async void OnSwapCompleted(bool wasValid)
        {
            if (!wasValid) return;

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
            _accumulatedEffects.AddRange(comboEffects);

            _gridUI?.VisualizeMatchesAndEffects(matches, comboEffects);
            _gridManager.ProcessMatches(matches);

            await ToSignal(GetTree().CreateTimer(0.6f), SceneTreeTimer.SignalName.Timeout);
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
                ApplyComboEffect(effect);
                await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
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
            if (activeHero == null || !activeHero.IsAlive) return;

            if (effect.ElementType == ElementType.Fire || effect.ElementType == ElementType.Sword)
            {
                ApplyDamageEffect(effect, activeHero);
            }
            else if (effect.ElementType == ElementType.Heal)
            {
                ApplyHealEffect(effect, activeHero);
            }
            else if (effect.ElementType == ElementType.Shield)
            {
                ApplyShieldEffect(effect, activeHero);
            }
        }

        private void ApplyDamageEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int damage = activeHero.CalculateDamage(effect.Damage, effect.ElementType);
            
            if (effect.IsAoE)
            {
                _cameraShake?.ShakeStrong();
                ComboParticles.SpawnComboEffect(this, new Vector2(640, 360), effect.ElementType, effect.ComboLevel);
                
                foreach (Enemy enemy in Enemies.Where(e => e.IsAlive))
                {
                    DealDamageToEnemy(enemy, damage, effect, activeHero, isAoE: true);
                }
            }
            else
            {
                Enemy target = Enemies.FirstOrDefault(e => e.IsAlive);
                if (target != null)
                {
                    _cameraShake?.ShakeMedium();
                    ComboParticles.SpawnComboEffect(this, new Vector2(640, 300), effect.ElementType, effect.ComboLevel);
                    DealDamageToEnemy(target, damage, effect, activeHero, isAoE: false);
                }
            }
        }

        private void DealDamageToEnemy(Enemy target, int damage, ComboEffect effect, PlayerCharacter activeHero, bool isAoE)
        {
            int reflected = target.TakeDamage(damage, activeHero.AttackType, canReflect: true);
            Vector2 particlePos = isAoE ? new Vector2(400, 200) : new Vector2(640, 250);
            
            ComboParticles.SpawnDamageNumber(this, particlePos, damage);

            if (effect.StatusEffect != null)
            {
                target.ApplyStatusEffect(effect.StatusEffect);
            }

            if (reflected > 0)
            {
                _ = activeHero.TakeDamage(reflected, target.AttackType, canReflect: false);
            }
        }

        private void ApplyHealEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int healing = PlayerCharacter.CalculateHealing(effect.Healing);
            _cameraShake?.ShakeLight();
            ComboParticles.SpawnComboEffect(this, new Vector2(640, 360), ElementType.Heal, effect.ComboLevel);

            HeroSystem.Mage.Heal(healing);
            HeroSystem.Warrior.Heal(healing);

            ComboParticles.SpawnHealNumber(this, new Vector2(200, 100), healing);
            ComboParticles.SpawnHealNumber(this, new Vector2(1000, 100), healing);

            if (effect.ComboLevel == 2)
            {
                HeroSystem.Mage.ClearNegativeEffects();
                HeroSystem.Warrior.ClearNegativeEffects();
            }

            if (effect.StatusEffect != null)
            {
                HeroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                HeroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        private void ApplyShieldEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int shield = PlayerCharacter.CalculateShield(effect.Shield);
            _cameraShake?.ShakeLight();
            ComboParticles.SpawnComboEffect(this, new Vector2(640, 360), ElementType.Shield, effect.ComboLevel);

            HeroSystem.Mage.AddShield(shield);
            HeroSystem.Warrior.AddShield(shield);

            ComboParticles.SpawnShieldNumber(this, new Vector2(200, 100), shield);
            ComboParticles.SpawnShieldNumber(this, new Vector2(1000, 100), shield);

            if (effect.StatusEffect != null)
            {
                HeroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                HeroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        private async Task StartEnemyTurn()
        {
            ChangePhase(BattlePhase.EnemyTurn);
            
            List<Enemy> activeEnemies = Enemies.Where(e => e.IsAlive).ToList();
            foreach (Enemy enemy in activeEnemies)
            {
                enemy.ProcessStatusEffects();
            }

            foreach (Enemy enemy in activeEnemies)
            {
                if (!enemy.IsAlive) continue;

                PerformEnemyAction(enemy);
                await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
            }

            if (!HeroSystem.IsAnyAlive) return; 

            if (Enemies.All(e => !e.IsAlive))
            {
                OnWaveCompleted();
                return;
            }

            StartNextTurn();
        }

        private void PerformEnemyAction(Enemy enemy)
        {
            if (enemy.IsStunned) return; 
            
            List<PlayerCharacter> aliveHeroes = new List<PlayerCharacter>();
            if (HeroSystem.Mage.IsAlive) aliveHeroes.Add(HeroSystem.Mage);
            if (HeroSystem.Warrior.IsAlive) aliveHeroes.Add(HeroSystem.Warrior);

            if (aliveHeroes.Count == 0) return; 

            PlayerCharacter target = SelectTarget(aliveHeroes, enemy);

            if (enemy.IsBoss)
            {
                PerformNecromancerAction(enemy, target);
            }
            else
            {
                ExecuteStandardEnemyAttack(enemy, target);
            }
        }

        private void ExecuteStandardEnemyAttack(Enemy enemy, PlayerCharacter target)
        {
            int damage = enemy.PerformAttack();
            if (damage > 0)
            {
                _cameraShake?.ShakeLight();
                int reflected = target.TakeDamage(damage, enemy.AttackType, canReflect: true);

                Vector2 targetPos = target == HeroSystem.Mage ? new Vector2(200, 100) : new Vector2(1000, 100);
                ComboParticles.SpawnDamageNumber(this, targetPos, damage);

                if (reflected > 0)
                {
                    _ = enemy.TakeDamage(reflected, target.AttackType, canReflect: false);
                }
            }
        }

        private static PlayerCharacter SelectTarget(List<PlayerCharacter> aliveHeroes, Enemy enemy)
        {
            PlayerCharacter wounded = aliveHeroes
                .Where(h => h.CurrentHealth < h.MaxHealth * 0.3f)
                .OrderBy(h => h.CurrentHealth)
                .FirstOrDefault();

            if (wounded != null) return wounded;

            if (enemy.IsMiniboss || enemy.IsBoss)
            {
                return aliveHeroes.OrderBy(h => h.BaseDefense).First();
            }

            return aliveHeroes[GD.RandRange(0, aliveHeroes.Count - 1)];
        }

        private void PerformNecromancerAction(Enemy necromancer, PlayerCharacter target)
        {
            if (necromancer.IsStunned) return; 
            
            _necromancerTurnCount++;
            Enemy.NecromancerAbility ability = necromancer.GetNecromancerAbility(_necromancerTurnCount);

            if (ability == Enemy.NecromancerAbility.SummonSkeleton)
            {
                ExecuteNecromancerSummon();
            }
            else if (ability == Enemy.NecromancerAbility.DarkBolt)
            {
                ExecuteNecromancerDarkBolt(necromancer, target);
            }
            else if (ability == Enemy.NecromancerAbility.WeakeningDarkness)
            {
                ExecuteNecromancerWeaken();
            }
        }

        private void ExecuteNecromancerSummon()
        {
            if (Enemies.Count < GameConstants.MAX_ENEMIES_PER_WAVE)
            {
                Enemy skeleton = Enemy.Create(EnemyType.SkeletonWarrior, CurrentWave);
                skeleton.CharacterDied += () => OnEnemyDied(skeleton);
                Enemies.Add(skeleton);
            }
        }

        private void ExecuteNecromancerDarkBolt(Enemy necromancer, PlayerCharacter target)
        {
            int damage = necromancer.PerformAttack();
            int reflected = target.TakeDamage(damage, AttackType.Magical, canReflect: true);
            if (reflected > 0)
            {
                _ = necromancer.TakeDamage(reflected, target.AttackType, canReflect: false);
            }
        }

        private void ExecuteNecromancerWeaken()
        {
            StatusEffectData weakenEffect = new StatusEffectData(StatusEffect.Weakened, 1, 0);
            HeroSystem.Mage.ApplyStatusEffect(weakenEffect);
            HeroSystem.Warrior.ApplyStatusEffect(weakenEffect);
        }

        private void StartNextTurn()
        {
            _gridManager.ResetSwaps();
            ChangePhase(BattlePhase.PlayerSwap);
        }

        private void OnEnemyDied(Enemy enemy)
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
                    InventoryManager.Instance.AddItems(new List<EquipmentData> { item });
                }
            }

            if (Enemies.All(e => !e.IsAlive))
            {
                _ = CallDeferred(MethodName.OnWaveCompleted);
            }
        }

        private void OnBothHeroesDied()
        {
            if (_battleEndedSignaled) return;

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

            if (HeroSystem != null)
            {
                HeroSystem.BothHeroesDied -= OnBothHeroesDied;
            }

            foreach (Enemy enemy in Enemies)
            {
                enemy.QueueFree();
            }
            Enemies.Clear();
        }
    }
}
