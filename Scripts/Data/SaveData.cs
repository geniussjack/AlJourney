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
        /// Current wave number.
        /// </summary>
        public int CurrentWave { get; set; }

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

        /// <summary>
        /// Timestamp of last save.
        /// </summary>
        public string LastSaveTime { get; set; }

        public SaveData()
        {
            CurrentWave = 1;
            Coins = 0;
            PermanentUpgrades = [];
            ActiveArtifacts = [];
            LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Creates a fresh save with both heroes at starting stats.
        /// </summary>
        public static SaveData CreateNew()
        {
            var save = new SaveData
            {
                CurrentWave = 1,
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
    }
}