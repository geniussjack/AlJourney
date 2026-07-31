using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// База данных способностей персонажей.
    /// У Эльтариона (Маг) и Элдрика (Пехотинец) — по одной атакующей и одной защитной/поддерживающей
    /// способности каждый (Этап 1), плюс уникальная ультимативная способность у каждого (Этап 2). Это
    /// финальный набор способностей для самих героев (не заглушка) — они не привязаны к экипировке и не
    /// разблокируются за монеты, в отличие от будущих наёмников, чьи способности будут определяться
    /// подклассом/типом снаряжения.
    /// </summary>
    public static class AbilityDatabase
    {
        /// <summary>
        /// Атакующая способность Эльтариона: одиночный урон по врагу.
        /// </summary>
        public static readonly AbilityData AltarionAttack = new(
            "altarion_fireball", "ABILITY_ALTARION_FIREBALL", AbilityType.Attack, AbilityElement.Fire,
            "res://Resources/Sprites/Abilities/fireball.png",
            "ABILITY_ALTARION_FIREBALL_DESC",
            0,
            new Dictionary<string, int> { ["damage"] = 22 },
            AbilityTargetType.Enemy
        );

        /// <summary>
        /// Защитная способность Эльтариона: исцеление себя или союзника.
        /// </summary>
        public static readonly AbilityData AltarionSupport = new(
            "altarion_healing_light", "ABILITY_ALTARION_HEALING_LIGHT", AbilityType.Support, AbilityElement.Heal,
            "res://Resources/Sprites/Abilities/healing_light.png",
            "ABILITY_ALTARION_HEALING_LIGHT_DESC",
            0,
            new Dictionary<string, int> { ["heal"] = 18 },
            AbilityTargetType.AllyOrSelf
        );

        /// <summary>
        /// Ультимативная способность Эльтариона: огненный шторм по всем живым врагам (AoE).
        /// </summary>
        public static readonly AbilityData AltarionUltimate = new(
            "altarion_meteor_storm", "ABILITY_ALTARION_METEOR_STORM", AbilityType.Attack, AbilityElement.Fire,
            "res://Resources/Sprites/Abilities/meteor_storm.png",
            "ABILITY_ALTARION_METEOR_STORM_DESC",
            0,
            new Dictionary<string, int> { ["damage"] = 40 },
            AbilityTargetType.Enemy,
            IsAoE: true,
            IsUltimate: true
        );

        /// <summary>
        /// Атакующая способность Элдрика: одиночный удар по врагу.
        /// </summary>
        public static readonly AbilityData AldricAttack = new(
            "aldric_sword_strike", "ABILITY_ALDRIC_SWORD_STRIKE", AbilityType.Attack, AbilityElement.Sword,
            "res://Resources/Sprites/Abilities/sword_strike.png",
            "ABILITY_ALDRIC_SWORD_STRIKE_DESC",
            0,
            new Dictionary<string, int> { ["damage"] = 26 },
            AbilityTargetType.Enemy
        );

        /// <summary>
        /// Защитная способность Элдрика: щит на себя или союзника.
        /// </summary>
        public static readonly AbilityData AldricSupport = new(
            "aldric_shield_wall", "ABILITY_ALDRIC_SHIELD_WALL", AbilityType.Support, AbilityElement.Shield,
            "res://Resources/Sprites/Abilities/shield_wall.png",
            "ABILITY_ALDRIC_SHIELD_WALL_DESC",
            0,
            new Dictionary<string, int> { ["shield"] = 22 },
            AbilityTargetType.AllyOrSelf
        );

        /// <summary>
        /// Ультимативная способность Элдрика: сокрушающий удар по врагу с наибольшим текущим HP.
        /// Цель выбирается автоматически (см. <see cref="Battle.Rules.AbilityTargetingRules.SelectHighestHealthTarget"/>),
        /// без подтверждения игроком.
        /// </summary>
        public static readonly AbilityData AldricUltimate = new(
            "aldric_crushing_blow", "ABILITY_ALDRIC_CRUSHING_BLOW", AbilityType.Attack, AbilityElement.Sword,
            "res://Resources/Sprites/Abilities/crushing_blow.png",
            "ABILITY_ALDRIC_CRUSHING_BLOW_DESC",
            0,
            new Dictionary<string, int> { ["damage"] = 70 },
            AbilityTargetType.Enemy,
            IsAoE: false,
            IsUltimate: true
        );

        /// <summary>
        /// Плоский реестр всех способностей по Id. Используется только устаревшей системой разблокировки/
        /// экипировки способностей (<see cref="AlJourney.Scripts.Managers.AbilitySystem"/>), которая в бою
        /// не участвует в боевой логике героев (см. REDESIGN_NOTES.md) и оставлена нетронутой вне рамок задачи.
        /// </summary>
        public static readonly Dictionary<string, AbilityData> Templates = new()
        {
            [AltarionAttack.Id] = AltarionAttack,
            [AltarionSupport.Id] = AltarionSupport,
            [AldricAttack.Id] = AldricAttack,
            [AldricSupport.Id] = AldricSupport
        };

        /// <summary>
        /// Возвращает пару фиксированных способностей (атака, защита) для главного героя указанного класса.
        /// Применимо только к Эльтариону/Элдрику — у наёмников способности будут определяться подклассом
        /// снаряжения, а не этим методом (см. REDESIGN_NOTES.md, раздел 4).
        /// </summary>
        /// <param name="heroClass">Класс героя.</param>
        /// <returns>Кортеж с атакующей и защитной способностью героя.</returns>
        public static (AbilityData Attack, AbilityData Support) GetHeroAbilities(CharacterClass heroClass)
        {
            return heroClass switch
            {
                CharacterClass.Mage => (AltarionAttack, AltarionSupport),
                CharacterClass.Warrior => (AldricAttack, AldricSupport),
                _ => (AltarionAttack, AltarionSupport)
            };
        }

        /// <summary>
        /// Возвращает уникальную ультимативную способность главного героя указанного класса.
        /// </summary>
        /// <param name="heroClass">Класс героя.</param>
        /// <returns>Ультимативная способность героя.</returns>
        public static AbilityData GetHeroUltimate(CharacterClass heroClass)
        {
            return heroClass switch
            {
                CharacterClass.Mage => AltarionUltimate,
                CharacterClass.Warrior => AldricUltimate,
                _ => AltarionUltimate
            };
        }
    }
}
