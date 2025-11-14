using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
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

        private DualHeroSystem _heroSystem;
        private List<Enemy> _enemies;
        private BattlePhase _currentPhase;
        private int _currentWave;
        private int _necromancerTurnCount;

        private GridManager _gridManager;
        private ComboSystem _comboSystem;

        /// <summary>
        /// Current battle phase.
        /// </summary>
        public BattlePhase CurrentPhase => _currentPhase;

        /// <summary>
        /// Current wave number.
        /// </summary>
        public int CurrentWave => _currentWave;

        /// <summary>
        /// Dual hero system reference.
        /// </summary>
        public DualHeroSystem HeroSystem => _heroSystem;

        /// <summary>
        /// List of active enemies.
        /// </summary>
        public List<Enemy> Enemies => _enemies;

        public override void _Ready()
        {
            _enemies = [];
            _currentPhase = BattlePhase.PlayerSwap;
            _necromancerTurnCount = 0;

            // Get managers
            _gridManager = GetNode<GridManager>("/root/GridManager");
            _comboSystem = GetNode<ComboSystem>("/root/ComboSystem");

            // Connect signals
            _gridManager.SwapCompleted += OnSwapCompleted;

            GD.Print("[BattleManager] Initialized for dual hero system");
        }

        /// <summary>
        /// Starts a new battle with dual heroes and wave number.
        /// </summary>
        public void StartBattle(DualHeroSystem heroSystem, int waveNumber)
        {
            _heroSystem = heroSystem;
            _currentWave = waveNumber;
            _necromancerTurnCount = 0;

            // Connect hero signals
            _heroSystem.BothHeroesDied += OnBothHeroesDied;

            // Generate enemies for wave
            GenerateWaveEnemies();

            // Initialize grid
            _gridManager.InitializeGrid();

            // Start player turn
            _currentPhase = BattlePhase.PlayerSwap;
            EmitSignal(SignalName.BattleStarted);
            EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);

            GD.Print($"[BattleManager] Battle started - Wave {_currentWave}");
        }

        /// <summary>
        /// Generates enemies based on current wave.
        /// </summary>
        private void GenerateWaveEnemies()
        {
            _enemies.Clear();

            bool isMinibossWave = _currentWave % GameConstants.MINIBOSS_WAVE_INTERVAL == 0;
            bool isBossWave = _currentWave % GameConstants.BOSS_WAVE_INTERVAL == 0;

            if (isBossWave)
            {
                // Boss wave: Necromancer + 2 minions
                var boss = Enemy.Create(EnemyType.Necromancer, _currentWave);
                boss.CharacterDied += () => OnEnemyDied(boss);
                _enemies.Add(boss);

                // Add 2 random basic enemies
                for (int i = 0; i < 2; i++)
                {
                    var minion = Enemy.Create(GetRandomBasicEnemyType(), _currentWave);
                    minion.CharacterDied += () => OnEnemyDied(minion);
                    _enemies.Add(minion);
                }

                GD.Print($"[BattleManager] Boss wave! Necromancer + 2 minions");
            }
            else if (isMinibossWave)
            {
                // Miniboss wave: 1 miniboss + 2-3 basic enemies
                var miniboss = Enemy.Create(GetRandomMinibossType(), _currentWave);
                miniboss.CharacterDied += () => OnEnemyDied(miniboss);
                _enemies.Add(miniboss);

                int minionCount = GD.RandRange(2, 3);
                for (int i = 0; i < minionCount; i++)
                {
                    var minion = Enemy.Create(GetRandomBasicEnemyType(), _currentWave);
                    minion.CharacterDied += () => OnEnemyDied(minion);
                    _enemies.Add(minion);
                }

                GD.Print($"[BattleManager] Miniboss wave! {miniboss.CharacterName} + {minionCount} minions");
            }
            else
            {
                // Regular wave: 3-5 basic enemies
                int enemyCount = CalculateEnemyCount();

                for (int i = 0; i < enemyCount; i++)
                {
                    var enemy = Enemy.Create(GetRandomBasicEnemyType(), _currentWave);
                    enemy.CharacterDied += () => OnEnemyDied(enemy);
                    _enemies.Add(enemy);
                }

                GD.Print($"[BattleManager] Regular wave with {enemyCount} enemies");
            }
        }

        /// <summary>
        /// Calculates number of enemies based on wave.
        /// </summary>
        private int CalculateEnemyCount()
        {
            int baseCount = 3;
            int additionalEnemies = _currentWave / GameConstants.ENEMY_COUNT_INCREASE_EVERY;
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
            if (!wasValid) return;

            // Check if player has remaining swaps
            if (_gridManager.RemainingSwaps <= 0)
            {
                // Process all matches and move to combo phase
                ProcessPlayerTurn();
            }
        }

        /// <summary>
        /// Processes player turn after all swaps completed.
        /// </summary>
        private void ProcessPlayerTurn()
        {
            _currentPhase = BattlePhase.PlayerCombo;
            EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);

            // Find and process all matches
            var matches = _gridManager.FindAllMatches();

            if (matches.Count > 0)
            {
                // Process matches will trigger cascade
                _gridManager.ProcessMatches(matches);

                // Process combo effects
                var comboEffects = _comboSystem.ProcessMatches(matches);
                OnCombosProcessed(comboEffects);
            }
            else
            {
                // No matches, move to enemy turn
                StartEnemyTurn();
            }
        }

        /// <summary>
        /// Called when combo system finishes processing matches.
        /// </summary>
        private void OnCombosProcessed(List<ComboEffect> effects)
        {
            // Apply all combo effects
            foreach (var effect in effects)
            {
                ApplyComboEffect(effect);
            }

            // Check for cascade matches
            var cascadeMatches = _gridManager.FindAllMatches();
            if (cascadeMatches.Count > 0)
            {
                GD.Print("[BattleManager] Cascade detected!");
                _gridManager.ProcessMatches(cascadeMatches);

                // Process cascade effects
                var cascadeEffects = _comboSystem.ProcessMatches(cascadeMatches);

                // Apply cascade effects
                foreach (var effect in cascadeEffects)
                {
                    ApplyComboEffect(effect);
                }
            }

            // Process hero status effects (regeneration, etc)
            _heroSystem.ProcessStatusEffects();

            // After all combos, start enemy turn
            CallDeferred(MethodName.StartEnemyTurn);
        }

        /// <summary>
        /// Applies a single combo effect to battle.
        /// Routes effect to the appropriate hero based on element type.
        /// </summary>
        private void ApplyComboEffect(ComboEffect effect)
        {
            // Get the hero responsible for this element
            PlayerCharacter activeHero = _heroSystem.GetHeroForElement(effect.ElementType);

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
                foreach (Enemy enemy in _enemies.Where(e => e.IsAlive))
                {
                    int reflected = enemy.TakeDamage(damage, activeHero.AttackType);

                    // Apply status effect
                    if (effect.StatusEffect != null)
                    {
                        enemy.ApplyStatusEffect(effect.StatusEffect);
                    }

                    // Handle reflect damage
                    if (reflected > 0)
                    {
                        activeHero.TakeDamage(reflected, enemy.AttackType);
                    }
                }
            }
            else
            {
                // Hit single target (first alive enemy)
                Enemy target = _enemies.FirstOrDefault(e => e.IsAlive);
                if (target != null)
                {
                    GD.Print($"[BattleManager] {heroName} attacks {target.CharacterName} with {elementName} for {damage} damage!");
                    int reflected = target.TakeDamage(damage, activeHero.AttackType);

                    // Apply status effect
                    if (effect.StatusEffect != null)
                    {
                        target.ApplyStatusEffect(effect.StatusEffect);
                    }

                    // Handle reflect damage
                    if (reflected > 0)
                    {
                        activeHero.TakeDamage(reflected, target.AttackType);
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

            // Heal BOTH heroes
            _heroSystem.Mage.Heal(healing);
            _heroSystem.Warrior.Heal(healing);

            // 4-match: Clear negative effects on both heroes
            if (effect.ComboLevel == 2)
            {
                _heroSystem.Mage.ClearNegativeEffects();
                _heroSystem.Warrior.ClearNegativeEffects();
                GD.Print("[BattleManager] Negative effects cleared from both heroes!");
            }

            // Apply status effect (regeneration) to both heroes
            if (effect.StatusEffect != null)
            {
                _heroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                _heroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        /// <summary>
        /// Applies shield combo effect to BOTH heroes.
        /// </summary>
        private void ApplyShieldEffect(ComboEffect effect, PlayerCharacter activeHero)
        {
            int shield = PlayerCharacter.CalculateShield(effect.Shield);

            GD.Print($"[BattleManager] {activeHero.CharacterName} grants {shield} shield to both heroes!");

            // Shield BOTH heroes
            _heroSystem.Mage.AddShield(shield);
            _heroSystem.Warrior.AddShield(shield);

            // Apply status effect (reflect/immunity) to both heroes
            if (effect.StatusEffect != null)
            {
                _heroSystem.Mage.ApplyStatusEffect(effect.StatusEffect);
                _heroSystem.Warrior.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        /// <summary>
        /// Starts enemy turn phase.
        /// </summary>
        private void StartEnemyTurn()
        {
            _currentPhase = BattlePhase.EnemyTurn;
            EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);

            GD.Print("[BattleManager] Enemy turn starting...");

            // Process enemy status effects first
            foreach (Enemy enemy in _enemies.Where(e => e.IsAlive))
            {
                enemy.ProcessStatusEffects();
            }

            // Each enemy attacks
            foreach (Enemy enemy in _enemies.Where(e => e.IsAlive))
            {
                PerformEnemyAction(enemy);
            }

            // Check if both heroes died
            if (!_heroSystem.IsAnyAlive)
            {
                return; // OnBothHeroesDied will handle game over
            }

            // Check if all enemies dead
            if (_enemies.All(e => !e.IsAlive))
            {
                OnWaveCompleted();
                return;
            }

            // Start next player turn
            CallDeferred(MethodName.StartNextTurn);
        }

        /// <summary>
        /// Performs action for a single enemy.
        /// Enemy randomly targets one of the alive heroes.
        /// </summary>
        private void PerformEnemyAction(Enemy enemy)
        {
            // Get list of alive heroes
            List<PlayerCharacter> aliveHeroes = [];
            if (_heroSystem.Mage.IsAlive) aliveHeroes.Add(_heroSystem.Mage);
            if (_heroSystem.Warrior.IsAlive) aliveHeroes.Add(_heroSystem.Warrior);

            if (aliveHeroes.Count == 0)
            {
                return; // No targets
            }

            // Select random target
            PlayerCharacter target = aliveHeroes[GD.RandRange(0, aliveHeroes.Count - 1)];

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
                    int reflected = target.TakeDamage(damage, enemy.AttackType);

                    // Handle reflect damage
                    if (reflected > 0)
                    {
                        enemy.TakeDamage(reflected, target.AttackType);
                    }
                }
            }
        }

        /// <summary>
        /// Performs Necromancer's rotating abilities.
        /// </summary>
        private void PerformNecromancerAction(Enemy necromancer, PlayerCharacter target)
        {
            _necromancerTurnCount++;
            Enemy.NecromancerAbility ability = necromancer.GetNecromancerAbility(_necromancerTurnCount);

            switch (ability)
            {
                case Enemy.NecromancerAbility.SummonSkeleton:
                    // Summon one skeleton if space available
                    if (_enemies.Count < GameConstants.MAX_ENEMIES_PER_WAVE)
                    {
                        var skeleton = Enemy.Create(EnemyType.SkeletonWarrior, _currentWave);
                        skeleton.CharacterDied += () => OnEnemyDied(skeleton);
                        _enemies.Add(skeleton);
                        GD.Print("[BattleManager] Necromancer summoned a Skeleton!");
                    }
                    break;

                case Enemy.NecromancerAbility.DarkBolt:
                    // Magic damage to target
                    int damage = necromancer.PerformAttack();
                    GD.Print($"[BattleManager] Necromancer casts Dark Bolt at {target.CharacterName}");
                    int reflected = target.TakeDamage(damage, AttackType.Magical);
                    if (reflected > 0)
                    {
                        necromancer.TakeDamage(reflected, target.AttackType);
                    }
                    break;

                case Enemy.NecromancerAbility.WeakeningDarkness:
                    // Apply weakness debuff to BOTH heroes
                    var weakenEffect = new StatusEffectData(StatusEffect.Weakened, 1, 0);
                    _heroSystem.Mage.ApplyStatusEffect(weakenEffect);
                    _heroSystem.Warrior.ApplyStatusEffect(weakenEffect);
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
            _currentPhase = BattlePhase.PlayerSwap;
            EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);

            GD.Print("[BattleManager] New player turn started");
        }

        /// <summary>
        /// Called when an enemy dies.
        /// </summary>
        private void OnEnemyDied(Enemy enemy)
        {
            EmitSignal(SignalName.EnemyDefeated, enemy);

            // Award coins
            GameStateManager.Instance.AddCoins(enemy.CoinReward);

            GD.Print($"[BattleManager] {enemy.CharacterName} defeated! +{enemy.CoinReward} coins");

            // Check if all enemies dead
            if (_enemies.All(e => !e.IsAlive))
            {
                CallDeferred(MethodName.OnWaveCompleted);
            }
        }

        /// <summary>
        /// Called when both heroes die.
        /// </summary>
        private void OnBothHeroesDied()
        {
            GD.Print("[BattleManager] Both heroes defeated - Game Over");
            EmitSignal(SignalName.BattleEnded, false);

            // Trigger game over
            SceneManager.GameOver();
        }

        /// <summary>
        /// Called when wave is completed.
        /// </summary>
        private void OnWaveCompleted()
        {
            _currentPhase = BattlePhase.WaveTransition;
            EmitSignal(SignalName.PhaseChanged, (int)_currentPhase);
            EmitSignal(SignalName.WaveCompleted);

            GD.Print($"[BattleManager] Wave {_currentWave} completed!");

            // Update game state with both heroes' stats
            var (mageHealth, mageMaxHealth, mageDamage, mageDefense, warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense) = _heroSystem.GetCombinedStats();
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
        /// Cleans up battle.
        /// </summary>
        public void EndBattle()
        {
            // Cleanup
            foreach (Enemy enemy in _enemies)
            {
                enemy.QueueFree();
            }
            _enemies.Clear();

            if (_heroSystem != null)
            {
                _heroSystem.BothHeroesDied -= OnBothHeroesDied;
            }

            GD.Print("[BattleManager] Battle ended");
        }
    }
}