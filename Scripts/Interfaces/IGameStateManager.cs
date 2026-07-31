using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for the game's global state.
    /// Responsible for high-level game management: saving/loading, moving between waves, and managing the economy and hero stats.
    /// </summary>
    public interface IGameStateManager
    {
        /// <summary>
        /// The current global game state.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// The current save data, including the player's progress.
        /// </summary>
        SaveData CurrentSave { get; }

        /// <summary>
        /// The current enemy wave the player has reached.
        /// </summary>
        int CurrentWave { get; }

        /// <summary>
        /// Id of the campaign map level the player is currently on or should attempt next.
        /// </summary>
        string CurrentLevelId { get; }

        /// <summary>
        /// Ids of every campaign level already completed.
        /// </summary>
        IReadOnlyCollection<string> CompletedLevelIds { get; }

        /// <summary>
        /// The number of coins available to the player.
        /// </summary>
        int Coins { get; }

        /// <summary>
        /// Returns true if there is currently an active game session.
        /// </summary>
        bool IsGameActive { get; }

        /// <summary>
        /// Starts a new game, resetting progress to its initial state.
        /// </summary>
        void StartNewGame();

        /// <summary>
        /// Loads a game from the provided save data.
        /// </summary>
        void LoadGame(SaveData saveData);

        /// <summary>
        /// Advances to the next enemy wave.
        /// </summary>
        void NextWave();

        /// <summary>
        /// Marks the level selected on the campaign map as the current one, without starting it immediately.
        /// </summary>
        void SelectLevel(string levelId);

        /// <summary>
        /// Begins an attempt at the given campaign map level.
        /// </summary>
        void StartLevel(LevelDefinition level);

        /// <summary>
        /// Marks a campaign map level as completed and, if applicable, advances progress further.
        /// </summary>
        void CompleteLevel(string levelId);

        /// <summary>
        /// Grants the player the specified number of coins.
        /// </summary>
        void AddCoins(int amount);

        /// <summary>
        /// Attempts to spend the specified number of coins.
        /// Returns true if the player had enough funds and they were successfully spent.
        /// </summary>
        bool SpendCoins(int amount);

        /// <summary>
        /// Updates the base and current stats of both heroes.
        /// </summary>
        void UpdateHeroStats(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense);

        /// <summary>
        /// Changes the current global game state to a new one.
        /// </summary>
        void ChangeState(GameState newState);

        /// <summary>
        /// Ends the game with the given outcome.
        /// </summary>
        void EndGame(bool isVictory);

        /// <summary>
        /// Returns to the game's main menu from the current state.
        /// </summary>
        void ReturnToMainMenu();
    }
}
