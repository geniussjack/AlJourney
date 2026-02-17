using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Simple working loot system without signal issues.
    /// </summary>
    public partial class LootSystem : Node
    {
        public static LootSystem Instance { get; private set; } = null!;

        private readonly Dictionary<string, EquipmentData> _equipmentTemplates = [];

        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            InitializeEquipmentTemplates();
            GD.Print("[LootSystem] Initialized");
        }

        private void InitializeEquipmentTemplates()
        {
            // Common items
            _equipmentTemplates["rusty_sword"] = new EquipmentData(
                "rusty_sword", "Ржавый меч", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["damage"] = 2 }, []);

            _equipmentTemplates["old_staff"] = new EquipmentData(
                "old_staff", "Старый посох", EquipmentSlot.Weapon, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["magic_damage"] = 2 }, []);

            // Uncommon items
            _equipmentTemplates["steel_blade"] = new EquipmentData(
                "steel_blade", "Стальной клинок", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["damage"] = 5, ["crit_chance"] = 10 }, []);

            _equipmentTemplates["apprentice_staff"] = new EquipmentData(
                "apprentice_staff", "Посох ученика", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, 1, 10,
                new Dictionary<string, int> { ["magic_damage"] = 5, ["mana_regen"] = 1 }, []);

            // Rare items
            _equipmentTemplates["ice_sword"] = new EquipmentData(
                "ice_sword", "Ледяной меч", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["damage"] = 8, ["slow"] = 20 }, []);

            _equipmentTemplates["fire_staff"] = new EquipmentData(
                "fire_staff", "Огненный посох", EquipmentSlot.Weapon, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["magic_damage"] = 8, ["burn"] = 25 }, []);

            // Epic items
            _equipmentTemplates["shadow_blade"] = new EquipmentData(
                "shadow_blade", "Меч теней", EquipmentSlot.Weapon, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["damage"] = 12, ["invisibility"] = 30 }, []);

            _equipmentTemplates["elemental_staff"] = new EquipmentData(
                "elemental_staff", "Посох стихий", EquipmentSlot.Weapon, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["magic_damage"] = 12, ["random_element"] = 50 }, []);

            // Legendary items
            _equipmentTemplates["excalibur"] = new EquipmentData(
                "excalibur", "Экскалибур", EquipmentSlot.Weapon, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["damage"] = 20, ["lifesteal"] = 15 }, []);

            _equipmentTemplates["archmage_staff"] = new EquipmentData(
                "archmage_staff", "Посох архимага", EquipmentSlot.Weapon, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["magic_damage"] = 20, ["double_spells"] = 100 }, []);

            // Armor items
            _equipmentTemplates["leather_armor"] = new EquipmentData(
                "leather_armor", "Кожаная броня", EquipmentSlot.Body, EquipmentRarity.Common, 1, 5,
                new Dictionary<string, int> { ["defense"] = 3 }, []);

            _equipmentTemplates["dragon_scales"] = new EquipmentData(
                "dragon_scales", "Драконья чешуя", EquipmentSlot.Body, EquipmentRarity.Legendary, 1, 25,
                new Dictionary<string, int> { ["defense"] = 15, ["immunity_burn"] = 100 }, []);

            // Accessories
            _equipmentTemplates["power_ring"] = new EquipmentData(
                "power_ring", "Кольцо силы", EquipmentSlot.Ring, EquipmentRarity.Rare, 1, 15,
                new Dictionary<string, int> { ["damage"] = 10 }, []);

            _equipmentTemplates["life_amulet"] = new EquipmentData(
                "life_amulet", "Амулет жизни", EquipmentSlot.Necklace, EquipmentRarity.Epic, 1, 20,
                new Dictionary<string, int> { ["hp_percent"] = 20 }, []);
        }

        public List<EquipmentData> GenerateBossLoot(int waveNumber)
        {
            int dropCount = GD.RandRange(3, 11);
            List<EquipmentData> loot = [];

            GD.Print($"[LootSystem] Generating {dropCount} items for boss at wave {waveNumber}");

            for (int i = 0; i < dropCount; i++)
            {
                EquipmentRarity rarity = DetermineRarity();
                EquipmentSlot slot = DetermineSlot();
                EquipmentData item = GenerateEquipment(rarity, slot);

                if (item != null)
                {
                    loot.Add(item);
                }
            }

            return loot;
        }

        private static EquipmentRarity DetermineRarity()
        {
            float roll = GD.Randf() * 100;

            return roll < 40
                ? EquipmentRarity.Common
                : roll < 70
                ? EquipmentRarity.Uncommon
                : roll < 85 ? EquipmentRarity.Rare : roll < 95 ? EquipmentRarity.Epic : EquipmentRarity.Legendary;
        }

        private static EquipmentSlot DetermineSlot()
        {
            float roll = GD.Randf() * 100;

            return roll < 25
                ? EquipmentSlot.Weapon
                : roll < 40
                ? EquipmentSlot.Head
                : roll < 55
                ? EquipmentSlot.Body
                : roll < 70 ? EquipmentSlot.Legs : roll < 85 ? EquipmentSlot.Necklace : roll < 92 ? EquipmentSlot.Ring : EquipmentSlot.Earring;
        }

        private EquipmentData GenerateEquipment(EquipmentRarity rarity, EquipmentSlot slot)
        {
            List<EquipmentData> templates = [.. _equipmentTemplates.Values.Where(item => item.Slot == slot && item.Rarity == rarity)];

            return templates.Count > 0 ? templates[GD.RandRange(0, templates.Count)] : GenerateBasicEquipment(rarity, slot);
        }

        private static Dictionary<string, int> GetBasicStats(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => new Dictionary<string, int> { ["damage"] = 1 },
                EquipmentSlot.Head => new Dictionary<string, int> { ["defense"] = 1 },
                EquipmentSlot.Body => new Dictionary<string, int> { ["defense"] = 2 },
                EquipmentSlot.Legs => new Dictionary<string, int> { ["defense"] = 1 },
                EquipmentSlot.Necklace => new Dictionary<string, int> { ["hp_percent"] = 5 },
                EquipmentSlot.Ring => new Dictionary<string, int> { ["damage"] = 2 },
                EquipmentSlot.Earring => new Dictionary<string, int> { ["defense"] = 1 },
                _ => []
            };
        }

        private static EquipmentData GenerateBasicEquipment(EquipmentRarity rarity, EquipmentSlot slot)
        {
            string name = $"{rarity} {slot}";
            Dictionary<string, int> stats = GetBasicStats(slot);

            int maxLevel = rarity switch
            {
                EquipmentRarity.Common => 5,
                EquipmentRarity.Uncommon => 10,
                EquipmentRarity.Rare => 15,
                EquipmentRarity.Epic => 20,
                EquipmentRarity.Legendary => 25,
                _ => 5
            };

            return new EquipmentData(
                $"{rarity}_{slot}",
                name,
                slot,
                rarity,
                1,
                maxLevel,
                stats,
                []);
        }
    }
}
