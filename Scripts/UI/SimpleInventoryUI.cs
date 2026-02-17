using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Simple inventory UI for equipment management.
    /// </summary>
    public partial class SimpleInventoryUI : Control
    {
        // UI Elements
        private VBoxContainer _inventoryContainer;
        private VBoxContainer _equipmentContainer;
        private Label _coinsLabel;
        private Button _closeButton;
        private Button _upgradeButton;

        // Current selected item
        private EquipmentData _selectedItem;
        private CharacterClass _selectedHero = CharacterClass.Mage;

        public override void _Ready()
        {
            SetupUI();
            RefreshUI();
        }

        private void SetupUI()
        {
            // Main container
            VBoxContainer mainContainer = new();
            AddChild(mainContainer);
            mainContainer.Size = Size;
            mainContainer.Position = Vector2.Zero;

            // Header
            HBoxContainer header = new();
            mainContainer.AddChild(header);

            _coinsLabel = new Label();
            header.AddChild(_coinsLabel);

            _closeButton = new Button { Text = "Закрыть" };
            header.AddChild(_closeButton);
            _closeButton.Pressed += OnClosePressed;

            // Content area
            HBoxContainer contentContainer = new();
            mainContainer.AddChild(contentContainer);

            // Inventory section
            VBoxContainer inventorySection = new();
            contentContainer.AddChild(inventorySection);

            Label inventoryLabel = new() { Text = "ИНВЕНТАРЬ" };
            inventorySection.AddChild(inventoryLabel);

            _inventoryContainer = new VBoxContainer();
            inventorySection.AddChild(_inventoryContainer);

            // Equipment section
            VBoxContainer equipmentSection = new();
            contentContainer.AddChild(equipmentSection);

            Label equipmentLabel = new() { Text = "ЭКИПИРОВКА" };
            equipmentSection.AddChild(equipmentLabel);

            _equipmentContainer = new VBoxContainer();
            equipmentSection.AddChild(_equipmentContainer);

            // Item details section
            VBoxContainer detailsSection = new();
            contentContainer.AddChild(detailsSection);

            Label detailsLabel = new() { Text = "ДЕТАЛИ ПРЕДМЕТА" };
            detailsSection.AddChild(detailsLabel);

            _upgradeButton = new Button { Text = "Прокачать" };
            detailsSection.AddChild(_upgradeButton);
            _upgradeButton.Pressed += OnUpgradePressed;
        }

        private void RefreshUI()
        {
            // Update coins
            _coinsLabel.Text = $"Монеты: {GameStateManager.Instance.Coins}";

            // Clear inventory
            foreach (Node child in _inventoryContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Display inventory items
            IReadOnlyList<EquipmentData> inventory = InventoryManager.Instance.GetInventory();
            foreach (EquipmentData item in inventory)
            {
                Button itemButton = new()
                {
                    Text = $"{item.Name} (Ур. {item.CurrentLevel})",
                    Modulate = item.GetRarityColor()
                };
                _inventoryContainer.AddChild(itemButton);
                itemButton.Pressed += () => OnItemSelected(item);
            }

            // Clear equipment
            foreach (Node child in _equipmentContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Display equipment for selected hero
            Dictionary<EquipmentSlot, EquipmentData> equipment = InventoryManager.Instance.GetHeroEquipment(_selectedHero);
            foreach (KeyValuePair<EquipmentSlot, EquipmentData> kvp in equipment)
            {
                Label slotLabel = new()
                {
                    Text = $"{kvp.Key}: {kvp.Value.Name}",
                    Modulate = kvp.Value.GetRarityColor()
                };
                _equipmentContainer.AddChild(slotLabel);
            }
        }

        private void OnItemSelected(EquipmentData item)
        {
            _selectedItem = item;
            GD.Print($"[SimpleInventoryUI] Selected item: {item.Name}");
            RefreshUI();
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

        public override void _Process(double delta)
        {
            // Refresh UI periodically
            if (Engine.GetPhysicsFrames() % 60 == 0) // Every second
            {
                RefreshUI();
            }
        }
    }
}
