using AlJourney.Scripts.Core;
using System;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Serializable data structure for game save/load system.
    /// Now supports dual hero system.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Save file schema version for migration support.
        /// </summary>
        public int SchemaVersion { get; set; } = 1;

        /// <summary>
        /// Current wave number.
        /// </summary>
        public int CurrentWave { get; set; }

        /// <summary>
        /// Highest wave reached (for statistics).
        /// </summary>
        public int HighestWave { get; set; }

        /// <summary>
        /// Player's accumulated coins.
        /// </summary>
        public int Coins { get; set; }

        // === MAGE STATS ===
        /// <summary>
        /// Mage's current HP.
        /// </summary>
        public int MageHealth { get; set; }

        /// <summary>
        /// Mage's current maximum HP.
        /// </summary>
        public int MageMaxHealth { get; set; }

        /// <summary>
        /// Mage's damage stat.
        /// </summary>
        public int MageDamage { get; set; }

        /// <summary>
        /// Mage's defense stat.
        /// </summary>
        public int MageDefense { get; set; }

        // === WARRIOR STATS ===
        /// <summary>
        /// Warrior's current HP.
        /// </summary>
        public int WarriorHealth { get; set; }

        /// <summary>
        /// Warrior's current maximum HP.
        /// </summary>
        public int WarriorMaxHealth { get; set; }

        /// <summary>
        /// Warrior's damage stat.
        /// </summary>
        public int WarriorDamage { get; set; }

        /// <summary>
        /// Warrior's defense stat.
        /// </summary>
        public int WarriorDefense { get; set; }

        /// <summary>
        /// Permanent upgrades purchased from shop.
        /// </summary>
        public Dictionary<string, int> PermanentUpgrades { get; set; }

        /// <summary>
        /// Active artifacts (for future implementation).
        /// </summary>
        public List<string> ActiveArtifacts { get; set; }

        // === EQUIPMENT ===
        /// <summary>
        /// Equipment for each hero.
        /// </summary>
        public Dictionary<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> HeroEquipment { get; set; }

        /// <summary>
        /// Player inventory items.
        /// </summary>
        public List<EquipmentData> Inventory { get; set; }

        // === ABILITIES ===
        /// <summary>
        /// Unlocked abilities for each hero.
        /// </summary>
        public Dictionary<CharacterClass, List<AbilityData>> UnlockedAbilities { get; set; }

        /// <summary>
        /// Equipped abilities for each hero.
        /// </summary>
        public Dictionary<CharacterClass, List<AbilityData>> EquippedAbilities { get; set; }

        /// <summary>
        /// Timestamp of last save.
        /// </summary>
        public string LastSaveTime { get; set; }

        public SaveData()
        {
            CurrentWave = 1;
            HighestWave = 1;
            Coins = 0;
            PermanentUpgrades = [];
            ActiveArtifacts = [];

            // Initialize equipment
            HeroEquipment = [];
            Inventory = [];

            // Initialize abilities
            UnlockedAbilities = [];
            EquippedAbilities = [];

            LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Creates a fresh save with both heroes at starting stats.
        /// </summary>
        public static SaveData CreateNew()
        {
            SaveData save = new()
            {
                CurrentWave = 1,
                HighestWave = 1,
                Coins = 0,
                // Mage starting stats
                MageMaxHealth = GameConstants.MAGE_BASE_HP,
                MageHealth = GameConstants.MAGE_BASE_HP,
                MageDamage = GameConstants.MAGE_BASE_DAMAGE,
                MageDefense = GameConstants.MAGE_BASE_DEFENSE,

                // Warrior starting stats
                WarriorMaxHealth = GameConstants.WARRIOR_BASE_HP,
                WarriorHealth = GameConstants.WARRIOR_BASE_HP,
                WarriorDamage = GameConstants.WARRIOR_BASE_DAMAGE,
                WarriorDefense = GameConstants.WARRIOR_BASE_DEFENSE
            };

            return save;
        }

        /// <summary>
        /// Migrates old save data to current schema version.
        /// </summary>
        /// <param name="oldData">Save data with outdated schema</param>
        /// <returns>Migrated save data or null if migration fails</returns>
        public static SaveData Migrate(SaveData oldData)
        {
            if (oldData.SchemaVersion == 1)
            {
                // Current version - no migration needed
                return oldData;
            }

            // Future: Add migration logic for older versions
            Godot.GD.Print($"[SaveData] Migrating from schema version {oldData.SchemaVersion} to 1");

            // For now, return null to indicate migration failure
            // In the future, add specific migration paths here
            return null;
        }
    }
}
