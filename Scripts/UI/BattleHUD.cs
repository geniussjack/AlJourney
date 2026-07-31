using AlJourney.Scripts.Battle;
using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.UI
{
    /// <summary>
    /// UI for the battle screen. Responsible for displaying hero health and shields, enemy info, the current wave, and available actions.
    /// </summary>
    public partial class BattleHUD : Control
    {
        private Label _mageNameLabel;
        private ProgressBar _mageHealthBar;
        private Label _mageHealthLabel;
        private Label _mageShieldLabel;

        private Label _warriorNameLabel;
        private ProgressBar _warriorHealthBar;
        private Label _warriorHealthLabel;
        private Label _warriorShieldLabel;

        private Container _enemiesContainer;

        private Label _coinsLabel;
        private Label _waveLabel;
        private Label _ultimateChargeLabel;


        private DualHeroSystem _heroSystem;
        private BattleManager _battleManager;
        private readonly List<EnemyHealthBar> _enemyHealthBars = [];
        private PauseMenu _pauseMenu;

        private Control _mageInfoContainer;
        private Control _warriorInfoContainer;
        private HBoxContainer _mageStatusContainer;
        private HBoxContainer _warriorStatusContainer;

        private AlJourney.Scripts.Utils.DamageFlash _mageDamageFlash;
        private AlJourney.Scripts.Utils.DamageFlash _warriorDamageFlash;

        private Button _inventoryButton;

        /// <summary>
        /// Called when the node is initialized. Sets up references to child UI elements and subscribes to game state and combo change events.
        /// </summary>
        public override void _Ready()
        {
            _mageNameLabel = GetNode<Label>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageName");
            _mageHealthBar = GetNode<ProgressBar>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageHealthBar");
            _mageHealthLabel = GetNode<Label>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageHealthLabel");
            _mageShieldLabel = GetNode<Label>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageShieldLabel");

            _warriorNameLabel = GetNode<Label>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorName");
            _warriorHealthBar = GetNode<ProgressBar>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorHealthBar");
            _warriorHealthLabel = GetNode<Label>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorHealthLabel");
            _warriorShieldLabel = GetNode<Label>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorShieldLabel");

            _mageInfoContainer = GetNode<Control>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MagePortraitContainer");
            _warriorInfoContainer = GetNode<Control>("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorPortraitContainer");

            _enemiesContainer = GetNode<Container>("../DecorativeLayer/RightPanel/MarginContainer/VBoxContainer");

            _waveLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/WaveLabel");
            _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/CoinsContainer/CoinsLabel");

            HBoxContainer bottomBar = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/BottomBar");
            _ultimateChargeLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
            bottomBar.AddChild(_ultimateChargeLabel);
            UpdateUltimateCharge(0, BattleManager.MaxUltimateCharge);

            _inventoryButton = GetNode<Button>("MarginContainer/VBoxContainer/TopBar/InventoryButton");
            _inventoryButton.Pressed += OnInventoryButtonPressed;

            GameStateManager.Instance.CoinsChanged += OnCoinsChanged;
            GameStateManager.Instance.WaveChanged += OnWaveChanged;

            PackedScene pauseScene = GD.Load<PackedScene>("res://Scenes/UI/PauseMenu.tscn");
            if (pauseScene != null)
            {
                _pauseMenu = pauseScene.Instantiate<PauseMenu>();
                AddChild(_pauseMenu);
            }
            else
            {
                GD.PrintErr("[BattleHUD] Failed to load PauseMenu.tscn");
            }

            GD.Print("[BattleHUD] Initialized for dual hero system");
        }

        /// <summary>
        /// Initializes the HUD for the hero party and battle manager: sets up initial health/shield values,
        /// damage-taken effects, and portrait clicks for confirming an ability's target.
        /// </summary>
        /// <param name="heroSystem">The hero party management system.</param>
        /// <param name="battleManager">The turn-based battle manager that confirmed targets are passed to.</param>
        public void Initialize(DualHeroSystem heroSystem, BattleManager battleManager)
        {
            _heroSystem = heroSystem;
            _battleManager = battleManager;

            _heroSystem.HeroHealthChanged += OnHeroHealthChanged;
            _heroSystem.HeroShieldChanged += OnHeroShieldChanged;

            _battleManager.TurnStateChanged += RefreshTargetHighlights;
            _battleManager.PhaseChanged += OnBattlePhaseChanged;
            _battleManager.UltimateChargeChanged += OnUltimateChargeChanged;

            _mageInfoContainer.MouseFilter = MouseFilterEnum.Stop;
            _warriorInfoContainer.MouseFilter = MouseFilterEnum.Stop;
            _mageInfoContainer.GuiInput += @event => OnAllyPortraitGuiInput(@event, _heroSystem.Mage);
            _warriorInfoContainer.GuiInput += @event => OnAllyPortraitGuiInput(@event, _heroSystem.Warrior);

            _heroSystem.Mage.ShieldChanged += (shield) => UpdateHeroShield(CharacterClass.Mage, shield);
            _heroSystem.Warrior.ShieldChanged += (shield) => UpdateHeroShield(CharacterClass.Warrior, shield);

            _mageNameLabel.Text = Tr("UI_BATTLE_ALTARION");
            _warriorNameLabel.Text = Tr("UI_BATTLE_ALDRIC");

            UpdateHeroHealth(CharacterClass.Mage, _heroSystem.Mage.CurrentHealth, _heroSystem.Mage.MaxHealth);
            UpdateHeroHealth(CharacterClass.Warrior, _heroSystem.Warrior.CurrentHealth, _heroSystem.Warrior.MaxHealth);
            UpdateHeroShield(CharacterClass.Mage, _heroSystem.Mage.CurrentShield);
            UpdateHeroShield(CharacterClass.Warrior, _heroSystem.Warrior.CurrentShield);

            UpdateWave(GameStateManager.Instance.CurrentWave);
            UpdateCoins(GameStateManager.Instance.Coins);

            _mageDamageFlash = new AlJourney.Scripts.Utils.DamageFlash();
            _mageInfoContainer.AddChild(_mageDamageFlash);
            _heroSystem.Mage.DamageTaken += (_) => _mageDamageFlash.FlashDamage();
            _heroSystem.Mage.Healed += (_) => _mageDamageFlash.FlashHeal();

            _warriorDamageFlash = new AlJourney.Scripts.Utils.DamageFlash();
            _warriorInfoContainer.AddChild(_warriorDamageFlash);
            _heroSystem.Warrior.DamageTaken += (_) => _warriorDamageFlash.FlashDamage();
            _heroSystem.Warrior.Healed += (_) => _warriorDamageFlash.FlashHeal();

            _mageStatusContainer = new HBoxContainer() { Alignment = BoxContainer.AlignmentMode.Center };
            _mageInfoContainer.AddChild(_mageStatusContainer);
            _warriorStatusContainer = new HBoxContainer() { Alignment = BoxContainer.AlignmentMode.Center };
            _warriorInfoContainer.AddChild(_warriorStatusContainer);

            _heroSystem.Mage.StatusEffectAdded += (_, _, _) => UpdateHeroStatusEffects(CharacterClass.Mage);
            _heroSystem.Mage.StatusEffectRemoved += (_) => UpdateHeroStatusEffects(CharacterClass.Mage);
            _heroSystem.Warrior.StatusEffectAdded += (_, _, _) => UpdateHeroStatusEffects(CharacterClass.Warrior);
            _heroSystem.Warrior.StatusEffectRemoved += (_) => UpdateHeroStatusEffects(CharacterClass.Warrior);

            GD.Print($"[BattleHUD] Initialized for {_heroSystem.Mage.CharacterName} and {_heroSystem.Warrior.CharacterName}");
        }

        private void UpdateHeroStatusEffects(CharacterClass heroClass)
        {
            HBoxContainer container = heroClass == CharacterClass.Mage ? _mageStatusContainer : _warriorStatusContainer;
            Character hero = heroClass == CharacterClass.Mage ? _heroSystem.Mage : _heroSystem.Warrior;

            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }

            foreach (StatusEffectData effect in hero.GetActiveEffects())
            {
                Color rectColor = Colors.White;
                string iconEmoji = "❓";
                switch (effect.Type)
                {
                    case StatusEffect.Burning: iconEmoji = "🔥"; rectColor = Colors.Orange; break;
                    case StatusEffect.Bleeding: iconEmoji = "🩸"; rectColor = Colors.Red; break;
                    case StatusEffect.Freeze: iconEmoji = "❄️"; rectColor = Colors.Aqua; break;
                    case StatusEffect.Shock: iconEmoji = "⚡"; rectColor = Colors.Yellow; break;
                    case StatusEffect.Vulnerable: iconEmoji = "💔"; rectColor = Colors.Purple; break;
                    case StatusEffect.Stunned: iconEmoji = "💫"; rectColor = Colors.Gray; break;
                    case StatusEffect.Weakened: iconEmoji = "📉"; rectColor = Colors.Brown; break;
                    case StatusEffect.ShieldReflect: iconEmoji = "🛡️"; rectColor = Colors.LightBlue; break;
                    case StatusEffect.Immunity: iconEmoji = "✨"; rectColor = Colors.Gold; break;
                    case StatusEffect.Regeneration: iconEmoji = "💚"; rectColor = Colors.Green; break;
                }

                Label icon = new()
                {
                    Text = iconEmoji,
                    Modulate = rectColor,
                    TooltipText = $"{effect.Type} (Осталось: {effect.Duration})",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                icon.AddThemeFontSizeOverride("font_size", 24);
                container.AddChild(icon);
            }
        }

        private void OnHeroHealthChanged(CharacterClass heroClass, int currentHealth, int maxHealth)
        {
            UpdateHeroHealth(heroClass, currentHealth, maxHealth);
        }

        private void UpdateHeroHealth(CharacterClass heroClass, int currentHealth, int maxHealth)
        {
            ProgressBar healthBar = heroClass == CharacterClass.Mage ? _mageHealthBar : _warriorHealthBar;
            Label healthLabel = heroClass == CharacterClass.Mage ? _mageHealthLabel : _warriorHealthLabel;

            healthBar.MaxValue = maxHealth;
            healthBar.Value = currentHealth;
            healthLabel.Text = $"{currentHealth} / {maxHealth}";

            float healthPercent = (float)currentHealth / maxHealth;
            healthBar.Modulate = healthPercent > 0.5f ? Colors.Green : healthPercent > 0.25f ? Colors.Yellow : Colors.Red;
        }

        private void OnHeroShieldChanged(CharacterClass heroClass, int shieldAmount)
        {
            UpdateHeroShield(heroClass, shieldAmount);
        }

        private void UpdateHeroShield(CharacterClass heroClass, int shieldAmount)
        {
            Label shieldLabel = heroClass == CharacterClass.Mage ? _mageShieldLabel : _warriorShieldLabel;

            if (shieldAmount > 0)
            {
                shieldLabel.Show();
                shieldLabel.Text = $"{Tr("UI_BATTLE_SHIELD")} {shieldAmount}";
            }
            else
            {
                shieldLabel.Hide();
            }
        }

        /// <summary>
        /// Creates and configures health bars for the given list of enemies, first clearing any old data.
        /// </summary>
        /// <param name="enemies">The current list of enemies in the level.</param>
        public void SetupEnemies(List<Enemy> enemies)
        {
            ClearEnemies();

            foreach (Enemy enemy in enemies)
            {
                EnemyHealthBar enemyBar = new();
                enemyBar.Initialize(enemy, _battleManager);
                _enemiesContainer.AddChild(enemyBar);
                _enemyHealthBars.Add(enemyBar);
            }

            GD.Print($"[BattleHUD] Setup {enemies.Count} enemy health bars");
        }

        private void ClearEnemies()
        {
            foreach (EnemyHealthBar bar in _enemyHealthBars)
            {
                bar.QueueFree();
            }
            _enemyHealthBars.Clear();
        }

        /// <summary>
        /// Updates the text display of the remaining move count on the board.
        /// </summary>
        public static void UpdateSwaps(int _)
        {
            // Swaps text has been removed as per 1-swap-per-turn rule
        }

        private void OnWaveChanged(int waveNumber)
        {
            UpdateWave(waveNumber);
        }

        private void UpdateWave(int waveNumber)
        {
            _waveLabel.Text = $"{Tr("UI_BATTLE_WAVE")} {waveNumber}";
        }

        private void OnCoinsChanged(int coins)
        {
            UpdateCoins(coins);
        }

        private void UpdateCoins(int coins)
        {
            _coinsLabel.Text = $"{coins}";
        }

        private void OnInventoryButtonPressed()
        {
            PackedScene inventoryScene = GD.Load<PackedScene>("res://Scenes/UI/InventoryUI.tscn");
            if (inventoryScene != null)
            {
                Control inventory = inventoryScene.Instantiate<Control>();
                AddChild(inventory);
            }
            else
            {
                GD.PrintErr("[BattleHUD] Failed to load InventoryUI.tscn");
            }
        }

        private void OnPausePressed()
        {
            GD.Print("[BattleHUD] Pause pressed");
            _pauseMenu?.Pause();
        }

        private void OnBattlePhaseChanged(BattlePhase newPhase)
        {
            RefreshTargetHighlights();
        }

        /// <summary>
        /// Updates the highlight and clickability of ally portraits and enemy health bars
        /// to match the current list of valid targets for the selected ability.
        /// </summary>
        private void RefreshTargetHighlights()
        {
            if (_battleManager == null)
            {
                return;
            }

            IReadOnlyList<Character> validTargets = _battleManager.GetValidTargets();

            SetAllySelectable(_mageInfoContainer, validTargets.Contains(_heroSystem.Mage));
            SetAllySelectable(_warriorInfoContainer, validTargets.Contains(_heroSystem.Warrior));

            foreach (EnemyHealthBar bar in _enemyHealthBars)
            {
                bar.SetSelectable(validTargets.Contains(bar.Enemy));
            }
        }

        private static void SetAllySelectable(Control container, bool selectable)
        {
            container.Modulate = selectable ? new Color(1.2f, 1.2f, 0.5f) : Colors.White;
        }

        private void OnAllyPortraitGuiInput(InputEvent @event, PlayerCharacter member)
        {
            if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                _battleManager?.ConfirmTarget(member);
            }
        }

        private void OnUltimateChargeChanged(int charge, int maxCharge)
        {
            UpdateUltimateCharge(charge, maxCharge);
        }

        private void UpdateUltimateCharge(int charge, int maxCharge)
        {
            _ultimateChargeLabel.Text = $"{Tr("UI_BATTLE_ULTIMATE_CHARGE")} {charge}/{maxCharge}";
            _ultimateChargeLabel.Modulate = charge >= maxCharge ? Colors.Gold : Colors.White;
        }

        /// <summary>
        /// Called when the node is removed from the tree. Unsubscribes from all global and local events to prevent memory leaks.
        /// </summary>
        public override void _ExitTree()
        {
            if (_heroSystem != null)
            {
                _heroSystem.HeroHealthChanged -= OnHeroHealthChanged;
                _heroSystem.HeroShieldChanged -= OnHeroShieldChanged;
            }

            if (_battleManager != null)
            {
                _battleManager.TurnStateChanged -= RefreshTargetHighlights;
                _battleManager.PhaseChanged -= OnBattlePhaseChanged;
                _battleManager.UltimateChargeChanged -= OnUltimateChargeChanged;
            }

            GameStateManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameStateManager.Instance.WaveChanged -= OnWaveChanged;
        }
    }

    /// <summary>
    /// UI component representing a single enemy's health bar. Displays the name and current health, and reacts to damage or healing.
    /// </summary>
    public partial class EnemyHealthBar : VBoxContainer
    {
        private Label _nameLabel;
        private ProgressBar _healthBar;
        private Label _healthLabel;
        private BattleManager _battleManager;
        private bool _isSelectable;
        private AlJourney.Scripts.Utils.DamageFlash _damageFlash;
        private TextureRect _portrait;

        /// <summary>
        /// The enemy this health bar is bound to.
        /// </summary>
        public Enemy Enemy { get; private set; }

        /// <summary>
        /// Constructor. Creates and configures the visual elements of the health bar: name, the bar itself, and the health text.
        /// </summary>
        public EnemyHealthBar()
        {
            MouseFilter = MouseFilterEnum.Stop;
            GuiInput += OnGuiInput;

            HBoxContainer row = new();
            row.AddThemeConstantOverride("separation", 10);
            AddChild(row);

            Control portraitContainer = new()
            {
                CustomMinimumSize = new Vector2(96, 96)
            };
            row.AddChild(portraitContainer);

            _portrait = new TextureRect();
            _portrait.SetAnchorsPreset(LayoutPreset.Center);
            _portrait.GrowHorizontal = GrowDirection.Both;
            _portrait.GrowVertical = GrowDirection.Both;
            _portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            _portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            _portrait.CustomMinimumSize = new Vector2(96, 96);
            portraitContainer.AddChild(_portrait);

            VBoxContainer textContainer = new()
            {
                CustomMinimumSize = new Vector2(150, 0)
            };
            textContainer.AddThemeConstantOverride("separation", 2);
            row.AddChild(textContainer);

            _nameLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                ClipText = true,
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
            };
            textContainer.AddChild(_nameLabel);

            _healthBar = new ProgressBar
            {
                CustomMinimumSize = new Vector2(150, 20),
                ShowPercentage = false
            };
            textContainer.AddChild(_healthBar);

            _healthLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            textContainer.AddChild(_healthLabel);
        }

        private HBoxContainer _statusContainer;

        /// <summary>
        /// Initializes the health bar with a specific enemy's data, subscribes to its health-change and death events, and sets the color based on the enemy type.
        /// </summary>
        /// <param name="enemy">The enemy this health bar is bound to.</param>
        /// <param name="battleManager">The battle manager that the confirmed target on click is passed to.</param>
        public void Initialize(Enemy enemy, BattleManager battleManager)
        {
            Enemy = enemy;
            _battleManager = battleManager;
            Enemy.HealthChanged += OnHealthChanged;
            Enemy.CharacterDied += OnEnemyDied;

            _nameLabel.Text = Enemy.CharacterName;

            string spritePath = Enemy.EnemyType switch
            {
                EnemyType.Slime => "res://Resources/Sprites/Characters/slime_sprite.png",
                _ => "res://Resources/Sprites/Characters/skeleton_sprite.png"
            };
            _portrait.Texture = GD.Load<Texture2D>(spritePath);
            AnimatePortrait();
            UpdateHealth(Enemy.CurrentHealth, Enemy.MaxHealth);

            _healthBar.Modulate = Enemy.IsBoss ? Colors.Purple : Enemy.IsMiniboss ? Colors.Orange : Colors.Red;

            _damageFlash = new AlJourney.Scripts.Utils.DamageFlash();
            AddChild(_damageFlash);
            Enemy.DamageTaken += (_) => _damageFlash.FlashDamage();
            Enemy.Healed += (_) => _damageFlash.FlashHeal();

            _statusContainer = new HBoxContainer() { Alignment = BoxContainer.AlignmentMode.Center };
            // Add _statusContainer to the text container below the health label
            Node textContainer = _healthLabel.GetParent();
            textContainer.AddChild(_statusContainer);

            Enemy.StatusEffectAdded += (_, _, _) => UpdateStatusEffects();
            Enemy.StatusEffectRemoved += (_) => UpdateStatusEffects();
            UpdateStatusEffects();
        }

        private void UpdateStatusEffects()
        {
            foreach (Node child in _statusContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (StatusEffectData effect in Enemy.GetActiveEffects())
            {
                Color rectColor = Colors.White;
                string iconEmoji = "❓";
                switch (effect.Type)
                {
                    case StatusEffect.Burning: iconEmoji = "🔥"; rectColor = Colors.Orange; break;
                    case StatusEffect.Bleeding: iconEmoji = "🩸"; rectColor = Colors.Red; break;
                    case StatusEffect.Freeze: iconEmoji = "❄️"; rectColor = Colors.Aqua; break;
                    case StatusEffect.Shock: iconEmoji = "⚡"; rectColor = Colors.Yellow; break;
                    case StatusEffect.Vulnerable: iconEmoji = "💔"; rectColor = Colors.Purple; break;
                    case StatusEffect.Stunned: iconEmoji = "💫"; rectColor = Colors.Gray; break;
                    case StatusEffect.Weakened: iconEmoji = "📉"; rectColor = Colors.Brown; break;
                    case StatusEffect.ShieldReflect: iconEmoji = "🛡️"; rectColor = Colors.LightBlue; break;
                    case StatusEffect.Immunity: iconEmoji = "✨"; rectColor = Colors.Gold; break;
                    case StatusEffect.Regeneration: iconEmoji = "💚"; rectColor = Colors.Green; break;
                }

                Label icon = new()
                {
                    Text = iconEmoji,
                    Modulate = rectColor,
                    TooltipText = $"{effect.Type} (Осталось: {effect.Duration})",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                icon.AddThemeFontSizeOverride("font_size", 24);
                _statusContainer.AddChild(icon);
            }
        }

        private void AnimatePortrait()
        {
            _portrait.PivotOffset = new Vector2(48, 48); // 96x96 default size

            Tween tween = CreateTween();
            _ = tween.SetLoops();
            _ = tween.SetTrans(Tween.TransitionType.Sine);
            _ = tween.SetEase(Tween.EaseType.InOut);

            float delay = GD.Randf() * 0.5f;
            float dur1 = 1.0f + (GD.Randf() * 0.2f);
            float dur2 = 1.0f + (GD.Randf() * 0.2f);

            _ = tween.TweenInterval(delay);
            _ = tween.TweenProperty(_portrait, "scale", new Vector2(1.1f, 1.1f), dur1);
            _ = tween.Parallel().TweenProperty(_portrait, "position", _portrait.Position - new Vector2(0, 4), dur1);
            _ = tween.TweenProperty(_portrait, "scale", new Vector2(1.0f, 1.0f), dur2);
            _ = tween.Parallel().TweenProperty(_portrait, "position", _portrait.Position, dur2);
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
            _nameLabel.Text = Enemy.CharacterName;
        }

        private void OnEnemyDied()
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
            _ = tween.TweenCallback(Callable.From(QueueFree));
        }

        /// <summary>
        /// Marks this enemy as a valid (or invalid) target for the currently selected ability
        /// and highlights the health bar accordingly.
        /// </summary>
        public void SetSelectable(bool selectable)
        {
            _isSelectable = selectable;
            Modulate = selectable ? new Color(1.3f, 1.3f, 0.6f) : Colors.White;
        }

        private void OnGuiInput(InputEvent @event)
        {
            if (_isSelectable && @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                _battleManager?.ConfirmTarget(Enemy);
            }
        }

        /// <summary>
        /// Called when the node is removed. Unsubscribes from the associated enemy's events.
        /// </summary>
        public override void _ExitTree()
        {
            if (Enemy != null)
            {
                Enemy.HealthChanged -= OnHealthChanged;
                Enemy.CharacterDied -= OnEnemyDied;
            }
        }
    }
}
