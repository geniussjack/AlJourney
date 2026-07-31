using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourneyTests.Scripts.Data
{
    public class CampaignDatabaseTests
    {
        [Fact]
        public void Levels_AllHaveUniqueIds()
        {
            List<string> ids = [.. CampaignDatabase.Levels.Select(level => level.Id)];

            Assert.Equal(ids.Count, ids.Distinct().Count());
        }

        [Fact]
        public void Levels_AllHaveAtLeastOneWaveWithAtLeastOneEnemy()
        {
            foreach (LevelDefinition level in CampaignDatabase.Levels)
            {
                Assert.NotEmpty(level.Waves);
                foreach (WaveDefinition wave in level.Waves)
                {
                    Assert.NotEmpty(wave.Enemies);
                    Assert.All(wave.Enemies, spawn => Assert.True(spawn.Count >= 1));
                }
            }
        }

        [Fact]
        public void Levels_EveryRequiredLevelId_ReferencesAnExistingLevel()
        {
            HashSet<string> allIds = [.. CampaignDatabase.Levels.Select(level => level.Id)];

            foreach (LevelDefinition level in CampaignDatabase.Levels)
            {
                if (level.RequiredLevelId is not null)
                {
                    Assert.Contains(level.RequiredLevelId, allIds);
                }
            }
        }

        [Fact]
        public void FirstLevelId_ExistsAndHasNoPrerequisite()
        {
            LevelDefinition first = CampaignDatabase.GetLevel(CampaignDatabase.FirstLevelId);

            Assert.NotNull(first);
            Assert.Null(first.RequiredLevelId);
            Assert.False(first.IsBranch);
        }

        [Fact]
        public void MainLevels_OrderInLocation_IsContiguousStartingAtOneWithinEachLocation()
        {
            foreach (IGrouping<LocationId, LevelDefinition> group in CampaignDatabase.Levels
                .Where(level => !level.IsBranch)
                .GroupBy(level => level.Location))
            {
                List<int> orders = [.. group.Select(level => level.OrderInLocation).Order()];
                List<int> expected = [.. Enumerable.Range(1, orders.Count)];

                Assert.Equal(expected, orders);
            }
        }

        [Fact]
        public void EveryLocation_HasAtLeastOneMainLevel()
        {
            foreach (LocationId location in System.Enum.GetValues<LocationId>())
            {
                Assert.Contains(CampaignDatabase.Levels, level => level.Location == location && !level.IsBranch);
            }
        }

        [Fact]
        public void GetLevel_UnknownId_ReturnsNull()
        {
            Assert.Null(CampaignDatabase.GetLevel("not_a_real_level"));
        }

        [Fact]
        public void GetLevel_KnownId_ReturnsMatchingLevel()
        {
            LevelDefinition level = CampaignDatabase.GetLevel(CampaignDatabase.FirstLevelId);

            Assert.NotNull(level);
            Assert.Equal(CampaignDatabase.FirstLevelId, level.Id);
        }

        [Fact]
        public void GetNextMainLevel_WithinSameLocation_ReturnsNextOrder()
        {
            LevelDefinition next = CampaignDatabase.GetNextMainLevel("village_ruins_1");

            Assert.NotNull(next);
            Assert.Equal("village_ruins_2", next.Id);
        }

        [Fact]
        public void GetNextMainLevel_AtEndOfLocation_ReturnsFirstLevelOfNextLocation()
        {
            LevelDefinition next = CampaignDatabase.GetNextMainLevel("village_ruins_4");

            Assert.NotNull(next);
            Assert.Equal(LocationId.DarkForest, next.Location);
            Assert.Equal(1, next.OrderInLocation);
        }

        [Fact]
        public void GetNextMainLevel_AfterFinalCampaignLevel_ReturnsNull()
        {
            LevelDefinition finalLevel = CampaignDatabase.Levels
                .Where(level => !level.IsBranch)
                .OrderByDescending(level => level.DifficultyRating)
                .First();

            Assert.Null(CampaignDatabase.GetNextMainLevel(finalLevel.Id));
        }

        [Fact]
        public void GetNextMainLevel_ForBranchLevel_ReturnsNull()
        {
            LevelDefinition branch = CampaignDatabase.Levels.First(level => level.IsBranch);

            Assert.Null(CampaignDatabase.GetNextMainLevel(branch.Id));
        }

        [Fact]
        public void GetNextMainLevel_ForUnknownId_ReturnsNull()
        {
            Assert.Null(CampaignDatabase.GetNextMainLevel("not_a_real_level"));
        }

        [Fact]
        public void BranchLevels_HaveAMiniboss()
        {
            foreach (LevelDefinition branch in CampaignDatabase.Levels.Where(level => level.IsBranch))
            {
                bool hasMiniboss = branch.Waves
                    .SelectMany(wave => wave.Enemies)
                    .Any(spawn => spawn.Type is EnemyType.GeneralOfDraugr or EnemyType.Arhiskeleton);

                Assert.True(hasMiniboss, $"Branch level '{branch.Id}' is expected to guard a miniboss.");
            }
        }

        [Fact]
        public void GetLocationNameKey_ReturnsADistinctKeyForEachLocation()
        {
            List<string> keys = [.. System.Enum.GetValues<LocationId>().Select(CampaignDatabase.GetLocationNameKey)];

            Assert.Equal(keys.Count, keys.Distinct().Count());
            Assert.All(keys, key => Assert.StartsWith("LOCATION_", key));
        }
    }
}
