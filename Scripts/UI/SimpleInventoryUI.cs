using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Упрощенный пользовательский интерфейс инвентаря. Отображает список доступных предметов, текущую экипировку героев, а также предоставляет функционал для просмотра характеристик и улучшения предметов.
    /// Работает в связке с InventoryUI.tscn.
    /// </summary>
    public partial class SimpleInventoryUI : Control
    {
        private VBoxContainer _inventoryContainer;
        private VBoxContainer _equipmentContainer;
        private Label _coinsLabel;
        private Label _itemDetailsLabel;
        private Button _closeButton;
        private Button _upgradeButton;

        private EquipmentData _selectedItem;
        private CharacterClass _selectedHero = CharacterClass.Mage;

        /// <summary>
        /// Вызывается при инициализации узла. Настраивает элементы интерфейса (контейнеры инвентаря и экипировки, метки), подписывается на нажатия кнопок и обновляет отображение данных.
        /// </summary>
        public override void _Ready()
        {
            _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/Header/CoinsLabel");
            _closeButton = GetNode<Button>("MarginContainer/VBoxContainer/Header/CloseButton");
            
            _inventoryContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ContentHBox/InventorySection/ScrollContainer/InventoryContainer");
            _equipmentContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ContentHBox/EquipmentSection/ScrollContainer/EquipmentContainer");
            
            _itemDetailsLabel = GetNode<Label>("MarginContainer/VBoxContainer/ContentHBox/DetailsSection/ItemDetailsLabel");
            _upgradeButton = GetNode<Button>("MarginContainer/VBoxContainer/ContentHBox/DetailsSection/UpgradeButton");

            _closeButton.Pressed += OnClosePressed;
            _upgradeButton.Pressed += OnUpgradePressed;

            RefreshUI();
        }

        private void RefreshUI()
        {
            _coinsLabel.Text = $"Монеты: {GameStateManager.Instance.Coins}";

            foreach (Node child in _inventoryContainer.GetChildren())
            {
                child.QueueFree();
            }

            IReadOnlyList<EquipmentData> inventory = InventoryManager.Instance.GetInventory();
            foreach (EquipmentData item in inventory)
            {
                Button itemButton = new Button
                {
                    Text = $"{item.Name} (Ур. {item.CurrentLevel})",
                    Modulate = item.GetRarityColor()
                };
                _inventoryContainer.AddChild(itemButton);
                
                EquipmentData currentItem = item;
                itemButton.Pressed += () => OnItemSelected(currentItem);
            }

            foreach (Node child in _equipmentContainer.GetChildren())
            {
                child.QueueFree();
            }

            Dictionary<EquipmentSlot, EquipmentData> equipment = InventoryManager.Instance.GetHeroEquipment(_selectedHero);
            foreach (KeyValuePair<EquipmentSlot, EquipmentData> kvp in equipment)
            {
                Label slotLabel = new Label
                {
                    Text = $"{kvp.Key}: {kvp.Value.Name}",
                    Modulate = kvp.Value.GetRarityColor()
                };
                _equipmentContainer.AddChild(slotLabel);
            }
            
            UpdateDetailsPanel();
        }

        private void UpdateDetailsPanel()
        {
            if (_selectedItem == null)
            {
                _itemDetailsLabel.Text = "Выберите предмет для просмотра деталей.";
                _upgradeButton.Disabled = true;
                return;
            }

            _itemDetailsLabel.Text = $"{_selectedItem.Name}\nРедкость: {_selectedItem.Rarity}\nУровень: {_selectedItem.CurrentLevel}";
            _upgradeButton.Disabled = false;
        }

        private void OnItemSelected(EquipmentData item)
        {
            _selectedItem = item;
            GD.Print($"[SimpleInventoryUI] Selected item: {item.Name}");
            UpdateDetailsPanel();
        }

        private void OnUpgradePressed()
        {
            if (_selectedItem == null)
            {
                GD.Print("[SimpleInventoryUI] No item selected for upgrade");
                return;
            }

            bool success = InventoryManager.Instance.UpgradeEquipment(_selectedItem);
            if (success)
            {
                GD.Print($"[SimpleInventoryUI] Successfully upgraded {_selectedItem.Name}");
                RefreshUI();
            }
        }

        private void OnClosePressed()
        {
            GD.Print("[SimpleInventoryUI] Closing inventory");
            QueueFree();
        }

        /// <summary>
        /// Вызывается каждый кадр. Выполняет периодическое обновление интерфейса инвентаря (раз в 60 физических кадров).
        /// </summary>
        /// <param name="delta">Время, прошедшее с предыдущего кадра.</param>
        public override void _Process(double delta)
        {
            if (Engine.GetPhysicsFrames() % 60 == 0) 
            {
                RefreshUI();
            }
        }
    }
}
