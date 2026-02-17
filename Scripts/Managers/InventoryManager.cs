using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Simple working inventory manager without signal issues.
    /// </summary>
    public partial class InventoryManager : Node
    {
        public static InventoryManager Instance { get; private set; } = null!;

        private readonly List<EquipmentData> _inventory = [];
        private readonly Dictionary<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> _heroEquipment = [];

        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            GD.Print("[InventoryManager] Initialized");
        }

        public void AddItems(List<EquipmentData> items)
        {
            foreach (EquipmentData item in items)
            {
                _inventory.Add(item);
                GD.Print($"[InventoryManager] Added: {item.Name}");
            }
        }

        public bool EquipItem(CharacterClass hero, EquipmentData item)
        {
            if (!_heroEquipment.TryGetValue(hero, out Dictionary<EquipmentSlot, EquipmentData> value))
            {
                value = [];
                _heroEquipment[hero] = value;
            }

            if (value.TryGetValue(item.Slot, out _))
            {
                _ = UnequipItem(hero, item.Slot);
            }

            value[item.Slot] = item;
            GD.Print($"[InventoryManager] Equipped {item.Name} to {hero}");
            return true;
        }

        public EquipmentData UnequipItem(CharacterClass hero, EquipmentSlot slot)
        {
            if (!_heroEquipment.TryGetValue(hero, out Dictionary<EquipmentSlot, EquipmentData> heroSlots))
            {
                return null!;
            }

            if (!heroSlots.TryGetValue(slot, out EquipmentData item))
            {
                return null!;
            }

            _ = heroSlots.Remove(slot);
            _inventory.Add(item);
            GD.Print($"[InventoryManager] Unequipped {item.Name} from {hero}");
            return item;
        }

        public bool UpgradeEquipment(EquipmentData item)
        {
            // Get current wave number for cost scaling
            int waveNumber = GameStateManager.Instance.CurrentWave;
            int cost = item.GetUpgradeCost(waveNumber);
            
            if (cost == 0 || GameStateManager.Instance.Coins < cost)
            {
                GD.Print($"[InventoryManager] Not enough coins to upgrade {item.Name}. Need: {cost}, Have: {GameStateManager.Instance.Coins}");
                return false;
            }

            _ = GameStateManager.Instance.SpendCoins(cost);
            EquipmentData upgradedItem = item.Upgrade();

            int inventoryIndex = _inventory.IndexOf(item);
            if (inventoryIndex >= 0)
            {
                _inventory[inventoryIndex] = upgradedItem;
            }
            else
            {
                foreach (Dictionary<EquipmentSlot, EquipmentData> heroEquipment in _heroEquipment.Values)
                {
                    if (heroEquipment.ContainsValue(item))
                    {
                        EquipmentSlot slot = heroEquipment.First(kvp => kvp.Value == item).Key;
                        heroEquipment[slot] = upgradedItem;
                        break;
                    }
                }
            }

            GD.Print($"[InventoryManager] Upgraded {item.Name} to level {upgradedItem.CurrentLevel}");
            return true;
        }

        public IReadOnlyList<EquipmentData> GetInventory()
        {
            return _inventory.AsReadOnly();
        }

        public Dictionary<EquipmentSlot, EquipmentData> GetHeroEquipment(CharacterClass hero)
        {
            return _heroEquipment.TryGetValue(hero, out Dictionary<EquipmentSlot, EquipmentData> equipment) ? equipment : [];
        }

        public List<EquipmentData> GetEquipmentByRarity(EquipmentRarity rarity)
        {
            return [.. _inventory.Where(item => item.Rarity == rarity)];
        }

        public List<EquipmentData> GetEquipmentBySlot(EquipmentSlot slot)
        {
            return [.. _inventory.Where(item => item.Slot == slot)];
        }
    }
}
