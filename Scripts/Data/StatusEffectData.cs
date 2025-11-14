using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Represents an active status effect on a character.
    /// </summary>
    public class StatusEffectData(StatusEffect type, int duration, int power, float extraData = 0f)
    {
        /// <summary>
        /// Type of the status effect.
        /// </summary>
        public StatusEffect Type { get; set; } = type;

        /// <summary>
        /// Remaining duration in turns.
        /// </summary>
        public int Duration { get; set; } = duration;

        /// <summary>
        /// Effect power (damage per turn, shield amount, etc).
        /// </summary>
        public int Power { get; set; } = power;

        /// <summary>
        /// Additional data for specific effects (e.g., reflect percentage).
        /// </summary>
        public float ExtraData { get; set; } = extraData;

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