using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Элемент AbilityData.
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
        /// Возвращает ElementColor.
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
        /// Проверяет, является ли AttackAbility.
        /// </summary>
        public bool IsAttackAbility => Type == AbilityType.Attack;

        /// <summary>
        /// Проверяет, является ли SupportAbility.
        /// </summary>
        public bool IsSupportAbility => Type == AbilityType.Support;

        /// <summary>
        /// Возвращает PrimaryEffect.
        /// </summary>
        public int GetPrimaryEffect()
        {
            return Effects.Values.FirstOrDefault();
        }

        /// <summary>
        /// Возвращает Effect.
        /// </summary>
        public int GetEffect(string effectName)
        {
            return Effects.TryGetValue(effectName, out int value) ? value : 0;
        }

        /// <summary>
        /// Элемент ToString.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Type} - {Element})";
        }
    }
}
