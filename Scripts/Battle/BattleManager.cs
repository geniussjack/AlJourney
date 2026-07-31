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
    /// Global manager for the turn-based combat system. Manages the player party's turn queue
    /// (the player chooses which of the living combatants acts next, then their ability and target),
    /// enemy turns, enemy waves, and damage and loot payouts.
    /// </summary>
    public partial class BattleManager : Node, IBattleManager
    {
        /// <summary>
        /// Raised at the start of a new battle.
        /// </summary>
        [Signal]
        public delegate void BattleStartedEventHandler();

        /// <summary>
        /// Raised when the current battle phase changes.
        /// </summary>
        /// <param name="newPhase">The new phase, from the BattlePhase enum.</param>
        [Signal]
        public delegate void PhaseChangedEventHandler(BattlePhase newPhase);

        /// <summary>
        /// Raised on any change to the player's turn selection state (actor selection, ability selection,
        /// target resolution). Used by the target-selection UI to refresh itself.
        /// </summary>
        [Signal]
        public delegate void TurnStateChangedEventHandler();

        /// <summary>
        /// Raised when every wave of the current level has been cleared (the level is fully completed).
        /// Before Stage 3's campaign map, a wave and a level matched one-to-one; now a level can consist
        /// of several consecutive waves (see <see cref="WaveAdvanced"/>), and this signal only fires once
        /// the last one is cleared.
        /// </summary>
        [Signal]
        public delegate void LevelCompletedEventHandler();

        /// <summary>
        /// Raised when advancing to the next wave within the same level (not the last one) — combat
        /// continues without leaving the scene, but the UI (e.g. enemy health bars in
        /// <see cref="Scripts.UI.BattleHUD"/>) needs to refresh for the new <see cref="Enemies"/> lineup.
        /// </summary>
        /// <param name="waveIndex">The new current wave's index (zero-based) within the level.</param>
        /// <param name="totalWaves">The total number of waves in the level.</param>
        [Signal]
        public delegate void WaveAdvancedEventHandler(int waveIndex, int totalWaves);

        /// <summary>
        /// Raised when the battle ends.
        /// </summary>
        /// <param name="playerWon">True if the player won.</param>
        [Signal]
        public delegate void BattleEndedEventHandler(bool playerWon);

        /// <summary>
        /// Raised every time one of the enemies dies.
        /// </summary>
        /// <param name="enemy">A reference to the defeated enemy.</param>
        [Signal]
        public delegate void EnemyDefeatedEventHandler(Enemy enemy);

        /// <summary>
        /// Raised when the party's shared ultimate charge changes.
        /// </summary>
        /// <param name="charge">The current charge value.</param>
        /// <param name="maxCharge">The maximum charge value.</param>
        [Signal]
        public delegate void UltimateChargeChangedEventHandler(int charge, int maxCharge);

        /// <summary>
        /// The maximum value of the party's shared ultimate charge.
        /// </summary>
        public const int MaxUltimateCharge = 100;

        /// <summary>
        /// The fixed ultimate charge gained per successful action
        /// (a party attack landing on an enemy, or an enemy attack landing on the party).
        /// </summary>
        public const int UltimateChargePerAction = 25;

        public int NecromancerTurnCount { get; private set; }
        public void IncrementNecromancerTurnCount()
        {
            NecromancerTurnCount++;
        }

        private CameraShake _cameraShake;
        private bool _battleEndedSignaled;
        private bool _isLevelCompleted;
        private LevelDefinition _level;
        private int _currentWaveIndex;

        private List<PlayerCharacter> _pendingActors = [];

        /// <summary>
        /// The party's current total ultimate charge (0..<see cref="MaxUltimateCharge"/>).
        /// </summary>
        public int UltimateCharge { get; private set; }

        /// <summary>
        /// True if the ultimate charge is full and it's ready to use.
        /// </summary>
        public bool IsUltimateReady => UltimateCharge >= MaxUltimateCharge;

        /// <summary>
        /// The current battle phase.
        /// </summary>
        public BattlePhase CurrentPhase { get; private set; }

        /// <summary>
        /// The current level's difficulty (see <see cref="Data.LevelDefinition.DifficultyRating"/>),
        /// used as the input to <see cref="ScalingSystem"/> — shared by every wave of a single level.
        /// </summary>
        public int CurrentWave { get; private set; }

        /// <summary>
        /// The current wave's index (zero-based) within the level's waves.
        /// </summary>
        public int CurrentWaveIndex => _currentWaveIndex;

        /// <summary>
        /// The total number of waves in the current level.
        /// </summary>
        public int TotalWavesInLevel => _level?.Waves.Count ?? 0;

        /// <summary>
        /// Reference to the hero party system.
        /// </summary>
        public DualHeroSystem HeroSystem { get; private set; }

        /// <summary>
        /// The list of every enemy currently active on the field.
        /// </summary>
        public List<Enemy> Enemies { get; private set; }

        /// <summary>
        /// Party members who have not yet acted in the current round.
        /// </summary>
        public IReadOnlyList<PlayerCharacter> PendingActors => _pendingActors;

        /// <summary>
        /// The actor selected by the player for the current turn.
        /// </summary>
        public PlayerCharacter SelectedActor { get; private set; }

        /// <summary>
        /// The ability selected for the current turn.
        /// </summary>
        public AbilityData SelectedAbility { get; private set; }

        /// <summary>
        /// Initializes the battle manager.
        /// </summary>
        public override void _Ready()
        {
            Enemies = [];
            CurrentPhase = BattlePhase.PlayerTurn;
            NecromancerTurnCount = 0;
            _battleEndedSignaled = false;
            _isLevelCompleted = false;
            UltimateCharge = 0;

            GD.Print("[BattleManager] Initialized for party-based turn combat");
        }

        /// <summary>
        /// Starts the battle for the given campaign map level with the provided party system.
        /// The level's waves spawn one after another as they're cleared, without leaving combat
        /// (see <see cref="OnEnemiesCleared"/>).
        /// </summary>
        /// <param name="heroSystem">The player's party system object.</param>
        /// <param name="level">The campaign map level defining the waves and their difficulty.</param>
        /// <param name="cameraShake">Optional camera shake controller.</param>
        public void StartBattle(DualHeroSystem heroSystem, LevelDefinition level, CameraShake cameraShake = null)
        {
            HeroSystem = heroSystem;
            _level = level;
            CurrentWave = level.DifficultyRating;
            _currentWaveIndex = 0;
            NecromancerTurnCount = 0;
            _cameraShake = cameraShake;
            _battleEndedSignaled = false;
            _isLevelCompleted = false;
            UltimateCharge = 0;

            HeroSystem.PartyDefeated += OnPartyDefeated;

            SpawnCurrentWave();
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
        /// Selects the actor who will take the next turn. The player determines the turn order among
        /// the living party members who haven't acted yet this round.
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
        /// Selects the ability the chosen actor will use: attack, support, or (once fully charged) the
        /// ultimate ability. The ultimate resolves immediately, without a separate target confirmation —
        /// it either hits an area, or picks its own target by its own rules (see <see cref="ResolveUltimate"/>).
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
        /// Returns the list of valid targets for the selected ability's targeting.
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
        /// Confirms the target and immediately resolves the selected ability's effect.
        /// If this was the last party member who hadn't acted yet, the enemy turn begins.
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
        /// Immediately resolves the selected actor's ultimate ability and resets the party's shared charge.
        /// AoE ultimates hit every living enemy; single-target ones hit the enemy with the highest current
        /// HP (auto-selected, without player involvement).
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
        /// Removes an actor from the queue of party members who haven't acted this round, and either
        /// passes the turn to the next party member, or (if this was the last one) starts the enemy turn.
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
        /// Increases (clamped) the party's shared ultimate charge and notifies subscribers.
        /// Called both for successful party attacks and for enemy attacks landing on the party.
        /// </summary>
        /// <param name="amount">The amount of charge to add.</param>
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

        /// <summary>
        /// Spawns the enemies for the level's current wave (<see cref="_currentWaveIndex"/>) using the
        /// curated composition from <see cref="LevelDefinition.Waves"/>, replacing the previous
        /// <see cref="Enemies"/> lineup.
        /// </summary>
        private void SpawnCurrentWave()
        {
            // The base wave's enemies aren't added to the scene tree, but creatures summoned by a boss
            // (see EnemyAIController.ExecuteNecromancerSummon) are — and they need to be explicitly
            // freed here, otherwise on advancing to the next wave within the level they'd remain hanging
            // as BattleManager child nodes until the end of the whole battle.
            foreach (Enemy leftover in Enemies)
            {
                if (leftover.IsInsideTree())
                {
                    leftover.QueueFree();
                }
            }
            Enemies.Clear();

            WaveDefinition wave = _level.Waves[_currentWaveIndex];
            foreach (EnemySpawnDefinition spawn in wave.Enemies)
            {
                Enemy enemy = EnemySpawner.SpawnEnemy(spawn.Type, CurrentWave, spawn.Count);
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
                    OnEnemiesCleared();
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
                Callable.From(OnEnemiesCleared).CallDeferred();
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

        /// <summary>
        /// Called when every enemy on the field is dead. Can fire from two independent places —
        /// <see cref="OnEnemyDied"/> deferred (via <c>CallDeferred</c>) and <see cref="StartEnemyTurn"/>
        /// synchronously right after the enemy turn — so it re-checks the current state of
        /// <see cref="Enemies"/> at the start rather than relying solely on being called: by the time the
        /// deferred call fires, the wave may have already changed (see <see cref="SpawnCurrentWave"/>),
        /// in which case the repeat call should silently do nothing instead of firing against an
        /// already-different, alive wave. The separate <see cref="_isLevelCompleted"/> flag only guards
        /// the branch that completes the level as a whole, where the <see cref="Enemies"/> lineup stays
        /// unchanged (all dead) until the end of the battle.
        /// </summary>
        private void OnEnemiesCleared()
        {
            if (_isLevelCompleted || Enemies.Count == 0 || Enemies.Any(e => e.IsAlive))
            {
                return;
            }

            if (_currentWaveIndex + 1 < _level.Waves.Count)
            {
                _currentWaveIndex++;
                SpawnCurrentWave();

                _ = EmitSignal(SignalName.WaveAdvanced, _currentWaveIndex, _level.Waves.Count);
                StartPlayerTurn();
                return;
            }

            _isLevelCompleted = true;

            ChangePhase(BattlePhase.WaveTransition);
            _ = EmitSignal(SignalName.LevelCompleted);

            (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) = HeroSystem.GetCombinedStats();
            GameStateManager.Instance.UpdateHeroStats(
                mageHealth, mageMaxHealth, mageDamage, mageDefense,
                warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense
            );

            GameStateManager.Instance.CompleteLevel(_level.Id);
        }

        /// <summary>
        /// Clears the battle state, unsubscribes from signals and removes enemies.
        /// Called when transitioning to the results screen or a menu.
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
