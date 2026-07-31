using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;

namespace AlJourneyTests.Scripts.Data
{
    public class EquipmentDataTests
    {
        private static EquipmentData CreateItem(
            EquipmentRarity rarity = EquipmentRarity.Common,
            int currentLevel = 1,
            int maxLevel = 10,
            Dictionary<string, int>? baseStats = null)
        {
            return new EquipmentData(
                "test_item",
                "Test Item",
                "TEST_DESC",
                EquipmentSlot.Weapon,
                rarity,
                currentLevel,
                maxLevel,
                baseStats ?? new Dictionary<string, int> { ["damage"] = 5 },
                []);
        }

        [Fact]
        public void Constructor_StoresAllSuppliedValues()
        {
            Dictionary<string, int> stats = new() { ["damage"] = 5 };
            Dictionary<string, string> specials = new() { ["burn"] = "10%" };
            EquipmentData item = new("id", "Name", "DESC", EquipmentSlot.Ring, EquipmentRarity.Rare, 2, 15, stats, specials);

            Assert.Equal("id", item.Id);
            Assert.Equal("Name", item.Name);
            Assert.Equal("DESC", item.DescriptionKey);
            Assert.Equal(EquipmentSlot.Ring, item.Slot);
            Assert.Equal(EquipmentRarity.Rare, item.Rarity);
            Assert.Equal(2, item.CurrentLevel);
            Assert.Equal(15, item.MaxLevel);
            Assert.Same(stats, item.BaseStats);
            Assert.Same(specials, item.SpecialAbilities);
        }

        [Fact]
        public void GetRarityColor_KnownRarities_ReturnMatchingNamedColors()
        {
            Assert.Equal(Colors.Gray, CreateItem(rarity: EquipmentRarity.Common).GetRarityColor());
            Assert.Equal(Colors.Green, CreateItem(rarity: EquipmentRarity.Uncommon).GetRarityColor());
            Assert.Equal(Colors.Blue, CreateItem(rarity: EquipmentRarity.Rare).GetRarityColor());
            Assert.Equal(Colors.Purple, CreateItem(rarity: EquipmentRarity.Epic).GetRarityColor());
            Assert.Equal(Colors.Orange, CreateItem(rarity: EquipmentRarity.Legendary).GetRarityColor());
        }

        [Fact]
        public void GetRarityColor_UnknownRarity_ReturnsWhite()
        {
            EquipmentData item = CreateItem(rarity: (EquipmentRarity)999);

            Assert.Equal(Colors.White, item.GetRarityColor());
        }

        [Theory]
        [InlineData(EquipmentRarity.Common, 40f)]
        [InlineData(EquipmentRarity.Uncommon, 30f)]
        [InlineData(EquipmentRarity.Rare, 15f)]
        [InlineData(EquipmentRarity.Epic, 10f)]
        [InlineData(EquipmentRarity.Legendary, 5f)]
        public void GetDropChance_KnownRarity_ReturnsExpectedChance(EquipmentRarity rarity, float expected)
        {
            EquipmentData item = CreateItem(rarity: rarity);

            Assert.Equal(expected, item.GetDropChance());
        }

        [Fact]
        public void GetDropChance_UnknownRarity_ReturnsZero()
        {
            EquipmentData item = CreateItem(rarity: (EquipmentRarity)999);

            Assert.Equal(0f, item.GetDropChance());
        }

        [Fact]
        public void GetUpgradeCost_AtMaxLevel_ReturnsZeroRegardlessOfWave()
        {
            EquipmentData item = CreateItem(currentLevel: 10, maxLevel: 10);

            Assert.Equal(0, item.GetUpgradeCost());
            Assert.Equal(0, item.GetUpgradeCost(waveNumber: 5));
        }

        [Theory]
        [InlineData(EquipmentRarity.Common, 50)]
        [InlineData(EquipmentRarity.Uncommon, 100)]
        [InlineData(EquipmentRarity.Rare, 200)]
        [InlineData(EquipmentRarity.Epic, 400)]
        [InlineData(EquipmentRarity.Legendary, 800)]
        public void GetUpgradeCost_BelowMaxLevel_NoWave_ReturnsBaseCostTimesCurrentLevel(EquipmentRarity rarity, int baseCostForRarity)
        {
            EquipmentData item = CreateItem(rarity: rarity, currentLevel: 3, maxLevel: 10);

            Assert.Equal(baseCostForRarity * 3, item.GetUpgradeCost());
        }

        [Fact]
        public void GetUpgradeCost_UnknownRarity_UsesCommonBaseCost()
        {
            EquipmentData item = CreateItem(rarity: (EquipmentRarity)999, currentLevel: 2, maxLevel: 10);

            Assert.Equal(50 * 2, item.GetUpgradeCost());
        }

        [Fact]
        public void GetUpgradeCost_WithWaveNumber_ScalesThroughScalingSystem()
        {
            EquipmentData item = CreateItem(rarity: EquipmentRarity.Rare, currentLevel: 3, maxLevel: 10);
            const int levelCost = 200 * 3;

            int result = item.GetUpgradeCost(waveNumber: 7);

            Assert.Equal(ScalingSystem.ScaleCost(levelCost, 7), result);
        }

        [Fact]
        public void Upgrade_AtMaxLevel_ReturnsSameInstance()
        {
            EquipmentData item = CreateItem(currentLevel: 10, maxLevel: 10);

            EquipmentData result = item.Upgrade();

            Assert.Same(item, result);
        }

        [Fact]
        public void Upgrade_BelowMaxLevel_IncrementsLevelAndAllBaseStats()
        {
            EquipmentData item = CreateItem(
                currentLevel: 1,
                maxLevel: 10,
                baseStats: new Dictionary<string, int> { ["damage"] = 5, ["burn_damage"] = 2 });

            EquipmentData result = item.Upgrade();

            Assert.NotSame(item, result);
            Assert.Equal(2, result.CurrentLevel);
            Assert.Equal(6, result.BaseStats["damage"]);
            Assert.Equal(3, result.BaseStats["burn_damage"]);

            // The original instance must remain unchanged (record immutability).
            Assert.Equal(1, item.CurrentLevel);
            Assert.Equal(5, item.BaseStats["damage"]);
        }

        [Fact]
        public void GetTotalStats_AtLevelOne_ReturnsBaseStatsUnchanged()
        {
            EquipmentData item = CreateItem(currentLevel: 1, baseStats: new Dictionary<string, int> { ["damage"] = 5 });

            Dictionary<string, int> totals = item.GetTotalStats();

            Assert.Equal(5, totals["damage"]);
        }

        [Fact]
        public void GetTotalStats_AboveLevelOne_AddsLevelBonusToEachStat()
        {
            EquipmentData item = CreateItem(
                currentLevel: 3,
                baseStats: new Dictionary<string, int> { ["damage"] = 5, ["defense"] = 2 });

            Dictionary<string, int> totals = item.GetTotalStats();

            Assert.Equal(7, totals["damage"]);
            Assert.Equal(4, totals["defense"]);
        }

        [Fact]
        public void ToString_FormatsNameRarityAndLevel()
        {
            EquipmentData item = CreateItem(rarity: EquipmentRarity.Epic, currentLevel: 4, maxLevel: 20) with { Name = "Dragon Scales" };

            Assert.Equal("Dragon Scales (Epic) - Level 4/20", item.ToString());
        }
    }
}
