using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления сценами.
    /// </summary>
    /// <summary>
    /// Менеджер ISceneManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface ISceneManager
    {
        void LoadScene(GameState state);
        void LoadSceneByPath(string scenePath);
        void DeferredSceneChange(string scenePath);
        void ReloadCurrentScene();
    }
}
