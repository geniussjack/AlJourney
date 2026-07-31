using AlJourney.Scripts.Battle;
using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.UI;
using AlJourney.Scripts.Utils;
using Godot;

namespace AlJourney.Scripts.Scenes
{
    /// <summary>
    /// Главный контроллер сцены боя.
    /// Объединяет и координирует все системы, связанные с битвой:
    /// пользовательский интерфейс, систему отряда героев и менеджер пошагового боя.
    /// </summary>
    public partial class BattleScene : Node
    {
        private BattleHUD _battleHUD;
        private TurnActionPanel _turnActionPanel;
        private BattleManager _battleManager;
        private DualHeroSystem _heroSystem;
        private Camera2D _camera;
        private CameraShake _cameraShake;

        private GameStateManager _gameStateManager;
        private bool _isBattleTransitionQueued;

        /// <summary>
        /// Инициализирует сцену боя. Настраивает камеру, загружает данные героев из сохранения,
        /// инициализирует интерфейс и подписывается на события.
        /// Запускает начало битвы для текущей волны.
        /// </summary>
        public override void _Ready()
        {
            _gameStateManager = GameStateManager.Instance;

            _battleHUD = GetNode<BattleHUD>("CanvasLayer/BattleHUD");
            _battleManager = GetNode<BattleManager>("BattleManager");
            _isBattleTransitionQueued = false;

            _camera = new Camera2D
            {
                Enabled = true,
                Position = new Vector2(960, 540)
            };
            AddChild(_camera);

            _cameraShake = new CameraShake();
            _camera.AddChild(_cameraShake);

            _heroSystem = new DualHeroSystem();
            AddChild(_heroSystem);

            InitializeHeroes();

            _battleHUD.Initialize(_heroSystem, _battleManager);

            _turnActionPanel = new TurnActionPanel();
            GetNode<CanvasLayer>("CanvasLayer").AddChild(_turnActionPanel);

            _battleManager.LevelCompleted += OnLevelCompleted;
            _battleManager.WaveAdvanced += OnWaveAdvanced;
            _battleManager.BattleEnded += OnBattleEnded;
            _battleManager.EnemyDefeated += OnEnemyDefeated;

            LevelDefinition level = CampaignDatabase.GetLevel(_gameStateManager.CurrentLevelId)
                ?? CampaignDatabase.GetLevel(CampaignDatabase.FirstLevelId);

            _gameStateManager.StartLevel(level);
            _battleManager.StartBattle(_heroSystem, level, _cameraShake);

            _turnActionPanel.Initialize(_battleManager);

            _battleHUD.SetupEnemies(_battleManager.Enemies);

            StartPortraitAnimations();

            GD.Print($"[BattleScene] Battle started - Level {level.Id} (difficulty {level.DifficultyRating})");
        }

        private void StartPortraitAnimations()
        {
            TextureRect mage = GetNodeOrNull<TextureRect>("CanvasLayer/DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MagePortraitContainer/MagePortrait");
            TextureRect warrior = GetNodeOrNull<TextureRect>("CanvasLayer/DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorPortraitContainer/WarriorPortrait");

            GD.Print($"[BattleScene] Portraits loaded: Mage={mage != null}, Warrior={warrior != null}");

            AnimatePortrait(mage);
            AnimatePortrait(warrior);
        }

