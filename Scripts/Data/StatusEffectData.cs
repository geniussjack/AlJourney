using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Структура, описывающая статусный эффект, наложенный на персонажа или врага (например, горение или оглушение).
    /// </summary>
    public record StatusEffectData(StatusEffect Type, int Duration, int Power, float ExtraData = 0f)
    {
        /// <summary>
        /// Возвращает копию текущего статусного эффекта с уменьшенной на 1 длительностью (применяется каждый ход).
        /// </summary>
        /// <returns>Новый экземпляр StatusEffectData с обновленной длительностью.</returns>
        public StatusEffectData TickDuration()
        {
            return this with { Duration = Duration - 1 };
        }

        /// <summary>
        /// Указывает, должен ли данный статусный эффект быть удален (если его длительность истекла).
        /// </summary>
        public bool ShouldRemove => Duration <= 0;
    }
}
