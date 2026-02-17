using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Represents an equipment item with stats and abilities.
    /// </summary>
    public record EquipmentData(
        string Id,
        string Name,
        EquipmentSlot Slot,
        EquipmentRarity Rarity,
        int CurrentLevel,
        int MaxLevel,
        Dictionary<string, int> BaseStats,
        Dictionary<string, string> SpecialAbilities
    )
    {
        /// <summary>
        /// Gets the color associated with this rarity.
        /// </summary>
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
        /// Gets the drop chance percentage for this rarity.
        /// </summary>
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
        /// Gets the upgrade cost for the next level, scaled by wave number.
        /// </summary>
        /// <param name="waveNumber">Current wave number for scaling</param>
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
            
            // Apply wave scaling if wave number is provided
            if (waveNumber > 0)
            {
                return ScalingSystem.ScaleCost(levelCost, waveNumber);
            }
            
            return levelCost;
        }

        /// <summary>
        /// Upgrades the equipment to the next level.
        /// </summary>
        public EquipmentData Upgrade()
        {
            if (CurrentLevel >= MaxLevel)
            {
                return this;
            }

            Dictionary<string, int> newStats = new(BaseStats);
            foreach (string stat in newStats.Keys.ToList())
            {
                newStats[stat]++; // Each level adds +1 to each stat
            }

            return this with { CurrentLevel = CurrentLevel + 1, BaseStats = newStats };
        }

        /// <summary>
        /// Gets the total stats including level bonuses.
        /// </summary>
        public Dictionary<string, int> GetTotalStats()
        {
            Dictionary<string, int> totalStats = new(BaseStats);
            foreach (string stat in totalStats.Keys.ToList())
            {
                totalStats[stat] += CurrentLevel - 1; // Add level bonuses
            }
            return totalStats;
        }

        public override string ToString()
        {
            return $"{Name} ({Rarity}) - Level {CurrentLevel}/{MaxLevel}";
        }
    }
}
