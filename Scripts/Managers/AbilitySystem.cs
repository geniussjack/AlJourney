using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Simple working ability system without signal issues.
    /// </summary>
    public partial class AbilitySystem : Node
    {
        public static AbilitySystem Instance { get; private set; } = null!;

        private readonly Dictionary<string, AbilityData> _abilityTemplates = [];
        private readonly Dictionary<CharacterClass, List<AbilityData>> _equippedAbilities = [];

        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            InitializeAbilityTemplates();
            GD.Print("[AbilitySystem] Initialized");
        }

        private void InitializeAbilityTemplates()
        {
            // Fire Attack Abilities
            _abilityTemplates["fire_storm"] = new AbilityData(
                "fire_storm", "Огненный шторм", AbilityType.Attack, AbilityElement.Fire,
                "res://Assets/Sprites/Abilities/fire_storm.png",
                "Массовая атака огненными шарами по всем врагам",
                100,
                new Dictionary<string, int> { ["damage"] = 25, ["aoe_radius"] = 3 }
            );

            _abilityTemplates["meteor_rain"] = new AbilityData(
                "meteor_rain", "Метеоритный дождь", AbilityType.Attack, AbilityElement.Fire,
                "res://Assets/Sprites/Abilities/meteor_rain.png",
                "Призыв метеоритов, которые наносят урон по области",
                150,
                new Dictionary<string, int> { ["damage"] = 40, ["impact_radius"] = 2 }
            );

            // Sword Attack Abilities
            _abilityTemplates["whirlwind"] = new AbilityData(
                "whirlwind", "Вихрь клинков", AbilityType.Attack, AbilityElement.Sword,
                "res://Assets/Sprites/Abilities/whirlwind.png",
                "Атака множественными клинками вокруг героя",
                80,
                new Dictionary<string, int> { ["damage"] = 15, ["hits"] = 5 }
            );

            _abilityTemplates["champion_strike"] = new AbilityData(
                "champion_strike", "Удар чемпиона", AbilityType.Attack, AbilityElement.Sword,
                "res://Assets/Sprites/Abilities/champion_strike.png",
                "Мощный одиночный удар с высоким крит.шансом",
                120,
                new Dictionary<string, int> { ["damage"] = 35, ["crit_chance"] = 50 }
            );

            // Heal Support Abilities
            _abilityTemplates["healing_wave"] = new AbilityData(
                "healing_wave", "Волна жизни", AbilityType.Support, AbilityElement.Heal,
                "res://Assets/Sprites/Abilities/healing_wave.png",
                "Массовое лечение обоих героев",
                60,
                new Dictionary<string, int> { ["heal"] = 30, ["aoe_radius"] = 5 }
            );

            _abilityTemplates["regeneration_aura"] = new AbilityData(
                "regeneration_aura", "Аура регенерации", AbilityType.Support, AbilityElement.Heal,
                "res://Assets/Sprites/Abilities/regeneration_aura.png",
                "Включает регенерацию здоровья для обоих героев",
                80,
                new Dictionary<string, int> { ["hp_regen"] = 5, ["duration"] = 10 }
            );

            // Shield Support Abilities
            _abilityTemplates["bone_wall"] = new AbilityData(
                "bone_wall", "Стена костей", AbilityType.Support, AbilityElement.Shield,
                "res://Assets/Sprites/Abilities/bone_wall.png",
                "Создает защитную стену, блокирующую урон",
                100,
                new Dictionary<string, int> { ["defense"] = 20, ["duration"] = 5 }
            );

            _abilityTemplates["guardian_summon"] = new AbilityData(
                "guardian_summon", "Призыв стража", AbilityType.Support, AbilityElement.Shield,
                "res://Assets/Sprites/Abilities/guardian_summon.png",
                "Призывает временного стража для защиты",
                150,
                new Dictionary<string, int> { ["guardian_hp"] = 50, ["duration"] = 8 }
            );
        }

        public List<AbilityData> GetAvailableAbilities(CharacterClass heroClass)
        {
            return [.. _abilityTemplates.Values.Where(ability => IsAbilityForHero(ability, heroClass))];
        }

        public List<AbilityData> GetEquippedAbilities(CharacterClass heroClass)
        {
            return _equippedAbilities.TryGetValue(heroClass, out List<AbilityData> abilities) ? abilities : [];
        }

        public bool UnlockAbility(CharacterClass hero, AbilityData ability)
        {
            if (GameStateManager.Instance.Coins < ability.UnlockCost)
            {
                return false;
            }

            _ = GameStateManager.Instance.SpendCoins(ability.UnlockCost);

            if (!_equippedAbilities.TryGetValue(hero, out List<AbilityData> value))
            {
                value = [];
                _equippedAbilities[hero] = value;
            }

            value.Add(ability);
            GD.Print($"[SimpleAbilitySystem] Unlocked ability {ability.Name} for {hero}");
            return true;
        }

        public bool EquipAbility(CharacterClass hero, AbilityData ability)
        {
            if (!_equippedAbilities.TryGetValue(hero, out List<AbilityData> value))
            {
                value = [];
                _equippedAbilities[hero] = value;
            }

            if (value.Count >= 3)
            {
                return false;
            }

            value.Add(ability);
            GD.Print($"[SimpleAbilitySystem] Equipped ability {ability.Name} for {hero}");
            return true;
        }

        public int GetAbilityEffect(CharacterClass hero, string effectName)
        {
            return !_equippedAbilities.TryGetValue(hero, out List<AbilityData> abilities) ? 0 : abilities.Sum(ability => ability.GetEffect(effectName));
        }

        public Dictionary<string, int> GetTotalAbilityStats(CharacterClass hero)
        {
            Dictionary<string, int> totalStats = [];

            if (!_equippedAbilities.TryGetValue(hero, out List<AbilityData> abilities))
            {
                return totalStats;
            }

            foreach (AbilityData ability in abilities)
            {
                foreach (KeyValuePair<string, int> effect in ability.Effects)
                {
                    if (totalStats.ContainsKey(effect.Key))
                    {
                        totalStats[effect.Key] += effect.Value;
                    }
                    else
                    {
                        totalStats[effect.Key] = effect.Value;
                    }
                }
            }

            return totalStats;
        }

        private static bool IsAbilityForHero(AbilityData ability, CharacterClass heroClass)
        {
            return (heroClass == CharacterClass.Mage && (ability.Element == AbilityElement.Fire || ability.Element == AbilityElement.Heal)) ||
                   (heroClass == CharacterClass.Warrior && (ability.Element == AbilityElement.Sword || ability.Element == AbilityElement.Shield));
        }
    }
}
