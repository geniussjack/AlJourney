using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Ability system manager. Responsible for managing unlocking, equipping and applying ability effects for characters.
    /// </summary>
    public partial class AbilitySystem : Node, IAbilitySystem
    {
        /// <summary>
        /// Global instance of the ability system manager.
        /// </summary>
        public static AbilitySystem Instance { get; private set; } = null!;

        private readonly Dictionary<string, AbilityData> _abilityTemplates = AbilityDatabase.Templates;
        private readonly Dictionary<CharacterClass, List<AbilityData>> _equippedAbilities = [];

        /// <summary>
        /// Initializes the ability system node when it is added to the scene tree.
        /// Ensures only a single instance of the manager exists.
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
        /// Gets the list of every ability available to the given character class.
        /// </summary>
        /// <param name="heroClass">The character class to get available abilities for.</param>
        /// <returns>The list of available abilities.</returns>
        public List<AbilityData> GetAvailableAbilities(CharacterClass heroClass)
        {
            return [.. _abilityTemplates.Values.Where(ability => IsAbilityForHero(ability, heroClass))];
        }

        /// <summary>
        /// Gets the list of abilities currently equipped by the given character class.
        /// </summary>
        /// <param name="heroClass">The character class to get equipped abilities for.</param>
        /// <returns>The list of equipped abilities, or an empty list if none are equipped.</returns>
        public List<AbilityData> GetEquippedAbilities(CharacterClass heroClass)
        {
            return _equippedAbilities.TryGetValue(heroClass, out List<AbilityData> abilities) ? abilities : [];
        }

        /// <summary>
        /// Unlocks the given ability for the specified character in exchange for in-game currency.
        /// </summary>
        /// <param name="hero">The character class the ability is unlocked for.</param>
        /// <param name="ability">The ability data to unlock.</param>
        /// <returns><c>true</c> if the ability was successfully unlocked; <c>false</c> if there weren't enough coins.</returns>
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
        /// Equips the given ability to the specified character. A single character cannot have more than 3 abilities equipped.
        /// </summary>
        /// <param name="hero">The character class the ability is equipped to.</param>
        /// <param name="ability">The ability to equip.</param>
        /// <returns><c>true</c> if the ability was successfully equipped; <c>false</c> if the 3-ability limit was reached.</returns>
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
        /// Computes and returns the total value of a specific effect across all of the character's equipped abilities.
        /// </summary>
        /// <param name="hero">The character class.</param>
        /// <param name="effectName">The name of the effect to look up.</param>
        /// <returns>The total value of the effect.</returns>
        public int GetAbilityEffect(CharacterClass hero, string effectName)
        {
            return !_equippedAbilities.TryGetValue(hero, out List<AbilityData> abilities) ? 0 : abilities.Sum(ability => ability.GetEffect(effectName));
        }

        /// <summary>
        /// Gets a dictionary with the total value of every stat granted by the given character's equipped abilities.
        /// </summary>
        /// <param name="hero">The character class.</param>
        /// <returns>A dictionary where the key is the stat name and the value is its total bonus.</returns>
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
