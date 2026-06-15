using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;
using System.Collections.Generic;

namespace AlJourney.Scripts.Tests
{
    public class LootSystemTest : TestClass
    {
        private LootSystem _lootSystem;

        public LootSystemTest(Node testScene) : base(testScene) { }

        [SetupAll]
        public void SetupAll()
        {
            _lootSystem = new LootSystem();
            _lootSystem._Ready(); // Initialize the singleton
        }

        [CleanupAll]
        public void CleanupAll()
        {
            _lootSystem.QueueFree();
        }

        [Test]
        public void GenerateBossLootReturnsMultipleItems()
        {
            List<EquipmentData> loot = _lootSystem.GenerateBossLoot(5);

            _ = loot.ShouldNotBeNull();
            loot.Count.ShouldBeGreaterThanOrEqualTo(3);
            loot.Count.ShouldBeLessThanOrEqualTo(11);
        }

        [Test]
        public void GenerateNormalLootReturnsOneItem()
        {
            EquipmentData item = _lootSystem.GenerateNormalLoot(2);

            _ = item.ShouldNotBeNull();
            item.Rarity.ShouldNotBe(EquipmentRarity.Legendary, "Normal enemies should very rarely or never drop legendary directly like this without scaling down");
        }
    }
}
