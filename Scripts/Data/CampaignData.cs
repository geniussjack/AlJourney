using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Describes a single spawn within a wave: the enemy type and stack size (see <see cref="Characters.Enemy.Create"/>).
    /// </summary>
    /// <param name="Type">The enemy type.</param>
    /// <param name="Count">The number of creatures in the stack.</param>
    public record EnemySpawnDefinition(EnemyType Type, int Count = 1);

    /// <summary>
    /// Describes a single wave pass within a level — a set of spawns that appear at the same time.
    /// </summary>
    /// <param name="Enemies">The list of spawns for this wave.</param>
    public record WaveDefinition(IReadOnlyList<EnemySpawnDefinition> Enemies);

    /// <summary>
    /// Describes a single campaign map level: its location, position within it, unlock requirement, and
    /// the curated (predetermined) sequence of waves that play out back-to-back without leaving combat
    /// within a single attempt at the level.
    /// </summary>
    /// <param name="Id">The level's unique identifier.</param>
    /// <param name="Location">The location this level belongs to.</param>
    /// <param name="OrderInLocation">The level's order within its location (for display on the map).</param>
    /// <param name="Waves">The level's curated wave sequence.</param>
    /// <param name="DifficultyRating">
    /// The level's numeric difficulty, used in place of a wave number as the input for
    /// <see cref="Core.ScalingSystem"/> (scaling of enemy stats, rewards and shop prices).
    /// </param>
    /// <param name="IsBranch">
    /// True if the level is an optional branch off the main line (a source of resources and, in the
    /// future, rarity catalysts) rather than part of the mandatory linear chain to the necromancer.
    /// </param>
    /// <param name="RequiredLevelId">
    /// The Id of the level that must be completed to unlock this one, or <c>null</c> for the very first
    /// level of the campaign.
    /// </param>
    public record LevelDefinition(
        string Id,
        LocationId Location,
        int OrderInLocation,
        IReadOnlyList<WaveDefinition> Waves,
        int DifficultyRating,
        bool IsBranch = false,
        string RequiredLevelId = null
    );
}
