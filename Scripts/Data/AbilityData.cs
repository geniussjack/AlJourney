using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Represents an ability that can be equipped by characters.
    /// </summary>
    public record AbilityData(
        string Id,
        string Name,
        AbilityType Type,
        AbilityElement Element,
        string IconPath,
        string Description,
        int UnlockCost,
        Dictionary<string, int> Effects
    )
    {
        /// <summary>
        /// Gets the color associated with this element.
        /// </summary>
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
        /// Checks if this ability is an attack type.
        /// </summary>
        public bool IsAttackAbility => Type == AbilityType.Attack;

        /// <summary>
        /// Checks if this ability is a support type.
        /// </summary>
        public bool IsSupportAbility => Type == AbilityType.Support;

        /// <summary>
        /// Gets the primary effect value.
        /// </summary>
        public int GetPrimaryEffect()
        {
            return Effects.Values.FirstOrDefault();
        }

        /// <summary>
        /// Gets a specific effect value.
        /// </summary>
        public int GetEffect(string effectName)
        {
            return Effects.TryGetValue(effectName, out int value) ? value : 0;
        }

        public override string ToString()
        {
            return $"{Name} ({Type} - {Element})";
        }
    }
}
