using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Data
{
    /// <summary>
    /// Equipment data class. Stores equipment templates and their parameters.
    /// </summary>
    public static class EquipmentDatabase
    {
        /// <summary>
        /// The registry of equipment templates, keyed by item Id.
        /// </summary>
        public static readonly Dictionary<string, EquipmentData> Templates = new()
        {
            // Mage Weapons
            ["fireball"] = new EquipmentData(
                "fireball", "WPN_NAME_FIREBALL", "WPN_DESC_FIREBALL", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 10,
                new Dictionary<string, int> { ["damage"] = 5, ["burn_damage"] = 2 }, []),

            ["iceball"] = new EquipmentData(
                "iceball", "WPN_NAME_ICEBALL", "WPN_DESC_ICEBALL", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["damage"] = 2, ["weaken_amount"] = 30 }, []),

            ["electroball"] = new EquipmentData(
                "electroball", "WPN_NAME_ELECTROBALL", "WPN_DESC_ELECTROBALL", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 10,
                new Dictionary<string, int> { ["damage"] = 3, ["shock_amount"] = 50 }, []),

            // Warrior Weapons
            ["sword"] = new EquipmentData(
                "sword", "WPN_NAME_SWORD", "WPN_DESC_SWORD", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 10,
                new Dictionary<string, int> { ["damage"] = 5 }, []),

            ["axe"] = new EquipmentData(
                "axe", "WPN_NAME_AXE", "WPN_DESC_AXE", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["damage"] = 3, ["bleed_damage"] = 2 }, []),

            ["spear"] = new EquipmentData(
                "spear", "WPN_NAME_SPEAR", "WPN_DESC_SPEAR", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 10,
                new Dictionary<string, int> { ["damage"] = 2, ["vulnerable_amount"] = 50 }, []),

            // Armor (keeping a couple for defaults if needed)
            ["leather_armor"] = new EquipmentData(
                "leather_armor", "WPN_NAME_LEATHER_ARMOR", "", EquipmentSlot.Body, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["defense"] = 3 }, []),

            ["dragon_scales"] = new EquipmentData(
                "dragon_scales", "WPN_NAME_DRAGON_SCALES", "", EquipmentSlot.Body, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["defense"] = 15, ["immunity_burn"] = 100 }, []),

            ["power_ring"] = new EquipmentData(
                "power_ring", "WPN_NAME_POWER_RING", "", EquipmentSlot.Ring, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["damage"] = 10 }, []),

            ["life_amulet"] = new EquipmentData(
                "life_amulet", "WPN_NAME_LIFE_AMULET", "", EquipmentSlot.Necklace, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["hp_percent"] = 20 }, [])
        };
    }
}
