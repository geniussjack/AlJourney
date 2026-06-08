using AlJourney.Scripts.Managers;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Core;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;
using System.IO;

namespace AlJourney.Tests
{
    public class SaveSystemTest : TestClass
    {
        private SaveSystem _saveSystem;
        private GameStateManager _gameStateManager;

        public SaveSystemTest(Node testScene) : base(testScene) { }

        [SetupAll]
        public void SetupAll()
        {
            _gameStateManager = new GameStateManager();
            _gameStateManager._Ready(); // Init singleton

            _saveSystem = new SaveSystem();
            _saveSystem._Ready(); // Init singleton
        }

        [CleanupAll]
        public void CleanupAll()
        {
            _saveSystem.QueueFree();
            _gameStateManager.QueueFree();
        }

        [Test]
        public void InitialSaveFileDoesNotExist()
        {
            // Убедимся, что файл старого сохранения удален
            _saveSystem.DeleteSave();
            _saveSystem.SaveFileExists().ShouldBeFalse();
        }

        [Test]
        public void SaveGameWithoutDataFails()
        {
            // Очищаем CurrentSave
            _gameStateManager.StartNewGame();
            var prevSave = GameStateManager.Instance.CurrentSave;
            // Устанавливаем в null
            typeof(GameStateManager).GetProperty("CurrentSave").SetValue(_gameStateManager, null);
            
            bool result = _saveSystem.SaveGame();
            result.ShouldBeFalse();

            // Возвращаем обратно
            typeof(GameStateManager).GetProperty("CurrentSave").SetValue(_gameStateManager, prevSave);
        }

        [Test]
        public void SaveAndLoadGameWorksCorrectly()
        {
            _gameStateManager.StartNewGame();
            var currentSave = GameStateManager.Instance.CurrentSave;
            currentSave.Coins = 100;
            currentSave.CurrentWave = 5;
            
            bool saveResult = _saveSystem.SaveGame();
            saveResult.ShouldBeTrue();
            _saveSystem.SaveFileExists().ShouldBeTrue();

            SaveData loadedData = _saveSystem.LoadGame();
            loadedData.ShouldNotBeNull();
            loadedData.Coins.ShouldBe(100);
            loadedData.CurrentWave.ShouldBe(5);
            loadedData.MageMaxHealth.ShouldBeGreaterThan(0);
        }

        [Test]
        public void DeleteSaveWorksCorrectly()
        {
            _gameStateManager.StartNewGame();
            _saveSystem.SaveGame();
            _saveSystem.SaveFileExists().ShouldBeTrue();

            _saveSystem.DeleteSave();
            _saveSystem.SaveFileExists().ShouldBeFalse();
            _saveSystem.LoadGame().ShouldBeNull();
        }

        [Test]
        public void ValidateSaveDataRejectsInvalidWave()
        {
            _gameStateManager.StartNewGame();
            var currentSave = GameStateManager.Instance.CurrentSave;
            currentSave.CurrentWave = -1; // Invalid

            _saveSystem.SaveGame();

            SaveData loadedData = _saveSystem.LoadGame();
            loadedData.ShouldBeNull("LoadGame should return null for invalid save data");
        }
    }
}
