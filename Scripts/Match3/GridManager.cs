using AlJourney.Scripts.Core;
using AlJourney.Scripts.Interfaces;
using AlJourney.Scripts.Managers;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Глобальный синглтон-менеджер, управляющий логикой игрового поля.
    /// Отвечает за генерацию элементов, проверку возможных ходов, поиск совпадений
    /// и применение гравитации при исчезновении камней.
    /// Не содержит визуальной логики.
    /// </summary>
    public partial class GridManager : Node, IGridManager
    {
        /// <summary>
        /// Сигнал вызывается после первичной генерации доски или её полного обновления.
        /// </summary>
        [Signal]
        public delegate void GridInitializedEventHandler();

        /// <summary>
        /// Вызывается после попытки игрока поменять два элемента местами.
        /// </summary>
        /// <param name="wasValid">True, если свап привел к совпадению и был успешен; False, если элементы вернулись назад.</param>
        [Signal]
        public delegate void SwapCompletedEventHandler(bool wasValid);

        /// <summary>
        /// Вызывается, когда на поле найдены совпадения.
        /// </summary>
        /// <param name="matchCount">Количество уникальных комбинаций, найденных за этот проход.</param>
        [Signal]
        public delegate void MatchesFoundEventHandler(int matchCount);

        /// <summary>
        /// Вызывается после того, как пустые места заполнены новыми элементами,
        /// что может запустить новую цепную реакцию.
        /// </summary>
        [Signal]
        public delegate void GridRefillCompletedEventHandler();

        private ElementData[,] _grid;

        /// <summary>
        /// Размер игрового поля по ширине и высоте.
        /// </summary>
        public int GridSize { get; private set; }

        /// <summary>
        /// Количество доступных обменов в текущий ход игрока.
        /// Если опускается до 0, ход считается завершенным.
        /// </summary>
        public int RemainingSwaps { get; private set; }

        /// <summary>
        /// Инициализация параметров менеджера при запуске игры.
        /// </summary>
        public override void _Ready()
        {
            GridSize = GameConstants.GRID_SIZE;
            _grid = new ElementData[GridSize, GridSize];
            RemainingSwaps = 0;

            GD.Print("[GridManager] Initialized");
        }

        /// <summary>
        /// Полностью очищает и заново генерирует игровое поле.
        /// Гарантирует, что на сгенерированном поле не будет изначальных
        /// совпадений 3-в-ряд и будут доступны возможные ходы.
        /// </summary>
        public void InitializeGrid()
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    _grid[x, y] = GenerateSafeElement(x, y);
                }
            }

            RemainingSwaps = GameConstants.PLAYER_SWAPS_PER_TURN;
            _ = EmitSignal(SignalName.GridInitialized);

            GD.Print("[GridManager] Grid initialized with no initial matches");
        }

        private ElementData GenerateSafeElement(int x, int y)
        {
            ElementType exclude1 = GetHorizontalExclude(x, y);
            ElementType exclude2 = GetVerticalExclude(x, y);

            ElementType selectedType = GetRandomValidType(exclude1, exclude2);
            return new ElementData(selectedType, x, y);
        }

        private ElementType GetHorizontalExclude(int x, int y)
        {
            if (x < 2)
            {
                return ElementType.None;
            }

            ElementType? type1 = _grid[x - 1, y]?.Type;
            ElementType? type2 = _grid[x - 2, y]?.Type;
            return (type1 != null && type1 == type2) ? type1.Value : ElementType.None;
        }

        private ElementType GetVerticalExclude(int x, int y)
        {
            if (y < 2)
            {
                return ElementType.None;
            }

            ElementType? type1 = _grid[x, y - 1]?.Type;
            ElementType? type2 = _grid[x, y - 2]?.Type;
            return (type1 != null && type1 == type2) ? type1.Value : ElementType.None;
        }

        private static ElementType GetRandomValidType(ElementType exclude1, ElementType exclude2)
        {
            ElementType[] allTypes = [ElementType.Fire, ElementType.Heal, ElementType.Sword, ElementType.Shield];
            List<ElementType> validTypes = [];

            foreach (ElementType t in allTypes)
            {
                if (t != exclude1 && t != exclude2 && t != ElementType.None)
                {
                    validTypes.Add(t);
                }
            }

            return validTypes.Count == 0 ? (ElementType)GD.RandRange(1, 4) : validTypes[GD.RandRange(0, validTypes.Count - 1)];
        }

        /// <summary>
        /// Возвращает логический объект элемента по заданным координатам.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <returns>Объект ElementData, либо null, если координаты выходят за пределы поля.</returns>
        public ElementData GetElement(int x, int y)
        {
            return !GridValidator.IsValidPosition(x, y, GridSize) ? null : _grid[x, y];
        }

        /// <summary>
        /// Выполняет попытку поменять два соседних элемента местами.
        /// Если обмен не приводит ни к одному совпадению,
        /// элементы возвращаются на свои исходные позиции.
        /// </summary>
        /// <param name="x1">X первой ячейки.</param>
        /// <param name="y1">Y первой ячейки.</param>
        /// <param name="x2">X второй ячейки.</param>
        /// <param name="y2">Y второй ячейки.</param>
        /// <returns>True, если свап прошел успешно и образовал комбо. Иначе False.</returns>
        public bool TrySwap(int x1, int y1, int x2, int y2)
        {
            if (RemainingSwaps <= 0 || !GridValidator.IsValidPosition(x1, y1, GridSize) || !GridValidator.IsValidPosition(x2, y2, GridSize))
            {
                return false;
            }

            if (!ArePositionsAdjacent(x1, y1, x2, y2))
            {
                return false;
            }

            SwapElements(x1, y1, x2, y2);
            AudioManager.Instance?.PlaySwapSound();

            if (FindAllMatches().Count > 0)
            {
                RemainingSwaps--;
                _ = EmitSignal(SignalName.SwapCompleted, true);
                return true;
            }

            // Откат свапа, если нет совпадений
            SwapElements(x1, y1, x2, y2);
            AudioManager.Instance?.PlaySwapSound();
            _ = EmitSignal(SignalName.SwapCompleted, false);
            return false;
        }

        private static bool ArePositionsAdjacent(int x1, int y1, int x2, int y2)
        {
            int deltaX = Mathf.Abs(x2 - x1);
            int deltaY = Mathf.Abs(y2 - y1);
            return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        }

        private void SwapElements(int x1, int y1, int x2, int y2)
        {
            (_grid[x1, y1], _grid[x2, y2]) = (_grid[x2, y2], _grid[x1, y1]);

            _grid[x1, y1].X = x1;
            _grid[x1, y1].Y = y1;
            _grid[x2, y2].X = x2;
            _grid[x2, y2].Y = y2;
        }

        /// <summary>
        /// Сканирует всю сетку по горизонтали и вертикали в поисках линий
        /// из 3 или более одинаковых камней.
        /// </summary>
        /// <returns>Список объектов MatchResult, каждый из которых содержит информацию о собранной линии.</returns>
        public List<MatchResult> FindAllMatches()
        {
            return MatchDetector.FindAllMatches(_grid, GridSize);
        }
        public void ProcessMatches(List<MatchResult> matches)
        {
            if (matches == null || matches.Count == 0)
            {
                return;
            }

            MarkMatchedElements(matches);
            ClearMatchedElements();
            ApplyGravity();
            RefillGrid();
        }

        private void MarkMatchedElements(List<MatchResult> matches)
        {
            foreach (MatchResult match in matches)
            {
                if (match?.MatchedPositions == null)
                {
                    continue;
                }

                foreach ((int x, int y) in match.MatchedPositions)
                {
                    if (GridValidator.IsValidPosition(x, y, GridSize) && _grid[x, y] != null)
                    {
                        _grid[x, y].IsMatched = true;
                    }
                }
            }
        }

        private void ClearMatchedElements()
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (_grid[x, y]?.IsMatched == true)
                    {
                        _grid[x, y] = null;
                    }
                }
            }
        }

        private void ApplyGravity()
        {
            for (int x = 0; x < GridSize; x++)
            {
                int writePos = GridSize - 1;

                for (int y = GridSize - 1; y >= 0; y--)
                {
                    if (_grid[x, y] != null)
                    {
                        if (y != writePos)
                        {
                            _grid[x, writePos] = _grid[x, y];
                            _grid[x, writePos].Y = writePos;
                            _grid[x, y] = null;
                        }
                        writePos--;
                    }
                }
            }
        }

        private void RefillGrid()
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (_grid[x, y] == null)
                    {
                        _grid[x, y] = ElementData.CreateRandom(x, y);
                    }
                }
            }

            _ = EmitSignal(SignalName.GridRefillCompleted);
            CheckAndReshuffleIfNeeded();
        }

        /// <summary>
        /// Восстанавливает лимит доступных обменов для игрока.
        /// </summary>
        public void ResetSwaps()
        {
            RemainingSwaps = GameConstants.PLAYER_SWAPS_PER_TURN;
        }

        public void CheckAndReshuffleIfNeeded()
        {
            if (!HasValidMoves())
            {
                GD.Print("[GridManager] No valid moves - reshuffling board");
                ReshuffleBoard();
            }
        }

        private void ReshuffleBoard()
        {
            const int maxAttempts = 10;

            for (int attempts = 0; attempts < maxAttempts; attempts++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    for (int y = 0; y < GridSize; y++)
                    {
                        _grid[x, y] = GenerateSafeElement(x, y);
                    }
                }

                if (HasValidMoves())
                {
                    GD.Print($"[GridManager] Reshuffle succeeded on attempt {attempts + 1}");
                    _ = EmitSignal(SignalName.GridRefillCompleted);
                    return;
                }
            }

            // Крайний случай: заполняем поле гарантированно имеющим ходы набором
            GD.PrintErr("[GridManager] Could not find valid moves after max reshuffles, forcing generation");
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    _grid[x, y] = new ElementData((ElementType)(1 + ((x + y) % 4)), x, y);
                }
            }

            // Гарантируем наличие как минимум одного валидного хода без изначальных совпадений
            // Свап (0,0) и (0,1) по вертикали или (0,0) и (1,0) по горизонтали создаст совпадение
            if (GridSize >= 4)
            {
                _grid[0, 0] = new ElementData((ElementType)1, 0, 0);
                _grid[1, 0] = new ElementData((ElementType)2, 1, 0);
                _grid[1, 1] = new ElementData((ElementType)1, 1, 1);
                _grid[2, 1] = new ElementData((ElementType)1, 2, 1);
                _grid[3, 1] = new ElementData((ElementType)2, 3, 1);
            }

            _ = EmitSignal(SignalName.GridRefillCompleted);
        }

        public bool HasValidMoves()
        {
            return GridValidator.HasValidMoves(_grid, GridSize);
        }

        /// <summary>
        /// Возвращает текущее состояние внутренней двумерной матрицы элементов.
        /// </summary>
        /// <returns>Массив ElementData[,] текущего размера GridSize.</returns>
        public ElementData[,] GetGrid()
        {
            return _grid;
        }
    }
}
