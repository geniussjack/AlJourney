using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Класс данных AbilityDatabase. Сохраняет информацию и параметры.
    /// </summary>
    public static class AbilityDatabase
    {
        /// <summary>
        /// Элемент Templates.
        /// </summary>
        public static readonly Dictionary<string, AbilityData> Templates = new()
        {
            ["fire_storm"] = new AbilityData(
                "fire_storm", "Fire Storm", AbilityType.Attack, AbilityElement.Fire,
                "res://Assets/Sprites/Abilities/fire_storm.png",
                "Massive attack with fireballs hitting all enemies",
                100,
                new Dictionary<string, int> { ["damage"] = 25, ["aoe_radius"] = 3 }
            ),

            ["meteor_rain"] = new AbilityData(
                "meteor_rain", "Meteor Rain", AbilityType.Attack, AbilityElement.Fire,
                "res://Assets/Sprites/Abilities/meteor_rain.png",
                "Summons meteors dealing area of effect damage",
                150,
                new Dictionary<string, int> { ["damage"] = 40, ["impact_radius"] = 2 }
            ),

            ["whirlwind"] = new AbilityData(
                "whirlwind", "Whirlwind", AbilityType.Attack, AbilityElement.Sword,
                "res://Assets/Sprites/Abilities/whirlwind.png",
                "Multiple blade strikes around the hero",
                80,
                new Dictionary<string, int> { ["damage"] = 15, ["hits"] = 5 }
            ),

            ["champion_strike"] = new AbilityData(
                "champion_strike", "Champion's Strike", AbilityType.Attack, AbilityElement.Sword,
                "res://Assets/Sprites/Abilities/champion_strike.png",
                "A powerful single strike with high critical chance",
                120,
                new Dictionary<string, int> { ["damage"] = 35, ["crit_chance"] = 50 }
            ),

            ["healing_wave"] = new AbilityData(
                "healing_wave", "Healing Wave", AbilityType.Support, AbilityElement.Heal,
                "res://Assets/Sprites/Abilities/healing_wave.png",
                "Mass heal for both heroes",
                60,
                new Dictionary<string, int> { ["heal"] = 30, ["aoe_radius"] = 5 }
            ),

            ["regeneration_aura"] = new AbilityData(
                "regeneration_aura", "Regeneration Aura", AbilityType.Support, AbilityElement.Heal,
                "res://Assets/Sprites/Abilities/regeneration_aura.png",
                "Activates health regeneration for both heroes",
                80,
                new Dictionary<string, int> { ["hp_regen"] = 5, ["duration"] = 10 }
            ),

            ["bone_wall"] = new AbilityData(
                "bone_wall", "Bone Wall", AbilityType.Support, AbilityElement.Shield,
                "res://Assets/Sprites/Abilities/bone_wall.png",
                "Creates a defensive wall blocking damage",
                100,
                new Dictionary<string, int> { ["defense"] = 20, ["duration"] = 5 }
            ),

            ["guardian_summon"] = new AbilityData(
                "guardian_summon", "Guardian Summon", AbilityType.Support, AbilityElement.Shield,
                "res://Assets/Sprites/Abilities/guardian_summon.png",
                "Summons a temporary guardian for defense",
                150,
                new Dictionary<string, int> { ["guardian_hp"] = 50, ["duration"] = 8 }
            )
        };
    }
}
