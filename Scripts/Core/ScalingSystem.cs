using Godot;

namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Система масштабирования характеристик, наград и стоимости.
    /// </summary>
    /// <summary>
    /// Менеджер ScalingSystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public static class ScalingSystem
    {
        private const float ENEMY_STAT_COEFFICIENT = 0.10f;
        private const float REWARD_COEFFICIENT = 0.1f;
        private const float COST_COEFFICIENT = 0.05f;

        /// <summary>
        /// Масштабирует характеристику врага в зависимости от номера волны.
        /// </summary>
        /// <summary>
        /// Элемент ScaleEnemyStat.
        /// </summary>
        public static int ScaleEnemyStat(int baseStat, int waveNumber)
        {
            return Mathf.CeilToInt(baseStat * (1 + waveNumber * ENEMY_STAT_COEFFICIENT));
        }

        /// <summary>
        /// Масштабирует награду в зависимости от номера волны.
        /// </summary>
        /// <summary>
        /// Элемент ScaleReward.
        /// </summary>
        public static int ScaleReward(int baseReward, int waveNumber)
        {
            return Mathf.CeilToInt(baseReward * (1 + waveNumber * REWARD_COEFFICIENT));
        }

        /// <summary>
        /// Масштабирует стоимость в зависимости от номера волны.
        /// </summary>
        /// <summary>
        /// Элемент ScaleCost.
        /// </summary>
        public static int ScaleCost(int baseCost, int waveNumber)
        {
            return Mathf.CeilToInt(baseCost * (1 + waveNumber * COST_COEFFICIENT));
        }

        /// <summary>
        /// Возвращает количество врагов для указанной волны.
        /// </summary>
        /// <summary>
        /// Возвращает EnemyCount.
        /// </summary>
        public static int GetEnemyCount(int waveNumber)
        {
            int count = GameConstants.ENEMY_COUNT_BASE
                + (waveNumber - 1) / GameConstants.ENEMY_COUNT_INCREASE_EVERY;
            return Mathf.Min(count, GameConstants.MAX_ENEMIES_PER_WAVE);
        }

        /// <summary>
        /// Проверяет, разблокированы ли враги-скелеты на текущей волне.
        /// </summary>
        /// <summary>
        /// Проверяет, является ли SkeletonUnlocked.
        /// </summary>
        public static bool IsSkeletonUnlocked(int waveNumber)
        {
            return waveNumber >= GameConstants.SKELETON_UNLOCK_WAVE;
        }
    }
}
