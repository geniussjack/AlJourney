using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for managing hero abilities.
    /// Provides methods to retrieve, unlock and equip abilities, and to compute their effects.
    /// </summary>
    public interface IAbilitySystem
    {
        /// <summary>
        /// Returns the list of every ability available to the given hero class.
        /// </summary>
        List<AbilityData> GetAvailableAbilities(CharacterClass heroClass);

        /// <summary>
        /// Returns the list of abilities currently equipped by the given hero.
        /// </summary>
        List<AbilityData> GetEquippedAbilities(CharacterClass heroClass);

        /// <summary>
        /// Unlocks the given ability for the given hero, making it available to equip.
        /// </summary>
        bool UnlockAbility(CharacterClass hero, AbilityData ability);

        /// <summary>
        /// Equips an ability to a hero, applying its effects in-game.
        /// </summary>
        bool EquipAbility(CharacterClass hero, AbilityData ability);

        /// <summary>
        /// Gets the total numeric value of a specific effect, based on the hero's equipped abilities.
        /// </summary>
        int GetAbilityEffect(CharacterClass hero, string effectName);

        /// <summary>
        /// Returns a dictionary of every total stat bonus granted by the hero's active abilities.
        /// </summary>
        Dictionary<string, int> GetTotalAbilityStats(CharacterClass hero);
    }
}
