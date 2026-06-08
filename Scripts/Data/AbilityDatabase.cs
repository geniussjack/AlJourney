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
                "fire_storm", "Огненный шторм", AbilityType.Attack, AbilityElement.Fire,
                "res://Assets/Sprites/Abilities/fire_storm.png",
                "Массовая атака огненными шарами по всем врагам",
                100,
                new Dictionary<string, int> { ["damage"] = 25, ["aoe_radius"] = 3 }
            ),

            ["meteor_rain"] = new AbilityData(
                "meteor_rain", "Метеоритный дождь", AbilityType.Attack, AbilityElement.Fire,
                "res://Assets/Sprites/Abilities/meteor_rain.png",
                "Призыв метеоритов, которые наносят урон по области",
                150,
                new Dictionary<string, int> { ["damage"] = 40, ["impact_radius"] = 2 }
            ),

            ["whirlwind"] = new AbilityData(
                "whirlwind", "Вихрь клинков", AbilityType.Attack, AbilityElement.Sword,
                "res://Assets/Sprites/Abilities/whirlwind.png",
                "Атака множественными клинками вокруг героя",
                80,
                new Dictionary<string, int> { ["damage"] = 15, ["hits"] = 5 }
            ),

            ["champion_strike"] = new AbilityData(
                "champion_strike", "Удар чемпиона", AbilityType.Attack, AbilityElement.Sword,
                "res://Assets/Sprites/Abilities/champion_strike.png",
                "Мощный одиночный удар с высоким крит.шансом",
                120,
                new Dictionary<string, int> { ["damage"] = 35, ["crit_chance"] = 50 }
            ),

            ["healing_wave"] = new AbilityData(
                "healing_wave", "Волна жизни", AbilityType.Support, AbilityElement.Heal,
                "res://Assets/Sprites/Abilities/healing_wave.png",
                "Массовое лечение обоих героев",
                60,
                new Dictionary<string, int> { ["heal"] = 30, ["aoe_radius"] = 5 }
            ),

            ["regeneration_aura"] = new AbilityData(
                "regeneration_aura", "Аура регенерации", AbilityType.Support, AbilityElement.Heal,
                "res://Assets/Sprites/Abilities/regeneration_aura.png",
                "Включает регенерацию здоровья для обоих героев",
                80,
                new Dictionary<string, int> { ["hp_regen"] = 5, ["duration"] = 10 }
            ),

            ["bone_wall"] = new AbilityData(
                "bone_wall", "Стена костей", AbilityType.Support, AbilityElement.Shield,
                "res://Assets/Sprites/Abilities/bone_wall.png",
                "Создает защитную стену, блокирующую урон",
                100,
                new Dictionary<string, int> { ["defense"] = 20, ["duration"] = 5 }
            ),

            ["guardian_summon"] = new AbilityData(
                "guardian_summon", "Призыв стража", AbilityType.Support, AbilityElement.Shield,
                "res://Assets/Sprites/Abilities/guardian_summon.png",
                "Призывает временного стража для защиты",
                150,
                new Dictionary<string, int> { ["guardian_hp"] = 50, ["duration"] = 8 }
            )
        };
    }
}
