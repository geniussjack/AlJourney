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
    /// Main controller for the battle scene.
    /// Wires together and coordinates every system involved in combat:
    /// the UI, the hero party system, and the turn-based combat manager.
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
        /// Initializes the battle scene. Sets up the camera, loads hero data from the save,
        /// initializes the UI, and subscribes to events.
        /// Starts the battle for the current wave.
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
        /// Called once every wave of the current level has been cleared. The level (not the wave) is the
        /// unit of exiting combat: the shop no longer opens here, the player returns to the campaign map.
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
        /// Called when advancing to the next wave within the same level (combat continues without
        /// leaving the scene) — the enemy health bars need to refresh for the new lineup.
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
        /// Runs when the battle scene is removed from the node tree.
        /// Unsubscribes from every manager event to avoid memory leaks and calls into destroyed
        /// objects, and properly wraps up the battle.
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
