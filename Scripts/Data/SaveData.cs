using AlJourney.Scripts.Core;
using System;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Serializable data structure for game save/load system.
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

        /// <summary>
        /// Selected character class.
        /// </summary>
        public CharacterClass SelectedCharacter { get; set; }

        /// <summary>
        /// Player's current HP.
        /// </summary>
        public int PlayerHealth { get; set; }

        /// <summary>
        /// Player's current maximum HP.
        /// </summary>
        public int PlayerMaxHealth { get; set; }

        /// <summary>
        /// Player's damage stat.
        /// </summary>
        public int PlayerDamage { get; set; }

        /// <summary>
        /// Player's defense stat.
        /// </summary>
        public int PlayerDefense { get; set; }

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
            SelectedCharacter = CharacterClass.Warrior;
            PermanentUpgrades = [];
            ActiveArtifacts = [];
            LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Creates a fresh save with initial character stats.
        /// </summary>
        public static SaveData CreateNew(CharacterClass characterClass)
        {
            var save = new SaveData
            {
                SelectedCharacter = characterClass,
                CurrentWave = 1,
                Coins = 0
            };

            // Set initial stats based on character
            if (characterClass == CharacterClass.Mage)
            {
                save.PlayerMaxHealth = GameConstants.MAGE_BASE_HP;
                save.PlayerHealth = GameConstants.MAGE_BASE_HP;
                save.PlayerDamage = GameConstants.MAGE_BASE_DAMAGE;
                save.PlayerDefense = GameConstants.MAGE_BASE_DEFENSE;
            }
            else // Warrior
            {
                save.PlayerMaxHealth = GameConstants.WARRIOR_BASE_HP;
                save.PlayerHealth = GameConstants.WARRIOR_BASE_HP;
                save.PlayerDamage = GameConstants.WARRIOR_BASE_DAMAGE;
                save.PlayerDefense = GameConstants.WARRIOR_BASE_DEFENSE;
            }

            return save;
        }
    }
}
