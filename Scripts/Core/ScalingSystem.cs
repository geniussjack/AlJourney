using Godot;

namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Static class that computes scaling for enemy stats, rewards and item costs based on the current wave.
    /// Provides a gradual increase in difficulty and reward value as the player progresses.
    /// </summary>
    public static class ScalingSystem
    {
        private const float ENEMY_STAT_COEFFICIENT = 0.10f;
        private const float REWARD_COEFFICIENT = 0.1f;
        private const float COST_COEFFICIENT = 0.05f;

        /// <summary>
        /// Computes the scaled value of an enemy stat for the given wave.
        /// </summary>
        /// <param name="baseStat">Base stat value on the first wave.</param>
        /// <param name="waveNumber">Current wave number.</param>
        /// <returns>The computed stat value, adjusted by the wave multiplier.</returns>
        public static int ScaleEnemyStat(int baseStat, int waveNumber)
        {
            return Mathf.CeilToInt(baseStat * (1 + (waveNumber * ENEMY_STAT_COEFFICIENT)));
        }

        /// <summary>
        /// Computes the increased reward the player receives on the given wave.
        /// </summary>
        /// <param name="baseReward">Base reward amount.</param>
        /// <param name="waveNumber">Current wave number.</param>
        /// <returns>The computed reward value, adjusted by the multiplier.</returns>
        public static int ScaleReward(int baseReward, int waveNumber)
        {
            return Mathf.CeilToInt(baseReward * (1 + (waveNumber * REWARD_COEFFICIENT)));
        }

        /// <summary>
        /// Computes the scaled cost of shop items or upgrades based on the current wave.
        /// </summary>
        /// <param name="baseCost">Base cost of the item or upgrade.</param>
        /// <param name="waveNumber">Current wave number.</param>
        /// <returns>The computed cost, adjusted by the price multiplier.</returns>
        public static int ScaleCost(int baseCost, int waveNumber)
        {
            return Mathf.CeilToInt(baseCost * (1 + (waveNumber * COST_COEFFICIENT)));
        }
    }
}
