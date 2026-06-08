using AlJourney.Scripts.Core;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Глобальный синглтон-менеджер, управляющий логикой игрового поля (сетки Match-3).
    /// Отвечает за генерацию элементов, проверку возможных ходов (свапов), поиск совпадений 
    /// (каскадов) и применение гравитации при исчезновении камней.
    /// Не содержит визуальной логики (отделено от UI).
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
        /// Вызывается, когда на поле найдены совпадения (линии из 3 и более).
        /// </summary>
        /// <param name="matchCount">Количество уникальных комбинаций, найденных за этот проход.</param>
        [Signal]
        public delegate void MatchesFoundEventHandler(int matchCount);

        /// <summary>
        /// Вызывается после того, как пустые места заполнены новыми элементами,
        /// что может запустить новую цепную реакцию (каскад).
        /// </summary>
        [Signal]
        public delegate void GridRefillCompletedEventHandler();

        private ElementData[,] _grid;

        /// <summary>
        /// Размер игрового поля по ширине и высоте (стандартно 5x5).
        /// </summary>
        public int GridSize { get; private set; }

        /// <summary>
        /// Количество доступных обменов (свапов) в текущий ход игрока.
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
            if (x < 2) return ElementType.None;
            ElementType? type1 = _grid[x - 1, y]?.Type;
            ElementType? type2 = _grid[x - 2, y]?.Type;
            return (type1 != null && type1 == type2) ? type1.Value : ElementType.None;
        }

        private ElementType GetVerticalExclude(int x, int y)
        {
            if (y < 2) return ElementType.None;
            ElementType? type1 = _grid[x, y - 1]?.Type;
            ElementType? type2 = _grid[x, y - 2]?.Type;
            return (type1 != null && type1 == type2) ? type1.Value : ElementType.None;
        }

        private ElementType GetRandomValidType(ElementType exclude1, ElementType exclude2)
        {
            ElementType[] allTypes = { ElementType.Fire, ElementType.Heal, ElementType.Sword, ElementType.Shield };
            List<ElementType> validTypes = new List<ElementType>();
            
            foreach (ElementType t in allTypes)
            {
                if (t != exclude1 && t != exclude2 && t != ElementType.None)
                {
                    validTypes.Add(t);
                }
            }
            
            if (validTypes.Count == 0)
            {
                return (ElementType)GD.RandRange(1, 4);
            }
            
            return validTypes[GD.RandRange(0, validTypes.Count - 1)];
        }

        /// <summary>
        /// Возвращает логический объект элемента по заданным координатам.
        /// </summary>
        /// <param name="x">Координата X (от 0 до GridSize - 1).</param>
        /// <param name="y">Координата Y (от 0 до GridSize - 1).</param>
        /// <returns>Объект ElementData, либо null, если координаты выходят за пределы поля.</returns>
        public ElementData GetElement(int x, int y)
        {
            return !IsValidPosition(x, y) ? null : _grid[x, y];
        }

        /// <summary>
        /// Выполняет попытку поменять два соседних элемента местами.
        /// Если обмен не приводит ни к одному совпадению (3-в-ряд), 
        /// элементы возвращаются на свои исходные позиции.
        /// </summary>
        /// <param name="x1">X первой ячейки.</param>
        /// <param name="y1">Y первой ячейки.</param>
        /// <param name="x2">X второй ячейки.</param>
        /// <param name="y2">Y второй ячейки.</param>
        /// <returns>True, если свап прошел успешно и образовал комбо. Иначе False.</returns>
        public bool TrySwap(int x1, int y1, int x2, int y2)
        {
            if (RemainingSwaps <= 0 || !IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
            {
                return false;
            }

            if (!ArePositionsAdjacent(x1, y1, x2, y2))
            {
                return false;
            }

            SwapElements(x1, y1, x2, y2);

            if (FindAllMatches().Count > 0)
            {
                RemainingSwaps--;
                _ = EmitSignal(SignalName.SwapCompleted, true);
                return true;
            }
            
            // Откат свапа, если нет совпадений
            SwapElements(x1, y1, x2, y2);
            _ = EmitSignal(SignalName.SwapCompleted, false);
            return false;
        }

        private bool ArePositionsAdjacent(int x1, int y1, int x2, int y2)
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
            List<MatchResult> allMatches = new List<MatchResult>();
            
            ScanMatchesHorizontal(allMatches);
            ScanMatchesVertical(allMatches);

            if (allMatches.Count > 0)
            {
                _ = EmitSignal(SignalName.MatchesFound, allMatches.Count);
            }

            return allMatches;
        }

        private void ScanMatchesHorizontal(List<MatchResult> results)
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize - 2; x++)
                {
                    MatchResult match = CheckLineMatch(x, y, 1, 0); 
                    if (match != null)
                    {
                        results.Add(match);
                        x += match.MatchCount - 1; 
                    }
                }
            }
        }

        private void ScanMatchesVertical(List<MatchResult> results)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize - 2; y++)
                {
                    MatchResult match = CheckLineMatch(x, y, 0, 1); 
                    if (match != null)
                    {
                        results.Add(match);
                        y += match.MatchCount - 1; 
                    }
                }
            }
        }

        private MatchResult CheckLineMatch(int startX, int startY, int deltaX, int deltaY)
        {
            ElementData startElement = _grid[startX, startY];
            if (startElement == null || startElement.Type == ElementType.None)
            {
                return null;
            }

            int matchCount = 1;
            List<(int, int)> matchedPositions = new List<(int, int)> { (startX, startY) };

            for (int i = 1; i < GridSize; i++)
            {
                int x = startX + (i * deltaX);
                int y = startY + (i * deltaY);

                if (!IsValidPosition(x, y)) break;

                ElementData element = _grid[x, y];
                if (element == null || !startElement.CanMatchWith(element)) break;

                matchCount++;
                matchedPositions.Add((x, y));
            }

            if (matchCount >= GameConstants.MATCH_MIN_LENGTH)
            {
                return new MatchResult(startElement.Type, matchCount, deltaX == 1)
                {
                    MatchedPositions = matchedPositions
                };
            }

            return null;
        }

        /// <summary>
        /// Уничтожает совпавшие камни, применяет гравитацию к висящим сверху
        /// и генерирует новые элементы в пустых местах.
        /// </summary>
        /// <param name="matches">Список подтвержденных совпадений для удаления.</param>
        public void ProcessMatches(List<MatchResult> matches)
        {
            if (matches == null || matches.Count == 0) return;
            
            MarkMatchedElements(matches);
            ClearMatchedElements();
            ApplyGravity();
            RefillGrid();
        }

        private void MarkMatchedElements(List<MatchResult> matches)
        {
            foreach (MatchResult match in matches)
            {
                if (match?.MatchedPositions == null) continue;
                
                foreach ((int x, int y) in match.MatchedPositions)
                {
                    if (IsValidPosition(x, y) && _grid[x, y] != null)
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
                    if (_grid[x, y] != null && _grid[x, y].IsMatched)
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
        /// Восстанавливает лимит доступных обменов для игрока (например, в начале нового хода).
        /// </summary>
        public void ResetSwaps()
        {
            RemainingSwaps = GameConstants.PLAYER_SWAPS_PER_TURN;
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GridSize && y >= 0 && y < GridSize;
        }

        /// <summary>
        /// Симулирует все возможные ходы на доске, чтобы убедиться, 
        /// что игрок не оказался в безвыходной ситуации (Deadlock).
        /// </summary>
        /// <returns>True, если на доске есть хотя бы один легальный ход, собирающий комбо.</returns>
        public bool HasValidMoves()
        {
            return CheckHorizontalPotentialMoves() || CheckVerticalPotentialMoves();
        }

        private bool CheckHorizontalPotentialMoves()
        {
            for (int x = 0; x < GridSize - 1; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (WouldCreateMatch(x, y, x + 1, y)) return true;
                }
            }
            return false;
        }

        private bool CheckVerticalPotentialMoves()
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize - 1; y++)
                {
                    if (WouldCreateMatch(x, y, x, y + 1)) return true;
                }
            }
            return false;
        }

        private bool WouldCreateMatch(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2)) return false;

            ElementData elem1 = _grid[x1, y1];
            ElementData elem2 = _grid[x2, y2];

            if (elem1 == null || elem2 == null) return false;

            // Временный свап
            _grid[x1, y1] = elem2;
            _grid[x2, y2] = elem1;

            bool createsMatch = CheckMatchAtPosition(x1, y1) || CheckMatchAtPosition(x2, y2);

            // Откат
            _grid[x1, y1] = elem1;
            _grid[x2, y2] = elem2;

            return createsMatch;
        }

        private bool CheckMatchAtPosition(int x, int y)
        {
            if (!IsValidPosition(x, y)) return false;

            ElementData element = _grid[x, y];
            if (element == null || element.Type == ElementType.None) return false;

            return CheckHorizontalLength(x, y, element) >= GameConstants.MATCH_MIN_LENGTH || 
                   CheckVerticalLength(x, y, element) >= GameConstants.MATCH_MIN_LENGTH;
        }

        private int CheckHorizontalLength(int startX, int y, ElementData element)
        {
            int count = 1;
            for (int i = startX - 1; i >= 0 && _grid[i, y] != null && element.CanMatchWith(_grid[i, y]); i--) count++;
            for (int i = startX + 1; i < GridSize && _grid[i, y] != null && element.CanMatchWith(_grid[i, y]); i++) count++;
            return count;
        }

        private int CheckVerticalLength(int x, int startY, ElementData element)
        {
            int count = 1;
            for (int i = startY - 1; i >= 0 && _grid[x, i] != null && element.CanMatchWith(_grid[x, i]); i--) count++;
            for (int i = startY + 1; i < GridSize && _grid[x, i] != null && element.CanMatchWith(_grid[x, i]); i++) count++;
            return count;
        }

        /// <summary>
        /// Автоматически перемешивает все камни на доске, если метод <see cref="HasValidMoves"/> 
        /// возвращает false (нет доступных ходов).
        /// </summary>
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
            int attempts = 0;
            const int maxAttempts = 3;
            
            while (attempts < maxAttempts)
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
                    EmitSignal(SignalName.GridRefillCompleted);
                    return;
                }
                attempts++;
            }
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