        private void AnimatePortrait(TextureRect portrait)
        {
            if (portrait == null)
            {
                return;
            }

            portrait.PivotOffset = portrait.Size / 2;
            if (portrait.PivotOffset == Vector2.Zero)
            {
                portrait.PivotOffset = new Vector2(48, 48); // Fallback
            }

            GD.Print($"[BattleScene] Animating portrait: {portrait.Name} with PivotOffset={portrait.PivotOffset}");

            Tween tween = CreateTween();
            _ = tween.SetLoops();
            _ = tween.SetTrans(Tween.TransitionType.Sine);
            _ = tween.SetEase(Tween.EaseType.InOut);

            float delay = GD.Randf() * 0.5f;
            float dur1 = 1.0f + (GD.Randf() * 0.2f);
            float dur2 = 1.0f + (GD.Randf() * 0.2f);

            _ = tween.TweenInterval(delay);
            _ = tween.TweenProperty(portrait, "scale", new Vector2(1.1f, 1.1f), dur1);
            _ = tween.Parallel().TweenProperty(portrait, "position", portrait.Position - new Vector2(0, 4), dur1);
            _ = tween.TweenProperty(portrait, "scale", new Vector2(1.0f, 1.0f), dur2);
            _ = tween.Parallel().TweenProperty(portrait, "position", portrait.Position, dur2);
        }

        private void InitializeHeroes()
        {
            SaveData saveData = _gameStateManager.CurrentSave;

            if (saveData != null)
            {
                _heroSystem.LoadFromSave(
                    saveData.MageHealth, saveData.MageMaxHealth, saveData.MageDamage, saveData.MageDefense,
                    saveData.WarriorHealth, saveData.WarriorMaxHealth, saveData.WarriorDamage, saveData.WarriorDefense
                );

                GD.Print("[BattleScene] Heroes loaded from save");
            }
            else
            {
                GD.Print("[BattleScene] New heroes created with base stats");
            }
        }

        private void OnEnemyDefeated(Enemy enemy)
        {
            GD.Print($"[BattleScene] Enemy defeated: {enemy.CharacterName}");
        }

        /// <summary>
        /// Вызывается, когда все волны текущего уровня пройдены. Уровень (не волна) — это единица
        /// выхода из боя: магазин здесь больше не открывается, игрок возвращается на карту кампании.
        /// </summary>
        private void OnLevelCompleted()
        {
            if (_isBattleTransitionQueued)
            {
                return;
            }

            _isBattleTransitionQueued = true;
            GD.Print("[BattleScene] Level completed! Transitioning to campaign map...");

            SaveHeroStats();

            GetTree().CreateTimer(1.0f).Timeout += SceneManager.GoToMap;
        }

        /// <summary>
        /// Вызывается при переходе к следующей волне внутри того же уровня (бой продолжается без
        /// выхода из сцены) — нужно обновить полоски здоровья врагов под новый состав.
        /// </summary>
        private void OnWaveAdvanced(int waveIndex, int totalWaves)
        {
            _battleHUD.SetupEnemies(_battleManager.Enemies);
            GD.Print($"[BattleScene] Advanced to wave {waveIndex + 1}/{totalWaves} within the level");
        }

        private void OnBattleEnded(bool playerWon)
        {
            if (!playerWon)
            {
                if (_isBattleTransitionQueued)
                {
                    return;
                }

                _isBattleTransitionQueued = true;
                GD.Print("[BattleScene] Battle lost - transitioning to Game Over...");

                GetTree().CreateTimer(1.5f).Timeout += SceneManager.GameOver;
            }
        }

        private void SaveHeroStats()
        {
            (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) = _heroSystem.GetCombinedStats();
            _gameStateManager.UpdateHeroStats(
                mageHealth, mageMaxHealth, mageDamage, mageDefense,
                warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense
            );

            GD.Print("[BattleScene] Hero stats saved");
        }

        /// <summary>
        /// Выполняется при удалении сцены боя из дерева узлов.
        /// Отписывается от всех событий менеджеров, чтобы избежать утечек памяти и вызовов методов уничтоженных объектов,
        /// а также корректно завершает бой.
        /// </summary>
        public override void _ExitTree()
        {
            if (_battleManager != null)
            {
                _battleManager.LevelCompleted -= OnLevelCompleted;
                _battleManager.WaveAdvanced -= OnWaveAdvanced;
                _battleManager.BattleEnded -= OnBattleEnded;
                _battleManager.EnemyDefeated -= OnEnemyDefeated;
            }

            _battleManager?.EndBattle();
        }
    }
}
