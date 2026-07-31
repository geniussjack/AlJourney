using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using System.Globalization;

namespace AlJourneyTests.Scripts.Data
{
    public class SaveDataTests
    {
        [Fact]
        public void Constructor_SetsFirstWaveDefaultsAndEmptyCollections()
        {
            SaveData save = new();

            Assert.Equal(1, save.SchemaVersion);
            Assert.Equal(1, save.CurrentWave);
            Assert.Equal(1, save.HighestWave);
            Assert.Equal(0, save.Coins);
            Assert.Empty(save.PermanentUpgrades);
            Assert.Empty(save.ActiveArtifacts);
            Assert.Empty(save.HeroEquipment);
            Assert.Empty(save.Inventory);
            Assert.Empty(save.UnlockedAbilities);
            Assert.Empty(save.EquippedAbilities);
        }

        [Fact]
        public void Constructor_SetsLastSaveTimeToAParsableTimestamp()
        {
            SaveData save = new();

            bool parsed = DateTime.TryParseExact(
                save.LastSaveTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

            Assert.True(parsed);
        }

        [Fact]
        public void CreateNew_SetsBaseHeroStatsFromGameConstants()
        {
            SaveData save = SaveData.CreateNew();

            Assert.Equal(1, save.CurrentWave);
            Assert.Equal(1, save.HighestWave);
            Assert.Equal(0, save.Coins);

            Assert.Equal(GameConstants.MAGE_BASE_HP, save.MageMaxHealth);
            Assert.Equal(GameConstants.MAGE_BASE_HP, save.MageHealth);
            Assert.Equal(GameConstants.MAGE_BASE_DAMAGE, save.MageDamage);
            Assert.Equal(GameConstants.MAGE_BASE_DEFENSE, save.MageDefense);

            Assert.Equal(GameConstants.WARRIOR_BASE_HP, save.WarriorMaxHealth);
            Assert.Equal(GameConstants.WARRIOR_BASE_HP, save.WarriorHealth);
            Assert.Equal(GameConstants.WARRIOR_BASE_DAMAGE, save.WarriorDamage);
            Assert.Equal(GameConstants.WARRIOR_BASE_DEFENSE, save.WarriorDefense);
        }

        [Fact]
        public void CreateNew_GrantsAllSixStartingWeaponsToInventory()
        {
            SaveData save = SaveData.CreateNew();

            Assert.Equal(6, save.Inventory.Count);
            string[] expectedIds = ["fireball", "iceball", "electroball", "sword", "axe", "spear"];
            foreach (string id in expectedIds)
            {
                Assert.Contains(save.Inventory, item => item.Id == id);
            }
        }

        [Fact]
        public void CreateNew_EquipsStartingWeaponsToMageAndWarrior()
        {
            SaveData save = SaveData.CreateNew();

            EquipmentData mageWeapon = save.HeroEquipment[CharacterClass.Mage][EquipmentSlot.Weapon];
            EquipmentData warriorWeapon = save.HeroEquipment[CharacterClass.Warrior][EquipmentSlot.Weapon];

            Assert.Equal("fireball", mageWeapon.Id);
            Assert.Equal("sword", warriorWeapon.Id);
        }

        [Fact]
        public void Migrate_CurrentSchemaVersion_ReturnsSameInstanceUnchanged()
        {
            SaveData save = SaveData.CreateNew();

            SaveData? result = SaveData.Migrate(save);

            // SchemaVersion is always 1 in the current game version, so this is the only branch
            // reachable without a running Godot engine (otherwise SaveData.Migrate calls Godot.GD.Print,
            // which is unavailable in a headless xUnit process without Godot).
            Assert.Same(save, result);
        }
    }
}
