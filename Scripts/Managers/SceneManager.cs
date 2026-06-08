using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер SceneManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class SceneManager : Node, ISceneManager
    {
        public static SceneManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Элемент SceneLoadStartedEventHandler.
        /// </summary>
        public delegate void SceneLoadStartedEventHandler(string sceneName);

        [Signal]
        /// <summary>
        /// Элемент SceneLoadCompletedEventHandler.
        /// </summary>
        public delegate void SceneLoadCompletedEventHandler(string sceneName);

        private readonly Dictionary<GameState, string> _scenePaths = new()
        {
            { GameState.MainMenu, "res://Scenes/UI/MainMenu.tscn" },
            { GameState.Battle, "res://Scenes/Battle/BattleScene.tscn" },
            { GameState.Shop, "res://Scenes/UI/ShopScene.tscn" },
            { GameState.GameOver, "res://Scenes/UI/GameOverScreen.tscn" },
            { GameState.Victory, "res://Scenes/UI/VictoryScreen.tscn" }
        };

        private Node _currentScene;
        private bool _isTransitioning;

        /// <summary>
        /// Элемент _Ready.
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
        /// Загружает Scene.
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
        /// Загружает SceneByPath.
        /// </summary>
        public void LoadSceneByPath(string scenePath)
        {
            if (_isTransitioning)
            {
                GD.PrintErr("[SceneManager] Scene transition already in progress");
                return;
            }

            _ = CallDeferred(nameof(DeferredSceneChange), scenePath);
        }

        /// <summary>
        /// Элемент DeferredSceneChange.
        /// </summary>
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
        /// Элемент ReloadCurrentScene.
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
        /// Элемент GoToMainMenu.
        /// </summary>
        public static void GoToMainMenu()
        {
            GameStateManager.Instance.ReturnToMainMenu();
            Instance.LoadScene(GameState.MainMenu);
        }

        /// <summary>
        /// Запускает NewGame.
        /// </summary>
        public static void StartNewGame()
        {
            GameStateManager.Instance.StartNewGame();
            Instance.LoadScene(GameState.Battle);
        }

        /// <summary>
        /// Элемент ContinueGame.
        /// </summary>
        public static void ContinueGame()
        {
            SaveData saveData = SaveSystem.Instance.LoadGame();
            if (saveData != null)
            {
                GameStateManager.Instance.LoadGame(saveData);
                Instance.LoadScene(GameState.Battle);
            }
            else
            {
                GD.PrintErr("[SceneManager] No save data to continue from");
            }
        }

        /// <summary>
        /// Элемент GoToShop.
        /// </summary>
        public static void GoToShop()
        {
            GameStateManager.Instance.ChangeState(GameState.Shop);
            Instance.LoadScene(GameState.Shop);
        }

        /// <summary>
        /// Элемент ReturnToBattle.
        /// </summary>
        public static void ReturnToBattle()
        {
            GameStateManager.Instance.ChangeState(GameState.Battle);
            Instance.LoadScene(GameState.Battle);
        }

        /// <summary>
        /// Элемент GameOver.
        /// </summary>
        public static void GameOver()
        {
            GameStateManager.Instance.EndGame(false);
            Instance.LoadScene(GameState.GameOver);
        }
    }
}
