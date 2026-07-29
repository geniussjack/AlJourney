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
    /// Пользовательский интерфейс боевого экрана. Отвечает за отображение здоровья, щитов героев, информации о врагах, текущей волне и количестве доступных ходов.
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
        /// Вызывается при инициализации узла. Настраивает ссылки на дочерние элементы интерфейса, подписывается на события изменения состояния игры и комбо.
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
        /// Инициализирует HUD для работы с отрядом героев и менеджером боя: настраивает начальные значения
        /// здоровья/щитов, эффекты получения урона, а также клики по портретам для выбора цели способности.
        /// </summary>
        /// <param name="heroSystem">Система управления отрядом героев.</param>
        /// <param name="battleManager">Менеджер пошагового боя, которому передаются подтверждённые цели.</param>
        public void Initialize(DualHeroSystem heroSystem, BattleManager battleManager)
        {
            _heroSystem = heroSystem;
            _battleManager = battleManager;

            _heroSystem.HeroHealthChanged += OnHeroHealthChanged;
            _heroSystem.HeroShieldChanged += OnHeroShieldChanged;

            _battleManager.TurnStateChanged += RefreshTargetHighlights;
            _battleManager.PhaseChanged += OnBattlePhaseChanged;

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

            _heroSystem.Mage.StatusEffectAdded += (effectType, duration, power) => UpdateHeroStatusEffects(CharacterClass.Mage);
            _heroSystem.Mage.StatusEffectRemoved += (effectType) => UpdateHeroStatusEffects(CharacterClass.Mage);
            _heroSystem.Warrior.StatusEffectAdded += (effectType, duration, power) => UpdateHeroStatusEffects(CharacterClass.Warrior);
            _heroSystem.Warrior.StatusEffectRemoved += (effectType) => UpdateHeroStatusEffects(CharacterClass.Warrior);

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
        /// Создает и настраивает полоски здоровья для предоставленного списка врагов, предварительно очищая старые данные.
        /// </summary>
        /// <param name="enemies">Список текущих врагов на уровне.</param>
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
        /// Обновляет текстовое отображение количества оставшихся перемещений элементов на поле.
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
        /// Обновляет подсветку и кликабельность портретов союзников и полосок здоровья врагов
        /// в соответствии с текущим списком допустимых целей выбранной способности.
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

        /// <summary>
        /// Вызывается при удалении узла из дерева. Отписывается от всех глобальных и локальных событий для предотвращения утечек памяти.
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
            }

            GameStateManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameStateManager.Instance.WaveChanged -= OnWaveChanged;
        }
    }

    /// <summary>
    /// UI-компонент, представляющий полоску здоровья конкретного врага. Отображает имя, текущее здоровье и реагирует на получение урона или лечение.
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
        /// Враг, к которому привязана данная полоска здоровья.
        /// </summary>
        public Enemy Enemy { get; private set; }

        /// <summary>
        /// Конструктор. Создает и настраивает визуальные элементы полоски здоровья: имя, саму полоску и текст здоровья.
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
        /// Инициализирует полоску здоровья данными конкретного врага, подписывается на его события изменения здоровья и смерти, а также настраивает цвет в зависимости от типа врага.
        /// </summary>
        /// <param name="enemy">Враг, к которому привязывается данная полоска здоровья.</param>
        /// <param name="battleManager">Менеджер боя, которому передаётся подтверждённая цель по клику.</param>
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

            Enemy.StatusEffectAdded += (effectType, duration, power) => UpdateStatusEffects();
            Enemy.StatusEffectRemoved += (effectType) => UpdateStatusEffects();
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
        /// Помечает данного врага как допустимую (или недопустимую) цель для наведения текущей способности
        /// и подсвечивает полоску здоровья соответствующим образом.
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
        /// Вызывается при удалении узла. Отписывается от событий связанного врага.
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
