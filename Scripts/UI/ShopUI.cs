using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Shop UI controller for purchasing permanent upgrades between waves.
    /// </summary>
    public partial class ShopUI : Control
    {
        // UI References
        private Label _waveLabel;
        private Label _coinsLabel;
        private Button _continueButton;

        // Mage upgrade buttons
        private Button _mageHealthButton;
        private Button _mageDamageButton;
        private Button _mageDefenseButton;

        // Warrior upgrade buttons
        private Button _warriorHealthButton;
        private Button _warriorDamageButton;
        private Button _warriorDefenseButton;

        // Upgrade labels (showing prices)
        private Label _mageHealthLabel;
        private Label _mageDamageLabel;
        private Label _mageDefenseLabel;
        private Label _warriorHealthLabel;
        private Label _warriorDamageLabel;
        private Label _warriorDefenseLabel;

        // Current prices
        private int _mageHealthPrice;
        private int _mageDamagePrice;
        private int _mageDefensePrice;
        private int _warriorHealthPrice;
        private int _warriorDamagePrice;
        private int _warriorDefensePrice;

        public override void _Ready()
        {
            // Get UI elements
            _waveLabel = GetNode<Label>("MarginContainer/VBoxContainer/Header/WaveLabel");
            _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/Header/CoinsLabel");
            _continueButton = GetNode<Button>("MarginContainer/VBoxContainer/ContinueButton");

            // Get Mage upgrade buttons
            _mageHealthButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/HealthUpgrade/BuyButton");
            _mageDamageButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DamageUpgrade/BuyButton");
            _mageDefenseButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DefenseUpgrade/BuyButton");

            // Get Warrior upgrade buttons
            _warriorHealthButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/HealthUpgrade/BuyButton");
            _warriorDamageButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DamageUpgrade/BuyButton");
            _warriorDefenseButton = GetNode<Button>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DefenseUpgrade/BuyButton");

            // Get upgrade labels
            _mageHealthLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/HealthUpgrade/PriceLabel");
            _mageDamageLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DamageUpgrade/PriceLabel");
            _mageDefenseLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DefenseUpgrade/PriceLabel");
            _warriorHealthLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/HealthUpgrade/PriceLabel");
            _warriorDamageLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DamageUpgrade/PriceLabel");
            _warriorDefenseLabel = GetNode<Label>("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DefenseUpgrade/PriceLabel");

            // Connect signals
            _mageHealthButton.Pressed += () => OnUpgradePurchased(UpgradeType.MageHealth);
            _mageDamageButton.Pressed += () => OnUpgradePurchased(UpgradeType.MageDamage);
            _mageDefenseButton.Pressed += () => OnUpgradePurchased(UpgradeType.MageDefense);
            _warriorHealthButton.Pressed += () => OnUpgradePurchased(UpgradeType.WarriorHealth);
            _warriorDamageButton.Pressed += () => OnUpgradePurchased(UpgradeType.WarriorDamage);
            _warriorDefenseButton.Pressed += () => OnUpgradePurchased(UpgradeType.WarriorDefense);
            _continueButton.Pressed += OnContinuePressed;

            // Initialize shop
            InitializeShop();

            GD.Print("[ShopUI] Initialized");
        }

        /// <summary>
        /// Initializes shop with current wave and prices.
        /// </summary>
        private void InitializeShop()
        {
            int currentWave = GameStateManager.Instance.CurrentWave;
            int coins = GameStateManager.Instance.Coins;

            // Update display
            _waveLabel.Text = $"Wave {currentWave} Complete!";
            _coinsLabel.Text = $"💰 {coins}";

            // Calculate prices based on wave
            CalculatePrices(currentWave);

            // Update price labels and button states
            UpdateShopDisplay();

            GD.Print($"[ShopUI] Shop opened for Wave {currentWave}");
        }

        /// <summary>
        /// Calculates upgrade prices based on current wave.
        /// </summary>
        private void CalculatePrices(int wave)
        {
            float scaleFactor = GameConstants.SHOP_WAVE_SCALE_FACTOR;

            // Base price increases with wave number
            int basePrice = Mathf.CeilToInt(10 * (1 + (wave * 0.5f)));

            // Health upgrades (more expensive)
            _mageHealthPrice = Mathf.CeilToInt(basePrice * scaleFactor * 1.2f);
            _warriorHealthPrice = Mathf.CeilToInt(basePrice * scaleFactor * 1.2f);

            // Damage upgrades (medium price)
            _mageDamagePrice = Mathf.CeilToInt(basePrice * scaleFactor);
            _warriorDamagePrice = Mathf.CeilToInt(basePrice * scaleFactor);

            // Defense upgrades (cheaper)
            _mageDefensePrice = Mathf.CeilToInt(basePrice * scaleFactor * 0.8f);
            _warriorDefensePrice = Mathf.CeilToInt(basePrice * scaleFactor * 0.8f);
        }

        /// <summary>
        /// Updates shop display with current prices and availability.
        /// </summary>
        private void UpdateShopDisplay()
        {
            int coins = GameStateManager.Instance.Coins;

            // Update Mage upgrades
            UpdateUpgradeButton(_mageHealthButton, _mageHealthLabel, _mageHealthPrice, coins,
                $"+{GetHealthUpgradeAmount()} Max HP");
            UpdateUpgradeButton(_mageDamageButton, _mageDamageLabel, _mageDamagePrice, coins,
                $"+{GetDamageUpgradeAmount()} Damage");
            UpdateUpgradeButton(_mageDefenseButton, _mageDefenseLabel, _mageDefensePrice, coins,
                $"+{GetDefenseUpgradeAmount()} Defense");

            // Update Warrior upgrades
            UpdateUpgradeButton(_warriorHealthButton, _warriorHealthLabel, _warriorHealthPrice, coins,
                $"+{GetHealthUpgradeAmount()} Max HP");
            UpdateUpgradeButton(_warriorDamageButton, _warriorDamageLabel, _warriorDamagePrice, coins,
                $"+{GetDamageUpgradeAmount()} Damage");
            UpdateUpgradeButton(_warriorDefenseButton, _warriorDefenseLabel, _warriorDefensePrice, coins,
                $"+{GetDefenseUpgradeAmount()} Defense");
        }

        /// <summary>
        /// Updates a single upgrade button's state and label.
        /// </summary>
        private static void UpdateUpgradeButton(Button button, Label priceLabel, int price, int currentCoins, string upgradeText)
        {
            bool canAfford = currentCoins >= price;
            button.Disabled = !canAfford;
            priceLabel.Text = $"{upgradeText}\n💰 {price}";

            // Visual feedback
            priceLabel.Modulate = canAfford ? Colors.White : Colors.Gray;
        }

        /// <summary>
        /// Gets random health upgrade amount.
        /// </summary>
        private static int GetHealthUpgradeAmount()
        {
            return GD.RandRange(GameConstants.SHOP_UPGRADE_HP_MIN, GameConstants.SHOP_UPGRADE_HP_MAX);
        }

        /// <summary>
        /// Gets random damage upgrade amount.
        /// </summary>
        private static int GetDamageUpgradeAmount()
        {
            return GD.RandRange(GameConstants.SHOP_UPGRADE_DAMAGE_MIN, GameConstants.SHOP_UPGRADE_DAMAGE_MAX);
        }

        /// <summary>
        /// Gets random defense upgrade amount.
        /// </summary>
        private static int GetDefenseUpgradeAmount()
        {
            return GD.RandRange(GameConstants.SHOP_UPGRADE_DEFENSE_MIN, GameConstants.SHOP_UPGRADE_DEFENSE_MAX);
        }

        /// <summary>
        /// Called when an upgrade is purchased.
        /// </summary>
        private void OnUpgradePurchased(UpgradeType upgradeType)
        {
            int price = GetUpgradePrice(upgradeType);

            // Check if player can afford
            if (!GameStateManager.Instance.SpendCoins(price))
            {
                GD.Print($"[ShopUI] Cannot afford {upgradeType} upgrade");
                // Play error sound or shake animation
                Button button = GetUpgradeButton(upgradeType);
                if (button != null)
                {
                    ShakeButton(button);
                }
                return;
            }

            // Apply upgrade
            ApplyUpgrade(upgradeType);

            // Play sound
            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Animate purchase success
            Button purchasedButton = GetUpgradeButton(upgradeType);
            if (purchasedButton != null)
            {
                PulseButton(purchasedButton);
            }

            // Update display
            _coinsLabel.Text = $"💰 {GameStateManager.Instance.Coins}";
            UpdateShopDisplay();

            GD.Print($"[ShopUI] Purchased {upgradeType} upgrade for {price} coins");
        }

        /// <summary>
        /// Gets button for upgrade type.
        /// </summary>
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

        /// <summary>
        /// Shakes button when purchase fails.
        /// </summary>
        private void ShakeButton(Button button)
        {
            Vector2 originalPos = button.Position;
            Tween tween = CreateTween();
            _ = tween.TweenProperty(button, "position:x", originalPos.X + 5, 0.05f);
            _ = tween.TweenProperty(button, "position:x", originalPos.X - 5, 0.05f);
            _ = tween.TweenProperty(button, "position:x", originalPos.X, 0.05f);
        }

        /// <summary>
        /// Pulses button on successful purchase.
        /// </summary>
        private void PulseButton(Button button)
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(button, "scale", new Vector2(1.1f, 1.1f), 0.1f);
            _ = tween.TweenProperty(button, "scale", Vector2.One, 0.1f);
        }

        /// <summary>
        /// Gets price for specific upgrade type.
        /// </summary>
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

        /// <summary>
        /// Applies purchased upgrade to save data.
        /// </summary>
        private static void ApplyUpgrade(UpgradeType type)
        {
            SaveData saveData = GameStateManager.Instance.CurrentSave;
            if (saveData == null)
            {
                return;
            }

            switch (type)
            {
                case UpgradeType.MageHealth:
                    int mageHealthUpgrade = GetHealthUpgradeAmount();
                    saveData.MageMaxHealth += mageHealthUpgrade;
                    saveData.MageHealth += mageHealthUpgrade; // Also heal by upgrade amount
                    GD.Print($"[ShopUI] Mage Max HP +{mageHealthUpgrade}");
                    break;

                case UpgradeType.MageDamage:
                    int mageDamageUpgrade = GetDamageUpgradeAmount();
                    saveData.MageDamage += mageDamageUpgrade;
                    GD.Print($"[ShopUI] Mage Damage +{mageDamageUpgrade}");
                    break;

                case UpgradeType.MageDefense:
                    int mageDefenseUpgrade = GetDefenseUpgradeAmount();
                    saveData.MageDefense += mageDefenseUpgrade;
                    GD.Print($"[ShopUI] Mage Defense +{mageDefenseUpgrade}");
                    break;

                case UpgradeType.WarriorHealth:
                    int warriorHealthUpgrade = GetHealthUpgradeAmount();
                    saveData.WarriorMaxHealth += warriorHealthUpgrade;
                    saveData.WarriorHealth += warriorHealthUpgrade;
                    GD.Print($"[ShopUI] Warrior Max HP +{warriorHealthUpgrade}");
                    break;

                case UpgradeType.WarriorDamage:
                    int warriorDamageUpgrade = GetDamageUpgradeAmount();
                    saveData.WarriorDamage += warriorDamageUpgrade;
                    GD.Print($"[ShopUI] Warrior Damage +{warriorDamageUpgrade}");
                    break;

                case UpgradeType.WarriorDefense:
                    int warriorDefenseUpgrade = GetDefenseUpgradeAmount();
                    saveData.WarriorDefense += warriorDefenseUpgrade;
                    GD.Print($"[ShopUI] Warrior Defense +{warriorDefenseUpgrade}");
                    break;
            }

            // Emit signal for stat change
            _ = GameStateManager.Instance.EmitSignal(GameStateManager.SignalName.HeroStatsChanged);
        }

        /// <summary>
        /// Called when Continue button is pressed.
        /// </summary>
        private void OnContinuePressed()
        {
            GD.Print("[ShopUI] Continue to next wave");

            AudioManager.Instance.PlaySfx("res://Resources/Audio/SFX/button_click.wav");

            // Save game before continuing
            _ = SaveSystem.Instance.SaveGame();

            // Return to battle
            SceneManager.ReturnToBattle();
        }

        /// <summary>
        /// Upgrade types enum.
        /// </summary>
        private enum UpgradeType
        {
            MageHealth,
            MageDamage,
            MageDefense,
            WarriorHealth,
            WarriorDamage,
            WarriorDefense
        }
    }
}
