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
            ["rusty_sword"] = new EquipmentData(
                "rusty_sword", "Rusty Sword", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["damage"] = 2 }, []),

            ["old_staff"] = new EquipmentData(
                "old_staff", "Old Staff", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["magic_damage"] = 2 }, []),

            ["steel_blade"] = new EquipmentData(
                "steel_blade", "Steel Blade", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["damage"] = 5, ["crit_chance"] = 10 }, []),

            ["apprentice_staff"] = new EquipmentData(
                "apprentice_staff", "Apprentice Staff", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["magic_damage"] = 5, ["mana_regen"] = 1 }, []),

            ["ice_sword"] = new EquipmentData(
                "ice_sword", "Ice Sword", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["damage"] = 8, ["slow"] = 20 }, []),

            ["fire_staff"] = new EquipmentData(
                "fire_staff", "Fire Staff", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["magic_damage"] = 8, ["burn"] = 25 }, []),

            ["shadow_blade"] = new EquipmentData(
                "shadow_blade", "Shadow Blade", EquipmentSlot.Weapon, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["damage"] = 12, ["invisibility"] = 30 }, []),

            ["elemental_staff"] = new EquipmentData(
                "elemental_staff", "Elemental Staff", EquipmentSlot.Weapon, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["magic_damage"] = 12, ["random_element"] = 50 }, []),

            ["excalibur"] = new EquipmentData(
                "excalibur", "Excalibur", EquipmentSlot.Weapon, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["damage"] = 20, ["lifesteal"] = 15 }, []),

            ["archmage_staff"] = new EquipmentData(
                "archmage_staff", "Archmage Staff", EquipmentSlot.Weapon, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["magic_damage"] = 20, ["double_spells"] = 100 }, []),

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
