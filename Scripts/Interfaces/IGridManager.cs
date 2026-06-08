using System.Collections.Generic;
using AlJourney.Scripts.Match3;

namespace AlJourney.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс управления полем Match-3.
    /// </summary>
    /// <summary>
    /// Менеджер IGridManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public interface IGridManager
    {
        int GridSize { get; }
        int RemainingSwaps { get; }

        void InitializeGrid();
        ElementData GetElement(int x, int y);
        bool TrySwap(int x1, int y1, int x2, int y2);
        List<MatchResult> FindAllMatches();
        void ProcessMatches(List<MatchResult> matches);
        void ResetSwaps();
        bool HasValidMoves();
        void CheckAndReshuffleIfNeeded();
        ElementData[,] GetGrid();
    }
}
