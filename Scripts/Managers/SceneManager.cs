using Godot;
using System.Collections.Generic;
using RoguelikeMatch3.Core;

namespace AltarionsJourney.Managers
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
            if (_isTransitioning)
            {
                GD.PrintErr("[SceneManager] Scene transition already in progress");
                return;
            }

            if (!_scenePaths.ContainsKey(state))
            {
                GD.PrintErr($"[SceneManager] No scene path defined for state: {state}");
                return;
            }

            string scenePath = _scenePaths[state];
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

            CallDeferred(MethodName._DeferredSceneChange, scenePath);
        }

        private void _DeferredSceneChange(string scenePath)
        {
            _isTransitioning = true;
            EmitSignal(SignalName.SceneLoadStarted, scenePath);

            // Free current scene
            if (_currentScene != null)
            {
                _currentScene.QueueFree();
            }

            // Load new scene
            var newSceneResource = GD.Load<PackedScene>(scenePath);
            if (newSceneResource == null)
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
        public void GoToMainMenu()
        {
            GameStateManager.Instance.ReturnToMainMenu();
            LoadScene(GameState.MainMenu);
        }

        /// <summary>
        /// Starts a new game with character selection.
        /// </summary>
        public void StartNewGame()
        {
            GameStateManager.Instance.ChangeState(GameState.CharacterSelect);
            LoadScene(GameState.CharacterSelect);
        }

        /// <summary>
        /// Continues from saved game.
        /// </summary>
        public void ContinueGame()
        {
            var saveData = SaveSystem.Instance.LoadGame();
            if (saveData != null)
            {
                GameStateManager.Instance.LoadGame(saveData);
                LoadScene(GameState.Battle);
            }
            else
            {
                GD.PrintErr("[SceneManager] No save data to continue from");
            }
        }

        /// <summary>
        /// Transitions to shop after wave completion.
        /// </summary>
        public void GoToShop()
        {
            GameStateManager.Instance.ChangeState(GameState.Shop);
            LoadScene(GameState.Shop);
        }

        /// <summary>
        /// Returns to battle from shop.
        /// </summary>
        public void ReturnToBattle()
        {
            GameStateManager.Instance.ChangeState(GameState.Battle);
            LoadScene(GameState.Battle);
        }

        /// <summary>
        /// Handles game over scenario.
        /// </summary>
        public void GameOver()
        {
            GameStateManager.Instance.EndGame(false);
            LoadScene(GameState.GameOver);
        }
    }
}