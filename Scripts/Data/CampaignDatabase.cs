using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Static campaign map database: locations, the main linear level chain from the village ruins to
    /// the necromancer's lair, and branches with minibosses.
    /// This is a working, easily adjustable level set — the exact wave composition will be tuned
    /// separately during balancing (see REDESIGN_NOTES.md, Stage 3).
    /// </summary>
    public static class CampaignDatabase
    {
        /// <summary>
        /// The first campaign level, available with no unlock conditions.
        /// </summary>
        public const string FirstLevelId = "village_ruins_1";

        /// <summary>
        /// Every campaign level in declaration order (main line and branches interleaved), in the
        /// order they are declared below in <see cref="BuildLevels"/>.
        /// </summary>
        public static readonly IReadOnlyList<LevelDefinition> Levels = BuildLevels();

        private static List<LevelDefinition> BuildLevels()
        {
            List<LevelDefinition> levels = [];

            // --- Location 1: Village Ruins ---
            // Starting location. Simplest enemies: slimes and zombies.
            AddMainLevel(levels, LocationId.VillageRuins, 1, difficulty: 1, requiredLevelId: null,
                Wave(Spawn(EnemyType.Slime, 2)));
            AddMainLevel(levels, LocationId.VillageRuins, 2, difficulty: 2,
                Wave(Spawn(EnemyType.Slime, 3)));
            AddMainLevel(levels, LocationId.VillageRuins, 3, difficulty: 3,
                Wave(Spawn(EnemyType.Zombie, 1), Spawn(EnemyType.Slime, 2)));
            AddMainLevel(levels, LocationId.VillageRuins, 4, difficulty: 4,
                Wave(Spawn(EnemyType.Zombie, 2)),
                Wave(Spawn(EnemyType.Zombie, 1), Spawn(EnemyType.Slime, 2)));

            // --- Location 2: Dark Forest ---
            // Skeletons (warrior and archer) are introduced; zombies remain a filler enemy.
            AddMainLevel(levels, LocationId.DarkForest, 1, difficulty: 5, requiredLevelId: LevelId(LocationId.VillageRuins, 4),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2)));
            AddMainLevel(levels, LocationId.DarkForest, 2, difficulty: 6,
                Wave(Spawn(EnemyType.SkeletonWarrior, 1), Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.DarkForest, 3, difficulty: 7,
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.Zombie, 1)),
                Wave(Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.DarkForest, 4, difficulty: 8,
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.SkeletonArcher, 2)),
                Wave(Spawn(EnemyType.Zombie, 2)));
            // Branch: the first miniboss, the General of Draugr.
            AddBranchLevel(levels, LocationId.DarkForest, "dark_forest_branch_1", difficulty: 7,
                requiredLevelId: LevelId(LocationId.DarkForest, 1),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2)),
                Wave(Spawn(EnemyType.GeneralOfDraugr)));

            // --- Location 3: Buried Catacombs ---
            // The Draugr trio is introduced; skeletons remain a filler enemy.
            AddMainLevel(levels, LocationId.BuriedCatacombs, 1, difficulty: 9, requiredLevelId: LevelId(LocationId.DarkForest, 4),
                Wave(Spawn(EnemyType.DraugrWarrior, 2)));
            AddMainLevel(levels, LocationId.BuriedCatacombs, 2, difficulty: 10,
                Wave(Spawn(EnemyType.DraugrDefender, 1), Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.BuriedCatacombs, 3, difficulty: 11,
                Wave(Spawn(EnemyType.DraugrCaster, 2), Spawn(EnemyType.SkeletonWarrior, 1)),
                Wave(Spawn(EnemyType.DraugrWarrior, 2)));
            AddMainLevel(levels, LocationId.BuriedCatacombs, 4, difficulty: 12,
                Wave(Spawn(EnemyType.DraugrWarrior, 1), Spawn(EnemyType.DraugrDefender, 1), Spawn(EnemyType.DraugrCaster, 1)),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrCaster, 1)));
            // Branch: the second miniboss, the Archskeleton.
            AddBranchLevel(levels, LocationId.BuriedCatacombs, "buried_catacombs_branch_1", difficulty: 11,
                requiredLevelId: LevelId(LocationId.BuriedCatacombs, 1),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.SkeletonArcher, 1)),
                Wave(Spawn(EnemyType.Arhiskeleton)));

            // --- Location 4: Frozen Wastes ---
            // The heaviest "regular" mixed waves before the necromancer's lair.
            AddMainLevel(levels, LocationId.FrozenWastes, 1, difficulty: 13, requiredLevelId: LevelId(LocationId.BuriedCatacombs, 4),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.SkeletonArcher, 1)));
            AddMainLevel(levels, LocationId.FrozenWastes, 2, difficulty: 14,
                Wave(Spawn(EnemyType.DraugrDefender, 2), Spawn(EnemyType.DraugrCaster, 1)));
            AddMainLevel(levels, LocationId.FrozenWastes, 3, difficulty: 15,
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrCaster, 2)),
                Wave(Spawn(EnemyType.SkeletonWarrior, 2), Spawn(EnemyType.SkeletonArcher, 2)));
            AddMainLevel(levels, LocationId.FrozenWastes, 4, difficulty: 16,
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrDefender, 1)),
                Wave(Spawn(EnemyType.DraugrCaster, 2), Spawn(EnemyType.SkeletonArcher, 1)));
            // Branch: a third encounter with the miniboss (General of Draugr) with reinforced guards.
            AddBranchLevel(levels, LocationId.FrozenWastes, "frozen_wastes_branch_1", difficulty: 15,
                requiredLevelId: LevelId(LocationId.FrozenWastes, 1),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrDefender, 1)),
                Wave(Spawn(EnemyType.GeneralOfDraugr)));

            // --- Location 5: Necromancer's Lair ---
            // Final heavy mixed waves and the fight against the main boss.
            AddMainLevel(levels, LocationId.NecromancerLair, 1, difficulty: 17, requiredLevelId: LevelId(LocationId.FrozenWastes, 4),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrCaster, 2)));
            AddMainLevel(levels, LocationId.NecromancerLair, 2, difficulty: 18,
                Wave(Spawn(EnemyType.Arhiskeleton)),
                Wave(Spawn(EnemyType.DraugrWarrior, 2), Spawn(EnemyType.DraugrDefender, 1)));
            AddMainLevel(levels, LocationId.NecromancerLair, 3, difficulty: 20,
                Wave(Spawn(EnemyType.Necromancer)));

            return levels;
        }

        /// <summary>
        /// Returns the localization key for the location's display name (see Data/Languages/translations.csv).
        /// </summary>
        public static string GetLocationNameKey(LocationId location)
        {
            return location switch
            {
                LocationId.VillageRuins => "LOCATION_VILLAGE_RUINS",
                LocationId.DarkForest => "LOCATION_DARK_FOREST",
                LocationId.BuriedCatacombs => "LOCATION_BURIED_CATACOMBS",
                LocationId.FrozenWastes => "LOCATION_FROZEN_WASTES",
                LocationId.NecromancerLair => "LOCATION_NECROMANCER_LAIR",
                _ => "LOCATION_VILLAGE_RUINS"
            };
        }

        /// <summary>
        /// Returns the campaign level with the given Id, or <c>null</c> if no such level exists.
        /// </summary>
        public static LevelDefinition GetLevel(string levelId)
        {
            foreach (LevelDefinition level in Levels)
            {
                if (level.Id == levelId)
                {
                    return level;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the level that comes next on the main line after the given one, within the same
        /// location, or the first level of the next location if the given level was the last one in its
        /// location. Branches are not part of the main sequence. Returns <c>null</c> after the final
        /// campaign level.
        /// </summary>
        public static LevelDefinition GetNextMainLevel(string completedLevelId)
        {
            LevelDefinition completed = GetLevel(completedLevelId);
            if (completed?.IsBranch != false)
            {
                return null;
            }

            LevelDefinition best = null;
            foreach (LevelDefinition level in Levels)
            {
                if (level.IsBranch || level.Location != completed.Location || level.OrderInLocation <= completed.OrderInLocation)
                {
                    continue;
                }

                if (best is null || level.OrderInLocation < best.OrderInLocation)
                {
                    best = level;
                }
            }

            if (best is not null)
            {
                return best;
            }

            int nextLocation = (int)completed.Location + 1;
            return nextLocation > (int)LocationId.NecromancerLair ? null : GetFirstLevelOfLocation((LocationId)nextLocation);
        }

        private static LevelDefinition GetFirstLevelOfLocation(LocationId location)
        {
            LevelDefinition first = null;
            foreach (LevelDefinition level in Levels)
            {
                if (level.IsBranch || level.Location != location)
                {
                    continue;
                }

                if (first is null || level.OrderInLocation < first.OrderInLocation)
                {
                    first = level;
                }
            }

            return first;
        }

        /// <summary>
        /// Builds the standard main-line level identifier from a location and order number.
        /// </summary>
        private static string LevelId(LocationId location, int orderInLocation)
        {
            return $"{ToSnakeCase(location)}_{orderInLocation}";
        }

        private static void AddMainLevel(List<LevelDefinition> levels, LocationId location, int orderInLocation, int difficulty, params WaveDefinition[] waves)
        {
            AddMainLevel(levels, location, orderInLocation, difficulty, null, waves);
        }

        private static void AddMainLevel(List<LevelDefinition> levels, LocationId location, int orderInLocation, int difficulty, string requiredLevelId, params WaveDefinition[] waves)
        {
            requiredLevelId ??= orderInLocation > 1 ? LevelId(location, orderInLocation - 1) : null;
            levels.Add(new LevelDefinition(LevelId(location, orderInLocation), location, orderInLocation, waves, difficulty, IsBranch: false, requiredLevelId));
        }

        private static void AddBranchLevel(List<LevelDefinition> levels, LocationId location, string id, int difficulty, string requiredLevelId, params WaveDefinition[] waves)
        {
            // Branches use a negative order so they don't participate in computing the next main-line
            // level (see GetNextMainLevel/GetFirstLevelOfLocation), while still staying tied to their
            // location for display purposes on the map.
            levels.Add(new LevelDefinition(id, location, -1, waves, difficulty, IsBranch: true, requiredLevelId));
        }

        private static WaveDefinition Wave(params EnemySpawnDefinition[] enemies)
        {
            return new WaveDefinition(enemies);
        }

        private static EnemySpawnDefinition Spawn(EnemyType type, int count = 1)
        {
            return new EnemySpawnDefinition(type, count);
        }

        private static string ToSnakeCase(LocationId location)
        {
            return location switch
            {
                LocationId.VillageRuins => "village_ruins",
                LocationId.DarkForest => "dark_forest",
                LocationId.BuriedCatacombs => "buried_catacombs",
                LocationId.FrozenWastes => "frozen_wastes",
                LocationId.NecromancerLair => "necromancer_lair",
                _ => location.ToString().ToLowerInvariant()
            };
        }
    }
}
