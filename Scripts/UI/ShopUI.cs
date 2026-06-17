using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Пользовательский интерфейс внутриигрового магазина. Позволяет игроку тратить монеты на улучшение характеристик героев между волнами.
    /// </summary>
    public partial class ShopUI : Control
    {
        private Label _waveLabel;
        private Label _coinsLabel;
        private Button _continueButton;
        private Button _homeButton;

        private Button _mageHealthButton;
        private Button _mageDamageButton;
        private Button _mageDefenseButton;

        private Button _warriorHealthButton;
        private Button _warriorDamageButton;
        private Button _warriorDefenseButton;

        private Label _mageHealthLabel;
        private Label _mageDamageLabel;
        private Label _mageDefenseLabel;
        private Label _warriorHealthLabel;
        private Label _warriorDamageLabel;
        private Label _warriorDefenseLabel;

        private int _mageHealthPrice;
        private int _mageDamagePrice;
        private int _mageDefensePrice;
        private int _warriorHealthPrice;
        private int _warriorDamagePrice;
        private int _warriorDefensePrice;

        private int _mageHealthUpgrade;
        private int _mageDamageUpgrade;
        private int _mageDefenseUpgrade;
        private int _warriorHealthUpgrade;
        private int _warriorDamageUpgrade;
        private int _warriorDefenseUpgrade;

        /// <summary>
        /// Вызывается при готовности узла. Настраивает все текстовые метки и кнопки для каждого типа улучшений, подписывается на события покупки и инициализирует данные магазина.
        /// </summary>
        public override void _Ready()
        {
            _waveLabel = GetNode<Label>("MarginContainer/VBoxContainer/Header/WaveLabel");
            _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/Header/CoinsLabel");
            _continueButton = GetNode<Button>("MarginContainer/VBoxContainer/BottomRow/ContinueButton");
            _homeButton = GetNode<Button>("MarginContainer/VBoxContainer/BottomRow/HomeButton");

            _mageHealthButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/HealthUpgrade/BuyButton");
            _mageDamageButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DamageUpgrade/BuyButton");
            _mageDefenseButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DefenseUpgrade/BuyButton");

            _warriorHealthButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/HealthUpgrade/BuyButton");
            _warriorDamageButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DamageUpgrade/BuyButton");
            _warriorDefenseButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DefenseUpgrade/BuyButton");

            _mageHealthLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/HealthUpgrade/PriceLabel");
            _mageDamageLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DamageUpgrade/PriceLabel");
            _mageDefenseLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DefenseUpgrade/PriceLabel");
            _warriorHealthLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/HealthUpgrade/PriceLabel");
            _warriorDamageLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DamageUpgrade/PriceLabel");
            _warriorDefenseLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DefenseUpgrade/PriceLabel");

            _mageHealthButton.Pressed += () => OnUpgradePurchased(UpgradeType.MageHealth);
            _mageDamageButton.Pressed += () => OnUpgradePurchased(UpgradeType.MageDamage);
            _mageDefenseButton.Pressed += () => OnUpgradePurchased(UpgradeType.MageDefense);
            _warriorHealthButton.Pressed += () => OnUpgradePurchased(UpgradeType.WarriorHealth);
            _warriorDamageButton.Pressed += () => OnUpgradePurchased(UpgradeType.WarriorDamage);
            _warriorDefenseButton.Pressed += () => OnUpgradePurchased(UpgradeType.WarriorDefense);
            _continueButton.Pressed += OnContinuePressed;
            _homeButton.Pressed += OnHomePressed;

            _continueButton.Text = Tr("UI_PAUSE_RESUME"); // Can just use continue/resume
            _homeButton.Text = Tr("UI_PAUSE_MAIN_MENU");

            GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/MageTitle").Text = Tr("UI_BATTLE_ALTARION") + " (" + Tr("CHARACTER_MAGE") + ")";
            GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/WarriorTitle").Text = Tr("UI_BATTLE_ALDRIC") + " (" + Tr("CHARACTER_WARRIOR") + ")";

            GetNode<Label>("MarginContainer/VBoxContainer/Header/ShopTitleLabel").Text = Tr("UI_SHOP_TITLE");

            InitializeShop();

            GD.Print("[ShopUI] Initialized");
        }

        private void InitializeShop()
        {
            int currentWave = GameStateManager.Instance.CurrentWave;
            int completedWave = Mathf.Max(1, currentWave - 1);
            int coins = GameStateManager.Instance.Coins;

            _waveLabel.Text = $"{Tr("UI_SHOP_NEXT_WAVE")}: {currentWave}";
            _coinsLabel.Text = $"{coins}";

            CalculatePrices(currentWave);
            UpdateShopDisplay();

            GD.Print($"[ShopUI] Shop opened after Wave {completedWave}");
        }

        private void CalculatePrices(int wave)
        {
            const float scaleFactor = GameConstants.SHOP_WAVE_SCALE_FACTOR;
            int basePrice = Mathf.CeilToInt(10 * (1 + (wave * 0.5f)));

            _mageHealthPrice = Mathf.CeilToInt(basePrice * scaleFactor * 1.2f);
            _warriorHealthPrice = Mathf.CeilToInt(basePrice * scaleFactor * 1.2f);
            _mageDamagePrice = Mathf.CeilToInt(basePrice * scaleFactor);
            _warriorDamagePrice = Mathf.CeilToInt(basePrice * scaleFactor);
            _mageDefensePrice = Mathf.CeilToInt(basePrice * scaleFactor * 0.8f);
            _warriorDefensePrice = Mathf.CeilToInt(basePrice * scaleFactor * 0.8f);

            _mageHealthUpgrade = GD.RandRange(GameConstants.SHOP_UPGRADE_HP_MIN, GameConstants.SHOP_UPGRADE_HP_MAX);
            _mageDamageUpgrade = GD.RandRange(GameConstants.SHOP_UPGRADE_DAMAGE_MIN, GameConstants.SHOP_UPGRADE_DAMAGE_MAX);
            _mageDefenseUpgrade = GD.RandRange(GameConstants.SHOP_UPGRADE_DEFENSE_MIN, GameConstants.SHOP_UPGRADE_DEFENSE_MAX);
            _warriorHealthUpgrade = GD.RandRange(GameConstants.SHOP_UPGRADE_HP_MIN, GameConstants.SHOP_UPGRADE_HP_MAX);
            _warriorDamageUpgrade = GD.RandRange(GameConstants.SHOP_UPGRADE_DAMAGE_MIN, GameConstants.SHOP_UPGRADE_DAMAGE_MAX);
            _warriorDefenseUpgrade = GD.RandRange(GameConstants.SHOP_UPGRADE_DEFENSE_MIN, GameConstants.SHOP_UPGRADE_DEFENSE_MAX);
        }

        private void UpdateShopDisplay()
        {
            int coins = GameStateManager.Instance.Coins;
            SaveData saveData = GameStateManager.Instance.CurrentSave;
            if (saveData == null)
            {
                return;
            }

            UpdateUpgradeButton(_mageHealthButton, _mageHealthLabel, _mageHealthPrice, coins, saveData.MageMaxHealth, _mageHealthUpgrade, Tr("UI_SHOP_HP"));
            UpdateUpgradeButton(_mageDamageButton, _mageDamageLabel, _mageDamagePrice, coins, saveData.MageDamage, _mageDamageUpgrade, Tr("UI_SHOP_DMG"));
            UpdateUpgradeButton(_mageDefenseButton, _mageDefenseLabel, _mageDefensePrice, coins, saveData.MageDefense, _mageDefenseUpgrade, Tr("UI_SHOP_DEF"));
            UpdateUpgradeButton(_warriorHealthButton, _warriorHealthLabel, _warriorHealthPrice, coins, saveData.WarriorMaxHealth, _warriorHealthUpgrade, Tr("UI_SHOP_HP"));
            UpdateUpgradeButton(_warriorDamageButton, _warriorDamageLabel, _warriorDamagePrice, coins, saveData.WarriorDamage, _warriorDamageUpgrade, Tr("UI_SHOP_DMG"));
            UpdateUpgradeButton(_warriorDefenseButton, _warriorDefenseLabel, _warriorDefensePrice, coins, saveData.WarriorDefense, _warriorDefenseUpgrade, Tr("UI_SHOP_DEF"));
        }

        private static void UpdateUpgradeButton(Button button, Label priceLabel, int price,
            int currentCoins, int currentStat, int upgradeAmount, string statName)
        {
            bool canAfford = currentCoins >= price;
            button.Disabled = !canAfford;
            button.Modulate = canAfford ? Colors.White : new Color(1, 1, 1, 0.4f);

            int newStat = currentStat + upgradeAmount;
            priceLabel.Text = $"{currentStat} -> {newStat} {statName}\n{button.GetNode<Control>("/root").Tr("UI_SHOP_COST")}: {price}";
            priceLabel.Modulate = canAfford ? Colors.White : Colors.Gray;
        }

        private void OnUpgradePurchased(UpgradeType upgradeType)
        {
            int price = GetUpgradePrice(upgradeType);

            if (!GameStateManager.Instance.SpendCoins(price))
            {
                GD.Print($"[ShopUI] Cannot afford {upgradeType}");
                Button btn = GetUpgradeButton(upgradeType);
                if (btn != null)
                {
                    ShakeButton(btn);
                }

                return;
            }

            ApplyUpgrade(upgradeType);
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));

            Button purchased = GetUpgradeButton(upgradeType);
            if (purchased != null)
            {
                PulseButton(purchased);
            }

            _coinsLabel.Text = $"{GameStateManager.Instance.Coins}";
            UpdateShopDisplay();

            GD.Print($"[ShopUI] Purchased {upgradeType} for {price} coins");
        }

        private Button GetUpgradeButton(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.MageHealth => _mageHealthButton,
                UpgradeType.MageDamage => _mageDamageButton,
                UpgradeType.MageDefense => _mageDefenseButton,
                UpgradeType.WarriorHealth => _warriorHealthButton,
                UpgradeType.WarriorDamage => _warriorDamageButton,
                UpgradeType.WarriorDefense => _warriorDefenseButton,
                _ => null
            };
        }

        private void ShakeButton(Button button)
        {
            Vector2 originalPos = button.Position;
            Tween tween = CreateTween();
            _ = tween.TweenProperty(button, "position:x", originalPos.X + 5, 0.05f);
            _ = tween.TweenProperty(button, "position:x", originalPos.X - 5, 0.05f);
            _ = tween.TweenProperty(button, "position:x", originalPos.X, 0.05f);
        }

        private void PulseButton(Button button)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(button, "scale", new Vector2(1.1f, 1.1f), 0.1f);
            _ = tween.TweenProperty(button, "scale", Vector2.One, 0.1f);
        }

        private int GetUpgradePrice(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.MageHealth => _mageHealthPrice,
                UpgradeType.MageDamage => _mageDamagePrice,
                UpgradeType.MageDefense => _mageDefensePrice,
                UpgradeType.WarriorHealth => _warriorHealthPrice,
                UpgradeType.WarriorDamage => _warriorDamagePrice,
                UpgradeType.WarriorDefense => _warriorDefensePrice,
                _ => 0
            };
        }

        private void ApplyUpgrade(UpgradeType type)
        {
            SaveData saveData = GameStateManager.Instance.CurrentSave;
            if (saveData == null)
            {
                return;
            }

            switch (type)
            {
                case UpgradeType.MageHealth:
                    saveData.MageMaxHealth += _mageHealthUpgrade;
                    saveData.MageHealth += _mageHealthUpgrade;
                    break;
                case UpgradeType.MageDamage:
                    saveData.MageDamage += _mageDamageUpgrade;
                    break;
                case UpgradeType.MageDefense:
                    saveData.MageDefense += _mageDefenseUpgrade;
                    break;
                case UpgradeType.WarriorHealth:
                    saveData.WarriorMaxHealth += _warriorHealthUpgrade;
                    saveData.WarriorHealth += _warriorHealthUpgrade;
                    break;
                case UpgradeType.WarriorDamage:
                    saveData.WarriorDamage += _warriorDamageUpgrade;
                    break;
                case UpgradeType.WarriorDefense:
                    saveData.WarriorDefense += _warriorDefenseUpgrade;
                    break;
            }

            _ = GameStateManager.Instance.EmitSignal(GameStateManager.SignalName.HeroStatsChanged);
            GD.Print($"[ShopUI] Applied upgrade: {type}");
        }

        private void OnContinuePressed()
        {
            GD.Print("[ShopUI] Continue to next wave");
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
            _ = SaveSystem.Instance.SaveGame();
            SceneManager.ReturnToBattle();
        }

        private void OnHomePressed()
        {
            GD.Print("[ShopUI] Return to main menu");
            _ = (AudioManager.Instance?.TryPlaySfx("res://Resources/Audio/SFX/button_click.wav"));
            _ = SaveSystem.Instance.SaveGame();
            SceneManager.GoToMainMenu();
        }

        private enum UpgradeType
        {
            MageHealth, MageDamage, MageDefense,
            WarriorHealth, WarriorDamage, WarriorDefense
        }
    }
}
