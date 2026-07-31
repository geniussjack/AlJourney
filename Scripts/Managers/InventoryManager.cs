using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Inventory manager. Responsible for storing items, equipping heroes, and upgrading equipment.
    /// </summary>
    public partial class InventoryManager : Node, IInventoryManager
    {
        /// <summary>
        /// Global instance of the inventory manager.
        /// </summary>
        public static InventoryManager Instance { get; private set; } = null!;

        private readonly List<EquipmentData> _inventory = [];
        private readonly Dictionary<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> _heroEquipment = [];

        /// <summary>
        /// Initializes the inventory manager node when it is added to the scene tree.
        /// Ensures only a single instance exists.
        /// </summary>
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

        /// <summary>
        /// Adds a list of items to the player's shared inventory.
        /// </summary>
        /// <param name="items">The list of items to add.</param>
        public void AddItems(List<EquipmentData> items)
        {
            foreach (EquipmentData item in items)
            {
                _inventory.Add(item);
                GD.Print($"[InventoryManager] Added: {item.Name}");
            }
        }

        /// <summary>
        /// Equips the given item to the selected hero. If the slot is already occupied, the old item is unequipped and returned to the inventory.
        /// </summary>
        /// <param name="hero">The hero class the item is equipped to.</param>
        /// <param name="item">The equipment item to equip.</param>
        /// <returns><c>true</c> if the item was successfully equipped.</returns>
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

        /// <summary>
        /// Unequips the item from the given slot on the selected hero and returns it to the shared inventory.
        /// </summary>
        /// <param name="hero">The hero class the item is unequipped from.</param>
        /// <param name="slot">The equipment slot to clear.</param>
        /// <returns>The unequipped item, or <c>null</c> if the slot was empty.</returns>
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

        /// <summary>
        /// Upgrades an equipment item for coins, if there are enough funds.
        /// </summary>
        /// <param name="item">The equipment item to upgrade.</param>
        /// <returns><c>true</c> if the item was successfully upgraded; otherwise <c>false</c>.</returns>
        public bool UpgradeEquipment(EquipmentData item)
        {
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

        /// <summary>
        /// Returns the list of every item currently in the player's inventory.
        /// </summary>
        /// <returns>A read-only list of inventory items.</returns>
        public IReadOnlyList<EquipmentData> GetInventory()
        {
            return _inventory.AsReadOnly();
        }

        /// <summary>
        /// Saves the current state of the inventory and hero equipment into a save data object.
        /// </summary>
        /// <param name="data">The save data object.</param>
        public void SaveToData(SaveData data)
        {
            data.Inventory = [.. _inventory];
            data.HeroEquipment = [];
            foreach (KeyValuePair<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> kvp in _heroEquipment)
            {
                data.HeroEquipment[kvp.Key] = new Dictionary<EquipmentSlot, EquipmentData>(kvp.Value);
            }
        }

        /// <summary>
        /// Loads the state of the inventory and hero equipment from a save data object.
        /// </summary>
        /// <param name="data">The save data object to load state from.</param>
        public void LoadFromData(SaveData data)
        {
            _inventory.Clear();
            if (data.Inventory != null)
            {
                _inventory.AddRange(data.Inventory);
            }

            _heroEquipment.Clear();
            if (data.HeroEquipment != null)
            {
                foreach (KeyValuePair<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> kvp in data.HeroEquipment)
                {
                    _heroEquipment[kvp.Key] = new Dictionary<EquipmentSlot, EquipmentData>(kvp.Value);
                }
            }
            GD.Print($"[InventoryManager] Loaded {_inventory.Count} items and equipment for {_heroEquipment.Count} heroes from save.");
        }

        /// <summary>
        /// Gets all equipment currently equipped by the given hero.
        /// </summary>
        /// <param name="hero">The hero class.</param>
        /// <returns>A dictionary where the keys are equipment slots and the values are the equipped items.</returns>
        public Dictionary<EquipmentSlot, EquipmentData> GetHeroEquipment(CharacterClass hero)
        {
            return _heroEquipment.TryGetValue(hero, out Dictionary<EquipmentSlot, EquipmentData> equipment) ? equipment : [];
        }

        public EquipmentData GetEquippedItem(CharacterClass hero, EquipmentSlot slot)
        {
            return GetHeroEquipment(hero).TryGetValue(slot, out EquipmentData item) ? item : null;
        }

        /// <summary>
        /// Gets the list of inventory items filtered by their rarity.
        /// </summary>
        /// <param name="rarity">The rarity to filter by.</param>
        /// <returns>The list of items with the given rarity.</returns>
        public List<EquipmentData> GetEquipmentByRarity(EquipmentRarity rarity)
        {
            return [.. _inventory.Where(item => item.Rarity == rarity)];
        }

        /// <summary>
        /// Gets the list of inventory items filtered by the equipment slot they are intended for.
        /// </summary>
        /// <param name="slot">The equipment slot to filter by.</param>
        /// <returns>The list of items for the given slot.</returns>
        public List<EquipmentData> GetEquipmentBySlot(EquipmentSlot slot)
        {
            return [.. _inventory.Where(item => item.Slot == slot)];
        }
    }
}
