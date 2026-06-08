using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Основной класс Enemy.
    /// </summary>
    public partial class Enemy : Character
    {
        private int _waveNumber;

        public EnemyType EnemyType { get; private set; }

        public int CoinReward { get; private set; }

        /// <summary>
        /// Проверяет, является ли Miniboss.
        /// </summary>
        public bool IsMiniboss => EnemyType is EnemyType.GeneralOfDraugr or EnemyType.Arhiskeleton;

        /// <summary>
        /// Проверяет, является ли Boss.
        /// </summary>
        public bool IsBoss => EnemyType == EnemyType.Necromancer;

        /// <summary>
        /// Элемент Create.
        /// </summary>
        public static Enemy Create(EnemyType enemyType, int waveNumber)
        {
            Enemy enemy = new()
            {
                EnemyType = enemyType,
                _waveNumber = waveNumber
            };

            (string name, int baseHp, int baseDmg, int baseDef, AttackType attackType, int coinReward) = GetEnemyBaseStats(enemyType);

            int scaledHp = ScalingSystem.ScaleEnemyStat(baseHp, waveNumber);
            int scaledDmg = ScalingSystem.ScaleEnemyStat(baseDmg, waveNumber);
            int scaledDefense = ScalingSystem.ScaleEnemyStat(baseDef, waveNumber);
            int scaledReward = ScalingSystem.ScaleReward(coinReward, waveNumber);

            enemy.Initialize(name, scaledHp, scaledDmg, scaledDefense, attackType);
            enemy.CoinReward = scaledReward;

            GD.Print($"[Enemy] Created {name} (Wave {waveNumber}) - HP: {scaledHp}, DMG: {scaledDmg}, DEF: {scaledDefense}, Reward: {scaledReward}");
            return enemy;
        }

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

                EnemyType.Slime => (
                    "Slime",
                    GameConstants.SLIME_HP,
                    GameConstants.SLIME_DAMAGE,
                    GameConstants.SLIME_DEFENSE,
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
        /// Элемент PerformAttack.
        /// </summary>
        public int PerformAttack()
        {
            if (!IsAlive || IsStunned)
            {
                return 0;
            }

            int damage = _baseDamage;

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

            return damage;
        }

        /// <summary>
        /// Возвращает NecromancerAbility.
        /// </summary>
        public NecromancerAbility GetNecromancerAbility(int turnNumber)
        {
            if (EnemyType != EnemyType.Necromancer)
            {
                return NecromancerAbility.None;
            }

            return (NecromancerAbility)((turnNumber % 3) + 1);
        }

        /// <summary>
        /// Основной класс NecromancerAbility.
        /// </summary>
        public enum NecromancerAbility
        {
            None,
            SummonSkeleton,    
            DarkBolt,          
            WeakeningDarkness  
        }
    }
}
