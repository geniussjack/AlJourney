using AlJourney.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Battle.Rules
{
    /// <summary>
    /// Чистые правила выбора целей для способностей: какие цели допустимы для наведения
    /// и на кого фактически распространяется эффект после подтверждения цели (с учётом AoE).
    /// Не зависит от Godot.Node — рассчитан на переиспользование с любым типом цели
    /// (в игре это <c>Character</c>/<c>PlayerCharacter</c>/<c>Enemy</c>) и покрывается модульными тестами.
    /// </summary>
    public static class AbilityTargetingRules
    {
        /// <summary>
        /// Возвращает список целей, на которые в принципе можно навести способность данного типа наведения.
        /// Атакующие способности целятся во врагов, защитные/поддерживающие — в союзников (включая себя).
        /// Мёртвые персонажи никогда не являются допустимой целью.
        /// </summary>
        /// <typeparam name="T">Тип цели (например, игровой персонаж).</typeparam>
        /// <param name="targetType">Тип наведения способности.</param>
        /// <param name="allies">Все союзники, включая самого применяющего способность.</param>
        /// <param name="enemies">Все враги на поле боя.</param>
        /// <param name="isAlive">Предикат, определяющий, жива ли цель.</param>
        /// <returns>Список допустимых целей для наведения.</returns>
        public static IReadOnlyList<T> GetValidTargets<T>(
            AbilityTargetType targetType,
            IReadOnlyList<T> allies,
            IReadOnlyList<T> enemies,
            Func<T, bool> isAlive) where T : class
        {
            ArgumentNullException.ThrowIfNull(allies);
            ArgumentNullException.ThrowIfNull(enemies);
            ArgumentNullException.ThrowIfNull(isAlive);

            IReadOnlyList<T> pool = targetType == AbilityTargetType.Enemy ? enemies : allies;
            return [.. pool.Where(isAlive)];
        }

        /// <summary>
        /// Возвращает итоговый список целей, на которые распространяется эффект способности после
        /// того, как игрок навёлся на конкретную цель. Для одиночных способностей — это сама выбранная
        /// цель (если она всё ещё допустима). Для AoE-способностей эффект распространяется на весь пул
        /// целей соответствующего типа наведения (всех живых врагов либо весь живой отряд).
        /// </summary>
        /// <typeparam name="T">Тип цели.</typeparam>
        /// <param name="targetType">Тип наведения способности.</param>
        /// <param name="isAoE">Является ли способность площадной.</param>
        /// <param name="chosenTarget">Цель, выбранная игроком (может быть null, если наведение ещё не подтверждено).</param>
        /// <param name="allies">Все союзники, включая самого применяющего способность.</param>
        /// <param name="enemies">Все враги на поле боя.</param>
        /// <param name="isAlive">Предикат, определяющий, жива ли цель.</param>
        /// <returns>Список целей, на которые фактически будет применён эффект.</returns>
        public static IReadOnlyList<T> ResolveEffectTargets<T>(
            AbilityTargetType targetType,
            bool isAoE,
            T chosenTarget,
            IReadOnlyList<T> allies,
            IReadOnlyList<T> enemies,
            Func<T, bool> isAlive) where T : class
        {
            return !isAoE
                ? chosenTarget is not null && isAlive(chosenTarget) ? [chosenTarget] : []
                : GetValidTargets(targetType, allies, enemies, isAlive);
        }

        /// <summary>
        /// Автоматически выбирает живую цель с наибольшим текущим здоровьем из списка кандидатов.
        /// Используется для одиночных ультимативных способностей, у которых игрок не наводится
        /// на цель вручную (например, «удар по врагу с наибольшим HP»).
        /// </summary>
        /// <typeparam name="T">Тип цели.</typeparam>
        /// <param name="candidates">Кандидаты на роль цели.</param>
        /// <param name="currentHealth">Функция, возвращающая текущее здоровье цели.</param>
        /// <param name="isAlive">Предикат, определяющий, жива ли цель.</param>
        /// <returns>Живая цель с наибольшим текущим здоровьем, или <c>null</c>, если живых кандидатов нет.</returns>
        public static T SelectHighestHealthTarget<T>(
            IReadOnlyList<T> candidates,
            Func<T, int> currentHealth,
            Func<T, bool> isAlive) where T : class
        {
            ArgumentNullException.ThrowIfNull(candidates);
            ArgumentNullException.ThrowIfNull(currentHealth);
            ArgumentNullException.ThrowIfNull(isAlive);

            return candidates.Where(isAlive).OrderByDescending(currentHealth).FirstOrDefault();
        }
    }
}
