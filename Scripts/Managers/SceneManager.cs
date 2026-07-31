using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Interfaces;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Scene manager. Responsible for loading and switching game scenes based on the game state.
    /// </summary>
    public partial class SceneManager : Node, ISceneManager
    {
        /// <summary>
        /// Global instance of the scene manager.
        /// </summary>
        public static SceneManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Raised before a new scene starts loading.
        /// </summary>
        /// <param name="sceneName">The path or name of the scene being loaded.</param>
        public delegate void SceneLoadStartedEventHandler(string sceneName);

        [Signal]
        /// <summary>
        /// Raised after a new scene has finished loading.
        /// </summary>
        /// <param name="sceneName">The path or name of the loaded scene.</param>
        public delegate void SceneLoadCompletedEventHandler(string sceneName);

        private readonly Dictionary<GameState, string> _scenePaths = new()
        {
            { GameState.MainMenu, "res://Scenes/UI/MainMenu.tscn" },
            { GameState.Map, "res://Scenes/UI/CampaignMapScene.tscn" },
            { GameState.Battle, "res://Scenes/Battle/BattleScene.tscn" },
            { GameState.Shop, "res://Scenes/UI/ShopScene.tscn" },
            { GameState.GameOver, "res://Scenes/UI/GameOverScreen.tscn" },
            { GameState.Victory, "res://Scenes/UI/VictoryScreen.tscn" }
        };

        private Node _currentScene;
        private bool _isTransitioning;

        /// <summary>
        /// Initializes the scene manager when added to the tree. Locates the currently active scene.
        /// </summary>
        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }

            Instance = this;
            _isTransitioning = false;

            Window root = GetTree().Root;
            _currentScene = root.GetChild(root.GetChildCount() - 1);

            GD.Print("[SceneManager] Initialized");
        }

        /// <summary>
        /// Starts loading the scene that corresponds to the given game state.
        /// </summary>
        /// <param name="state">The global game state to load a scene for.</param>
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
        /// Begins loading the scene at the given path. The switch happens on a deferred call.
        /// </summary>
        /// <param name="scenePath">The path to the scene file.</param>
        public void LoadSceneByPath(string scenePath)
        {
            if (_isTransitioning)
            {
                GD.PrintErr("[SceneManager] Scene transition already in progress");
                return;
            }

            Callable.From<string>(DeferredSceneChange).CallDeferred(scenePath);
        }

        /// <summary>
        /// Deferred method that safely replaces the current scene with a new one.
        /// Removes the old scene and adds the new one to the tree root.
        /// </summary>
        /// <param name="scenePath">The path to the scene file to load.</param>
        public void DeferredSceneChange(string scenePath)
        {
            _isTransitioning = true;
            _ = EmitSignal(SignalName.SceneLoadStarted, scenePath);

            _currentScene?.QueueFree();

            PackedScene newSceneResource = GD.Load<PackedScene>(scenePath);
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
            _ = EmitSignal(SignalName.SceneLoadCompleted, scenePath);

            GD.Print($"[SceneManager] Scene loaded: {scenePath}");
        }

        /// <summary>
        /// Reloads the currently active scene.
        /// </summary>
        public void ReloadCurrentScene()
        {
            if (_currentScene == null)
            {
                return;
            }

            string scenePath = _currentScene.SceneFilePath;
            if (!string.IsNullOrEmpty(scenePath))
            {
                LoadSceneByPath(scenePath);
            }
        }

        /// <summary>
        /// Static helper method: navigates to the main menu.
        /// </summary>
        public static void GoToMainMenu()
        {
            GameStateManager.Instance.ReturnToMainMenu();
            Instance.LoadScene(GameState.MainMenu);
        }

        /// <summary>
        /// Static helper method: starts a new game and navigates to the campaign map.
        /// </summary>
        public static void StartNewGame()
        {
            GameStateManager.Instance.StartNewGame();
            Instance.LoadScene(GameState.Map);
        }

        /// <summary>
        /// Static helper method: loads a save file and resumes the game on the campaign map.
        /// </summary>
        public static void ContinueGame()
        {
            SaveData saveData = SaveSystem.Instance.LoadGame();
            if (saveData != null)
            {
                GameStateManager.Instance.LoadGame(saveData);
                Instance.LoadScene(GameState.Map);
            }
            else
            {
                GD.PrintErr("[SceneManager] No save data to continue from");
            }
        }

        public void ShowOverlay(string scenePath)
        {
            PackedScene newSceneResource = GD.Load<PackedScene>(scenePath);
            if (newSceneResource != null)
            {
                Control overlay = newSceneResource.Instantiate<Control>();
                // Make sure the overlay renders on top of everything
                overlay.ZIndex = 100;

                // Add it to the current scene (BattleScene)
                if (_currentScene != null && IsInstanceValid(_currentScene))
                {
                    CanvasLayer canvas = _currentScene.GetNodeOrNull<CanvasLayer>("CanvasLayer");
                    if (canvas != null)
                    {
                        canvas.AddChild(overlay);
                        return;
                    }
                }

                GetTree().Root.AddChild(overlay);
            }
        }

        /// <summary>
        /// Static helper method: navigates to the campaign map — the hub between levels, from which
        /// the shop and the next level selection are accessible.
        /// </summary>
        public static void GoToMap()
        {
            GameStateManager.Instance.ChangeState(GameState.Map);
            Instance.LoadScene(GameState.Map);
        }

        /// <summary>
        /// Static helper method: navigates to the shop scene.
        /// </summary>
        public static void GoToShop()
        {
            GameStateManager.Instance.ChangeState(GameState.Shop);
            Instance.ShowOverlay("res://Scenes/UI/ShopScene.tscn");
        }

        /// <summary>
        /// Static helper method: ends the game in defeat and navigates to the "Game Over" screen.
        /// </summary>
        public static void GameOver()
        {
            GameStateManager.Instance.EndGame(false);
            Instance.ShowOverlay("res://Scenes/UI/GameOverScreen.tscn");
        }
    }
}
