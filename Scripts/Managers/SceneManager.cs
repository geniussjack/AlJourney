using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Manages scene transitions and loading.
    /// Singleton autoload node.
    /// </summary>
    public partial class SceneManager : Node
    {
        private static SceneManager _instance;

        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static SceneManager Instance => _instance;

        [Signal]
        public delegate void SceneLoadStartedEventHandler(string sceneName);

        [Signal]
        public delegate void SceneLoadCompletedEventHandler(string sceneName);

        // Scene paths - update these when scenes are created
        private readonly Dictionary<GameState, string> _scenePaths = new()
        {
            { GameState.MainMenu, "res://Scenes/UI/MainMenu.tscn" },
            { GameState.CharacterSelect, "res://Scenes/UI/CharacterSelect.tscn" },
            { GameState.Battle, "res://Scenes/Battle/BattleScene.tscn" },
            { GameState.Shop, "res://Scenes/UI/ShopScene.tscn" },
            { GameState.GameOver, "res://Scenes/UI/GameOverScreen.tscn" },
            { GameState.Victory, "res://Scenes/UI/VictoryScreen.tscn" }
        };

        private Node _currentScene;
        private bool _isTransitioning;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }

            _instance = this;
            _isTransitioning = false;

            // Get initial scene
            var root = GetTree().Root;
            _currentScene = root.GetChild(root.GetChildCount() - 1);

            GD.Print("[SceneManager] Initialized");
        }

        /// <summary>
        /// Loads scene based on game state.
        /// </summary>
        public void LoadScene(GameState state)
        {
            if (!_scenePaths.TryGetValue(state, out string scenePath))
            {
                GD.PrintErr($"[SceneManager] No scene path defined for state: {state}");
                return;
            }
            LoadSceneByPath(scenePath);
        }

        /// <summary>
        /// Loads scene by direct path.
        /// </summary>
        public void LoadSceneByPath(string scenePath)
        {
            if (_isTransitioning)
            {
                GD.PrintErr("[SceneManager] Scene transition already in progress");
                return;
            }

            // FIX: Используем nameof вместо строки
            CallDeferred(nameof(DeferredSceneChange), scenePath);
        }

        // FIX: Метод должен быть public для CallDeferred
        public void DeferredSceneChange(string scenePath)
        {
            _isTransitioning = true;
            EmitSignal(SignalName.SceneLoadStarted, scenePath);

            // Free current scene
            _currentScene?.QueueFree();

            // Load new scene
            var newSceneResource = GD.Load<PackedScene>(scenePath);
            if (newSceneResource is null)
            {
                GD.PrintErr($"[SceneManager] Failed to load scene: {scenePath}");
                _isTransitioning = false;
                return;
            }

            _currentScene = newSceneResource.Instantiate();
            GetTree().Root.AddChild(_currentScene);
            GetTree().CurrentScene = _currentScene;

            _isTransitioning = false;
            EmitSignal(SignalName.SceneLoadCompleted, scenePath);

            GD.Print($"[SceneManager] Scene loaded: {scenePath}");
        }

        /// <summary>
        /// Reloads the current scene.
        /// </summary>
        public void ReloadCurrentScene()
        {
            if (_currentScene == null) return;

            string scenePath = _currentScene.SceneFilePath;
            if (!string.IsNullOrEmpty(scenePath))
            {
                LoadSceneByPath(scenePath);
            }
        }

        /// <summary>
        /// Transitions to main menu.
        /// </summary>
        public static void GoToMainMenu()
        {
            GameStateManager.Instance.ReturnToMainMenu();
            Instance.LoadScene(GameState.MainMenu); // FIX: Через Instance
        }

        /// <summary>
        /// Starts a new game (no character selection needed).
        /// </summary>
        public static void StartNewGame()
        {
            GameStateManager.Instance.StartNewGame();
            Instance.LoadScene(GameState.Battle);
        }

        /// <summary>
        /// Continues from saved game.
        /// </summary>
        public static void ContinueGame()
        {
            var saveData = SaveSystem.Instance.LoadGame();
            if (saveData != null)
            {
                GameStateManager.Instance.LoadGame(saveData);
                Instance.LoadScene(GameState.Battle); // FIX: Через Instance
            }
            else
            {
                GD.PrintErr("[SceneManager] No save data to continue from");
            }
        }

        /// <summary>
        /// Transitions to shop after wave completion.
        /// </summary>
        public static void GoToShop()
        {
            GameStateManager.Instance.ChangeState(GameState.Shop);
            Instance.LoadScene(GameState.Shop); // FIX: Через Instance
        }

        /// <summary>
        /// Returns to battle from shop.
        /// </summary>
        public static void ReturnToBattle()
        {
            GameStateManager.Instance.ChangeState(GameState.Battle);
            Instance.LoadScene(GameState.Battle); // FIX: Через Instance
        }

        /// <summary>
        /// Handles game over scenario.
        /// </summary>
        public static void GameOver()
        {
            GameStateManager.Instance.EndGame(false);
            Instance.LoadScene(GameState.GameOver); // FIX: Через Instance
        }
    }
}