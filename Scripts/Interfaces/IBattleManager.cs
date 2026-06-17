using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.UI;
using AlJourney.Scripts.Utils;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления логикой боя.
    /// Контролирует фазы битвы, номера волн врагов и инициализацию боевых систем.
    /// </summary>
    public interface IBattleManager
    {
        /// <summary>
        /// Текущая фаза боя.
        /// </summary>
        BattlePhase CurrentPhase { get; }

        /// <summary>
        /// Номер текущей волны врагов в рамках боя.
        /// </summary>
        int CurrentWave { get; }

        /// <summary>
        /// Система управления дуэтом героев, участвующих в битве.
        /// </summary>
        DualHeroSystem HeroSystem { get; }

        /// <summary>
        /// Инициализирует менеджер боя, связывая его с интерфейсом игрового поля.
        /// </summary>
        void Initialize(GridUI gridUI);

        /// <summary>
        /// Запускает начало битвы для указанной волны, настраивая героев и опционально применяя эффекты тряски камеры.
        /// </summary>
        void StartBattle(DualHeroSystem heroSystem, int waveNumber, CameraShake cameraShake = null);

        /// <summary>
        /// Завершает текущую битву, очищая ресурсы и подводя итоги столкновения.
        /// </summary>
        void EndBattle();
    }
}
