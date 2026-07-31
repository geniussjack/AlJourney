using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Data structure representing an equipment item.
    /// Holds its type, rarity, upgrade level, and base stats and special abilities.
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
        /// Returns the color associated with the item's rarity tier.
        /// Used to highlight the item in the inventory or UI.
        /// </summary>
        /// <returns>The color matching the rarity.</returns>
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
        /// Returns the drop chance of the item based on its rarity.
        /// </summary>
        /// <returns>The drop probability.</returns>
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
        /// Computes the cost to upgrade the item to its next level.
        /// The cost may scale based on the current wave.
        /// </summary>
        /// <param name="waveNumber">Current wave number used for the price markup. When 0, the base cost is returned.</param>
        /// <returns>The number of coins required to upgrade, or 0 if the max level has been reached.</returns>
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
        /// Creates and returns an upgraded copy of the item, raising its level and base stats.
        /// If the item has already reached its max level, the current instance is returned.
        /// </summary>
        /// <returns>A new EquipmentData instance with an increased level and stats.</returns>
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
        /// Returns the item's total stats, accounting for its base values and current upgrade level.
        /// </summary>
        /// <returns>A dictionary of stat names and their total numeric values.</returns>
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
        /// Returns a string representation of the item, including its name, rarity and current level relative to the max.
        /// </summary>
        /// <returns>A string in the format "Name (Rarity) - Level Current/Max".</returns>
        public override string ToString()
        {
            return $"{Name} ({Rarity}) - Level {CurrentLevel}/{MaxLevel}";
        }
    }
}
