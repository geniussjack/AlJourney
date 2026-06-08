using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер сцен. Отвечает за загрузку и переключение игровых сцен на основе состояния игры.
    /// </summary>
    public partial class SceneManager : Node, ISceneManager
    {
        /// <summary>
        /// Глобальный экземпляр менеджера сцен (паттерн Singleton).
        /// </summary>
        public static SceneManager Instance { get; private set; }

        [Signal]
        /// <summary>
        /// Событие, вызываемое перед началом загрузки новой сцены.
        /// </summary>
        /// <param name="sceneName">Путь или имя загружаемой сцены.</param>
        public delegate void SceneLoadStartedEventHandler(string sceneName);

        [Signal]
        /// <summary>
        /// Событие, вызываемое после успешной загрузки новой сцены.
        /// </summary>
        /// <param name="sceneName">Путь или имя загруженной сцены.</param>
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
        /// Инициализирует менеджер сцен при добавлении в дерево. Находит текущую активную сцену.
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
        /// Инициирует загрузку сцены, соответствующей указанному состоянию игры.
        /// </summary>
        /// <param name="state">Глобальное состояние игры, для которого нужно загрузить сцену.</param>
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
        /// Начинает процесс загрузки сцены по указанному пути. Переключение происходит отложенно (deferred).
        /// </summary>
        /// <param name="scenePath">Путь к файлу сцены (формат res://).</param>
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
        /// Отложенный метод для безопасной замены текущей сцены на новую.
        /// Удаляет старую сцену и добавляет новую в корень дерева.
        /// </summary>
        /// <param name="scenePath">Путь к файлу загружаемой сцены.</param>
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
        /// Перезагружает текущую активную сцену.
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
        /// Статический вспомогательный метод: осуществляет переход в Главное меню.
        /// </summary>
        public static void GoToMainMenu()
        {
            GameStateManager.Instance.ReturnToMainMenu();
            Instance.LoadScene(GameState.MainMenu);
        }

        /// <summary>
        /// Статический вспомогательный метод: запускает новую игру и переходит на сцену битвы.
        /// </summary>
        public static void StartNewGame()
        {
            GameStateManager.Instance.StartNewGame();
            Instance.LoadScene(GameState.Battle);
        }

        /// <summary>
        /// Статический вспомогательный метод: загружает сохранение и продолжает игру на сцене битвы.
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
        /// Статический вспомогательный метод: переходит на сцену магазина.
        /// </summary>
        public static void GoToShop()
        {
            GameStateManager.Instance.ChangeState(GameState.Shop);
            Instance.LoadScene(GameState.Shop);
        }

        /// <summary>
        /// Статический вспомогательный метод: возвращается из других экранов (например, магазина) на сцену битвы.
        /// </summary>
        public static void ReturnToBattle()
        {
            GameStateManager.Instance.ChangeState(GameState.Battle);
            Instance.LoadScene(GameState.Battle);
        }

        /// <summary>
        /// Статический вспомогательный метод: завершает игру поражением и переходит на экран "Game Over".
        /// </summary>
        public static void GameOver()
        {
            GameStateManager.Instance.EndGame(false);
            Instance.LoadScene(GameState.GameOver);
        }
    }
}
