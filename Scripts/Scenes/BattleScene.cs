using AlJourney.Scripts.Battle;
using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Match3;
using AlJourney.Scripts.UI;
using AlJourney.Scripts.Utils;
using Godot;

namespace AlJourney.Scripts.Scenes
{
    /// <summary>
    /// Главный контроллер сцены боя. 
    /// Объединяет и координирует все системы, связанные с битвой: игровое поле,
    /// пользовательский интерфейс, систему героев и менеджер боя.
    /// </summary>
    public partial class BattleScene : Node
    {
        private BattleHUD _battleHUD;
        private GridUI _gridUI;
        private BattleManager _battleManager;
        private DualHeroSystem _heroSystem;
        private Camera2D _camera;
        private CameraShake _cameraShake;

        private GridManager _gridManager;
        private GameStateManager _gameStateManager;
        private bool _isBattleTransitionQueued;

        /// <summary>
        /// Инициализирует сцену боя. Настраивает камеру, загружает данные героев из сохранения, 
        /// инициализирует интерфейс и подписывается на события.
        /// Запускает начало битвы для текущей волны.
        /// </summary>
        public override void _Ready()
        {
            _gridManager = GetNode<GridManager>("/root/GridManager");
            _gameStateManager = GameStateManager.Instance;

            _battleHUD = GetNode<BattleHUD>("CanvasLayer/BattleHUD");
            _gridUI = GetNode<GridUI>("CanvasLayer/CenterContainer/GridUI");
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

            _battleHUD.Initialize(_heroSystem);
            _gridUI.Initialize(_heroSystem);

            _battleManager.Initialize(_gridUI);

            _battleManager.WaveCompleted += OnWaveCompleted;
            _battleManager.BattleEnded += OnBattleEnded;
            _battleManager.EnemyDefeated += OnEnemyDefeated;
            _battleManager.PhaseChanged += OnPhaseChanged;

            _gridManager.SwapCompleted += OnSwapCompleted;

            int currentWave = _gameStateManager.CurrentWave;
            _battleManager.StartBattle(_heroSystem, currentWave, _cameraShake);

            _battleHUD.SetupEnemies(_battleManager.Enemies);

            StartPortraitAnimations();

            GD.Print($"[BattleScene] Battle started - Wave {currentWave}");
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

        private void OnSwapCompleted(bool wasValid)
        {
            if (wasValid)
            {
                _battleHUD.UpdateSwaps(_gridManager.RemainingSwaps);
            }
        }

        private void OnPhaseChanged(BattlePhase newPhase)
        {
            if (newPhase == BattlePhase.PlayerSwap)
            {
                _battleHUD.UpdateSwaps(_gridManager.RemainingSwaps);
            }
        }

        private void OnEnemyDefeated(Enemy enemy)
        {
            GD.Print($"[BattleScene] Enemy defeated: {enemy.CharacterName}");
        }

        private void OnWaveCompleted()
        {
            if (_isBattleTransitionQueued)
            {
                return;
            }

            _isBattleTransitionQueued = true;
            GD.Print("[BattleScene] Wave completed! Transitioning to shop...");

            SaveHeroStats();

            GetTree().CreateTimer(1.0f).Timeout += SceneManager.GoToShop;
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
                _battleManager.WaveCompleted -= OnWaveCompleted;
                _battleManager.BattleEnded -= OnBattleEnded;
                _battleManager.EnemyDefeated -= OnEnemyDefeated;
            }

            _gridManager?.SwapCompleted -= OnSwapCompleted;

            _battleManager?.EndBattle();
        }
    }
}
