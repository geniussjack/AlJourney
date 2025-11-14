using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// Battle HUD controller.
    /// Displays player health, enemy health, coins, and turn information.
    /// </summary>
    public partial class BattleHUD : Control
    {
        // Player UI elements
        private Label _playerNameLabel;
        private ProgressBar _playerHealthBar;
        private Label _playerHealthLabel;
        private Label _playerShieldLabel;

        // Enemy UI container
        private HBoxContainer _enemiesContainer;

        // Game info
        private Label _waveLabel;
        private Label _coinsLabel;
        private Label _swapsLabel;

        // Buttons
        private Button _pauseButton;

        // References
        private PlayerCharacter _player;
        private readonly List<EnemyHealthBar> _enemyHealthBars = [];

        public override void _Ready()
        {
            // Get player UI elements
            _playerNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/PlayerInfo/PlayerName");
            _playerHealthBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/TopBar/PlayerInfo/HealthBar");
            _playerHealthLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/PlayerInfo/HealthLabel");
            _playerShieldLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/PlayerInfo/ShieldLabel");

            // Get enemy container
            _enemiesContainer = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/TopBar/EnemiesInfo");

            // Get game info
            _waveLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/WaveLabel");
            _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/CoinsLabel");
            _swapsLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/SwapsLabel");

            // Get buttons
            _pauseButton = GetNode<Button>("MarginContainer/VBoxContainer/TopBar/PauseButton");

            // Connect signals
            _pauseButton.Pressed += OnPausePressed;
            GameStateManager.Instance.CoinsChanged += OnCoinsChanged;
            GameStateManager.Instance.WaveChanged += OnWaveChanged;

            GD.Print("[BattleHUD] Initialized");
        }

        /// <summary>
        /// Initializes HUD with player reference.
        /// </summary>
        public void Initialize(PlayerCharacter player)
        {
            _player = player;

            // Connect player signals
            _player.HealthChanged += OnPlayerHealthChanged;
            _player.ShieldChanged += OnPlayerShieldChanged;

            // Update UI
            _playerNameLabel.Text = _player.CharacterName;
            UpdatePlayerHealth(_player.CurrentHealth, _player.MaxHealth);
            UpdatePlayerShield(_player.CurrentShield);
            UpdateWave(GameStateManager.Instance.CurrentWave);
            UpdateCoins(GameStateManager.Instance.Coins);

            GD.Print($"[BattleHUD] Initialized for player: {_player.CharacterName}");
        }

        /// <summary>
        /// Updates player health display.
        /// </summary>
        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            UpdatePlayerHealth(currentHealth, maxHealth);
        }

        /// <summary>
        /// Updates player health bar and label.
        /// </summary>
        private void UpdatePlayerHealth(int currentHealth, int maxHealth)
        {
            _playerHealthBar.MaxValue = maxHealth;
            _playerHealthBar.Value = currentHealth;
            _playerHealthLabel.Text = $"{currentHealth} / {maxHealth}";

            // Color coding based on health percentage
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent > 0.5f)
            {
                _playerHealthBar.Modulate = Colors.Green;
            }
            else if (healthPercent > 0.25f)
            {
                _playerHealthBar.Modulate = Colors.Yellow;
            }
            else
            {
                _playerHealthBar.Modulate = Colors.Red;
            }
        }

        /// <summary>
        /// Updates player shield display.
        /// </summary>
        private void OnPlayerShieldChanged(int shieldAmount)
        {
            UpdatePlayerShield(shieldAmount);
        }

        /// <summary>
        /// Updates shield label.
        /// </summary>
        private void UpdatePlayerShield(int shieldAmount)
        {
            if (shieldAmount > 0)
            {
                _playerShieldLabel.Show();
                _playerShieldLabel.Text = $"🛡️ {shieldAmount}";
            }
            else
            {
                _playerShieldLabel.Hide();
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
                var enemyBar = new EnemyHealthBar();
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
            foreach (var bar in _enemyHealthBars)
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
            GetTree().Paused = true;
            // TODO: Show pause menu
        }

        public override void _ExitTree()
        {
            // Disconnect signals
            if (_player != null)
            {
                _player.HealthChanged -= OnPlayerHealthChanged;
                _player.ShieldChanged -= OnPlayerShieldChanged;
            }

            GameStateManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameStateManager.Instance.WaveChanged -= OnWaveChanged;
        }
    }

    /// <summary>
    /// Individual enemy health bar widget.
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

        /// <summary>
        /// Initializes enemy health bar with enemy reference.
        /// </summary>
        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;

            // Connect signals
            _enemy.HealthChanged += OnHealthChanged;
            _enemy.CharacterDied += OnEnemyDied;

            // Set initial values
            _nameLabel.Text = _enemy.CharacterName;
            UpdateHealth(_enemy.CurrentHealth, _enemy.MaxHealth);

            // Color based on enemy type
            if (_enemy.IsBoss)
            {
                _healthBar.Modulate = Colors.Purple;
            }
            else if (_enemy.IsMiniboss)
            {
                _healthBar.Modulate = Colors.Orange;
            }
            else
            {
                _healthBar.Modulate = Colors.Red;
            }
        }

        /// <summary>
        /// Updates health display.
        /// </summary>
        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            UpdateHealth(currentHealth, maxHealth);
        }

        /// <summary>
        /// Updates health bar and label.
        /// </summary>
        private void UpdateHealth(int currentHealth, int maxHealth)
        {
            _healthBar.MaxValue = maxHealth;
            _healthBar.Value = currentHealth;
            _healthLabel.Text = $"{currentHealth}/{maxHealth}";
        }

        /// <summary>
        /// Called when enemy dies.
        /// </summary>
        private void OnEnemyDied()
        {
            // Fade out animation
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
            tween.TweenCallback(Callable.From(() => QueueFree()));
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