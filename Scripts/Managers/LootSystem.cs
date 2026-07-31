using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Global loot system manager. Responsible for generating equipment
    /// after defeating enemies.
    /// Determines item rarity and stats based on the current wave.
    /// </summary>
    public partial class LootSystem : Node, ILootSystem
    {
        /// <summary>
        /// Global access point for the loot system singleton.
        /// </summary>
        public static LootSystem Instance { get; private set; } = null!;

        private readonly Dictionary<string, EquipmentData> _equipmentTemplates = EquipmentDatabase.Templates;

        private static readonly (EquipmentRarity Rarity, float Chance)[] RarityWeights = [
            (EquipmentRarity.Common, 40f),
            (EquipmentRarity.Uncommon, 30f),
            (EquipmentRarity.Rare, 15f),
            (EquipmentRarity.Epic, 10f),
            (EquipmentRarity.Legendary, 5f)
        ];

        private static readonly (EquipmentSlot Slot, float Chance)[] SlotWeights = [
            (EquipmentSlot.Weapon, 25f),
            (EquipmentSlot.Head, 15f),
            (EquipmentSlot.Body, 15f),
            (EquipmentSlot.Legs, 15f),
            (EquipmentSlot.Necklace, 15f),
            (EquipmentSlot.Ring, 7f),
            (EquipmentSlot.Earring, 8f)
        ];

        /// <summary>
        /// Initializes the singleton. If an instance already exists, the duplicate is removed.
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
        /// Generates an expanded list of items after defeating a boss.
        /// </summary>
        /// <param name="waveNumber">The current wave number, used to scale rarity.</param>
        /// <returns>The list of generated equipment items.</returns>
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
        /// Generates a single item after defeating a normal enemy.
        /// The chance of high rarity is artificially lowered for balance.
        /// </summary>
        /// <param name="waveNumber">The current wave number.</param>
        /// <returns>The generated equipment item, or null on failure.</returns>
        public EquipmentData GenerateNormalLoot(int waveNumber)
        {
            EquipmentRarity rarity = DetermineRarity();

            // Lower the rarity for normal enemies
            if (rarity == EquipmentRarity.Legendary)
            {
                rarity = EquipmentRarity.Epic;
            }

            if (rarity == EquipmentRarity.Epic && GD.Randf() > 0.1f)
            {
                rarity = EquipmentRarity.Rare;
            }

            EquipmentSlot slot = DetermineSlot();
            EquipmentData item = GenerateEquipment(rarity, slot);

            GD.Print($"[LootSystem] Generated normal loot: {item.Name} at wave {waveNumber}");
            return item;
        }

        private static EquipmentRarity DetermineRarity()
        {
            float roll = GD.Randf() * 100f;
            float cumulative = 0f;

            foreach ((EquipmentRarity rarity, float chance) in RarityWeights)
            {
                cumulative += chance;
                if (roll <= cumulative)
                {
                    return rarity;
                }
            }

            return EquipmentRarity.Common;
        }

        private static EquipmentSlot DetermineSlot()
        {
            float roll = GD.Randf() * 100f;
            float cumulative = 0f;

            foreach ((EquipmentSlot slot, float chance) in SlotWeights)
            {
                cumulative += chance;
                if (roll <= cumulative)
                {
                    return slot;
                }
            }

            return EquipmentSlot.Earring;
        }

        private EquipmentData GenerateEquipment(EquipmentRarity rarity, EquipmentSlot slot)
        {
            List<EquipmentData> templates = [.. _equipmentTemplates.Values.Where(item => item.Slot == slot && item.Rarity == rarity)];

            return templates.Count > 0
                ? templates[GD.RandRange(0, templates.Count - 1)]
                : GenerateBasicEquipment(rarity, slot);
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
                "", // DescriptionKey
                slot,
                rarity,
                1,
                maxLevel,
                stats,
                []
            );
        }
    }
}
