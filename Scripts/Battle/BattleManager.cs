using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.UI;
using AlJourney.Scripts.Utils;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Manages battle flow, turn order, and combat resolution for dual hero system.
    /// </summary>
    public partial class BattleManager : Node
    {
        [Signal]
        public delegate void BattleStartedEventHandler();

        [Signal]
        public delegate void PhaseChangedEventHandler(BattlePhase newPhase);

        [Signal]
        public delegate void WaveCompletedEventHandler();

        [Signal]
        public delegate void BattleEndedEventHandler(bool playerWon);

        [Signal]
        public delegate void EnemyDefeatedEventHandler(Enemy enemy);

        private int _necromancerTurnCount;

        private GridManager _gridManager;
        private ComboSystem _comboSystem;
        private CameraShake _cameraShake;
        private GridUI _gridUI;
        
        // Signal connection tracking
        private bool _isConnectedToGridManager = false;

        // Accumulated combo effects during player turn
        private readonly List<ComboEffect> _accumulatedEffects = [];

        /// <summary>
        /// Current battle phase.
        /// </summary>
        public BattlePhase CurrentPhase { get; private set; }

        /// <summary>
        /// Current wave number.
        /// </summary>
        public int CurrentWave { get; private set; }

        /// <summary>
        /// Dual hero system reference.
        /// </summary>
        public DualHeroSystem HeroSystem { get; private set; }

        /// <summary>
        /// List of active enemies.
        /// </summary>
        public List<Enemy> Enemies { get; private set; }

        public override void _Ready()
        {
            Enemies = [];
            CurrentPhase = BattlePhase.PlayerSwap;
            _necromancerTurnCount = 0;

            // Get managers
            _gridManager = GetNode<GridManager>("/root/GridManager");
            _comboSystem = GetNode<ComboSystem>("/root/ComboSystem");

            // Connect signals with duplicate protection
            if (!_isConnectedToGridManager)
            {
                _gridManager.SwapCompleted += OnSwapCompleted;
                _isConnectedToGridManager = true;
            }

            GD.Print("[BattleManager] Initialized for dual hero system");
        }

        /// <summary>
        /// Initializes battle manager with GridUI reference.
        /// </summary>
        public void Initialize(GridUI gridUI)
        {
            _gridUI = gridUI;
            GD.Print("[BattleManager] GridUI reference set");
        }

        /// <summary>
        /// Starts a new battle with dual heroes and wave number.
        /// </summary>
        public void StartBattle(DualHeroSystem heroSystem, int waveNumber, CameraShake cameraShake = null)
        {
            HeroSystem = heroSystem;
            CurrentWave = waveNumber;
            _necromancerTurnCount = 0;
            _cameraShake = cameraShake;

            // Connect hero signals
            HeroSystem.BothHeroesDied += OnBothHeroesDied;

            // Generate enemies for wave
            GenerateWaveEnemies();

            // Initialize grid
            _gridManager.InitializeGrid();

            // Start player turn
            CurrentPhase = BattlePhase.PlayerSwap;
            _ = EmitSignal(SignalName.BattleStarted);
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            GD.Print($"[BattleManager] Battle started - Wave {CurrentWave}");
        }

        /// <summary>
        /// Generates enemies based on current wave.
        /// </summary>
        private void GenerateWaveEnemies()
        {
            Enemies.Clear();

            bool isMinibossWave = CurrentWave % GameConstants.MINIBOSS_WAVE_INTERVAL == 0;
            bool isBossWave = CurrentWave % GameConstants.BOSS_WAVE_INTERVAL == 0;

            if (isBossWave)
            {
                // Boss wave: Necromancer + 2 minions
                Enemy boss = Enemy.Create(EnemyType.Necromancer, CurrentWave);
                boss.CharacterDied += () => OnEnemyDied(boss);
                Enemies.Add(boss);

                // Add 2 random basic enemies
                for (int i = 0; i < 2; i++)
                {
                    Enemy minion = Enemy.Create(GetRandomBasicEnemyType(), CurrentWave);
                    minion.CharacterDied += () => OnEnemyDied(minion);
                    Enemies.Add(minion);
                }

                GD.Print("[BattleManager] Boss wave! Necromancer + 2 minions");
            }
            else if (isMinibossWave)
            {
                // Miniboss wave: 1 miniboss + 2-3 basic enemies
                Enemy miniboss = Enemy.Create(GetRandomMinibossType(), CurrentWave);
                miniboss.CharacterDied += () => OnEnemyDied(miniboss);
                Enemies.Add(miniboss);

                int minionCount = GD.RandRange(2, 3);
                for (int i = 0; i < minionCount; i++)
                {
                    Enemy minion = Enemy.Create(GetRandomBasicEnemyType(), CurrentWave);
                    minion.CharacterDied += () => OnEnemyDied(minion);
                    Enemies.Add(minion);
                }

                GD.Print($"[BattleManager] Miniboss wave! {miniboss.CharacterName} + {minionCount} minions");
            }
            else
            {
                // Regular wave: 3-5 basic enemies
                int enemyCount = CalculateEnemyCount();

                for (int i = 0; i < enemyCount; i++)
                {
                    Enemy enemy = Enemy.Create(GetRandomBasicEnemyType(), CurrentWave);
                    enemy.CharacterDied += () => OnEnemyDied(enemy);
                    Enemies.Add(enemy);
                }

                GD.Print($"[BattleManager] Regular wave with {enemyCount} enemies");
            }
        }

        /// <summary>
        /// Calculates number of enemies based on wave.
        /// </summary>
        private int CalculateEnemyCount()
        {
            const int baseCount = 3;
            int additionalEnemies = CurrentWave / GameConstants.ENEMY_COUNT_INCREASE_EVERY;
            int totalEnemies = Mathf.Min(baseCount + additionalEnemies, GameConstants.MAX_ENEMIES_PER_WAVE);

            return GD.RandRange(totalEnemies - 1, totalEnemies);
        }

        /// <summary>
        /// Gets a random basic enemy type.
        /// </summary>
        private static EnemyType GetRandomBasicEnemyType()
        {
            EnemyType[] basicTypes =
            [
                EnemyType.SkeletonWarrior,
                EnemyType.SkeletonArcher,
                EnemyType.Zombie,
                EnemyType.DraugrWarrior,
                EnemyType.DraugrDefender,
                EnemyType.DraugrCaster
            ];

            return basicTypes[GD.RandRange(0, basicTypes.Length - 1)];
        }

        /// <summary>
        /// Gets a random miniboss type.
        /// </summary>
        private static EnemyType GetRandomMinibossType()
        {
            return GD.Randf() < 0.5f ? EnemyType.GeneralOfDraugr : EnemyType.Arhiskeleton;
        }

        /// <summary>
        /// Called when player completes a swap.
        /// </summary>
        private void OnSwapCompleted(bool wasValid)
        {
            if (!wasValid)
            {
                return;
            }

            // Process matches immediately after each valid swap
            CurrentPhase = BattlePhase.PlayerCombo;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
            
            // Clear accumulated effects from previous swap
            _accumulatedEffects.Clear();
            
            // Process matches and cascades for this swap
            ProcessMatchesRecursive();
        }

        /// <summary>
        /// Processes player turn after all swaps completed.
        /// </summary>
        private void ProcessPlayerTurn()
        {
            CurrentPhase = BattlePhase.PlayerCombo;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            // Clear accumulated effects from previous turn
            _accumulatedEffects.Clear();
            _comboSystem.ResetCascade();

            // Process all matches and cascades
            ProcessMatchesRecursive();
        }

        /// <summary>
        /// Recursively processes matches and cascades, accumulating all effects.
        /// </summary>
        private void ProcessMatchesRecursive(bool isCascade = false)
        {
            // Find all matches on current grid
            List<MatchResult> matches = _gridManager.FindAllMatches();

            if (matches.Count == 0)
            {
                // No more matches - apply all accumulated effects
                ApplyAccumulatedEffects();
                return;
            }

            // Process combo effects
            List<ComboEffect> comboEffects = _comboSystem.ProcessMatches(matches, isCascade);

            // Accumulate effects
            _accumulatedEffects.AddRange(comboEffects);

            // Visualize matches and effects in GridUI
            _gridUI?.VisualizeMatchesAndEffects(matches, comboEffects);

            // Process matches in grid (remove, apply gravity, refill)
            _gridManager.ProcessMatches(matches);

            // Wait for animation and refill, then check for cascades
            GetTree().CreateTimer(0.6f).Timeout += () => ProcessMatchesRecursive(isCascade: true);
        }

        /// <summary>
        /// Applies all accumulated combo effects to heroes and enemies.
        /// </summary>
        private void ApplyAccumulatedEffects()
        {
            if (_accumulatedEffects.Count == 0)
            {
                GD.Print("[BattleManager] No combo effects to apply");
                
                // Check if player has more swaps
                if (_gridManager.RemainingSwaps > 0)
                {
                    // Return to swap phase
                    CurrentPhase = BattlePhase.PlayerSwap;
                    _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
                    GD.Print("[BattleManager] Returning to player swap phase");
                }
                else
                {
                    // All swaps consumed - start enemy turn
                    StartEnemyTurn();
                }
                return;
            }

            GD.Print($"[BattleManager] Applying {_accumulatedEffects.Count} accumulated combo effects");

            // Apply all accumulated combo effects
            foreach (ComboEffect effect in _accumulatedEffects)
            {
                ApplyComboEffect(effect);
            }

            // Clear accumulated effects
            _accumulatedEffects.Clear();

            // Process hero status effects (regeneration, etc)
            HeroSystem.ProcessStatusEffects();

            // Check if player has more swaps
            if (_gridManager.RemainingSwaps > 0)
            {
                // Return to swap phase
                CurrentPhase = BattlePhase.PlayerSwap;
                _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
                GD.Print("[BattleManager] Returning to player swap phase");
            }
            else
            {
                // All swaps consumed - start enemy turn
                _ = CallDeferred(MethodName.StartEnemyTurn);
            }
        }

        /// <summary>
        /// Applies a single combo effect to battle.
        /// Routes effect to the appropriate hero based on element type.
        /// </summary>
        private void ApplyComboEffect(ComboEffect effect)
        {
            // Get the hero responsible for this element
            PlayerCharacter activeHero = HeroSystem.GetHeroForElement(effect.ElementType);

            if (activeHero == null)
            {
                GD.PrintErr($"[BattleManager] No hero found for element type: {effect.ElementType}");
                return;
            }

            // Check if hero is alive
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

        /// <summary>
        /// Applies damage combo effect to enemies.
        /// </summary>
        private void ApplyDamageEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int damage = activeHero.CalculateDamage(effect.Damage, effect.ElementType);

            string heroName = activeHero.CharacterName;
            string elementName = effect.ElementType == ElementType.Fire ? "Fire" : "Sword";

            if (effect.IsAoE)
            {
                // Hit all enemies
                GD.Print($"[BattleManager] {heroName} uses {elementName} AoE for {damage} damage!");

                // Camera shake for AoE
                _cameraShake?.ShakeStrong();

                // Spawn combo particles
                ComboParticles.SpawnComboEffect(this, new Vector2(640, 360), effect.ElementType, effect.ComboLevel);

                foreach (Enemy enemy in Enemies.Where(e => e.IsAlive))
                {
                    int reflected = enemy.TakeDamage(damage, activeHero.AttackType, canReflect: true);

                    // Spawn damage number
                    ComboParticles.SpawnDamageNumber(this, new Vector2(400, 200), damage);

                    // Apply status effect
                    if (effect.StatusEffect != null)
                    {
                        enemy.ApplyStatusEffect(effect.StatusEffect);
                    }

                    // Handle reflect damage WITHOUT allowing another reflection
                    if (reflected > 0)
                    {
                        _ = activeHero.TakeDamage(reflected, enemy.AttackType, canReflect: false);
                    }
                }
            }
            else
            {
                // Hit single target (first alive enemy)
                Enemy target = Enemies.FirstOrDefault(e => e.IsAlive);
                if (target != null)
                {
                    GD.Print($"[BattleManager] {heroName} attacks {target.CharacterName} with {elementName} for {damage} damage!");

                    // Camera shake for single target
                    _cameraShake?.ShakeMedium();

                    // Spawn combo particles
                    ComboParticles.SpawnComboEffect(this, new Vector2(640, 300), effect.ElementType, effect.ComboLevel);

                    int reflected = target.TakeDamage(damage, activeHero.AttackType, canReflect: true);

                    // Spawn damage number
                    ComboParticles.SpawnDamageNumber(this, new Vector2(640, 250), damage);

                    // Apply status effect
                    if (effect.StatusEffect != null)
                    {
                        target.ApplyStatusEffect(effect.StatusEffect);
                    }

                    // Handle reflect damage WITHOUT allowing another reflection
                    if (reflected > 0)
                    {
                        _ = activeHero.TakeDamage(reflected, target.AttackType, canReflect: false);
                    }
                }
            }
        }

        /// <summary>
        /// Applies healing combo effect to BOTH heroes.
        /// </summary>
        private void ApplyHealEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int healing = PlayerCharacter.CalculateHealing(effect.Healing);

            GD.Print($"[BattleManager] {activeHero.CharacterName} heals both heroes for {healing} HP!");

            // Light camera shake for heal
            _cameraShake?.ShakeLight();

            // Spawn heal particles
            ComboParticles.SpawnComboEffect(this, new Vector2(640, 360), ElementType.Heal, effect.ComboLevel);

            // Heal BOTH heroes
            HeroSystem.Mage.Heal(healing);
            HeroSystem.Warrior.Heal(healing);

            // Spawn heal numbers
            ComboParticles.SpawnHealNumber(this, new Vector2(200, 100), healing);
            ComboParticles.SpawnHealNumber(this, new Vector2(1000, 100), healing);

            // 4-match: Clear negative effects on both heroes
            if (effect.ComboLevel == 2)
            {
                HeroSystem.Mage.ClearNegativeEffects();
                HeroSystem.Warrior.ClearNegativeEffects();
                GD.Print("[BattleManager] Negative effects cleared from both heroes!");
            }

            // Apply status effect (regeneration) to both heroes
            if (effect.StatusEffect != null)
            {
                HeroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                HeroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        /// <summary>
        /// Applies shield combo effect to BOTH heroes.
        /// </summary>
        private void ApplyShieldEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int shield = PlayerCharacter.CalculateShield(effect.Shield);

            GD.Print($"[BattleManager] {activeHero.CharacterName} grants {shield} shield to both heroes!");

            // Light camera shake for shield
            _cameraShake?.ShakeLight();

            // Spawn shield particles
            ComboParticles.SpawnComboEffect(this, new Vector2(640, 360), ElementType.Shield, effect.ComboLevel);

            // Shield BOTH heroes
            HeroSystem.Mage.AddShield(shield);
            HeroSystem.Warrior.AddShield(shield);

            // Spawn shield numbers
            ComboParticles.SpawnShieldNumber(this, new Vector2(200, 100), shield);
            ComboParticles.SpawnShieldNumber(this, new Vector2(1000, 100), shield);

            // Apply status effect (reflect/immunity) to both heroes
            if (effect.StatusEffect != null)
            {
                HeroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                HeroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        /// <summary>
        /// Starts enemy turn phase.
        /// </summary>
        private void StartEnemyTurn()
        {
            CurrentPhase = BattlePhase.EnemyTurn;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            GD.Print("[BattleManager] Enemy turn starting...");

            // Process enemy status effects first
            foreach (Enemy enemy in Enemies.Where(e => e.IsAlive))
            {
                enemy.ProcessStatusEffects();
            }

            // Each enemy attacks
            foreach (Enemy enemy in Enemies.Where(e => e.IsAlive))
            {
                PerformEnemyAction(enemy);
            }

            // Check if both heroes died
            if (!HeroSystem.IsAnyAlive)
            {
                return; // OnBothHeroesDied will handle game over
            }

            // Check if all enemies dead
            if (Enemies.All(e => !e.IsAlive))
            {
                OnWaveCompleted();
                return;
            }

            // Start next player turn
            _ = CallDeferred(MethodName.StartNextTurn);
        }

        /// <summary>
        /// Performs action for a single enemy.
        /// Enemy intelligently targets one of the alive heroes.
        /// </summary>
        private void PerformEnemyAction(Enemy enemy)
        {
            // CHECK: Is enemy stunned?
            if (enemy.IsStunned)
            {
                GD.Print($"[BattleManager] {enemy.CharacterName} is stunned and cannot act");
                return; // Skip all actions
            }
            
            // Get list of alive heroes
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
                return; // No targets
            }

            // Select target with AI logic
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

                    // Camera shake for enemy attack
                    _cameraShake?.ShakeLight();

                    int reflected = target.TakeDamage(damage, enemy.AttackType, canReflect: true);

                    // Spawn damage number
                    Vector2 targetPos = target == HeroSystem.Mage ? new Vector2(200, 100) : new Vector2(1000, 100);
                    ComboParticles.SpawnDamageNumber(this, targetPos, damage);

                    // Handle reflect damage WITHOUT allowing another reflection
                    if (reflected > 0)
                    {
                        _ = enemy.TakeDamage(reflected, target.AttackType, canReflect: false);
                    }
                }
            }
        }

        /// <summary>
        /// Selects target for enemy using AI logic.
        /// Priority: 1) Finish wounded hero (< 30% HP), 2) Random target
        /// </summary>
        private static PlayerCharacter SelectTarget(List<PlayerCharacter> aliveHeroes, Enemy enemy)
        {
            // Strategy 1: Finish off wounded hero (< 30% HP)
            PlayerCharacter wounded = aliveHeroes
                .Where(h => h.CurrentHealth < h.MaxHealth * 0.3f)
                .OrderBy(h => h.CurrentHealth)
                .FirstOrDefault();

            if (wounded != null)
            {
                GD.Print($"[BattleManager] {enemy.CharacterName} targets wounded {wounded.CharacterName}!");
                return wounded;
            }

            // Strategy 2: Target hero with less defense (for smart enemies)
            if (enemy.IsMiniboss || enemy.IsBoss)
            {
                PlayerCharacter weakestDefense = aliveHeroes
                    .OrderBy(h => h.BaseDefense)
                    .First();

                GD.Print($"[BattleManager] {enemy.CharacterName} targets {weakestDefense.CharacterName} (weaker defense)");
                return weakestDefense;
            }

            // Strategy 3: Random target (basic enemies)
            PlayerCharacter randomTarget = aliveHeroes[GD.RandRange(0, aliveHeroes.Count - 1)];
            return randomTarget;
        }

        /// <summary>
        /// Performs Necromancer's rotating abilities.
        /// </summary>
        private void PerformNecromancerAction(Enemy necromancer, PlayerCharacter target)
        {
            // CHECK: Is boss stunned?
            if (necromancer.IsStunned)
            {
                GD.Print($"[BattleManager] {necromancer.CharacterName} is stunned and cannot use abilities");
                return; // Skip all abilities including summons
            }
            
            _necromancerTurnCount++;
            Enemy.NecromancerAbility ability = necromancer.GetNecromancerAbility(_necromancerTurnCount);

            switch (ability)
            {
                case Enemy.NecromancerAbility.SummonSkeleton:
                    // Summon one skeleton if space available
                    if (Enemies.Count < GameConstants.MAX_ENEMIES_PER_WAVE)
                    {
                        Enemy skeleton = Enemy.Create(EnemyType.SkeletonWarrior, CurrentWave);
                        skeleton.CharacterDied += () => OnEnemyDied(skeleton);
                        Enemies.Add(skeleton);
                        GD.Print("[BattleManager] Necromancer summoned a Skeleton!");
                    }
                    break;

                case Enemy.NecromancerAbility.DarkBolt:
                    // Magic damage to target
                    int damage = necromancer.PerformAttack();
                    GD.Print($"[BattleManager] Necromancer casts Dark Bolt at {target.CharacterName}");
                    int reflected = target.TakeDamage(damage, AttackType.Magical, canReflect: true);
                    if (reflected > 0)
                    {
                        _ = necromancer.TakeDamage(reflected, target.AttackType, canReflect: false);
                    }
                    break;

                case Enemy.NecromancerAbility.WeakeningDarkness:
                    // Apply weakness debuff to BOTH heroes
                    StatusEffectData weakenEffect = new(StatusEffect.Weakened, 1, 0);
                    HeroSystem.Mage.ApplyStatusEffect(weakenEffect);
                    HeroSystem.Warrior.ApplyStatusEffect(weakenEffect);
                    GD.Print("[BattleManager] Necromancer cast Weakening Darkness on both heroes!");
                    break;
            }
        }

        /// <summary>
        /// Starts next player turn.
        /// </summary>
        private void StartNextTurn()
        {
            _gridManager.ResetSwaps();
            CurrentPhase = BattlePhase.PlayerSwap;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            GD.Print("[BattleManager] New player turn started");
        }

        /// <summary>
        /// Called when an enemy dies.
        /// </summary>
        private void OnEnemyDied(Enemy enemy)
        {
            _ = EmitSignal(SignalName.EnemyDefeated, enemy);

            // Award coins
            GameStateManager.Instance.AddCoins(enemy.CoinReward);
            GD.Print($"[BattleManager] {enemy.CharacterName} defeated! +{enemy.CoinReward} coins");

            // Generate boss loot if this is a boss
            if (enemy.IsBoss)
            {
                GenerateBossLoot(enemy);
            }

            // Check if all enemies dead
            if (Enemies.All(e => !e.IsAlive))
            {
                _ = CallDeferred(MethodName.OnWaveCompleted);
            }
        }

        /// <summary>
        /// Called when both heroes die.
        /// </summary>
        private void OnBothHeroesDied()
        {
            GD.Print("[BattleManager] Both heroes defeated - Game Over");
            _ = EmitSignal(SignalName.BattleEnded, false);

            // Trigger game over
            SceneManager.GameOver();
        }

        /// <summary>
        /// Generates boss loot and adds to inventory.
        /// </summary>
        private void GenerateBossLoot(Enemy _)
        {
            List<EquipmentData> loot = LootSystem.Instance.GenerateBossLoot(CurrentWave);

            // Add to inventory
            InventoryManager.Instance.AddItems(loot);

            GD.Print($"[BattleManager] Generated {loot.Count} items from boss at wave {CurrentWave}");
        }

        /// <summary>
        /// Called when wave is completed.
        /// </summary>
        private void OnWaveCompleted()
        {
            CurrentPhase = BattlePhase.WaveTransition;
            _ = EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
            _ = EmitSignal(SignalName.WaveCompleted);

            GD.Print($"[BattleManager] Wave {CurrentWave} completed!");

            // Update game state with both heroes' stats
            (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) = HeroSystem.GetCombinedStats();
            GameStateManager.Instance.UpdateHeroStats(
                mageHealth, mageMaxHealth, mageDamage, mageDefense,
                warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense
            );

            // Advance to next wave
            GameStateManager.Instance.NextWave();

            // Save game
            SaveSystem.Instance.AutoSave();

            // Transition to shop or next wave
            // (Will be handled by UI/SceneManager)
        }

        /// <summary>
        /// Cleans up battle and unsubscribes from all signals.
        /// </summary>
        public void EndBattle()
        {
            // Unsubscribe from GridManager signals
            if (_gridManager != null && _isConnectedToGridManager)
            {
                _gridManager.SwapCompleted -= OnSwapCompleted;
                _isConnectedToGridManager = false;
            }

            // Unsubscribe from hero system
            if (HeroSystem != null)
            {
                HeroSystem.BothHeroesDied -= OnBothHeroesDied;
            }

            // Cleanup enemies
            foreach (Enemy enemy in Enemies)
            {
                enemy.QueueFree();
            }
            Enemies.Clear();

            GD.Print("[BattleManager] Battle ended, all signals unsubscribed");
        }
    }
}
