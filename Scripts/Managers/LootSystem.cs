using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер LootSystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class LootSystem : Node, ILootSystem
    {
        /// <summary>
        /// Элемент Instance.
        /// </summary>
        public static LootSystem Instance { get; private set; } = null!;

        private readonly Dictionary<string, EquipmentData> _equipmentTemplates = EquipmentDatabase.Templates;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            GD.Print("[LootSystem] Initialized");
        }

        /// <summary>
        /// Генерирует BossLoot.
        /// </summary>
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

        /// <summary>
        /// Генерирует NormalLoot.
        /// </summary>
        public EquipmentData GenerateNormalLoot(int waveNumber)
        {
            EquipmentRarity rarity = DetermineRarity();
            // Demote rarity slightly for normal enemies
            if (rarity == EquipmentRarity.Legendary) rarity = EquipmentRarity.Epic;
            if (rarity == EquipmentRarity.Epic && GD.Randf() > 0.1f) rarity = EquipmentRarity.Rare;

            EquipmentSlot slot = DetermineSlot();
            EquipmentData item = GenerateEquipment(rarity, slot);
            
            GD.Print($"[LootSystem] Generated normal loot: {item.Name} at wave {waveNumber}");
            return item;
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
