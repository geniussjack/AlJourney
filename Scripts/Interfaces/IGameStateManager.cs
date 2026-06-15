using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для глобального состояния игры.
    /// Отвечает за высокоуровневое управление игрой: сохранение/загрузку, переход между волнами, управление экономикой и характеристиками героев.
    /// </summary>
    public interface IGameStateManager
    {
        /// <summary>
        /// Текущее глобальное состояние игры.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Текущие данные сохранения, включающие прогресс игрока.
        /// </summary>
        SaveData CurrentSave { get; }

        /// <summary>
        /// Текущая волна врагов, до которой дошел игрок.
        /// </summary>
        int CurrentWave { get; }

        /// <summary>
        /// Количество доступных игроку монет.
        /// </summary>
        int Coins { get; }

        /// <summary>
        /// Возвращает значение true, если в данный момент идет активная игровая сессия.
        /// </summary>
        bool IsGameActive { get; }

        /// <summary>
        /// Начинает новую игру, сбрасывая прогресс до начального состояния.
        /// </summary>
        void StartNewGame();

        /// <summary>
        /// Загружает игру на основе предоставленных данных сохранения.
        /// </summary>
        void LoadGame(SaveData saveData);

        /// <summary>
        /// Совершает переход к следующей волне врагов.
        /// </summary>
        void NextWave();

        /// <summary>
        /// Начисляет игроку указанное количество монет.
        /// </summary>
        void AddCoins(int amount);

        /// <summary>
        /// Пытается списать указанное количество монет.
        /// Возвращает true, если средств достаточно и они успешно списаны.
        /// </summary>
        bool SpendCoins(int amount);

        /// <summary>
        /// Обновляет базовые и текущие характеристики обоих героев.
        /// </summary>
        void UpdateHeroStats(int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense, int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense);

        /// <summary>
        /// Изменяет текущее глобальное состояние игры на новое.
        /// </summary>
        void ChangeState(GameState newState);

        /// <summary>
        /// Завершает игру с указанием результата.
        /// </summary>
        void EndGame(bool isVictory);

        /// <summary>
        /// Осуществляет возврат в главное меню игры из текущего состояния.
        /// </summary>
        void ReturnToMainMenu();
    }
}
