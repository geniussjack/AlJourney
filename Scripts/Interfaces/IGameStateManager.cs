using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для глобального состояния игры.
    /// </summary>
    /// <summary>
    /// Менеджер IGameStateManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface IGameStateManager
    {
        GameState CurrentState { get; }
        SaveData CurrentSave { get; }
        int CurrentWave { get; }
        int Coins { get; }
        bool IsGameActive { get; }

        void StartNewGame();
        void LoadGame(SaveData saveData);
        void NextWave();
        void AddCoins(int amount);
        bool SpendCoins(int amount);
        void UpdateHeroStats(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense);
        void ChangeState(GameState newState);
        void EndGame(bool isVictory);
        void ReturnToMainMenu();
    }
}
