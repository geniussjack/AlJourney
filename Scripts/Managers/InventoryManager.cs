using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер InventoryManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class InventoryManager : Node, IInventoryManager
    {
        /// <summary>
        /// Элемент Instance.
        /// </summary>
        public static InventoryManager Instance { get; private set; } = null!;

        private readonly List<EquipmentData> _inventory = [];
        private readonly Dictionary<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> _heroEquipment = [];

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
            GD.Print("[InventoryManager] Initialized");
        }

        /// <summary>
        /// Добавляет Items.
        /// </summary>
        public void AddItems(List<EquipmentData> items)
        {
            foreach (EquipmentData item in items)
            {
                _inventory.Add(item);
                GD.Print($"[InventoryManager] Added: {item.Name}");
            }
        }

        /// <summary>
        /// Экипирует Item.
        /// </summary>
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
        /// Снимает экипировку Item.
        /// </summary>
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
        /// Элемент UpgradeEquipment.
        /// </summary>
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
        /// Возвращает Inventory.
        /// </summary>
        public IReadOnlyList<EquipmentData> GetInventory()
        {
            return _inventory.AsReadOnly();
        }

        /// <summary>
        /// Сохраняет ToData.
        /// </summary>
        public void SaveToData(SaveData data)
        {
            data.Inventory = [.. _inventory];
            data.HeroEquipment = new Dictionary<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>>();
            foreach (KeyValuePair<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> kvp in _heroEquipment)
            {
                data.HeroEquipment[kvp.Key] = new Dictionary<EquipmentSlot, EquipmentData>(kvp.Value);
            }
        }

        /// <summary>
        /// Загружает FromData.
        /// </summary>
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
        /// Возвращает HeroEquipment.
        /// </summary>
        public Dictionary<EquipmentSlot, EquipmentData> GetHeroEquipment(CharacterClass hero)
        {
            return _heroEquipment.TryGetValue(hero, out Dictionary<EquipmentSlot, EquipmentData> equipment) ? equipment : [];
        }

        /// <summary>
        /// Возвращает EquipmentByRarity.
        /// </summary>
        public List<EquipmentData> GetEquipmentByRarity(EquipmentRarity rarity)
        {
            return [.. _inventory.Where(item => item.Rarity == rarity)];
        }

        /// <summary>
        /// Возвращает EquipmentBySlot.
        /// </summary>
        public List<EquipmentData> GetEquipmentBySlot(EquipmentSlot slot)
        {
            return [.. _inventory.Where(item => item.Slot == slot)];
        }
    }
}
