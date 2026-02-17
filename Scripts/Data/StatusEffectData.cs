using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Represents an active status effect on a character.
    /// </summary>
    public record StatusEffectData(StatusEffect Type, int Duration, int Power, float ExtraData = 0f)
    {
        /// <summary>
        /// Decreases duration by 1. Returns true if effect should be removed.
        /// </summary>
        public StatusEffectData TickDuration()
        {
            return this with { Duration = Duration - 1 };
        }

        /// <summary>
        /// Checks if effect should be removed.
        /// </summary>
        public bool ShouldRemove => Duration <= 0;
    }
}
