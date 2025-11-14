using AltarionsJourney.Core;

namespace AltarionsJourney.Data
{
    /// <summary>
    /// Represents an active status effect on a character.
    /// </summary>
    public class StatusEffectData
    {
        /// <summary>
        /// Type of the status effect.
        /// </summary>
        public StatusEffect Type { get; set; }

        /// <summary>
        /// Remaining duration in turns.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Effect power (damage per turn, shield amount, etc).
        /// </summary>
        public int Power { get; set; }

        /// <summary>
        /// Additional data for specific effects (e.g., reflect percentage).
        /// </summary>
        public float ExtraData { get; set; }

        public StatusEffectData(StatusEffect type, int duration, int power, float extraData = 0f)
        {
            Type = type;
            Duration = duration;
            Power = power;
            ExtraData = extraData;
        }

        /// <summary>
        /// Decreases duration by 1. Returns true if effect should be removed.
        /// </summary>
        public bool TickDuration()
        {
            Duration--;
            return Duration <= 0;
        }
    }
}