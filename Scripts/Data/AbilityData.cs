using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Data structure representing a character ability.
    /// Stores the id, name, type, element, icon path, description, unlock cost and effects of the ability.
    /// </summary>
    public record AbilityData(
        string Id,
        string Name,
        AbilityType Type,
        AbilityElement Element,
        string IconPath,
        string Description,
        int UnlockCost,
        Dictionary<string, int> Effects,
        AbilityTargetType TargetType,
        bool IsAoE = false,
        bool IsUltimate = false
    )
    {
        /// <summary>
        /// Returns the color associated with this ability's element.
        /// Used for color coding in the user interface.
        /// </summary>
        /// <returns>The color to display for the element.</returns>
        public Color GetElementColor()
        {
            return Element switch
            {
                AbilityElement.Fire => Colors.Orange,
                AbilityElement.Heal => Colors.Green,
                AbilityElement.Sword => Colors.Red,
                AbilityElement.Shield => Colors.Blue,
                _ => Colors.White
            };
        }

        /// <summary>
        /// Indicates whether this ability is an attack ability.
        /// </summary>
        public bool IsAttackAbility => Type == AbilityType.Attack;

        /// <summary>
        /// Indicates whether this ability is a support ability.
        /// </summary>
        public bool IsSupportAbility => Type == AbilityType.Support;

        /// <summary>
        /// Returns the value of the first effect in the ability's effect dictionary.
        /// Convenient for abilities that only have a single numeric effect.
        /// </summary>
        /// <returns>The value of the primary effect, or 0 if there are no effects.</returns>
        public int GetPrimaryEffect()
        {
            return Effects.Values.FirstOrDefault();
        }

        /// <summary>
        /// Returns the value of a specific effect by its name.
        /// </summary>
        /// <param name="effectName">The name of the effect.</param>
        /// <returns>The numeric value of the effect if found, otherwise 0.</returns>
        public int GetEffect(string effectName)
        {
            return Effects.TryGetValue(effectName, out int value) ? value : 0;
        }

        /// <summary>
        /// Returns a string representation of the ability, including its name, type and element.
        /// </summary>
        /// <returns>A string in the format "Name (Type - Element)".</returns>
        public override string ToString()
        {
            return $"{Name} ({Type} - {Element})";
        }
    }
}
