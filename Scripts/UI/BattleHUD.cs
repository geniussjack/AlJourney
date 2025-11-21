using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Battle HUD controller for dual hero system.
    /// Displays both Mage and Warrior health, enemy health, coins, and turn information.
    /// </summary>
    public partial class BattleHUD : Control
    {
        // Mage UI elements
        private Label _mageNameLabel;
        private ProgressBar _mageHealthBar;
        private Label _mageHealthLabel;
        private Label _mageShieldLabel;

        // Warrior UI elements
        private Label _warriorNameLabel;
        private ProgressBar _warriorHealthBar;
        private Label _warriorHealthLabel;
        private Label _warriorShieldLabel;

        // Enemy UI container
        private HBoxContainer _enemiesContainer;

        // Game info
        private Label _waveLabel;
        private Label _coinsLabel;
        private Label _swapsLabel;

        // Buttons
        private Button _pauseButton;

        // References
        private DualHeroSystem _heroSystem;
        private readonly List<EnemyHealthBar> _enemyHealthBars = [];
        private PauseMenu _pauseMenu;
        private ComboSystem _comboSystem;

        // Hero panels for highlighting
        private Control _mageInfoContainer;
        private Control _warriorInfoContainer;

        public override void _Ready()
        {
            // Get Mage UI elements
            _mageNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/MageInfo/MageName");
            _mageHealthBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/MageInfo/HealthBar");
            _mageHealthLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/MageInfo/HealthLabel");
            _mageShieldLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/MageInfo/ShieldLabel");

            // Get Warrior UI elements
            _warriorNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/WarriorInfo/WarriorName");
            _warriorHealthBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/WarriorInfo/HealthBar");
            _warriorHealthLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/WarriorInfo/HealthLabel");
            _warriorShieldLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/WarriorInfo/ShieldLabel");

            // Get hero info containers for highlighting
            _mageInfoContainer = GetNode<Control>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/MageInfo");
            _warriorInfoContainer = GetNode<Control>("MarginContainer/VBoxContainer/TopBar/HeroesContainer/WarriorInfo");

            // Get enemy container
            _enemiesContainer = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/TopBar/EnemiesInfo");

            // Get game info
            _waveLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/WaveLabel");
            _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/CoinsLabel");
            _swapsLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/SwapsLabel");

            // Get buttons
            _pauseButton = GetNode<Button>("MarginContainer/VBoxContainer/TopBar/PauseButton");

            // Get ComboSystem
            _comboSystem = GetNode<ComboSystem>("/root/ComboSystem");

            // Connect signals
            _pauseButton.Pressed += OnPausePressed;
            GameStateManager.Instance.CoinsChanged += OnCoinsChanged;
            GameStateManager.Instance.WaveChanged += OnWaveChanged;
            _comboSystem.CombosProcessed += OnCombosProcessed;

            // Create pause menu
            _pauseMenu = new PauseMenu();
            AddChild(_pauseMenu);

            GD.Print("[BattleHUD] Initialized for dual hero system");
        }

        /// <summary>
        /// Initializes HUD with dual hero system reference.
        /// </summary>
        public void Initialize(DualHeroSystem heroSystem)
        {
            _heroSystem = heroSystem;

            // Connect hero signals
            _heroSystem.HeroHealthChanged += OnHeroHealthChanged;
            _heroSystem.HeroShieldChanged += OnHeroShieldChanged;

            // Connect individual hero signals for shields
            _heroSystem.Mage.ShieldChanged += (shield) => UpdateHeroShield(CharacterClass.Mage, shield);
            _heroSystem.Warrior.ShieldChanged += (shield) => UpdateHeroShield(CharacterClass.Warrior, shield);

            // Update UI
            _mageNameLabel.Text = _heroSystem.Mage.CharacterName + " 🧙";
            _warriorNameLabel.Text = _heroSystem.Warrior.CharacterName + " ⚔️";

            UpdateHeroHealth(CharacterClass.Mage, _heroSystem.Mage.CurrentHealth, _heroSystem.Mage.MaxHealth);
            UpdateHeroHealth(CharacterClass.Warrior, _heroSystem.Warrior.CurrentHealth, _heroSystem.Warrior.MaxHealth);
            UpdateHeroShield(CharacterClass.Mage, _heroSystem.Mage.CurrentShield);
            UpdateHeroShield(CharacterClass.Warrior, _heroSystem.Warrior.CurrentShield);

            UpdateWave(GameStateManager.Instance.CurrentWave);
            UpdateCoins(GameStateManager.Instance.Coins);

            GD.Print($"[BattleHUD] Initialized for {_heroSystem.Mage.CharacterName} and {_heroSystem.Warrior.CharacterName}");
        }

        /// <summary>
        /// Updates hero health display.
        /// </summary>
        private void OnHeroHealthChanged(CharacterClass heroClass, int currentHealth, int maxHealth)
        {
            UpdateHeroHealth(heroClass, currentHealth, maxHealth);
        }

        /// <summary>
        /// Updates hero health bar and label.
        /// </summary>
        private void UpdateHeroHealth(CharacterClass heroClass, int currentHealth, int maxHealth)
        {
            ProgressBar healthBar = heroClass == CharacterClass.Mage ? _mageHealthBar : _warriorHealthBar;
            Label healthLabel = heroClass == CharacterClass.Mage ? _mageHealthLabel : _warriorHealthLabel;

            healthBar.MaxValue = maxHealth;
            healthBar.Value = currentHealth;
            healthLabel.Text = $"{currentHealth} / {maxHealth}";

            // Color coding based on health percentage
            float healthPercent = (float)currentHealth / maxHealth;
            healthBar.Modulate = healthPercent > 0.5f ? Colors.Green : healthPercent > 0.25f ? Colors.Yellow : Colors.Red;
        }

        /// <summary>
        /// Updates hero shield display.
        /// </summary>
        private void OnHeroShieldChanged(CharacterClass heroClass, int shieldAmount)
        {
            UpdateHeroShield(heroClass, shieldAmount);
        }

        /// <summary>
        /// Updates shield label.
        /// </summary>
        private void UpdateHeroShield(CharacterClass heroClass, int shieldAmount)
        {
            Label shieldLabel = heroClass == CharacterClass.Mage ? _mageShieldLabel : _warriorShieldLabel;

            if (shieldAmount > 0)
            {
                shieldLabel.Show();
                shieldLabel.Text = $"🛡️ {shieldAmount}";
            }
            else
            {
                shieldLabel.Hide();
            }
        }

        /// <summary>
        /// Adds enemy health bars to HUD.
        /// </summary>
        public void SetupEnemies(List<Enemy> enemies)
        {
            // Clear existing enemy bars
            ClearEnemies();

            // Create health bar for each enemy
            foreach (Enemy enemy in enemies)
            {
                EnemyHealthBar enemyBar = new();
                enemyBar.Initialize(enemy);
                _enemiesContainer.AddChild(enemyBar);
                _enemyHealthBars.Add(enemyBar);
            }

            GD.Print($"[BattleHUD] Setup {enemies.Count} enemy health bars");
        }

        /// <summary>
        /// Clears all enemy health bars.
        /// </summary>
        private void ClearEnemies()
        {
            foreach (EnemyHealthBar bar in _enemyHealthBars)
            {
                bar.QueueFree();
            }
            _enemyHealthBars.Clear();
        }

        /// <summary>
        /// Updates remaining swaps display.
        /// </summary>
        public void UpdateSwaps(int remainingSwaps)
        {
            _swapsLabel.Text = $"Swaps: {remainingSwaps}";
        }

        /// <summary>
        /// Updates wave number display.
        /// </summary>
        private void OnWaveChanged(int waveNumber)
        {
            UpdateWave(waveNumber);
        }

        /// <summary>
        /// Updates wave label.
        /// </summary>
        private void UpdateWave(int waveNumber)
        {
            _waveLabel.Text = $"Wave: {waveNumber}";
        }

        /// <summary>
        /// Updates coins display.
        /// </summary>
        private void OnCoinsChanged(int coins)
        {
            UpdateCoins(coins);
        }

        /// <summary>
        /// Updates coins label.
        /// </summary>
        private void UpdateCoins(int coins)
        {
            _coinsLabel.Text = $"💰 {coins}";
        }

        /// <summary>
        /// Called when pause button is pressed.
        /// </summary>
        private void OnPausePressed()
        {
            GD.Print("[BattleHUD] Pause pressed");
            _pauseMenu?.Pause();
        }

        /// <summary>
        /// Called when combos are processed to highlight active hero.
        /// </summary>
        private void OnCombosProcessed(int comboCount)
        {
            if (comboCount == 0 || _comboSystem == null)
            {
                return;
            }

            // Get last processed effects
            List<ComboEffect> effects = _comboSystem.GetLastProcessedEffects();
            if (effects == null || effects.Count == 0)
            {
                return;
            }

            // Determine which heroes are active based on element types
            bool mageActive = false;
            bool warriorActive = false;

            foreach (ComboEffect effect in effects)
            {
                switch (effect.ElementType)
                {
                    case ElementType.Fire:
                    case ElementType.Heal:
                        mageActive = true;
                        break;

                    case ElementType.Sword:
                    case ElementType.Shield:
                        warriorActive = true;
                        break;
                }
            }

            // Highlight active heroes
            if (mageActive)
            {
                HighlightHero(_mageInfoContainer, new Color(0.5f, 0.8f, 1.0f)); // Cyan glow for Mage
            }

            if (warriorActive)
            {
                HighlightHero(_warriorInfoContainer, new Color(1.0f, 0.7f, 0.3f)); // Orange glow for Warrior
            }
        }

        /// <summary>
        /// Highlights a hero container with a color glow effect.
        /// </summary>
        private void HighlightHero(Control container, Color highlightColor)
        {
            if (container == null)
            {
                return;
            }

            // Reset to white first
            container.Modulate = Colors.White;

            // Create highlight animation
            Tween tween = CreateTween();
            _ = tween.SetEase(Tween.EaseType.Out);
            _ = tween.SetTrans(Tween.TransitionType.Cubic);

            // Pulse to highlight color
            _ = tween.TweenProperty(container, "modulate", highlightColor, 0.2f);

            // Hold for a moment
            _ = tween.TweenInterval(0.3f);

            // Fade back to white
            _ = tween.TweenProperty(container, "modulate", Colors.White, 0.5f);
        }

        public override void _ExitTree()
        {
            // Disconnect signals
            if (_heroSystem != null)
            {
                _heroSystem.HeroHealthChanged -= OnHeroHealthChanged;
                _heroSystem.HeroShieldChanged -= OnHeroShieldChanged;
            }

            if (_comboSystem != null)
            {
                _comboSystem.CombosProcessed -= OnCombosProcessed;
            }

            GameStateManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameStateManager.Instance.WaveChanged -= OnWaveChanged;
        }
    }

    /// <summary>
    /// Individual enemy health bar widget (unchanged).
    /// </summary>
    public partial class EnemyHealthBar : VBoxContainer
    {
        private Label _nameLabel;
        private ProgressBar _healthBar;
        private Label _healthLabel;
        private Enemy _enemy;

        public override void _Ready()
        {
            // Create UI elements
            _nameLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            AddChild(_nameLabel);

            _healthBar = new ProgressBar
            {
                CustomMinimumSize = new Vector2(150, 20),
                ShowPercentage = false
            };
            AddChild(_healthBar);

            _healthLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            AddChild(_healthLabel);
        }

        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _enemy.HealthChanged += OnHealthChanged;
            _enemy.CharacterDied += OnEnemyDied;

            _nameLabel.Text = _enemy.CharacterName;
            UpdateHealth(_enemy.CurrentHealth, _enemy.MaxHealth);

            _healthBar.Modulate = _enemy.IsBoss ? Colors.Purple : _enemy.IsMiniboss ? Colors.Orange : Colors.Red;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            UpdateHealth(currentHealth, maxHealth);
        }

        private void UpdateHealth(int currentHealth, int maxHealth)
        {
            _healthBar.MaxValue = maxHealth;
            _healthBar.Value = currentHealth;
            _healthLabel.Text = $"{currentHealth}/{maxHealth}";
        }

        private void OnEnemyDied()
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
            _ = tween.TweenCallback(Callable.From(QueueFree));
        }

        public override void _ExitTree()
        {
            if (_enemy != null)
            {
                _enemy.HealthChanged -= OnHealthChanged;
                _enemy.CharacterDied -= OnEnemyDied;
            }
        }
    }
}
