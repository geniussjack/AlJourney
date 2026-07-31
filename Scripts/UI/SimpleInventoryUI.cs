using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Simplified inventory UI. Shows the item list, current equipment, and details for the selected item.
    /// </summary>
    public partial class SimpleInventoryUI : Control
    {
        private Label _coinsLabel;
        private Button _closeButton;
        private HBoxContainer _contentHBox;

        private TextureButton _heroToggleBtn;
        private Label _heroNameLabel;

        private TextureRect _weaponIcon;
        private Label _weaponNameLabel;
        private Label _weaponStatsLabel;
        private Label _weaponDescLabel;

        private Button _prevWeaponBtn;
        private Button _nextWeaponBtn;
        private Button _upgradeBtn;

        private CharacterClass _selectedHero = CharacterClass.Mage;
        private readonly List<EquipmentData> _availableWeapons = [];
        private int _selectedWeaponIndex = 0;

        public override void _Ready()
        {
            _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/Header/CoinsLabel");
            _closeButton = GetNode<Button>("MarginContainer/VBoxContainer/Header/CloseButton");
            _closeButton.Text = Tr("UI_CLOSE");
            _contentHBox = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/ContentHBox");

            _closeButton.Pressed += OnClosePressed;

            // Clear old UI
            foreach (Node child in _contentHBox.GetChildren())
            {
                child.QueueFree();
            }

            BuildNewUI();
            LoadHeroData();
        }

        public override void _Process(double delta)
        {
            if (Engine.GetPhysicsFrames() % 60 == 0)
            {
                _coinsLabel.Text = $"{Tr("UI_COINS")}: {GameStateManager.Instance.Coins}";
            }
        }

        private void BuildNewUI()
        {
            VBoxContainer mainVBox = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill, Alignment = BoxContainer.AlignmentMode.Center };
            _contentHBox.AddChild(mainVBox);

            // Hero Toggle
            _heroNameLabel = new Label() { HorizontalAlignment = HorizontalAlignment.Center };
            mainVBox.AddChild(_heroNameLabel);

            _heroToggleBtn = new TextureButton()
            {
                StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(48, 48),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter
            };
            _heroToggleBtn.Pressed += OnHeroToggled;
            mainVBox.AddChild(_heroToggleBtn);

            // Spacer
            mainVBox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });

            Label equipmentTitle = new() { Text = Tr("UI_EQUIPMENT"), HorizontalAlignment = HorizontalAlignment.Center };
            mainVBox.AddChild(equipmentTitle);

            // Weapon Selector
            HBoxContainer weaponHBox = new() { Alignment = BoxContainer.AlignmentMode.Center };
            mainVBox.AddChild(weaponHBox);

            _prevWeaponBtn = new Button() { Text = "<" };
            _prevWeaponBtn.Pressed += () => CycleWeapon(-1);
            weaponHBox.AddChild(_prevWeaponBtn);

            _weaponIcon = new TextureRect()
            {
                ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(64, 64)
            };
            weaponHBox.AddChild(_weaponIcon);

            _nextWeaponBtn = new Button() { Text = ">" };
            _nextWeaponBtn.Pressed += () => CycleWeapon(1);
            weaponHBox.AddChild(_nextWeaponBtn);

            _weaponNameLabel = new Label() { HorizontalAlignment = HorizontalAlignment.Center };
            mainVBox.AddChild(_weaponNameLabel);

            _weaponStatsLabel = new Label() { HorizontalAlignment = HorizontalAlignment.Center };
            mainVBox.AddChild(_weaponStatsLabel);

            _weaponDescLabel = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.Word,
                CustomMinimumSize = new Vector2(250, 0)
            };
            mainVBox.AddChild(_weaponDescLabel);

            // Spacer
            mainVBox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });

            _upgradeBtn = new Button() { Text = Tr("UI_UPGRADE") };
            _upgradeBtn.Pressed += OnUpgradePressed;
            mainVBox.AddChild(_upgradeBtn);
        }

        private void LoadHeroData()
        {
            _heroNameLabel.Text = _selectedHero == CharacterClass.Mage ? Tr("HERO_MAGE") : Tr("HERO_WARRIOR");
            string portraitPath = _selectedHero == CharacterClass.Mage
                ? "res://Resources/Sprites/Characters/mage_sprite.png"
                : "res://Resources/Sprites/Characters/warrior_sprite.png";

            _heroToggleBtn.TextureNormal = ResourceLoader.Exists(portraitPath) ? GD.Load<Texture2D>(portraitPath) : null;

            _availableWeapons.Clear();
            IReadOnlyList<EquipmentData> allItems = InventoryManager.Instance.GetInventory();
            foreach (EquipmentData item in allItems)
            {
                if (item.Slot == EquipmentSlot.Weapon &&
                    ((_selectedHero == CharacterClass.Mage && (item.Id.Contains("ball") || item.Id == "staff")) ||
                     (_selectedHero == CharacterClass.Warrior && (item.Id == "sword" || item.Id == "axe" || item.Id == "spear"))))
                {
                    _availableWeapons.Add(item);
                }
            }

            EquipmentData currentWeapon = InventoryManager.Instance.GetEquippedItem(_selectedHero, EquipmentSlot.Weapon);
            _selectedWeaponIndex = 0;
            for (int i = 0; i < _availableWeapons.Count; i++)
            {
                if (currentWeapon != null && _availableWeapons[i].Id == currentWeapon.Id)
                {
                    _selectedWeaponIndex = i;
                    break;
                }
            }

            UpdateWeaponDisplay();
        }

        private void CycleWeapon(int direction)
        {
            if (_availableWeapons.Count == 0)
            {
                return;
            }

            _selectedWeaponIndex += direction;
            if (_selectedWeaponIndex < 0)
            {
                _selectedWeaponIndex = _availableWeapons.Count - 1;
            }

            if (_selectedWeaponIndex >= _availableWeapons.Count)
            {
                _selectedWeaponIndex = 0;
            }

            EquipmentData newWeapon = _availableWeapons[_selectedWeaponIndex];
            _ = InventoryManager.Instance.EquipItem(_selectedHero, newWeapon);
            UpdateWeaponDisplay();
        }

        private void UpdateWeaponDisplay()
        {
            if (_availableWeapons.Count == 0 || _selectedWeaponIndex < 0 || _selectedWeaponIndex >= _availableWeapons.Count)
            {
                _weaponNameLabel.Text = "Нет оружия";
                _weaponStatsLabel.Text = "";
                _weaponDescLabel.Text = "";
                _weaponIcon.Texture = null;
                _upgradeBtn.Disabled = true;
                return;
            }

            EquipmentData weapon = _availableWeapons[_selectedWeaponIndex];
            _weaponNameLabel.Text = $"{Tr(weapon.Name)} (Ур. {weapon.CurrentLevel})";
            _weaponNameLabel.Modulate = weapon.GetRarityColor();

            string stats = "";
            foreach (KeyValuePair<string, int> stat in weapon.BaseStats)
            {
                string statName = Tr($"STAT_{stat.Key.ToUpper()}");
                stats += $"{statName}: {stat.Value}\n";
            }
            _weaponStatsLabel.Text = stats;
            _weaponDescLabel.Text = Tr(weapon.DescriptionKey);

            string iconPath = $"res://Resources/Sprites/Elements/{weapon.Id}_sprite.png";
            if (weapon.Id == "fireball")
            {
                iconPath = "res://Resources/Sprites/Elements/fireball_sprite.png";
            }
            else if (weapon.Id == "iceball")
            {
                iconPath = "res://Resources/Sprites/Elements/iceball_sprite.png";
            }
            else if (weapon.Id == "electroball")
            {
                iconPath = "res://Resources/Sprites/Elements/electroball_sprite.png";
            }
            else if (weapon.Id == "sword")
            {
                iconPath = "res://Resources/Sprites/Elements/sword_icon.png";
            }
            else if (weapon.Id == "axe")
            {
                iconPath = "res://Resources/Sprites/Elements/axe_sprite.png";
            }
            else if (weapon.Id == "spear")
            {
                iconPath = "res://Resources/Sprites/Elements/spear_sprite.png";
            }

            _weaponIcon.Texture = ResourceLoader.Exists(iconPath) ? GD.Load<Texture2D>(iconPath) : null;

            int cost = weapon.GetUpgradeCost(GameStateManager.Instance.CurrentWave);
            _upgradeBtn.Text = $"{Tr("UI_UPGRADE")} ({cost} {Tr("UI_COINS").ToLower()})";
            _upgradeBtn.Disabled = GameStateManager.Instance.Coins < cost;
            _coinsLabel.Text = $"{Tr("UI_COINS")}: {GameStateManager.Instance.Coins}";
        }

        private void OnHeroToggled()
        {
            _selectedHero = _selectedHero == CharacterClass.Mage ? CharacterClass.Warrior : CharacterClass.Mage;
            LoadHeroData();
        }

        private void OnUpgradePressed()
        {
            if (_availableWeapons.Count == 0)
            {
                return;
            }

            EquipmentData weapon = _availableWeapons[_selectedWeaponIndex];

            if (InventoryManager.Instance.UpgradeEquipment(weapon))
            {
                LoadHeroData();
            }
        }

        private void OnClosePressed()
        {
            QueueFree();
        }
    }
}
