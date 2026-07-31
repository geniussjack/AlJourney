using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Database of character abilities.
    /// Altarion (Mage) and Aldric (Warrior) each have one attack and one defensive/support ability
    /// (Stage 1), plus a unique ultimate ability each (Stage 2). This is the final ability set for the
    /// main heroes themselves (not a placeholder) — they are not tied to equipment and are not unlocked
    /// with coins, unlike future mercenaries, whose abilities will be determined by their equipment
    /// subclass/type.
    /// </summary>
    public static class AbilityDatabase
    {
        /// <summary>
        /// Altarion's attack ability: single-target damage to an enemy.
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
        /// Altarion's support ability: heals himself or an ally.
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
        /// Altarion's ultimate ability: a firestorm hitting every living enemy (AoE).
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
        /// Aldric's attack ability: single-target strike against an enemy.
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
        /// Aldric's support ability: shields himself or an ally.
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
        /// Aldric's ultimate ability: a crushing blow against the enemy with the highest current HP.
        /// The target is selected automatically (see <see cref="Battle.Rules.AbilityTargetingRules.SelectHighestHealthTarget"/>),
        /// without player confirmation.
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
        /// Flat registry of every ability by Id. Only used by the legacy ability unlock/equip system
        /// (<see cref="AlJourney.Scripts.Managers.AbilitySystem"/>), which does not participate in the
        /// heroes' combat logic (see REDESIGN_NOTES.md) and has been left untouched outside the scope of
        /// this task.
        /// </summary>
        public static readonly Dictionary<string, AbilityData> Templates = new()
        {
            [AltarionAttack.Id] = AltarionAttack,
            [AltarionSupport.Id] = AltarionSupport,
            [AldricAttack.Id] = AldricAttack,
            [AldricSupport.Id] = AldricSupport
        };

        /// <summary>
        /// Returns the fixed pair of abilities (attack, support) for the main hero of the given class.
        /// Only applies to Altarion/Aldric — mercenaries' abilities will be determined by their equipment
        /// subclass, not by this method (see REDESIGN_NOTES.md, section 4).
        /// </summary>
        /// <param name="heroClass">The hero's class.</param>
        /// <returns>A tuple with the hero's attack and support ability.</returns>
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
        /// Returns the unique ultimate ability of the main hero of the given class.
        /// </summary>
        /// <param name="heroClass">The hero's class.</param>
        /// <returns>The hero's ultimate ability.</returns>
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
