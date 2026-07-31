using AlJourney.Scripts.Characters;
using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Utils;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for managing turn-based combat logic.
    /// Controls the battle phases, the enemy wave number, and the current state of the player's turn
    /// selection (selected actor → selected ability → target).
    /// </summary>
    public interface IBattleManager
    {
        /// <summary>
        /// The current battle phase.
        /// </summary>
        BattlePhase CurrentPhase { get; }

        /// <summary>
        /// The current level's difficulty, shared by all of its waves (see <see cref="LevelDefinition.DifficultyRating"/>).
        /// </summary>
        int CurrentWave { get; }

        /// <summary>
        /// The current wave's index (zero-based) among the current level's waves.
        /// </summary>
        int CurrentWaveIndex { get; }

        /// <summary>
        /// The total number of waves in the current level.
        /// </summary>
        int TotalWavesInLevel { get; }

        /// <summary>
        /// The party system managing the heroes participating in the battle.
        /// </summary>
        DualHeroSystem HeroSystem { get; }

        /// <summary>
        /// Party members who have not yet acted in the current round.
        /// </summary>
        IReadOnlyList<PlayerCharacter> PendingActors { get; }

        /// <summary>
        /// The actor selected by the player for the current turn (or null if no selection has been made yet).
        /// </summary>
        PlayerCharacter SelectedActor { get; }

        /// <summary>
        /// The ability selected for the current turn (or null if no selection has been made yet).
        /// </summary>
        AbilityData SelectedAbility { get; }

        /// <summary>
        /// The party's current total ultimate charge.
        /// </summary>
        int UltimateCharge { get; }

        /// <summary>
        /// True if the ultimate charge is full and the ultimate is ready to use.
        /// </summary>
        bool IsUltimateReady { get; }

        /// <summary>
        /// Selects the actor who will take the next turn (the player determines turn order).
        /// </summary>
        void SelectActor(PlayerCharacter actor);

        /// <summary>
        /// Selects the ability the chosen actor will use.
        /// </summary>
        void SelectAbility(AbilityData ability);

        /// <summary>
        /// Returns the list of valid targets for the selected ability's targeting.
        /// </summary>
        IReadOnlyList<Character> GetValidTargets();

        /// <summary>
        /// Confirms the target and immediately resolves the selected ability's effect.
        /// </summary>
        void ConfirmTarget(Character target);

        /// <summary>
        /// Starts the battle for the given campaign map level, setting up the heroes and optionally
        /// applying camera shake effects.
        /// </summary>
        void StartBattle(DualHeroSystem heroSystem, LevelDefinition level, CameraShake cameraShake = null);

        /// <summary>
        /// Ends the current battle, releasing resources and wrapping up the encounter.
        /// </summary>
        void EndBattle();
    }
}
