using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Structure describing a status effect applied to a character or enemy.
    /// </summary>
    public record StatusEffectData(StatusEffect Type, int Duration, int Power, float ExtraData = 0f)
    {
        /// <summary>
        /// Returns a copy of this status effect with its duration reduced by 1.
        /// </summary>
        /// <returns>A new StatusEffectData instance with the updated duration.</returns>
        public StatusEffectData TickDuration()
        {
            return this with { Duration = Duration - 1 };
        }

        /// <summary>
        /// Indicates whether this status effect should be removed.
        /// </summary>
        public bool ShouldRemove => Duration <= 0;
    }
}
