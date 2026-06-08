using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.UI;
using AlJourney.Scripts.Utils;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления логикой боя.
    /// </summary>
    /// <summary>
    /// Менеджер IBattleManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface IBattleManager
    {
        BattlePhase CurrentPhase { get; }
        int CurrentWave { get; }
        DualHeroSystem HeroSystem { get; }

        void Initialize(GridUI gridUI);
        void StartBattle(DualHeroSystem heroSystem, int waveNumber, CameraShake cameraShake = null);
        void EndBattle();
    }
}
