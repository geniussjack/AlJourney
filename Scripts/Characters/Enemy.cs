using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Represents an enemy character with wave-based scaling.
    /// </summary>
    public partial class Enemy : Character
    {
        private EnemyType _enemyType;
        private int _waveNumber;
        private int _coinReward;

        /// <summary>
        /// Type of enemy.
        /// </summary>
        public EnemyType EnemyType => _enemyType;

        /// <summary>
        /// Coin reward for defeating this enemy.
        /// </summary>
        public int CoinReward => _coinReward;

        /// <summary>
        /// Is this a miniboss.
        /// </summary>
        public bool IsMiniboss => _enemyType == EnemyType.GeneralOfDraugr || _enemyType == EnemyType.Arhiskeleton;

        /// <summary>
        /// Is this the boss.
        /// </summary>
        public bool IsBoss => _enemyType == EnemyType.Necromancer;

        /// <summary>
        /// Creates an enemy of specified type scaled to wave number.
        /// </summary>
        public static Enemy Create(EnemyType enemyType, int waveNumber)
        {
            var enemy = new Enemy
            {
                _enemyType = enemyType,
                _waveNumber = waveNumber
            };

            // Get base stats
            (string name, int baseHp, int baseDmg, int baseDef, AttackType attackType, int coinReward) = GetEnemyBaseStats(enemyType);

            // Apply wave scaling
            int scaledHp = Mathf.CeilToInt(baseHp * (1 + waveNumber * GameConstants.ENEMY_HP_SCALE_PER_WAVE));
            int scaledDmg = Mathf.CeilToInt(baseDmg * (1 + waveNumber * GameConstants.ENEMY_DAMAGE_SCALE_PER_WAVE));

            enemy.Initialize(name, scaledHp, scaledDmg, baseDef, attackType);
            enemy._coinReward = coinReward;

            GD.Print($"[Enemy] Created {name} (Wave {waveNumber}) - HP: {scaledHp}, DMG: {scaledDmg}");
            return enemy;
        }

        /// <summary>
        /// Gets base stats for each enemy type.
        /// </summary>
        private static (string name, int hp, int damage, int defense, AttackType attackType, int coinReward) GetEnemyBaseStats(EnemyType type)
        {
            return type switch
            {
                EnemyType.SkeletonWarrior => (
                    "Skeleton Warrior",
                    GameConstants.SKELETON_WARRIOR_HP,
                    GameConstants.SKELETON_WARRIOR_DAMAGE,
                    GameConstants.SKELETON_WARRIOR_DEFENSE,
                    AttackType.Physical,
                    GameConstants.COINS_PER_BASIC_ENEMY
                ),

                EnemyType.SkeletonArcher => (
                    "Skeleton Archer",
                    GameConstants.SKELETON_ARCHER_HP,
                    GameConstants.SKELETON_ARCHER_DAMAGE,
                    GameConstants.SKELETON_ARCHER_DEFENSE,
                    AttackType.Physical,
                    GameConstants.COINS_PER_BASIC_ENEMY
                ),

                EnemyType.Zombie => (
                    "Zombie",
                    GameConstants.ZOMBIE_HP,
                    GameConstants.ZOMBIE_DAMAGE,
                    GameConstants.ZOMBIE_DEFENSE,
                    AttackType.Physical,
                    GameConstants.COINS_PER_BASIC_ENEMY
                ),

                EnemyType.DraugrWarrior => (
                    "Draugr Warrior",
                    GameConstants.DRAUGR_WARRIOR_HP,
                    GameConstants.DRAUGR_WARRIOR_DAMAGE,
                    GameConstants.DRAUGR_WARRIOR_DEFENSE,
                    AttackType.Physical,
                    GameConstants.COINS_PER_BASIC_ENEMY
                ),

                EnemyType.DraugrDefender => (
                    "Draugr Defender",
                    GameConstants.DRAUGR_DEFENDER_HP,
                    GameConstants.DRAUGR_DEFENDER_DAMAGE,
                    GameConstants.DRAUGR_DEFENDER_DEFENSE,
                    AttackType.Physical,
                    GameConstants.COINS_PER_BASIC_ENEMY
                ),

                EnemyType.DraugrCaster => (
                    "Draugr Caster",
                    GameConstants.DRAUGR_CASTER_HP,
                    GameConstants.DRAUGR_CASTER_DAMAGE,
                    GameConstants.DRAUGR_CASTER_DEFENSE,
                    AttackType.Magical,
                    GameConstants.COINS_PER_BASIC_ENEMY
                ),

                EnemyType.GeneralOfDraugr => (
                    "General of Draugr",
                    GameConstants.GENERAL_DRAUGR_HP,
                    GameConstants.GENERAL_DRAUGR_DAMAGE,
                    GameConstants.GENERAL_DRAUGR_DEFENSE,
                    AttackType.Physical,
                    GameConstants.COINS_PER_MINIBOSS
                ),

                EnemyType.Arhiskeleton => (
                    "Arhiskeleton",
                    GameConstants.ARHISKELETON_HP,
                    GameConstants.ARHISKELETON_DAMAGE,
                    GameConstants.ARHISKELETON_DEFENSE,
                    AttackType.Physical,
                    GameConstants.COINS_PER_MINIBOSS
                ),

                EnemyType.Necromancer => (
                    "Necromancer",
                    GameConstants.NECROMANCER_HP,
                    GameConstants.NECROMANCER_DAMAGE,
                    GameConstants.NECROMANCER_DEFENSE,
                    AttackType.Magical,
                    GameConstants.COINS_PER_BOSS
                ),

                _ => ("Unknown Enemy", 10, 5, 0, AttackType.Physical, 1)
            };
        }

        /// <summary>
        /// Performs enemy attack action.
        /// Returns damage to deal to player.
        /// </summary>
        public int PerformAttack()
        {
            if (!IsAlive || IsStunned) return 0;

            int damage = _baseDamage;

            // Special enemy abilities
            switch (_enemyType)
            {
                case EnemyType.Arhiskeleton:
                    // Fires multiple arrows
                    damage = _baseDamage * GameConstants.ARHISKELETON_ARROWS_PER_TURN;
                    GD.Print($"[{_name}] fires {GameConstants.ARHISKELETON_ARROWS_PER_TURN} arrows for {damage} damage!");
                    break;

                case EnemyType.GeneralOfDraugr:
                    // Can use magic attack occasionally (25% chance)
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
                    // Boss has rotating abilities (handled in BattleManager)
                    GD.Print($"[{_name}] prepares dark magic...");
                    break;

                default:
                    GD.Print($"[{_name}] attacks for {damage} damage!");
                    break;
            }

            return damage;
        }

        /// <summary>
        /// Gets special ability type for bosses (Necromancer).
        /// Rotates between abilities.
        /// </summary>
        public NecromancerAbility GetNecromancerAbility(int turnNumber)
        {
            if (_enemyType != EnemyType.Necromancer)
                return NecromancerAbility.None;

            // Rotate abilities every turn
            return (NecromancerAbility)((turnNumber % 3) + 1);
        }

        /// <summary>
        /// Necromancer special abilities.
        /// </summary>
        public enum NecromancerAbility
        {
            None,
            SummonSkeleton,    // Summons one skeleton
            DarkBolt,          // Magic damage
            WeakeningDarkness  // Reduces player damage/defense for 1 turn
        }
    }
}