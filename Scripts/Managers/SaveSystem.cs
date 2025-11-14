using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using System;
using System.Text.Json;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Handles saving and loading game data to/from JSON files.
    /// Singleton autoload node.
    /// </summary>
    public partial class SaveSystem : Node
    {
        private static SaveSystem _instance;

        /// <summary>
        /// Singleton instance accessor.
        /// </summary>
        public static SaveSystem Instance => _instance;

        [Signal]
        public delegate void SaveCompletedEventHandler(bool success);

        [Signal]
        public delegate void LoadCompletedEventHandler(bool success);

        private string _savePath;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }

            _instance = this;
            _savePath = GameConstants.SAVE_DIRECTORY + GameConstants.SAVE_FILE_NAME;

            // Ensure save directory exists
            DirAccess.MakeDirRecursiveAbsolute(GameConstants.SAVE_DIRECTORY);

            GD.Print($"[SaveSystem] Initialized. Save path: {_savePath}");
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = false
        };

        /// <summary>
        /// Saves current game state to JSON file.
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                var saveData = GameStateManager.Instance.CurrentSave;
                if (saveData == null)
                {
                    GD.PrintErr("[SaveSystem] No active save data to save");
                    EmitSignal(SignalName.SaveCompleted, false);
                    return false;
                }

                // Update timestamp
                saveData.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Serialize to JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = false
                };

                string jsonData = JsonSerializer.Serialize(saveData, _jsonOptions);

                // Write to file
                using var file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"[SaveSystem] Failed to open save file: {FileAccess.GetOpenError()}");
                    EmitSignal(SignalName.SaveCompleted, false);
                    return false;
                }

                file.StoreString(jsonData);
                file.Close();

                GD.Print($"[SaveSystem] Game saved successfully - Wave {saveData.CurrentWave}");
                EmitSignal(SignalName.SaveCompleted, true);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Save failed: {e.Message}");
                EmitSignal(SignalName.SaveCompleted, false);
                return false;
            }
        }

        /// <summary>
        /// Loads game state from JSON file.
        /// </summary>
        public SaveData LoadGame()
        {
            try
            {
                if (!FileAccess.FileExists(_savePath))
                {
                    GD.Print("[SaveSystem] No save file found");
                    EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                // Read file
                using var file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[SaveSystem] Failed to open save file: {FileAccess.GetOpenError()}");
                    EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                string jsonData = file.GetAsText();
                file.Close();

                // Deserialize
                var saveData = JsonSerializer.Deserialize<SaveData>(jsonData);

                if (saveData != null)
                {
                    GD.Print($"[SaveSystem] Game loaded successfully - Wave {saveData.CurrentWave}");
                    EmitSignal(SignalName.LoadCompleted, true);
                    return saveData;
                }
                else
                {
                    GD.PrintErr("[SaveSystem] Failed to deserialize save data");
                    EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Load failed: {e.Message}");
                EmitSignal(SignalName.LoadCompleted, false);
                return null;
            }
        }

        /// <summary>
        /// Checks if a save file exists.
        /// </summary>
        public bool SaveFileExists()
        {
            return FileAccess.FileExists(_savePath);
        }

        /// <summary>
        /// Deletes the current save file.
        /// </summary>
        public bool DeleteSave()
        {
            try
            {
                if (!FileAccess.FileExists(_savePath))
                {
                    GD.Print("[SaveSystem] No save file to delete");
                    return true;
                }

                DirAccess.RemoveAbsolute(_savePath);
                GD.Print("[SaveSystem] Save file deleted");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Failed to delete save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Auto-saves the game (called after wave completion).
        /// </summary>
        public void AutoSave()
        {
            if (GameStateManager.Instance.IsGameActive)
            {
                SaveGame();
                GD.Print("[SaveSystem] Auto-save completed");
            }
        }
    }
}