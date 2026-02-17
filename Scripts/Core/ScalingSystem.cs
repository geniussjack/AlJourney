using Godot;

namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Centralized system for dynamic scaling of game values based on wave progression.
    /// </summary>
    public static class ScalingSystem
    {
        // Scaling coefficients
        private const float ENEMY_STAT_COEFFICIENT = 0.15f;
        private const float REWARD_COEFFICIENT = 0.1f;
        private const float COST_COEFFICIENT = 0.05f;

        /// <summary>
        /// Calculates scaled enemy stat based on wave number.
        /// Formula: baseStat * (1 + wave * 0.15)
        /// </summary>
        /// <param name="baseStat">Base stat value</param>
        /// <param name="waveNumber">Current wave number</param>
        /// <returns>Scaled stat value rounded up</returns>
        public static int ScaleEnemyStat(int baseStat, int waveNumber)
        {
            return Mathf.CeilToInt(baseStat * (1 + waveNumber * ENEMY_STAT_COEFFICIENT));
        }

        /// <summary>
        /// Calculates scaled coin reward based on wave number.
        /// Formula: baseReward * (1 + wave * 0.1)
        /// </summary>
        /// <param name="baseReward">Base reward value</param>
        /// <param name="waveNumber">Current wave number</param>
        /// <returns>Scaled reward value rounded up</returns>
        public static int ScaleReward(int baseReward, int waveNumber)
        {
            return Mathf.CeilToInt(baseReward * (1 + waveNumber * REWARD_COEFFICIENT));
        }

        /// <summary>
        /// Calculates scaled upgrade cost based on wave number.
        /// Formula: baseCost * (1 + wave * 0.05)
        /// </summary>
        /// <param name="baseCost">Base cost value</param>
        /// <param name="waveNumber">Current wave number</param>
        /// <returns>Scaled cost value rounded up</returns>
        public static int ScaleCost(int baseCost, int waveNumber)
        {
            return Mathf.CeilToInt(baseCost * (1 + waveNumber * COST_COEFFICIENT));
        }
    }
}
