using Godot;

namespace AlJourney.Scripts.Core
{
    /// <summary>
    /// Статический класс для расчета масштабирования характеристик врагов, наград и стоимости предметов в зависимости от текущей волны.
    /// Обеспечивает постепенное повышение сложности и ценности наград по мере прохождения игры.
    /// </summary>
    public static class ScalingSystem
    {
        private const float ENEMY_STAT_COEFFICIENT = 0.10f;
        private const float REWARD_COEFFICIENT = 0.1f;
        private const float COST_COEFFICIENT = 0.05f;

        /// <summary>
        /// Вычисляет масштабированное значение характеристики врага для указанной волны.
        /// </summary>
        /// <param name="baseStat">Базовое значение характеристики на первой волне.</param>
        /// <param name="waveNumber">Номер текущей волны.</param>
        /// <returns>Рассчитанное значение характеристики с учетом множителя волны.</returns>
        public static int ScaleEnemyStat(int baseStat, int waveNumber)
        {
            return Mathf.CeilToInt(baseStat * (1 + (waveNumber * ENEMY_STAT_COEFFICIENT)));
        }

        /// <summary>
        /// Вычисляет увеличенный размер награды, получаемой игроком на указанной волне.
        /// </summary>
        /// <param name="baseReward">Базовый размер награды.</param>
        /// <param name="waveNumber">Номер текущей волны.</param>
        /// <returns>Рассчитанное значение награды с учетом множителя.</returns>
        public static int ScaleReward(int baseReward, int waveNumber)
        {
            return Mathf.CeilToInt(baseReward * (1 + (waveNumber * REWARD_COEFFICIENT)));
        }

        /// <summary>
        /// Вычисляет масштабированную стоимость предметов в магазине или улучшений в зависимости от текущей волны.
        /// </summary>
        /// <param name="baseCost">Базовая стоимость предмета или улучшения.</param>
        /// <param name="waveNumber">Номер текущей волны.</param>
        /// <returns>Рассчитанная стоимость с учетом множителя цены.</returns>
        public static int ScaleCost(int baseCost, int waveNumber)
        {
            return Mathf.CeilToInt(baseCost * (1 + (waveNumber * COST_COEFFICIENT)));
        }

        /// <summary>
        /// Определяет количество врагов, которые должны появиться на указанной волне.
        /// Количество врагов постепенно увеличивается, но не превышает установленный максимум.
        /// </summary>
        /// <param name="waveNumber">Номер текущей волны.</param>
        /// <returns>Количество врагов для генерации.</returns>
        public static int GetEnemyCount(int waveNumber)
        {
            int count = GameConstants.ENEMY_COUNT_BASE
                + ((waveNumber - 1) / GameConstants.ENEMY_COUNT_INCREASE_EVERY);
            return Mathf.Min(count, GameConstants.MAX_ENEMIES_PER_WAVE);
        }

        /// <summary>
        /// Проверяет, достигнута ли волна, на которой начинают появляться враги типа Скелет.
        /// </summary>
        /// <param name="waveNumber">Номер текущей волны.</param>
        /// <returns>True, если текущая волна больше или равна волне разблокировки скелетов.</returns>
        public static bool IsSkeletonUnlocked(int waveNumber)
        {
            return waveNumber >= GameConstants.SKELETON_UNLOCK_WAVE;
        }
    }
}
