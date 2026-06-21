using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Структура данных, представляющая предмет экипировки.
    /// Содержит информацию о типе, редкости, уровне прокачки, а также базовые характеристики и специальные способности предмета.
    /// </summary>
    public record EquipmentData(
        string Id,
        string Name,
        string DescriptionKey,
        EquipmentSlot Slot,
        EquipmentRarity Rarity,
        int CurrentLevel,
        int MaxLevel,
        Dictionary<string, int> BaseStats,
        Dictionary<string, string> SpecialAbilities
    )
    {
        /// <summary>
        /// Возвращает цвет, соответствующий уровню редкости предмета.
        /// Используется для подсветки предмета в инвентаре или интерфейсе.
        /// </summary>
        /// <returns>Цвет, соответствующий редкости.</returns>
        public Color GetRarityColor()
        {
            return Rarity switch
            {
                EquipmentRarity.Common => Colors.Gray,
                EquipmentRarity.Uncommon => Colors.Green,
                EquipmentRarity.Rare => Colors.Blue,
                EquipmentRarity.Epic => Colors.Purple,
                EquipmentRarity.Legendary => Colors.Orange,
                _ => Colors.White
            };
        }

        /// <summary>
        /// Возвращает шанс выпадения предмета в зависимости от его редкости.
        /// </summary>
        /// <returns>Вероятность выпадения.</returns>
        public float GetDropChance()
        {
            return Rarity switch
            {
                EquipmentRarity.Common => 40f,
                EquipmentRarity.Uncommon => 30f,
                EquipmentRarity.Rare => 15f,
                EquipmentRarity.Epic => 10f,
                EquipmentRarity.Legendary => 5f,
                _ => 0f
            };
        }

        /// <summary>
        /// Вычисляет стоимость улучшения предмета до следующего уровня.
        /// Стоимость может масштабироваться в зависимости от текущей волны.
        /// </summary>
        /// <param name="waveNumber">Номер текущей волны для расчёта наценки. При значении 0 возвращается базовая стоимость.</param>
        /// <returns>Количество монет, необходимое для улучшения, или 0, если достигнут максимальный уровень.</returns>
        public int GetUpgradeCost(int waveNumber = 0)
        {
            if (CurrentLevel >= MaxLevel)
            {
                return 0;
            }

            int baseCost = Rarity switch
            {
                EquipmentRarity.Common => 50,
                EquipmentRarity.Uncommon => 100,
                EquipmentRarity.Rare => 200,
                EquipmentRarity.Epic => 400,
                EquipmentRarity.Legendary => 800,
                _ => 50
            };

            int levelCost = baseCost * CurrentLevel;

            return waveNumber > 0 ? ScalingSystem.ScaleCost(levelCost, waveNumber) : levelCost;
        }

        /// <summary>
        /// Создает и возвращает улучшенную копию предмета, повышая его уровень и базовые характеристики.
        /// Если предмет уже достиг максимального уровня, возвращается текущий экземпляр.
        /// </summary>
        /// <returns>Новый экземпляр EquipmentData с повышенным уровнем и характеристиками.</returns>
        public EquipmentData Upgrade()
        {
            if (CurrentLevel >= MaxLevel)
            {
                return this;
            }

            Dictionary<string, int> newStats = new(BaseStats);
            foreach (string stat in newStats.Keys.ToList())
            {
                newStats[stat]++;
            }

            return this with { CurrentLevel = CurrentLevel + 1, BaseStats = newStats };
        }

        /// <summary>
        /// Возвращает итоговые характеристики предмета, учитывающие его базовые значения и текущий уровень улучшения.
        /// </summary>
        /// <returns>Словарь, содержащий названия характеристик и их итоговые числовые значения.</returns>
        public Dictionary<string, int> GetTotalStats()
        {
            Dictionary<string, int> totalStats = new(BaseStats);
            foreach (string stat in totalStats.Keys.ToList())
            {
                totalStats[stat] += CurrentLevel - 1;
            }
            return totalStats;
        }

        /// <summary>
        /// Возвращает строковое представление предмета, включающее его название, редкость и текущий уровень относительно максимального.
        /// </summary>
        /// <returns>Строка в формате "Название - Level Текущий/Максимальный".</returns>
        public override string ToString()
        {
            return $"{Name} ({Rarity}) - Level {CurrentLevel}/{MaxLevel}";
        }
    }
}
