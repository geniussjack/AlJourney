using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for the save system.
    /// Provides functionality to save and load the player's progress to/from a file, as well as auto-saving.
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// Saves the current game progress to a file.
        /// Returns true on a successful save.
        /// </summary>
        bool SaveGame();

        /// <summary>
        /// Loads the player's progress from the save file.
        /// Returns the loaded data, or null if no save was found or it was corrupted.
        /// </summary>
        SaveData LoadGame();

        /// <summary>
        /// Deletes the current save file.
        /// Returns true on a successful deletion.
        /// </summary>
        bool DeleteSave();

        /// <summary>
        /// Checks whether a save file exists on the device.
        /// </summary>
        bool SaveFileExists();

        /// <summary>
        /// Performs an automatic background save of the game.
        /// </summary>
        void AutoSave();
    }
}
