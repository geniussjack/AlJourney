using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Класс данных EquipmentDatabase. Сохраняет информацию и параметры.
    /// </summary>
    public static class EquipmentDatabase
    {
        /// <summary>
        /// Элемент Templates.
        /// </summary>
        public static readonly Dictionary<string, EquipmentData> Templates = new()
        {
            // Mage Weapons
            ["fireball"] = new EquipmentData(
                "fireball", "Fireball", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 10,
                new Dictionary<string, int> { ["magic_damage"] = 3 }, []),

            ["iceball"] = new EquipmentData(
                "iceball", "Iceball", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["magic_damage"] = 2 }, []),

            ["electroball"] = new EquipmentData(
                "electroball", "Electroball", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 10,
                new Dictionary<string, int> { ["magic_damage"] = 4 }, []),

            // Warrior Weapons
            ["sword"] = new EquipmentData(
                "sword", "Sword", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 10,
                new Dictionary<string, int> { ["damage"] = 3 }, []),

            ["axe"] = new EquipmentData(
                "axe", "Axe", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["damage"] = 4, ["defense"] = -1 }, []),

            ["spear"] = new EquipmentData(
                "spear", "Spear", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 10,
                new Dictionary<string, int> { ["damage"] = 2 }, []),

            // Armor (keeping a couple for defaults if needed)
            ["leather_armor"] = new EquipmentData(
                "leather_armor", "Leather Armor", EquipmentSlot.Body, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["defense"] = 3 }, []),

            ["dragon_scales"] = new EquipmentData(
                "dragon_scales", "Dragon Scales", EquipmentSlot.Body, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["defense"] = 15, ["immunity_burn"] = 100 }, []),

            ["power_ring"] = new EquipmentData(
                "power_ring", "Ring of Power", EquipmentSlot.Ring, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["damage"] = 10 }, []),

            ["life_amulet"] = new EquipmentData(
                "life_amulet", "Amulet of Life", EquipmentSlot.Necklace, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["hp_percent"] = 20 }, [])
        };
    }
}
