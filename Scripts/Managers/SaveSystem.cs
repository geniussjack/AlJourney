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
    /// Глобальный менеджер сохранения и загрузки прогресса.
    /// Отвечает за сериализацию игровых данных (статистика, инвентарь) 
    /// в JSON формат и чтение из локального хранилища.
    /// </summary>
    public partial class SaveSystem : Node, ISaveSystem
    {
        /// <summary>
        /// Глобальный доступ к синглтону системы сохранений.
        /// </summary>
        public static SaveSystem Instance { get; private set; } = null!;

        /// <summary>
        /// Вызывается после завершения операции сохранения.
        /// </summary>
        /// <param name="success">True, если сохранение прошло успешно.</param>
        [Signal]
        public delegate void SaveCompletedEventHandler(bool success);

        /// <summary>
        /// Вызывается после завершения операции загрузки данных.
        /// </summary>
        /// <param name="success">True, если загрузка данных и их валидация прошли успешно.</param>
        [Signal]
        public delegate void LoadCompletedEventHandler(bool success);

        private string _savePath;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Инициализация синглтона и создание директории сохранений, если она не существует.
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

        /// <summary>
        /// Сохраняет текущее состояние игры в файл (JSON).
        /// Включает прогресс волн, характеристики героев и состояние инвентаря.
        /// </summary>
        /// <returns>True, если сохранение завершилось успешно.</returns>
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
        /// Читает и десериализует файл сохранения. 
        /// Если структура устарела, пытается произвести миграцию. 
        /// Проводит валидацию целостности данных.
        /// </summary>
        /// <returns>Загруженный объект SaveData, либо null в случае ошибки.</returns>
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

                string jsonData;
                using (FileAccess file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Read))
                {
                    if (file == null)
                    {
                        GD.PrintErr($"[SaveSystem] Failed to open save file: {FileAccess.GetOpenError()}");
                        _ = EmitSignal(SignalName.LoadCompleted, false);
                        return null;
                    }

                    jsonData = file.GetAsText();
                }

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    GD.PrintErr("[SaveSystem] Save file is empty");
                    _ = EmitSignal(SignalName.LoadCompleted, false);
                    return null;
                }

                SaveData saveData = DeserializeAndMigrate(jsonData);

                if (saveData == null || !ValidateSaveData(saveData))
                {
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

        private SaveData DeserializeAndMigrate(string jsonData)
        {
            SaveData saveData = null;
            try
            {
                saveData = JsonSerializer.Deserialize<SaveData>(jsonData, JsonOptions);
            }
            catch (JsonException jsonEx)
            {
                GD.PrintErr($"[SaveSystem] JSON deserialization failed: {jsonEx.Message}");
                GD.PrintErr("[SaveSystem] Save file may be corrupted");
                return null;
            }

            if (saveData == null)
            {
                GD.PrintErr("[SaveSystem] Deserialized save data is null");
                return null;
            }

            if (saveData.SchemaVersion != 1)
            {
                GD.Print($"[SaveSystem] Outdated save schema (v{saveData.SchemaVersion}), attempting migration");
                saveData = SaveData.Migrate(saveData);

                if (saveData == null)
                {
                    GD.PrintErr("[SaveSystem] Save migration failed");
                    return null;
                }
            }

            return saveData;
        }

        /// <summary>
        /// Проверяет физическое существование файла сохранения.
        /// </summary>
        public bool SaveFileExists()
        {
            return FileAccess.FileExists(_savePath);
        }

        /// <summary>
        /// Удаляет текущий файл сохранения без возможности восстановления.
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

            if (!ValidateProgression(data) || !ValidateHeroStats(data))
            {
                return false;
            }

            GD.Print("[SaveSystem] Save data validation passed");
            return true;
        }

        private static bool ValidateProgression(SaveData data)
        {
            if (data.CurrentWave < 1 || data.HighestWave < 1 || data.HighestWave < data.CurrentWave)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid wave progress (Current:{data.CurrentWave}, Highest:{data.HighestWave})");
                return false;
            }

            if (data.Coins < 0)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid coins ({data.Coins})");
                return false;
            }

            return true;
        }

        private static bool ValidateHeroStats(SaveData data)
        {
            if (!ValidateHero(data.MageHealth, data.MageMaxHealth, data.MageDamage, data.MageDefense, "Mage")) return false;
            if (!ValidateHero(data.WarriorHealth, data.WarriorMaxHealth, data.WarriorDamage, data.WarriorDefense, "Warrior")) return false;
            
            return true;
        }

        private static bool ValidateHero(int health, int maxHealth, int damage, int defense, string heroName)
        {
            if (maxHealth <= 0 || health < 0 || health > maxHealth)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid {heroName} health ({health}/{maxHealth})");
                return false;
            }

            if (damage < 0 || defense < 0)
            {
                GD.PrintErr($"[SaveSystem] Validation failed: Invalid {heroName} stats (Dmg:{damage}, Def:{defense})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Выполняет сохранение, если игра находится в активном состоянии.
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
