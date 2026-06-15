using AlJourney.Scripts.Core;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для управления сценами в игре.
    /// Отвечает за загрузку, смену и перезагрузку игровых сцен на основе состояний или путей.
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        /// Загружает сцену, соответствующую указанному глобальному состоянию игры.
        /// </summary>
        void LoadScene(GameState state);

        /// <summary>
        /// Загружает конкретную сцену по её пути в проекте.
        /// </summary>
        void LoadSceneByPath(string scenePath);

        /// <summary>
        /// Запрашивает отложенную смену сцены по указанному пути.
        /// Безопасно для вызова в процессе обработки физики или сигналов.
        /// </summary>
        void DeferredSceneChange(string scenePath);

        /// <summary>
        /// Перезагружает текущую активную сцену.
        /// </summary>
        void ReloadCurrentScene();
    }
}
