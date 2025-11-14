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
    /// Manages battle flow, turn order, and combat resolution.
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

        private PlayerCharacter _player;
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
        /// Active player character.
        /// </summary>
        public PlayerCharacter Player => _player;

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

            GD.Print("[BattleManager] Initialized");
        }

        /// <summary>
        /// Starts a new battle with player and wave number.
        /// </summary>
        public void StartBattle(PlayerCharacter player, int waveNumber)
        {
            _player = player;
            _currentWave = waveNumber;
            _necromancerTurnCount = 0;

            // Connect player signals
            _player.CharacterDied += OnPlayerDied;

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

            return GD.RandRange(totalEnemies - 1, totalEnemies); // Small variance
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

                // FIX: Прямой вызов вместо сигнала
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

                // FIX: Прямой вызов для каскадов
                var cascadeEffects = _comboSystem.ProcessMatches(cascadeMatches);

                // Apply cascade effects
                foreach (var effect in cascadeEffects)
                {
                    ApplyComboEffect(effect);
                }
            }

            // Process player status effects (regeneration, etc)
            _player.ProcessStatusEffects();

            // After all combos, start enemy turn
            CallDeferred(nameof(StartEnemyTurn));
        }

        /// <summary>
        /// Applies a single combo effect to battle.
        /// </summary>
        private void ApplyComboEffect(ComboEffect effect)
        {
            switch (effect.ElementType)
            {
                case ElementType.Fire:
                case ElementType.Sword:
                    ApplyDamageEffect(effect);
                    break;

                case ElementType.Heal:
                    ApplyHealEffect(effect);
                    break;

                case ElementType.Shield:
                    ApplyShieldEffect(effect);
                    break;
            }
        }

        /// <summary>
        /// Applies damage combo effect to enemies.
        /// </summary>
        private void ApplyDamageEffect(ComboEffect effect)
        {
            int damage = _player.CalculateDamage(effect.Damage, effect.ElementType);

            if (effect.IsAoE)
            {
                // Hit all enemies
                GD.Print($"[BattleManager] AoE attack for {damage} damage!");
                foreach (Enemy enemy in _enemies.Where(e => e.IsAlive))
                {
                    int reflected = enemy.TakeDamage(damage, _player.AttackType);

                    // Apply status effect
                    if (effect.StatusEffect != null)
                    {
                        enemy.ApplyStatusEffect(effect.StatusEffect);
                    }

                    // Handle reflect damage
                    if (reflected > 0)
                    {
                        _player.TakeDamage(reflected, enemy.AttackType);
                    }
                }
            }
            else
            {
                // Hit single target (first alive enemy)
                Enemy target = _enemies.FirstOrDefault(e => e.IsAlive);
                if (target != null)
                {
                    GD.Print($"[BattleManager] Attacking {target.CharacterName} for {damage} damage!");
                    int reflected = target.TakeDamage(damage, _player.AttackType);

                    // Apply status effect
                    if (effect.StatusEffect != null)
                    {
                        target.ApplyStatusEffect(effect.StatusEffect);
                    }

                    // Handle reflect damage
                    if (reflected > 0)
                    {
                        _player.TakeDamage(reflected, target.AttackType);
                    }
                }
            }
        }

        /// <summary>
        /// Applies healing combo effect to player.
        /// </summary>
        private void ApplyHealEffect(ComboEffect effect)
        {
            int healing = PlayerCharacter.CalculateHealing(effect.Healing);
            _player.Heal(healing);

            // 4-match: Clear negative effects
            if (effect.ComboLevel == 2)
            {
                _player.ClearNegativeEffects();
                GD.Print("[BattleManager] Negative effects cleared!");
            }

            // Apply status effect (regeneration)
            if (effect.StatusEffect != null)
            {
                _player.ApplyStatusEffect(effect.StatusEffect);
            }
        }

        /// <summary>
        /// Applies shield combo effect to player.
        /// </summary>
        private void ApplyShieldEffect(ComboEffect effect)
        {
            int shield = PlayerCharacter.CalculateShield(effect.Shield);
            _player.AddShield(shield);

            // Apply status effect (reflect/immunity)
            if (effect.StatusEffect != null)
            {
                _player.ApplyStatusEffect(effect.StatusEffect);
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

            // Check if player died
            if (!_player.IsAlive)
            {
                return; // OnPlayerDied will handle game over
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
        /// </summary>
        private void PerformEnemyAction(Enemy enemy)
        {
            if (enemy.IsBoss)
            {
                PerformNecromancerAction(enemy);
            }
            else
            {
                int damage = enemy.PerformAttack();
                if (damage > 0)
                {
                    int reflected = _player.TakeDamage(damage, enemy.AttackType);

                    // Handle reflect damage
                    if (reflected > 0)
                    {
                        enemy.TakeDamage(reflected, _player.AttackType);
                    }
                }
            }
        }

        /// <summary>
        /// Performs Necromancer's rotating abilities.
        /// </summary>
        private void PerformNecromancerAction(Enemy necromancer)
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
                    // Magic damage
                    int damage = necromancer.PerformAttack();
                    int reflected = _player.TakeDamage(damage, AttackType.Magical);
                    if (reflected > 0)
                    {
                        necromancer.TakeDamage(reflected, _player.AttackType);
                    }
                    break;

                case Enemy.NecromancerAbility.WeakeningDarkness:
                    // Apply weakness debuff
                    var weakenEffect = new StatusEffectData(StatusEffect.Weakened, 1, 0);
                    _player.ApplyStatusEffect(weakenEffect);
                    GD.Print("[BattleManager] Necromancer cast Weakening Darkness!");
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
        /// Called when player dies.
        /// </summary>
        private void OnPlayerDied()
        {
            GD.Print("[BattleManager] Player defeated - Game Over");
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

            // Update game state
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

            if (_player != null)
            {
                _player.CharacterDied -= OnPlayerDied;
            }

            GD.Print("[BattleManager] Battle ended");
        }
    }
}