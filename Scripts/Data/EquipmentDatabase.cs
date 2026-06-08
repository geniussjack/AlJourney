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
                "rusty_sword", "Ржавый меч", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["damage"] = 2 }, []),

            ["old_staff"] = new EquipmentData(
                "old_staff", "Старый посох", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["magic_damage"] = 2 }, []),

            ["steel_blade"] = new EquipmentData(
                "steel_blade", "Стальной клинок", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["damage"] = 5, ["crit_chance"] = 10 }, []),

            ["apprentice_staff"] = new EquipmentData(
                "apprentice_staff", "Посох ученика", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["magic_damage"] = 5, ["mana_regen"] = 1 }, []),

            ["ice_sword"] = new EquipmentData(
                "ice_sword", "Ледяной меч", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["damage"] = 8, ["slow"] = 20 }, []),

            ["fire_staff"] = new EquipmentData(
                "fire_staff", "Огненный посох", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["magic_damage"] = 8, ["burn"] = 25 }, []),

            ["shadow_blade"] = new EquipmentData(
                "shadow_blade", "Меч теней", EquipmentSlot.Weapon, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["damage"] = 12, ["invisibility"] = 30 }, []),

            ["elemental_staff"] = new EquipmentData(
                "elemental_staff", "Посох стихий", EquipmentSlot.Weapon, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["magic_damage"] = 12, ["random_element"] = 50 }, []),

            ["excalibur"] = new EquipmentData(
                "excalibur", "Экскалибур", EquipmentSlot.Weapon, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["damage"] = 20, ["lifesteal"] = 15 }, []),

            ["archmage_staff"] = new EquipmentData(
                "archmage_staff", "Посох архимага", EquipmentSlot.Weapon, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["magic_damage"] = 20, ["double_spells"] = 100 }, []),

            ["leather_armor"] = new EquipmentData(
                "leather_armor", "Кожаная броня", EquipmentSlot.Body, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["defense"] = 3 }, []),

            ["dragon_scales"] = new EquipmentData(
                "dragon_scales", "Драконья чешуя", EquipmentSlot.Body, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["defense"] = 15, ["immunity_burn"] = 100 }, []),

            ["power_ring"] = new EquipmentData(
                "power_ring", "Кольцо силы", EquipmentSlot.Ring, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["damage"] = 10 }, []),

            ["life_amulet"] = new EquipmentData(
                "life_amulet", "Амулет жизни", EquipmentSlot.Necklace, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["hp_percent"] = 20 }, [])
        };
    }
}
