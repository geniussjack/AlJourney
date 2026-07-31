using AlJourney.Scripts.Core;
using System;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Class representing the data structure used to save and load player progress.
    /// Stores hero stat state, inventory, equipment, unlocked abilities and wave progress.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Save data schema version. Used to migrate old saves when the game is updated.
        /// </summary>
        public int SchemaVersion { get; set; } = 1;

        public int CurrentWave { get; set; }

        public int HighestWave { get; set; }

        /// <summary>
        /// Id of the campaign map level the player is currently on or should attempt next.
        /// See <see cref="CampaignDatabase"/>.
        /// </summary>
        public string CurrentLevelId { get; set; }

        /// <summary>
        /// Ids of every campaign level already completed (main line and branches).
        /// </summary>
        public HashSet<string> CompletedLevelIds { get; set; }

        public int Coins { get; set; }

        public int MageHealth { get; set; }

        public int MageMaxHealth { get; set; }

        public int MageDamage { get; set; }

        public int MageDefense { get; set; }

        public int WarriorHealth { get; set; }

        public int WarriorMaxHealth { get; set; }

        public int WarriorDamage { get; set; }

        public int WarriorDefense { get; set; }

        public Dictionary<string, int> PermanentUpgrades { get; set; }

        public List<string> ActiveArtifacts { get; set; }

        public Dictionary<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> HeroEquipment { get; set; }

        public List<EquipmentData> Inventory { get; set; }

        public Dictionary<CharacterClass, List<AbilityData>> UnlockedAbilities { get; set; }

        public Dictionary<CharacterClass, List<AbilityData>> EquippedAbilities { get; set; }

        public string LastSaveTime { get; set; }

        public SaveData()
        {
            CurrentWave = 1;
            HighestWave = 1;
            CurrentLevelId = CampaignDatabase.FirstLevelId;
            CompletedLevelIds = [];
            Coins = 0;
            PermanentUpgrades = [];
            ActiveArtifacts = [];

            HeroEquipment = [];
            Inventory = [];

            UnlockedAbilities = [];
            EquippedAbilities = [];

            LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Factory method that creates a new save profile with default starting values.
        /// Sets initial stats for the Mage and Warrior, and resets progress back to the first wave.
        /// </summary>
        /// <returns>A new SaveData instance with starting parameters.</returns>
        public static SaveData CreateNew()
        {
            SaveData save = new()
            {
                CurrentWave = 1,
                HighestWave = 1,
                CurrentLevelId = CampaignDatabase.FirstLevelId,
                CompletedLevelIds = [],
                Coins = 0,
                MageMaxHealth = GameConstants.MAGE_BASE_HP,
                MageHealth = GameConstants.MAGE_BASE_HP,
                MageDamage = GameConstants.MAGE_BASE_DAMAGE,
                MageDefense = GameConstants.MAGE_BASE_DEFENSE,

                WarriorMaxHealth = GameConstants.WARRIOR_BASE_HP,
                WarriorHealth = GameConstants.WARRIOR_BASE_HP,
                WarriorDamage = GameConstants.WARRIOR_BASE_DAMAGE,
                WarriorDefense = GameConstants.WARRIOR_BASE_DEFENSE
            };

            string[] startingWeapons = ["fireball", "iceball", "electroball", "sword", "axe", "spear"];
            foreach (string weaponId in startingWeapons)
            {
                if (EquipmentDatabase.Templates.TryGetValue(weaponId, out EquipmentData weaponData))
                {
                    save.Inventory.Add(weaponData);

                    if (weaponId == "fireball")
                    {
                        save.HeroEquipment[CharacterClass.Mage] = new Dictionary<EquipmentSlot, EquipmentData> { [EquipmentSlot.Weapon] = weaponData };
                    }
                    else if (weaponId == "sword")
                    {
                        save.HeroEquipment[CharacterClass.Warrior] = new Dictionary<EquipmentSlot, EquipmentData> { [EquipmentSlot.Weapon] = weaponData };
                    }
                }
            }

            return save;
        }

        /// <summary>
        /// Adapts data from older game versions into the current save structure.
        /// If the schema version is outdated, the data is converted to ensure compatibility.
        /// </summary>
        /// <param name="oldData">The old save data.</param>
        /// <returns>The migrated SaveData object, or null if migration failed.</returns>
        public static SaveData Migrate(SaveData oldData)
        {
            if (oldData.SchemaVersion == 1)
            {
                return oldData;
            }

            Godot.GD.Print($"[SaveData] Migrating from schema version {oldData.SchemaVersion} to 1");

            return null;
        }
    }
}
