using AlJourney.Scripts.Core;
using System;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    [Serializable]
    /// <summary>
    /// Класс данных SaveData. Сохраняет информацию и параметры.
    /// </summary>
    public class SaveData
    {
        /// <summary>
        /// Элемент SchemaVersion.
        /// </summary>
        public int SchemaVersion { get; set; } = 1;

        public int CurrentWave { get; set; }

        public int HighestWave { get; set; }

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
        /// Элемент CreateNew.
        /// </summary>
        public static SaveData CreateNew()
        {
            SaveData save = new()
            {
                CurrentWave = 1,
                HighestWave = 1,
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

            return save;
        }

        /// <summary>
        /// Элемент Migrate.
        /// </summary>
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
