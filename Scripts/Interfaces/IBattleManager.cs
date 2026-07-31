using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Utils;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления логикой пошагового боя.
    /// Контролирует фазы битвы, номер волны врагов и текущее состояние выбора хода игрока
    /// (выбранный боец → выбранная способность → цель).
    /// </summary>
    public interface IBattleManager
    {
        /// <summary>
        /// Текущая фаза боя.
        /// </summary>
        BattlePhase CurrentPhase { get; }

        /// <summary>
        /// Сложность текущего уровня, единая для всех его волн (см. <see cref="LevelDefinition.DifficultyRating"/>).
        /// </summary>
        int CurrentWave { get; }

        /// <summary>
        /// Индекс текущей волны (с нуля) внутри волн текущего уровня.
        /// </summary>
        int CurrentWaveIndex { get; }

        /// <summary>
        /// Общее количество волн в текущем уровне.
        /// </summary>
        int TotalWavesInLevel { get; }

        /// <summary>
        /// Система управления отрядом героев, участвующих в битве.
        /// </summary>
        DualHeroSystem HeroSystem { get; }

        /// <summary>
        /// Участники отряда, которые ещё не совершили ход в текущем раунде.
        /// </summary>
        IReadOnlyList<PlayerCharacter> PendingActors { get; }

        /// <summary>
        /// Боец, выбранный игроком для текущего хода (либо null, если выбор ещё не сделан).
        /// </summary>
        PlayerCharacter SelectedActor { get; }

        /// <summary>
        /// Способность, выбранная для текущего хода (либо null, если выбор ещё не сделан).
        /// </summary>
        AbilityData SelectedAbility { get; }

        /// <summary>
        /// Текущее значение общего заряда ульты отряда.
        /// </summary>
        int UltimateCharge { get; }

        /// <summary>
        /// Истина, если заряд ульты полон и она доступна к применению.
        /// </summary>
        bool IsUltimateReady { get; }

        /// <summary>
        /// Выбирает бойца, который совершит ход следующим (порядок хода определяет игрок).
        /// </summary>
        void SelectActor(PlayerCharacter actor);

        /// <summary>
        /// Выбирает способность, которую применит выбранный боец.
        /// </summary>
        void SelectAbility(AbilityData ability);

        /// <summary>
        /// Возвращает список допустимых целей для наведения выбранной способности.
        /// </summary>
        IReadOnlyList<Character> GetValidTargets();

        /// <summary>
        /// Подтверждает цель и немедленно разрешает эффект выбранной способности.
        /// </summary>
        void ConfirmTarget(Character target);

        /// <summary>
        /// Запускает начало битвы для указанного уровня карты кампании, настраивая героев и опционально
        /// применяя эффекты тряски камеры.
        /// </summary>
        void StartBattle(DualHeroSystem heroSystem, LevelDefinition level, CameraShake cameraShake = null);

        /// <summary>
        /// Завершает текущую битву, очищая ресурсы и подводя итоги столкновения.
        /// </summary>
        void EndBattle();
    }
}
