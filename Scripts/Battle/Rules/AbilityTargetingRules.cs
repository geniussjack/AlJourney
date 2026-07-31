#nullable enable
using AlJourney.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Battle.Rules
{
    /// <summary>
    /// Pure target-selection rules for abilities: which targets are valid to aim at, and who the
    /// effect actually applies to once a target is confirmed (accounting for AoE).
    /// Has no dependency on Godot.Node — designed for reuse with any target type
    /// (in-game this is <c>Character</c>/<c>PlayerCharacter</c>/<c>Enemy</c>) and is covered by unit tests.
    /// </summary>
    public static class AbilityTargetingRules
    {
        /// <summary>
        /// Returns the list of targets that can, in principle, be aimed at with an ability of the given
        /// targeting type. Attack abilities target enemies; defensive/support abilities target allies
        /// (including the caster). Dead characters are never a valid target.
        /// </summary>
        /// <typeparam name="T">The target type (e.g. a game character).</typeparam>
        /// <param name="targetType">The ability's targeting type.</param>
        /// <param name="allies">Every ally, including the caster themselves.</param>
        /// <param name="enemies">Every enemy on the battlefield.</param>
        /// <param name="isAlive">A predicate that determines whether a target is alive.</param>
        /// <returns>The list of valid targets to aim at.</returns>
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
        /// Returns the final list of targets the ability's effect applies to once the player has aimed
        /// at a specific target. For single-target abilities, this is the chosen target itself (if it's
        /// still valid). For AoE abilities, the effect spreads to the entire target pool for the
        /// matching targeting type (every living enemy, or the whole living party).
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="targetType">The ability's targeting type.</param>
        /// <param name="isAoE">Whether the ability is area-of-effect.</param>
        /// <param name="chosenTarget">The target chosen by the player (may be null if not yet confirmed).</param>
        /// <param name="allies">Every ally, including the caster themselves.</param>
        /// <param name="enemies">Every enemy on the battlefield.</param>
        /// <param name="isAlive">A predicate that determines whether a target is alive.</param>
        /// <returns>The list of targets the effect will actually be applied to.</returns>
        public static IReadOnlyList<T> ResolveEffectTargets<T>(
            AbilityTargetType targetType,
            bool isAoE,
            T? chosenTarget,
            IReadOnlyList<T> allies,
            IReadOnlyList<T> enemies,
            Func<T, bool> isAlive) where T : class
        {
            return !isAoE
                ? chosenTarget is not null && isAlive(chosenTarget) ? [chosenTarget] : []
                : GetValidTargets(targetType, allies, enemies, isAlive);
        }

        /// <summary>
        /// Automatically selects the living target with the highest current health from the candidate list.
        /// Used for single-target ultimate abilities the player doesn't manually aim, e.g. "strike the
        /// enemy with the highest HP".
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="candidates">The candidate targets.</param>
        /// <param name="currentHealth">A function returning a target's current health.</param>
        /// <param name="isAlive">A predicate that determines whether a target is alive.</param>
        /// <returns>The living target with the highest current health, or <c>null</c> if there are no living candidates.</returns>
        public static T? SelectHighestHealthTarget<T>(
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
