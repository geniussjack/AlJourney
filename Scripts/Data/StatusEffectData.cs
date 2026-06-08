using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Элемент StatusEffectData.
    /// </summary>
    public record StatusEffectData(StatusEffect Type, int Duration, int Power, float ExtraData = 0f)
    {
        /// <summary>
        /// Элемент TickDuration.
        /// </summary>
        public StatusEffectData TickDuration()
        {
            return this with { Duration = Duration - 1 };
        }

        /// <summary>
        /// Элемент ShouldRemove.
        /// </summary>
        public bool ShouldRemove => Duration <= 0;
    }
}
