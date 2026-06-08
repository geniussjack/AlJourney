using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер AbilitySystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class AbilitySystem : Node, IAbilitySystem
    {
        /// <summary>
        /// Элемент Instance.
        /// </summary>
        public static AbilitySystem Instance { get; private set; } = null!;

        private readonly Dictionary<string, AbilityData> _abilityTemplates = AbilityDatabase.Templates;
        private readonly Dictionary<CharacterClass, List<AbilityData>> _equippedAbilities = [];

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            GD.Print("[AbilitySystem] Initialized");
        }

        /// <summary>
        /// Возвращает AvailableAbilities.
        /// </summary>
        public List<AbilityData> GetAvailableAbilities(CharacterClass heroClass)
        {
            return [.. _abilityTemplates.Values.Where(ability => IsAbilityForHero(ability, heroClass))];
        }

        /// <summary>
        /// Возвращает EquippedAbilities.
        /// </summary>
        public List<AbilityData> GetEquippedAbilities(CharacterClass heroClass)
        {
            return _equippedAbilities.TryGetValue(heroClass, out List<AbilityData> abilities) ? abilities : [];
        }

        /// <summary>
        /// Элемент UnlockAbility.
        /// </summary>
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

        /// <summary>
        /// Экипирует Ability.
        /// </summary>
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

        /// <summary>
        /// Возвращает AbilityEffect.
        /// </summary>
        public int GetAbilityEffect(CharacterClass hero, string effectName)
        {
            return !_equippedAbilities.TryGetValue(hero, out List<AbilityData> abilities) ? 0 : abilities.Sum(ability => ability.GetEffect(effectName));
        }

        /// <summary>
        /// Возвращает TotalAbilityStats.
        /// </summary>
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
