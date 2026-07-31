using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Enemy class. Inherits from the base Character class.
    /// Handles enemy types, their base stats, wave scaling
    /// and type-specific attacks.
    /// </summary>
    public partial class Enemy : Character
    {
        private int _waveNumber;

        /// <summary>
        /// This enemy's type.
        /// </summary>
        public EnemyType EnemyType { get; private set; }

        /// <summary>
        /// The gold coin reward for defeating this enemy.
        /// </summary>
        public int CoinReward { get; private set; }

        /// <summary>
        /// The current number of creatures in the stack.
        /// </summary>
        public int StackCount { get => field > 0 ? Mathf.CeilToInt(CurrentHealth / ((float)TotalMaxHealth / field)) : 1; private set; }

        public new string CharacterName => StackCount > 1 ? $"{Tr(_name)} x{StackCount}" : Tr(_name);

        /// <summary>
        /// Whether this enemy is a miniboss.
        /// </summary>
        public bool IsMiniboss => EnemyType is EnemyType.GeneralOfDraugr or EnemyType.Arhiskeleton;

        /// <summary>
        /// Whether this enemy is the main boss.
        /// </summary>
        public bool IsBoss => EnemyType == EnemyType.Necromancer;

        private static readonly Dictionary<EnemyType, (string nameKey, int hp, int damage, int defense, AttackType attackType, int coinReward)> BaseStatsMap =
            new()
            {
                [EnemyType.SkeletonWarrior] = ("ENEMY_SKELETON_WARRIOR", GameConstants.SKELETON_WARRIOR_HP, GameConstants.SKELETON_WARRIOR_DAMAGE, GameConstants.SKELETON_WARRIOR_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
                [EnemyType.SkeletonArcher] = ("ENEMY_SKELETON_ARCHER", GameConstants.SKELETON_ARCHER_HP, GameConstants.SKELETON_ARCHER_DAMAGE, GameConstants.SKELETON_ARCHER_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
                [EnemyType.Zombie] = ("ENEMY_ZOMBIE", GameConstants.ZOMBIE_HP, GameConstants.ZOMBIE_DAMAGE, GameConstants.ZOMBIE_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
                [EnemyType.Slime] = ("ENEMY_SLIME", GameConstants.SLIME_HP, GameConstants.SLIME_DAMAGE, GameConstants.SLIME_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
                [EnemyType.DraugrWarrior] = ("ENEMY_DRAUGR_WARRIOR", GameConstants.DRAUGR_WARRIOR_HP, GameConstants.DRAUGR_WARRIOR_DAMAGE, GameConstants.DRAUGR_WARRIOR_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
                [EnemyType.DraugrDefender] = ("ENEMY_DRAUGR_DEFENDER", GameConstants.DRAUGR_DEFENDER_HP, GameConstants.DRAUGR_DEFENDER_DAMAGE, GameConstants.DRAUGR_DEFENDER_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
                [EnemyType.DraugrCaster] = ("ENEMY_DRAUGR_CASTER", GameConstants.DRAUGR_CASTER_HP, GameConstants.DRAUGR_CASTER_DAMAGE, GameConstants.DRAUGR_CASTER_DEFENSE, AttackType.Magical, GameConstants.COINS_PER_BASIC_ENEMY),
                [EnemyType.GeneralOfDraugr] = ("ENEMY_GENERAL_OF_DRAUGR", GameConstants.GENERAL_DRAUGR_HP, GameConstants.GENERAL_DRAUGR_DAMAGE, GameConstants.GENERAL_DRAUGR_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_MINIBOSS),
                [EnemyType.Arhiskeleton] = ("ENEMY_ARHISKELETON", GameConstants.ARHISKELETON_HP, GameConstants.ARHISKELETON_DAMAGE, GameConstants.ARHISKELETON_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_MINIBOSS),
                [EnemyType.Necromancer] = ("ENEMY_NECROMANCER", GameConstants.NECROMANCER_HP, GameConstants.NECROMANCER_DAMAGE, GameConstants.NECROMANCER_DEFENSE, AttackType.Magical, GameConstants.COINS_PER_BOSS)
            };

        /// <summary>
        /// Factory method that creates and initializes a new enemy of the given type.
        /// </summary>
        /// <param name="enemyType">The enemy type, from the EnemyType enum.</param>
        /// <param name="waveNumber">The current wave number, used to scale the enemy's stats.</param>
        /// <param name="stackCount">The number of enemies in the stack.</param>
        /// <returns>A new, configured Enemy instance.</returns>
        public static Enemy Create(EnemyType enemyType, int waveNumber, int stackCount = 1)
        {
            Enemy enemy = new()
            {
                EnemyType = enemyType,
                _waveNumber = waveNumber,
                StackCount = stackCount
            };

            (string name, int hp, int damage, int defense, AttackType attackType, int coinReward) = GetEnemyBaseStats(enemyType);

            int scaledHp = ScalingSystem.ScaleEnemyStat(hp, waveNumber);
            int scaledDmg = ScalingSystem.ScaleEnemyStat(damage, waveNumber);
            int scaledDefense = ScalingSystem.ScaleEnemyStat(defense, waveNumber);
            int scaledReward = ScalingSystem.ScaleReward(coinReward, waveNumber);

            int totalHp = scaledHp * stackCount;
            // Damage and defense are accounted for by the methods below, factoring in StackCount

            enemy.Initialize(name, totalHp, scaledDmg, scaledDefense, attackType);
            enemy.CoinReward = scaledReward * stackCount;

            GD.Print($"[Enemy] Created {name} x{stackCount} (Wave {waveNumber}) - Total HP: {totalHp}, Base DMG: {scaledDmg}, Base DEF: {scaledDefense}, Reward: {enemy.CoinReward}");
            return enemy;
        }

        private static (string nameKey, int hp, int damage, int defense, AttackType attackType, int coinReward) GetEnemyBaseStats(EnemyType type)
        {
            return BaseStatsMap.TryGetValue(type, out (string nameKey, int hp, int damage, int defense, AttackType attackType, int coinReward) stats) ? stats : ((string nameKey, int hp, int damage, int defense, AttackType attackType, int coinReward))("Unknown Enemy", 10, 5, 0, AttackType.Physical, 1);
        }

        /// <summary>
        /// Computes and returns the damage this enemy deals on the current turn.
        /// Accounts for certain enemy types' special behavior.
        /// </summary>
        /// <returns>The amount of base damage to deal to the heroes.</returns>
        public int PerformAttack()
        {
            if (!IsAlive || IsStunned)
            {
                return 0;
            }

            int damage = _baseDamage * StackCount;

            switch (EnemyType)
            {
                case EnemyType.Arhiskeleton:
                    damage = _baseDamage * GameConstants.ARHISKELETON_ARROWS_PER_TURN;
                    GD.Print($"[{_name}] fires {GameConstants.ARHISKELETON_ARROWS_PER_TURN} arrows for {damage} damage!");
                    break;

                case EnemyType.GeneralOfDraugr:
                    if (GD.Randf() < 0.25f)
                    {
                        damage = Mathf.CeilToInt(_baseDamage * 1.5f);
                        GD.Print($"[{_name}] uses magic attack for {damage} damage!");
                    }
                    else
                    {
                        GD.Print($"[{_name}] attacks for {damage} damage!");
                    }
                    break;

                case EnemyType.Necromancer:
                    GD.Print($"[{_name}] prepares dark magic...");
                    break;

                default:
                    GD.Print($"[{_name}] attacks for {damage} damage!");
                    break;
            }

            if (HasStatusEffect(StatusEffect.Freeze))
            {
                damage = Mathf.CeilToInt(damage * 0.7f);
                GD.Print($"[{_name}] Damage reduced by Freeze status: {damage}");
            }

            return damage;
        }

        /// <summary>
        /// Determines which ability the Necromancer will use on the current turn.
        /// The Necromancer cycles through its abilities.
        /// </summary>
        /// <param name="turnNumber">The Necromancer's turn number in the current battle.</param>
        /// <returns>The ability to use.</returns>
        public NecromancerAbility GetNecromancerAbility(int turnNumber)
        {
            return EnemyType != EnemyType.Necromancer ? NecromancerAbility.None : (NecromancerAbility)((turnNumber % 3) + 1);
        }

        /// <summary>
        /// The list of abilities available to the Necromancer boss.
        /// </summary>
        public enum NecromancerAbility
        {
            None,
            SummonSkeleton,
            DarkBolt,
            WeakeningDarkness
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            Tween tween = CreateTween();
            _ = tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
            _ = tween.TweenCallback(Callable.From(QueueFree));
        }
    }
}
