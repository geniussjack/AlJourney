using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Battle
{
    /// <summary>
    /// Сервис для спавна врагов. С Этапа 3 (карта кампании) состав волн — курируемый и задаётся
    /// в <see cref="Data.LevelDefinition.Waves"/> (см. <see cref="Data.CampaignDatabase"/>), а не
    /// генерируется формулой по номеру волны — этот класс лишь создаёт запрошенных существ.
    /// </summary>
    public static class EnemySpawner
    {
        /// <summary>
        /// Вспомогательный метод для спавна врага определенного типа.
        /// </summary>
        public static Enemy SpawnEnemy(EnemyType type, int wave, int count = 1)
        {
            return Enemy.Create(type, wave, count);
        }
    }
}
