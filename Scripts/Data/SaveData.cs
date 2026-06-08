using AlJourney.Scripts.Core;
using System;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    [Serializable]
    /// <summary>
    /// Класс, представляющий структуру данных для сохранения и загрузки прогресса игрока.
    /// Хранит состояние характеристик героев, инвентарь, экипировку, разблокированные способности и прогресс по волнам.
    /// </summary>
    public class SaveData
    {
        /// <summary>
        /// Версия схемы данных сохранения. Используется для миграции старых сохранений при обновлении игры.
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
        /// Фабричный метод, создающий новый профиль сохранения со стартовыми значениями по умолчанию.
        /// Устанавливает начальные характеристики для Мага и Воина, а также сбрасывает прогресс до первой волны.
        /// </summary>
        /// <returns>Новый экземпляр SaveData с начальными параметрами.</returns>
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
        /// Метод для адаптации (миграции) данных из старых версий игры в новую структуру сохранения.
        /// Если версия схемы устарела, данные преобразуются для обеспечения совместимости.
        /// </summary>
        /// <param name="oldData">Данные старого сохранения.</param>
        /// <returns>Обновленный объект SaveData или null, если миграция не удалась.</returns>
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
