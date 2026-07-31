using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourneyTests.Scripts.Data
{
    public class EquipmentDatabaseTests
    {
        [Fact]
        public void Templates_ContainsExpectedNumberOfItems()
        {
            Assert.Equal(10, EquipmentDatabase.Templates.Count);
        }

        [Theory]
        [InlineData("fireball")]
        [InlineData("iceball")]
        [InlineData("electroball")]
        [InlineData("sword")]
        [InlineData("axe")]
        [InlineData("spear")]
        [InlineData("leather_armor")]
        [InlineData("dragon_scales")]
        [InlineData("power_ring")]
        [InlineData("life_amulet")]
        public void Templates_EachEntry_IsKeyedByItsOwnId(string id)
        {
            Assert.True(EquipmentDatabase.Templates.TryGetValue(id, out EquipmentData? item));
            Assert.Equal(id, item!.Id);
        }

        [Fact]
        public void Templates_StartingWeapons_AreInTheWeaponSlot()
        {
            string[] weaponIds = ["fireball", "iceball", "electroball", "sword", "axe", "spear"];

            foreach (string id in weaponIds)
            {
                Assert.Equal(EquipmentSlot.Weapon, EquipmentDatabase.Templates[id].Slot);
            }
        }

        [Fact]
        public void Templates_AllEntries_HaveCurrentLevelWithinMaxLevel()
        {
            foreach (EquipmentData item in EquipmentDatabase.Templates.Values)
            {
                Assert.True(item.CurrentLevel >= 1);
                Assert.True(item.CurrentLevel <= item.MaxLevel);
            }
        }
    }
}
