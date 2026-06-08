using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер инвентаря. Отвечает за хранение предметов, экипировку героев и улучшение снаряжения.
    /// </summary>
    public partial class InventoryManager : Node, IInventoryManager
    {
        /// <summary>
        /// Глобальный экземпляр менеджера инвентаря (паттерн Singleton).
        /// </summary>
        public static InventoryManager Instance { get; private set; } = null!;

        private readonly List<EquipmentData> _inventory = [];
        private readonly Dictionary<CharacterClass, Dictionary<EquipmentSlot, EquipmentData>> _heroEquipment = [];

        /// <summary>
        /// Инициализирует узел менеджера инвентаря при его добавлении в дерево сцены.
        /// Гарантирует существование только одного экземпляра.
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
        /// Добавляет список предметов в общий инвентарь игрока.
        /// </summary>
        /// <param name="items">Список предметов для добавления.</param>
        public void AddItems(List<EquipmentData> items)
        {
            foreach (EquipmentData item in items)
            {
                _inventory.Add(item);
                GD.Print($"[InventoryManager] Added: {item.Name}");
            }
        }

        /// <summary>
        /// Экипирует указанный предмет на выбранного героя. Если слот уже занят, старый предмет снимается и возвращается в инвентарь.
        /// </summary>
        /// <param name="hero">Класс героя, на которого надевается предмет.</param>
        /// <param name="item">Предмет экипировки для надевания.</param>
        /// <returns><c>true</c>, если предмет успешно экипирован.</returns>
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
        /// Снимает предмет экипировки из указанного слота у выбранного героя и возвращает его в общий инвентарь.
        /// </summary>
        /// <param name="hero">Класс героя, с которого снимается предмет.</param>
        /// <param name="slot">Слот экипировки, который нужно освободить.</param>
        /// <returns>Снятый предмет, или <c>null</c>, если слот был пуст.</returns>
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
        /// Улучшает предмет экипировки за монеты, если хватает средств.
        /// </summary>
        /// <param name="item">Предмет экипировки для улучшения.</param>
        /// <returns><c>true</c>, если предмет успешно улучшен; иначе <c>false</c> (недостаточно средств или максимальный уровень).</returns>
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
        /// Возвращает список всех предметов, находящихся в инвентаре игрока (не экипированных).
        /// </summary>
        /// <returns>Список предметов инвентаря только для чтения.</returns>
        public IReadOnlyList<EquipmentData> GetInventory()
        {
            return _inventory.AsReadOnly();
        }

        /// <summary>
        /// Сохраняет текущее состояние инвентаря и экипировки героев в объект сохранения данных.
        /// </summary>
        /// <param name="data">Объект сохранения данных.</param>
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
        /// Загружает состояние инвентаря и экипировки героев из объекта сохранения данных.
        /// </summary>
        /// <param name="data">Объект сохранения данных, из которого загружается состояние.</param>
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
        /// Получает всю экипировку, надетую на указанного героя.
        /// </summary>
        /// <param name="hero">Класс героя.</param>
        /// <returns>Словарь, в котором ключи — это слоты экипировки, а значения — надетые предметы.</returns>
        public Dictionary<EquipmentSlot, EquipmentData> GetHeroEquipment(CharacterClass hero)
        {
            return _heroEquipment.TryGetValue(hero, out Dictionary<EquipmentSlot, EquipmentData> equipment) ? equipment : [];
        }

        /// <summary>
        /// Получает список предметов из инвентаря, отфильтрованный по их редкости.
        /// </summary>
        /// <param name="rarity">Редкость предметов для фильтрации.</param>
        /// <returns>Список предметов указанной редкости.</returns>
        public List<EquipmentData> GetEquipmentByRarity(EquipmentRarity rarity)
        {
            return [.. _inventory.Where(item => item.Rarity == rarity)];
        }

        /// <summary>
        /// Получает список предметов из инвентаря, отфильтрованный по слоту экипировки, для которого они предназначены.
        /// </summary>
        /// <param name="slot">Слот экипировки для фильтрации.</param>
        /// <returns>Список предметов для указанного слота.</returns>
        public List<EquipmentData> GetEquipmentBySlot(EquipmentSlot slot)
        {
            return [.. _inventory.Where(item => item.Slot == slot)];
        }
    }
}
