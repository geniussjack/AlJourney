using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;
using AlJourney.Scripts.Interfaces;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlJourney.Scripts.Managers
{
    /// <summary>
    /// Менеджер SaveSystem. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class SaveSystem : Node, ISaveSystem
    {
        /// <summary>
        /// Элемент Instance.
        /// </summary>
        public static SaveSystem Instance { get; private set; } = null!;

        [Signal]
        /// <summary>
        /// Сохраняет CompletedEventHandler.
        /// </summary>
        public delegate void SaveCompletedEventHandler(bool success);

        [Signal]
        /// <summary>
        /// Загружает CompletedEventHandler.
        /// </summary>
        public delegate void LoadCompletedEventHandler(bool success);

        private string _savePath;

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            if (Instance is not null)
            {
                QueueFree();
                return;
            }

            Instance = this;
            _savePath = GameConstants.SAVE_DIRECTORY + GameConstants.SAVE_FILE_NAME;

            if (DirAccess.MakeDirRecursiveAbsolute(GameConstants.SAVE_DIRECTORY) is Error.Ok)
            {
                GD.Print($"[SaveSystem] Initialized. Save path: {_savePath}");
            }
            else
            {
                GD.PrintErr($"[SaveSystem] Failed to create save directory: {GameConstants.SAVE_DIRECTORY}");
            }
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Сохраняет Game.
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                SaveData saveData = GameStateManager.Instance.CurrentSave;
                if (saveData == null)
                {
                    GD.PrintErr("[SaveSystem] No active save data to save");
                    _ = EmitSignal(SignalName.SaveCompleted, false);
                    return false;
                }

                InventoryManager.Instance?.SaveToData(saveData);

                saveData.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string jsonData = JsonSerializer.Serialize(saveData, JsonOptions);

                using FileAccess file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"[SaveSystem] Failed to open save file: {FileAccess.GetOpenError()}");
                    _ = EmitSignal(SignalName.SaveCompleted, false);
                    return false;
                }

                _ = file.StoreString(jsonData);
                file.Close();

                GD.Print($"[SaveSystem] Game saved successfully - Wave {saveData.CurrentWave}");
                _ = EmitSignal(SignalName.SaveCompleted, true);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Save failed: {e.Message}");
                _ = EmitSignal(SignalName.SaveCompleted, false);
                return false;
            }
        }

        /// <summary>
        /// Загружает Game.
        /// </summary>
        public SaveData LoadGame()
        {
            try
            {
                if (!FileAccess.FileExists(_savePath))
                {
                    GD.Print("[SaveSystem] No save file found");
                    _ = EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                using FileAccess file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[SaveSystem] Failed to open save file: {FileAccess.GetOpenError()}");
                    _ = EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                string jsonData = file.GetAsText();
                file.Close();

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    GD.PrintErr("[SaveSystem] Save file is empty");
                    _ = EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                SaveData saveData = null;
                try
                {
                    saveData = JsonSerializer.Deserialize<SaveData>(jsonData, JsonOptions);
                }
                catch (JsonException jsonEx)
                {
                    GD.PrintErr($"[SaveSystem] JSON deserialization failed: {jsonEx.Message}");
                    GD.PrintErr("[SaveSystem] Save file may be corrupted");
                    _ = EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                if (saveData == null)
                {
                    GD.PrintErr("[SaveSystem] Deserialized save data is null");
                    _ = EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                if (saveData.SchemaVersion != 1)
                {
                    GD.Print($"[SaveSystem] Outdated save schema (v{saveData.SchemaVersion}), attempting migration");
                    saveData = SaveData.Migrate(saveData);

                    if (saveData == null)
                    {
                        GD.PrintErr("[SaveSystem] Save migration failed");
                        _ = EmitSignal(SignalName.LoadCompleted, false);
                        return null;
                    }
                }

                if (!ValidateSaveData(saveData))
                {
                    GD.PrintErr("[SaveSystem] Save data validation failed - corrupted save");
                    _ = EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                GD.Print($"[SaveSystem] Game loaded successfully - Wave {saveData.CurrentWave}");
                _ = EmitSignal(SignalName.LoadCompleted, true);
                return saveData;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Load failed with exception: {e.Message}");
                GD.PrintErr($"[SaveSystem] Stack trace: {e.StackTrace}");
                _ = EmitSignal(SignalName.LoadCompleted, false);
                return null;
            }
        }

        /// <summary>
        /// Сохраняет FileExists.
        /// </summary>
        public bool SaveFileExists()
        {
            return FileAccess.FileExists(_savePath);
        }

        /// <summary>
        /// Удаляет Save.
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

                _ = DirAccess.RemoveAbsolute(_savePath);
                GD.Print("[SaveSystem] Save file deleted");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Failed to delete save: {e.Message}");
                return false;
            }
        }

        private static bool ValidateSaveData(SaveData data)
        {
            if (data == null)
            {
                GD.PrintErr("[SaveSystem] Validation failed: Save data is null");
                return false;
            }

            if (data.CurrentWave < 1)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid wave number ({data.CurrentWave})");
                return false;
            }

            if (data.MageMaxHealth <= 0 || data.MageHealth < 0 || data.MageHealth > data.MageMaxHealth)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid Mage health ({data.MageHealth}/{data.MageMaxHealth})");
                return false;
            }

            if (data.MageDamage < 0 || data.MageDefense < 0)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid Mage stats (Dmg:{data.MageDamage}, Def:{data.MageDefense})");
                return false;
            }

            if (data.WarriorMaxHealth <= 0 || data.WarriorHealth < 0 || data.WarriorHealth > data.WarriorMaxHealth)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid Warrior health ({data.WarriorHealth}/{data.WarriorMaxHealth})");
                return false;
            }

            if (data.WarriorDamage < 0 || data.WarriorDefense < 0)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid Warrior stats (Dmg:{data.WarriorDamage}, Def:{data.WarriorDefense})");
                return false;
            }

            if (data.Coins < 0)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid coins ({data.Coins})");
                return false;
            }

            if (data.HighestWave < 1 || data.HighestWave < data.CurrentWave)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid highest wave ({data.HighestWave})");
                return false;
            }

            GD.Print("[SaveSystem] Save data validation passed");
            return true;
        }

        /// <summary>
        /// Элемент AutoSave.
        /// </summary>
        public void AutoSave()
        {
            if (GameStateManager.Instance.IsGameActive)
            {
                _ = SaveGame();
                GD.Print("[SaveSystem] Auto-save completed");
            }
        }
    }
}
