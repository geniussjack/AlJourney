using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using Godot;
using System.Collections.Generic;

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

        private Label _waveLabel;
        private Label _coinsLabel;
        private Label _swapsLabel;


        private DualHeroSystem _heroSystem;
        private readonly List<EnemyHealthBar> _enemyHealthBars = [];
        private PauseMenu _pauseMenu;
        private ComboSystem _comboSystem;

        private Control _mageInfoContainer;
        private Control _warriorInfoContainer;

        private AlJourney.Scripts.Utils.DamageFlash _mageDamageFlash;
        private AlJourney.Scripts.Utils.DamageFlash _warriorDamageFlash;

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
            _swapsLabel = GetNode<Label>("MarginContainer/VBoxContainer/BottomBar/SwapsLabel");


            _comboSystem = GetNode<ComboSystem>("/root/ComboSystem");


            GameStateManager.Instance.CoinsChanged += OnCoinsChanged;
            GameStateManager.Instance.WaveChanged += OnWaveChanged;
            _comboSystem.CombosProcessed += OnCombosProcessed;

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
        /// Инициализирует HUD для работы с системой двух героев, настраивает начальные значения здоровья, щитов и эффекты получения урона.
        /// </summary>
        /// <param name="heroSystem">Система управления двумя героями.</param>
        public void Initialize(DualHeroSystem heroSystem)
        {
            _heroSystem = heroSystem;

            _heroSystem.HeroHealthChanged += OnHeroHealthChanged;
            _heroSystem.HeroShieldChanged += OnHeroShieldChanged;

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
            _heroSystem.Mage.DamageTaken += (amount) => _mageDamageFlash.FlashDamage();
            _heroSystem.Mage.Healed += (amount) => _mageDamageFlash.FlashHeal();

            _warriorDamageFlash = new AlJourney.Scripts.Utils.DamageFlash();
            _warriorInfoContainer.AddChild(_warriorDamageFlash);
            _heroSystem.Warrior.DamageTaken += (amount) => _warriorDamageFlash.FlashDamage();
            _heroSystem.Warrior.Healed += (amount) => _warriorDamageFlash.FlashHeal();

            GD.Print($"[BattleHUD] Initialized for {_heroSystem.Mage.CharacterName} and {_heroSystem.Warrior.CharacterName}");
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
                enemyBar.Initialize(enemy);
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
        /// <param name="remainingSwaps">Количество оставшихся перемещений.</param>
        public void UpdateSwaps(int remainingSwaps)
        {
            _swapsLabel.Text = $"{Tr("UI_BATTLE_SWAPS")} {remainingSwaps}";
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

        private void OnPausePressed()
        {
            GD.Print("[BattleHUD] Pause pressed");
            _pauseMenu?.Pause();
        }

        private void OnCombosProcessed(int comboCount)
        {
            if (comboCount == 0 || _comboSystem == null)
            {
                return;
            }

            List<ComboEffect> effects = _comboSystem.GetLastProcessedEffects();
            if (effects == null || effects.Count == 0)
            {
                return;
            }

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

            if (mageActive)
            {
                HighlightHero(_mageInfoContainer, new Color(0.5f, 0.8f, 1.0f));
            }

            if (warriorActive)
            {
                HighlightHero(_warriorInfoContainer, new Color(1.0f, 0.7f, 0.3f));
            }
        }

        private void HighlightHero(Control container, Color highlightColor)
        {
            if (container == null)
            {
                return;
            }

            container.Modulate = Colors.White;

            Tween tween = CreateTween();
            _ = tween.SetEase(Tween.EaseType.Out);
            _ = tween.SetTrans(Tween.TransitionType.Cubic);

            _ = tween.TweenProperty(container, "modulate", highlightColor, 0.2f);

            _ = tween.TweenInterval(0.3f);

            _ = tween.TweenProperty(container, "modulate", Colors.White, 0.5f);
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

            _comboSystem?.CombosProcessed -= OnCombosProcessed;

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
        private Enemy _enemy;
        private AlJourney.Scripts.Utils.DamageFlash _damageFlash;
        private TextureRect _portrait;

        /// <summary>
        /// Конструктор. Создает и настраивает визуальные элементы полоски здоровья: имя, саму полоску и текст здоровья.
        /// </summary>
        public EnemyHealthBar()
        {
            HBoxContainer row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);
            AddChild(row);

            Control portraitContainer = new Control();
            portraitContainer.CustomMinimumSize = new Vector2(96, 96);
            row.AddChild(portraitContainer);

            _portrait = new TextureRect();
            _portrait.SetAnchorsPreset(LayoutPreset.Center);
            _portrait.GrowHorizontal = GrowDirection.Both;
            _portrait.GrowVertical = GrowDirection.Both;
            _portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            _portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            _portrait.CustomMinimumSize = new Vector2(96, 96);
            portraitContainer.AddChild(_portrait);

            VBoxContainer textContainer = new VBoxContainer();
            textContainer.AddThemeConstantOverride("separation", 2);
            row.AddChild(textContainer);

            _nameLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Left
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

        /// <summary>
        /// Инициализирует полоску здоровья данными конкретного врага, подписывается на его события изменения здоровья и смерти, а также настраивает цвет в зависимости от типа врага.
        /// </summary>
        /// <param name="enemy">Враг, к которому привязывается данная полоска здоровья.</param>
        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _enemy.HealthChanged += OnHealthChanged;
            _enemy.CharacterDied += OnEnemyDied;

            _nameLabel.Text = _enemy.CharacterName;
            
            string spritePath = _enemy.EnemyType switch {
                EnemyType.Slime => "res://Resources/Sprites/Characters/slime_sprite.png",
                _ => "res://Resources/Sprites/Characters/skeleton_sprite.png"
            };
            _portrait.Texture = GD.Load<Texture2D>(spritePath);
            AnimatePortrait();
            UpdateHealth(_enemy.CurrentHealth, _enemy.MaxHealth);

            _healthBar.Modulate = _enemy.IsBoss ? Colors.Purple : _enemy.IsMiniboss ? Colors.Orange : Colors.Red;

            _damageFlash = new AlJourney.Scripts.Utils.DamageFlash();
            AddChild(_damageFlash);
            _enemy.DamageTaken += (amount) => _damageFlash.FlashDamage();
            _enemy.Healed += (amount) => _damageFlash.FlashHeal();
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
            _nameLabel.Text = _enemy.CharacterName;
        }

        private void OnEnemyDied()
        {
            Tween tween = CreateTween();
            _ = tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
            _ = tween.TweenCallback(Callable.From(QueueFree));
        }

        /// <summary>
        /// Вызывается при удалении узла. Отписывается от событий связанного врага.
        /// </summary>
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
