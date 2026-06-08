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
    /// Менеджер BattleManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class BattleManager : Node, IBattleManager
    {
        [Signal]
        /// <summary>
        /// Элемент BattleStartedEventHandler.
        /// </summary>
        public delegate void BattleStartedEventHandler();

        [Signal]
        /// <summary>
        /// Элемент PhaseChangedEventHandler.
        /// </summary>
        public delegate void PhaseChangedEventHandler(BattlePhase newPhase);

        [Signal]
        /// <summary>
        /// Элемент WaveCompletedEventHandler.
        /// </summary>
        public delegate void WaveCompletedEventHandler();

        [Signal]
        /// <summary>
        /// Элемент BattleEndedEventHandler.
        /// </summary>
        public delegate void BattleEndedEventHandler(bool playerWon);

        [Signal]
        /// <summary>
        /// Элемент EnemyDefeatedEventHandler.
        /// </summary>
        public delegate void EnemyDefeatedEventHandler(Enemy enemy);

        private int _necromancerTurnCount;

        private GridManager _gridManager;
        private ComboSystem _comboSystem;
        private CameraShake _cameraShake;
        private GridUI _gridUI;
        
        private bool _isConnectedToGridManager = false;
        private bool _battleEndedSignaled;

        private readonly List<ComboEffect> _accumulatedEffects = [];

        public BattlePhase CurrentPhase { get; private set; }

        public int CurrentWave { get; private set; }

        public DualHeroSystem HeroSystem { get; private set; }

        public List<Enemy> Enemies { get; private set; }

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            Enemies = [];
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
        /// Инициализирует .
        /// </summary>
        public void Initialize(GridUI gridUI)
        {
            _gridUI = gridUI;
            GD.Print("[BattleManager] GridUI reference set");
        }

        /// <summary>
        /// Запускает Battle.
        /// </summary>
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

            CurrentPhase = BattlePhase.PlayerSwap;
            _ = EmitSignal(SignalName.BattleStarted);
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            GD.Print($"[BattleManager] Battle started - Wave {CurrentWave}");
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

            GD.Print($"[BattleManager] Wave {CurrentWave}: {enemyCount} enemies" +
                     $" ({(skeletonUnlocked ? "Slime + Skeleton" : "Slime only")})");
        }

        private static EnemyType GetEnemyTypeForWave(bool skeletonUnlocked)
        {
            if (!skeletonUnlocked)
                return EnemyType.Slime;

            return GD.Randf() < 0.5f ? EnemyType.Slime : EnemyType.SkeletonWarrior;
        }

        private async void OnSwapCompleted(bool wasValid)
        {
            if (!wasValid)
            {
                return;
            }

            CurrentPhase = BattlePhase.PlayerCombo;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
            
            _accumulatedEffects.Clear();
            
            await ProcessMatchesRecursive();
        }

        private async void ProcessPlayerTurn()
        {
            CurrentPhase = BattlePhase.PlayerCombo;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            _accumulatedEffects.Clear();
            _comboSystem.ResetCascade();

            await ProcessMatchesRecursive();
        }

        private async Task ProcessMatchesRecursive(bool isCascade = false)
        {
            List<MatchResult> matches = _gridManager.FindAllMatches();

            if (matches.Count == 0)
            {
                _ = ApplyAccumulatedEffects();
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
                GD.Print("[BattleManager] No combo effects to apply");
                
                if (_gridManager.RemainingSwaps > 0)
                {
                    CurrentPhase = BattlePhase.PlayerSwap;
                    _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
                    GD.Print("[BattleManager] Returning to player swap phase");
                }
                else
                {
                    await StartEnemyTurn();
                }
                return;
            }

            GD.Print($"[BattleManager] Applying {_accumulatedEffects.Count} accumulated combo effects");

            foreach (ComboEffect effect in _accumulatedEffects)
            {
                ApplyComboEffect(effect);
                await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
            }

            _accumulatedEffects.Clear();

            HeroSystem.ProcessStatusEffects();

            if (_gridManager.RemainingSwaps > 0)
            {
                CurrentPhase = BattlePhase.PlayerSwap;
                _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
                GD.Print("[BattleManager] Returning to player swap phase");
            }
            else
            {
                await StartEnemyTurn();
            }
        }

        private void ApplyComboEffect(ComboEffect effect)
        {
            PlayerCharacter activeHero = HeroSystem.GetHeroForElement(effect.ElementType);

            if (activeHero == null)
            {
                GD.PrintErr($"[BattleManager] No hero found for element type: {effect.ElementType}");
                return;
            }

            if (!activeHero.IsAlive)
            {
                GD.Print($"[BattleManager] {activeHero.CharacterName} is dead, cannot use {effect.ElementType} combo");
                return;
            }

            switch (effect.ElementType)
            {
                case ElementType.Fire:
                case ElementType.Sword:
                    ApplyDamageEffect(effect, activeHero);
                    break;

                case ElementType.Heal:
                    ApplyHealEffect(effect, activeHero);
                    break;

                case ElementType.Shield:
                    ApplyShieldEffect(effect, activeHero);
                    break;
            }
        }

        private void ApplyDamageEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int damage = activeHero.CalculateDamage(effect.Damage, effect.ElementType);
            string heroName = activeHero.CharacterName;
            string elementName = effect.ElementType == ElementType.Fire ? "Fire" : "Sword";

            if (effect.IsAoE)
            {
                GD.Print($"[BattleManager] {heroName} uses {elementName} AoE for {damage} damage!");
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
                    GD.Print($"[BattleManager] {heroName} attacks {target.CharacterName} with {elementName} for {damage} damage!");
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

            GD.Print($"[BattleManager] {activeHero.CharacterName} heals both heroes for {healing} HP!");

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
                GD.Print("[BattleManager] Negative effects cleared from both heroes!");
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

            GD.Print($"[BattleManager] {activeHero.CharacterName} grants {shield} shield to both heroes!");

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
            CurrentPhase = BattlePhase.EnemyTurn;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            GD.Print("[BattleManager] Enemy turn starting...");

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

                PerformEnemyAction(enemy);
                await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
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

        private void PerformEnemyAction(Enemy enemy)
        {
            if (enemy.IsStunned)
            {
                GD.Print($"[BattleManager] {enemy.CharacterName} is stunned and cannot act");
                return; 
            }
            
            List<PlayerCharacter> aliveHeroes = [];
            if (HeroSystem.Mage.IsAlive)
            {
                aliveHeroes.Add(HeroSystem.Mage);
            }

            if (HeroSystem.Warrior.IsAlive)
            {
                aliveHeroes.Add(HeroSystem.Warrior);
            }

            if (aliveHeroes.Count == 0)
            {
                return; 
            }

            PlayerCharacter target = SelectTarget(aliveHeroes, enemy);

            if (enemy.IsBoss)
            {
                PerformNecromancerAction(enemy, target);
            }
            else
            {
                int damage = enemy.PerformAttack();
                if (damage > 0)
                {
                    GD.Print($"[BattleManager] {enemy.CharacterName} attacks {target.CharacterName}");

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
        }

        private static PlayerCharacter SelectTarget(List<PlayerCharacter> aliveHeroes, Enemy enemy)
        {
            PlayerCharacter wounded = aliveHeroes
                .Where(h => h.CurrentHealth < h.MaxHealth * 0.3f)
                .OrderBy(h => h.CurrentHealth)
                .FirstOrDefault();

            if (wounded != null)
            {
                GD.Print($"[BattleManager] {enemy.CharacterName} targets wounded {wounded.CharacterName}!");
                return wounded;
            }

            if (enemy.IsMiniboss || enemy.IsBoss)
            {
                PlayerCharacter weakestDefense = aliveHeroes
                    .OrderBy(h => h.BaseDefense)
                    .First();

                GD.Print($"[BattleManager] {enemy.CharacterName} targets {weakestDefense.CharacterName} (weaker defense)");
                return weakestDefense;
            }

            PlayerCharacter randomTarget = aliveHeroes[GD.RandRange(0, aliveHeroes.Count - 1)];
            return randomTarget;
        }

        private void PerformNecromancerAction(Enemy necromancer, PlayerCharacter target)
        {
            if (necromancer.IsStunned)
            {
                GD.Print($"[BattleManager] {necromancer.CharacterName} is stunned and cannot use abilities");
                return; 
            }
            
            _necromancerTurnCount++;
            Enemy.NecromancerAbility ability = necromancer.GetNecromancerAbility(_necromancerTurnCount);

            switch (ability)
            {
                case Enemy.NecromancerAbility.SummonSkeleton:
                    if (Enemies.Count < GameConstants.MAX_ENEMIES_PER_WAVE)
                    {
                        Enemy skeleton = Enemy.Create(EnemyType.SkeletonWarrior, CurrentWave);
                        skeleton.CharacterDied += () => OnEnemyDied(skeleton);
                        Enemies.Add(skeleton);
                        GD.Print("[BattleManager] Necromancer summoned a Skeleton!");
                    }
                    break;

                case Enemy.NecromancerAbility.DarkBolt:
                    int damage = necromancer.PerformAttack();
                    GD.Print($"[BattleManager] Necromancer casts Dark Bolt at {target.CharacterName}");
                    int reflected = target.TakeDamage(damage, AttackType.Magical, canReflect: true);
                    if (reflected > 0)
                    {
                        _ = necromancer.TakeDamage(reflected, target.AttackType, canReflect: false);
                    }
                    break;

                case Enemy.NecromancerAbility.WeakeningDarkness:
                    StatusEffectData weakenEffect = new(StatusEffect.Weakened, 1, 0);
                    HeroSystem.Mage.ApplyStatusEffect(weakenEffect);
                    HeroSystem.Warrior.ApplyStatusEffect(weakenEffect);
                    GD.Print("[BattleManager] Necromancer cast Weakening Darkness on both heroes!");
                    break;
            }
        }

        private void StartNextTurn()
        {
            _gridManager.ResetSwaps();
            CurrentPhase = BattlePhase.PlayerSwap;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            GD.Print("[BattleManager] New player turn started");
        }

        private void OnEnemyDied(Enemy enemy)
        {
            _ = EmitSignal(SignalName.EnemyDefeated, enemy);

            GameStateManager.Instance.AddCoins(enemy.CoinReward);
            GD.Print($"[BattleManager] {enemy.CharacterName} defeated! +{enemy.CoinReward} coins");

            if (enemy.IsBoss || enemy.IsMiniboss)
            {
                GenerateBossLoot(enemy);
            }
            else if (GD.Randf() <= 0.20f)
            {
                EquipmentData item = LootSystem.Instance.GenerateNormalLoot(CurrentWave);
                if (item != null)
                {
                    InventoryManager.Instance.AddItems([item]);
                    GD.Print($"[BattleManager] Normal enemy dropped loot: {item.Name}");
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
            GD.Print("[BattleManager] Both heroes defeated - Game Over");
            _ = EmitSignal(SignalName.BattleEnded, false);
        }

        private void GenerateBossLoot(Enemy _)
        {
            List<EquipmentData> loot = LootSystem.Instance.GenerateBossLoot(CurrentWave);

            InventoryManager.Instance.AddItems(loot);

            GD.Print($"[BattleManager] Generated {loot.Count} items from boss at wave {CurrentWave}");
        }

        private void OnWaveCompleted()
        {
            CurrentPhase = BattlePhase.WaveTransition;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
            _ = EmitSignal(SignalName.WaveCompleted);

            GD.Print($"[BattleManager] Wave {CurrentWave} completed!");

            (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) = HeroSystem.GetCombinedStats();
            GameStateManager.Instance.UpdateHeroStats(
                mageHealth, mageMaxHealth, mageDamage, mageDefense,
                warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense
            );

            GameStateManager.Instance.NextWave();

            SaveSystem.Instance.AutoSave();

        }

        /// <summary>
        /// Элемент EndBattle.
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

            GD.Print("[BattleManager] Battle ended, all signals unsubscribed");
        }
    }
}
