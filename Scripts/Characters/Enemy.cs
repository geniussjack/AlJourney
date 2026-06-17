using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Characters
{
    /// <summary>
    /// Класс врага. Наследует базовый класс Character.
    /// Обрабатывает типы врагов, их базовые характеристики, скалирование с волнами
    /// и специфические атаки.
    /// </summary>
    public partial class Enemy : Character
    {
        private int _waveNumber;

        /// <summary>
        /// Тип данного врага.
        /// </summary>
        public EnemyType EnemyType { get; private set; }

        /// <summary>
        /// Награда в виде золотых монет за убийство данного врага.
        /// </summary>
        public int CoinReward { get; private set; }

        private int _initialStackCount;

        /// <summary>
        /// Текущее количество существ в отряде.
        /// </summary>
        public int StackCount { get => _initialStackCount > 0 ? Mathf.CeilToInt(CurrentHealth / ((float)TotalMaxHealth / _initialStackCount)) : 1; private set { } }

        public new string CharacterName => StackCount > 1 ? $"{Tr(_name)} x{StackCount}" : Tr(_name);

        /// <summary>
        /// Является ли данный враг мини-боссом.
        /// </summary>
        public bool IsMiniboss => EnemyType is EnemyType.GeneralOfDraugr or EnemyType.Arhiskeleton;

        /// <summary>
        /// Является ли данный враг главным боссом.
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
        /// Фабричный метод для создания и инициализации нового врага определенного типа.
        /// </summary>
        /// <param name="enemyType">Тип врага из перечисления EnemyType.</param>
        /// <param name="waveNumber">Номер текущей волны для скалирования характеристик врага.</param>
        /// <param name="stackCount">Количество врагов в стаке.</param>
        /// <returns>Новый настроенный экземпляр Enemy.</returns>
        public static Enemy Create(EnemyType enemyType, int waveNumber, int stackCount = 1)
        {
            Enemy enemy = new()
            {
                EnemyType = enemyType,
                _waveNumber = waveNumber,
                _initialStackCount = stackCount
            };

            (string name, int hp, int damage, int defense, AttackType attackType, int coinReward) = GetEnemyBaseStats(enemyType);

            int scaledHp = ScalingSystem.ScaleEnemyStat(hp, waveNumber);
            int scaledDmg = ScalingSystem.ScaleEnemyStat(damage, waveNumber);
            int scaledDefense = ScalingSystem.ScaleEnemyStat(defense, waveNumber);
            int scaledReward = ScalingSystem.ScaleReward(coinReward, waveNumber);

            enemy.StackCount = scaledHp;

            int totalHp = scaledHp * stackCount;
            // Урон и защита будут учитываться в методах с учетом StackCount

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
        /// Вычисляет и возвращает наносимый урон врагом за текущий ход.
        /// Учитывает особенности некоторых типов врагов.
        /// </summary>
        /// <returns>Количество базового урона для нанесения героям.</returns>
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

            return damage;
        }

        /// <summary>
        /// Определяет способность Некроманта, которую он будет использовать на текущем ходу.
        /// Некромант использует ротацию способностей.
        /// </summary>
        /// <param name="turnNumber">Номер хода Некроманта в текущем бою.</param>
        /// <returns>Способность для применения.</returns>
        public NecromancerAbility GetNecromancerAbility(int turnNumber)
        {
            return EnemyType != EnemyType.Necromancer ? NecromancerAbility.None : (NecromancerAbility)((turnNumber % 3) + 1);
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
