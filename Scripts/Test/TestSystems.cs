using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Test
{
    /// <summary>
    /// Test script to verify equipment and ability systems are working.
    /// </summary>
    /// <summary>
    /// Менеджер TestSystems. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class TestSystems : Node
    {
        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            GD.Print("=== TESTING EQUIPMENT AND ABILITY SYSTEMS ===");

            // Test LootSystem
            if (LootSystem.Instance != null)
            {
                GD.Print("✅ LootSystem is loaded");
                List<EquipmentData> testLoot = LootSystem.Instance.GenerateBossLoot(10);
                GD.Print($"✅ Generated {testLoot.Count} test items");

                // Test InventoryManager
                if (InventoryManager.Instance != null)
                {
                    GD.Print("✅ InventoryManager is loaded");
                    InventoryManager.Instance.AddItems(testLoot);
                    IReadOnlyList<EquipmentData> inventory = InventoryManager.Instance.GetInventory();
                    GD.Print($"✅ Inventory now has {inventory.Count} items");

                    // Test upgrade
                    if (inventory.Count > 0)
                    {
                        EquipmentData firstItem = inventory[0];
                        GD.Print($"✅ Testing upgrade on {firstItem.Name}");
                        bool upgradeResult = InventoryManager.Instance.UpgradeEquipment(firstItem);
                        GD.Print($"✅ Upgrade result: {upgradeResult}");
                    }
                }
                else
                {
                    GD.Print("❌ InventoryManager is NOT loaded");
                }
            }
            else
            {
                GD.Print("❌ LootSystem is NOT loaded");
            }

            // Test AbilitySystem
            if (AbilitySystem.Instance != null)
            {
                GD.Print("✅ AbilitySystem is loaded");
                List<AbilityData> mageAbilities = AbilitySystem.Instance.GetAvailableAbilities(CharacterClass.Mage);
                GD.Print($"✅ Found {mageAbilities.Count} abilities for Mage");

                List<AbilityData> warriorAbilities = AbilitySystem.Instance.GetAvailableAbilities(CharacterClass.Warrior);
                GD.Print($"✅ Found {warriorAbilities.Count} abilities for Warrior");
            }
            else
            {
                GD.Print("❌ AbilitySystem is NOT loaded");
            }

            GD.Print("=== TEST COMPLETE ===");
        }
    }
}
