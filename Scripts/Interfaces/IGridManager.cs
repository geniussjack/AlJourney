using AlJourney.Scripts.Match3;
using System.Collections.Generic;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс управления игровым полем Match-3.
    /// Отвечает за логику сетки, перестановку элементов, поиск совпадений и перемешивание поля.
    /// </summary>
    public interface IGridManager
    {
        /// <summary>
        /// Размер игрового поля.
        /// </summary>
        int GridSize { get; }

        /// <summary>
        /// Количество оставшихся у игрока ходов.
        /// </summary>
        int RemainingSwaps { get; }

        /// <summary>
        /// Инициализирует и заполняет игровое поле элементами перед началом игры.
        /// </summary>
        void InitializeGrid();

        /// <summary>
        /// Возвращает данные элемента, расположенного на поле по указанным координатам.
        /// </summary>
        ElementData GetElement(int x, int y);

        /// <summary>
        /// Пытается поменять местами два элемента на поле.
        /// Возвращает true, если перестановка возможна и привела к совпадению.
        /// </summary>
        bool TrySwap(int x1, int y1, int x2, int y2);

        /// <summary>
        /// Ищет все текущие совпадения элементов на поле.
        /// </summary>
        List<MatchResult> FindAllMatches();

        /// <summary>
        /// Обрабатывает найденные совпадения: удаляет элементы, начисляет очки и вызывает падение новых.
        /// </summary>
        void ProcessMatches(List<MatchResult> matches);

        /// <summary>
        /// Сбрасывает количество доступных перестановок к начальному значению для текущего уровня/хода.
        /// </summary>
        void ResetSwaps();

        /// <summary>
        /// Проверяет, остались ли на поле возможные ходы для сбора комбинаций.
        /// </summary>
        bool HasValidMoves();

        /// <summary>
        /// Проверяет наличие возможных ходов, и если их нет — автоматически перемешивает поле.
        /// </summary>
        void CheckAndReshuffleIfNeeded();

        /// <summary>
        /// Возвращает двумерный массив всех элементов на игровом поле.
        /// </summary>
        ElementData[,] GetGrid();
    }
}
