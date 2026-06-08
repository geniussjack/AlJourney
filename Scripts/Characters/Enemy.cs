using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Класс врага. Наследует базовый класс Character.
    /// Обрабатывает типы врагов, их базовые характеристики, скалирование с волнами 
    /// и специфические атаки (включая умения боссов).
    /// </summary>
    public partial class Enemy : Character
    {
        private int _waveNumber;

        /// <summary>
        /// Тип данного врага (Слизь, Скелет, Босс и т.д.).
        /// </summary>
        public EnemyType EnemyType { get; private set; }

        /// <summary>
        /// Награда в виде золотых монет за убийство данного врага.
        /// </summary>
        public int CoinReward { get; private set; }

        /// <summary>
        /// Является ли данный враг мини-боссом (сильным противником).
        /// </summary>
        public bool IsMiniboss => EnemyType is EnemyType.GeneralOfDraugr or EnemyType.Arhiskeleton;

        /// <summary>
        /// Является ли данный враг главным боссом (Некромант).
        /// </summary>
        public bool IsBoss => EnemyType == EnemyType.Necromancer;

        private static readonly Dictionary<EnemyType, (string name, int hp, int damage, int defense, AttackType attackType, int coinReward)> BaseStatsMap = 
            new Dictionary<EnemyType, (string, int, int, int, AttackType, int)>
        {
            [EnemyType.SkeletonWarrior] = ("Skeleton Warrior", GameConstants.SKELETON_WARRIOR_HP, GameConstants.SKELETON_WARRIOR_DAMAGE, GameConstants.SKELETON_WARRIOR_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
            [EnemyType.SkeletonArcher] = ("Skeleton Archer", GameConstants.SKELETON_ARCHER_HP, GameConstants.SKELETON_ARCHER_DAMAGE, GameConstants.SKELETON_ARCHER_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
            [EnemyType.Zombie] = ("Zombie", GameConstants.ZOMBIE_HP, GameConstants.ZOMBIE_DAMAGE, GameConstants.ZOMBIE_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
            [EnemyType.Slime] = ("Slime", GameConstants.SLIME_HP, GameConstants.SLIME_DAMAGE, GameConstants.SLIME_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
            [EnemyType.DraugrWarrior] = ("Draugr Warrior", GameConstants.DRAUGR_WARRIOR_HP, GameConstants.DRAUGR_WARRIOR_DAMAGE, GameConstants.DRAUGR_WARRIOR_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
            [EnemyType.DraugrDefender] = ("Draugr Defender", GameConstants.DRAUGR_DEFENDER_HP, GameConstants.DRAUGR_DEFENDER_DAMAGE, GameConstants.DRAUGR_DEFENDER_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_BASIC_ENEMY),
            [EnemyType.DraugrCaster] = ("Draugr Caster", GameConstants.DRAUGR_CASTER_HP, GameConstants.DRAUGR_CASTER_DAMAGE, GameConstants.DRAUGR_CASTER_DEFENSE, AttackType.Magical, GameConstants.COINS_PER_BASIC_ENEMY),
            [EnemyType.GeneralOfDraugr] = ("General of Draugr", GameConstants.GENERAL_DRAUGR_HP, GameConstants.GENERAL_DRAUGR_DAMAGE, GameConstants.GENERAL_DRAUGR_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_MINIBOSS),
            [EnemyType.Arhiskeleton] = ("Arhiskeleton", GameConstants.ARHISKELETON_HP, GameConstants.ARHISKELETON_DAMAGE, GameConstants.ARHISKELETON_DEFENSE, AttackType.Physical, GameConstants.COINS_PER_MINIBOSS),
            [EnemyType.Necromancer] = ("Necromancer", GameConstants.NECROMANCER_HP, GameConstants.NECROMANCER_DAMAGE, GameConstants.NECROMANCER_DEFENSE, AttackType.Magical, GameConstants.COINS_PER_BOSS)
        };

        /// <summary>
        /// Фабричный метод для создания и инициализации нового врага определенного типа.
        /// </summary>
        /// <param name="enemyType">Тип врага из перечисления EnemyType.</param>
        /// <param name="waveNumber">Номер текущей волны для скалирования характеристик врага.</param>
        /// <returns>Новый настроенный экземпляр Enemy.</returns>
        public static Enemy Create(EnemyType enemyType, int waveNumber)
        {
            Enemy enemy = new Enemy
            {
                EnemyType = enemyType,
                _waveNumber = waveNumber
            };

            var stats = GetEnemyBaseStats(enemyType);

            int scaledHp = ScalingSystem.ScaleEnemyStat(stats.hp, waveNumber);
            int scaledDmg = ScalingSystem.ScaleEnemyStat(stats.damage, waveNumber);
            int scaledDefense = ScalingSystem.ScaleEnemyStat(stats.defense, waveNumber);
            int scaledReward = ScalingSystem.ScaleReward(stats.coinReward, waveNumber);

            enemy.Initialize(stats.name, scaledHp, scaledDmg, scaledDefense, stats.attackType);
            enemy.CoinReward = scaledReward;

            GD.Print($"[Enemy] Created {stats.name} (Wave {waveNumber}) - HP: {scaledHp}, DMG: {scaledDmg}, DEF: {scaledDefense}, Reward: {scaledReward}");
            return enemy;
        }

        private static (string name, int hp, int damage, int defense, AttackType attackType, int coinReward) GetEnemyBaseStats(EnemyType type)
        {
            if (BaseStatsMap.TryGetValue(type, out var stats))
            {
                return stats;
            }
            return ("Unknown Enemy", 10, 5, 0, AttackType.Physical, 1);
        }

        /// <summary>
        /// Вычисляет и возвращает наносимый урон врагом за текущий ход.
        /// Учитывает особенности некоторых типов врагов (например, множественные атаки Архискелета).
        /// </summary>
        /// <returns>Количество базового урона для нанесения героям.</returns>
        public int PerformAttack()
        {
            if (!IsAlive || IsStunned) return 0;

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
        /// Определяет способность Некроманта, которую он будет использовать на текущем ходу.
        /// Некромант использует ротацию способностей.
        /// </summary>
        /// <param name="turnNumber">Номер хода Некроманта в текущем бою.</param>
        /// <returns>Способность для применения (NecromancerAbility).</returns>
        public NecromancerAbility GetNecromancerAbility(int turnNumber)
        {
            if (EnemyType != EnemyType.Necromancer)
            {
                return NecromancerAbility.None;
            }

            return (NecromancerAbility)((turnNumber % 3) + 1);
        }

        /// <summary>
        /// Список доступных способностей для босса Некроманта.
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
