using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Interface for managing scenes in the game.
    /// Responsible for loading, switching and reloading game scenes based on state or path.
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        /// Loads the scene corresponding to the given global game state.
        /// </summary>
        void LoadScene(GameState state);

        /// <summary>
        /// Loads a specific scene by its path in the project.
        /// </summary>
        void LoadSceneByPath(string scenePath);

        /// <summary>
        /// Requests a deferred scene change to the given path.
        /// Safe to call while processing physics or signals.
        /// </summary>
        void DeferredSceneChange(string scenePath);

        /// <summary>
        /// Reloads the currently active scene.
        /// </summary>
        void ReloadCurrentScene();
    }
}
