using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// База данных способностей персонажей.
    /// В Этапе 1 редизайна (пошаговый бой) у Эльтариона (Маг) и Эльдрика (Пехотинец) — по одной
    /// атакующей и одной защитной/поддерживающей способности каждый. Это финальный набор способностей
    /// для самих героев (не заглушка) — они не привязаны к экипировке и не разблокируются за монеты,
    /// в отличие от будущих наёмников, чьи способности будут определяться подклассом/типом снаряжения.
    /// </summary>
    public static class AbilityDatabase
    {
        /// <summary>
        /// Атакующая способность Эльтариона: одиночный урон по врагу.
        /// </summary>
        public static readonly AbilityData EltarionAttack = new(
            "eltarion_fireball", "ABILITY_ELTARION_FIREBALL", AbilityType.Attack, AbilityElement.Fire,
            "res://Resources/Sprites/Abilities/fireball.png",
            "ABILITY_ELTARION_FIREBALL_DESC",
            0,
            new Dictionary<string, int> { ["damage"] = 22 },
            AbilityTargetType.Enemy
        );

        /// <summary>
        /// Защитная способность Эльтариона: исцеление себя или союзника.
        /// </summary>
        public static readonly AbilityData EltarionSupport = new(
            "eltarion_healing_light", "ABILITY_ELTARION_HEALING_LIGHT", AbilityType.Support, AbilityElement.Heal,
            "res://Resources/Sprites/Abilities/healing_light.png",
            "ABILITY_ELTARION_HEALING_LIGHT_DESC",
            0,
            new Dictionary<string, int> { ["heal"] = 18 },
            AbilityTargetType.AllyOrSelf
        );

        /// <summary>
        /// Атакующая способность Эльдрика: одиночный удар по врагу.
        /// </summary>
        public static readonly AbilityData EldricAttack = new(
            "eldric_sword_strike", "ABILITY_ELDRIC_SWORD_STRIKE", AbilityType.Attack, AbilityElement.Sword,
            "res://Resources/Sprites/Abilities/sword_strike.png",
            "ABILITY_ELDRIC_SWORD_STRIKE_DESC",
            0,
            new Dictionary<string, int> { ["damage"] = 26 },
            AbilityTargetType.Enemy
        );

        /// <summary>
        /// Защитная способность Эльдрика: щит на себя или союзника.
        /// </summary>
        public static readonly AbilityData EldricSupport = new(
            "eldric_shield_wall", "ABILITY_ELDRIC_SHIELD_WALL", AbilityType.Support, AbilityElement.Shield,
            "res://Resources/Sprites/Abilities/shield_wall.png",
            "ABILITY_ELDRIC_SHIELD_WALL_DESC",
            0,
            new Dictionary<string, int> { ["shield"] = 22 },
            AbilityTargetType.AllyOrSelf
        );

        /// <summary>
        /// Плоский реестр всех способностей по Id. Используется только устаревшей системой разблокировки/
        /// экипировки способностей (<see cref="AlJourney.Scripts.Managers.AbilitySystem"/>), которая в Этапе 1
        /// не участвует в боевой логике героев (см. REDESIGN_NOTES.md) и оставлена нетронутой вне рамок задачи.
        /// </summary>
        public static readonly Dictionary<string, AbilityData> Templates = new()
        {
            [EltarionAttack.Id] = EltarionAttack,
            [EltarionSupport.Id] = EltarionSupport,
            [EldricAttack.Id] = EldricAttack,
            [EldricSupport.Id] = EldricSupport
        };

        /// <summary>
        /// Возвращает пару фиксированных способностей (атака, защита) для главного героя указанного класса.
        /// Применимо только к Эльтариону/Эльдрику — у наёмников способности будут определяться подклассом
        /// снаряжения, а не этим методом (см. REDESIGN_NOTES.md, раздел 4).
        /// </summary>
        /// <param name="heroClass">Класс героя.</param>
        /// <returns>Кортеж с атакующей и защитной способностью героя.</returns>
        public static (AbilityData Attack, AbilityData Support) GetHeroAbilities(CharacterClass heroClass)
        {
            return heroClass switch
            {
                CharacterClass.Mage => (EltarionAttack, EltarionSupport),
                CharacterClass.Warrior => (EldricAttack, EldricSupport),
                _ => (EltarionAttack, EltarionSupport)
            };
        }
    }
}
