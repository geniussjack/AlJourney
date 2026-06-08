using AlJourney.Scripts.Core;
using Godot;
using AlJourney.Scripts.Interfaces;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Менеджер GridManager. Отвечает за управление соответствующей подсистемой.
    /// </summary>
    public partial class GridManager : Node, IGridManager
    {
        [Signal]
        /// <summary>
        /// Элемент GridInitializedEventHandler.
        /// </summary>
        public delegate void GridInitializedEventHandler();

        [Signal]
        /// <summary>
        /// Элемент SwapCompletedEventHandler.
        /// </summary>
        public delegate void SwapCompletedEventHandler(bool wasValid);

        [Signal]
        /// <summary>
        /// Элемент MatchesFoundEventHandler.
        /// </summary>
        public delegate void MatchesFoundEventHandler(int matchCount);

        [Signal]
        /// <summary>
        /// Элемент GridRefillCompletedEventHandler.
        /// </summary>
        public delegate void GridRefillCompletedEventHandler();

        private ElementData[,] _grid;

        /// <summary>
        /// Размер игрового поля (NxN).
        /// </summary>
        public int GridSize { get; private set; }

        /// <summary>
        /// Оставшееся количество ходов (свапов) игрока.
        /// </summary>
        public int RemainingSwaps { get; private set; }

        /// <summary>
        /// Элемент _Ready.
        /// </summary>
        public override void _Ready()
        {
            GridSize = GameConstants.GRID_SIZE;
            _grid = new ElementData[GridSize, GridSize];
            RemainingSwaps = 0;

            GD.Print("[GridManager] Initialized");
        }

        /// <summary>
        /// Инициализирует сетку безопасными элементами (без совпадений 3-в-ряд на старте).
        /// Сбрасывает количество доступных свапов.
        /// </summary>
        /// <summary>
        /// Инициализирует Grid.
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
            ElementType[] allTypes = [ElementType.Fire, ElementType.Heal, ElementType.Sword, ElementType.Shield];
            ElementType exclude1 = ElementType.None;
            ElementType exclude2 = ElementType.None;

            if (x >= 2)
            {
                ElementType? type1 = _grid[x - 1, y]?.Type;
                ElementType? type2 = _grid[x - 2, y]?.Type;
                if (type1 != null && type1 == type2 && type1 != ElementType.None)
                {
                    exclude1 = type1.Value;
                }
            }

            if (y >= 2)
            {
                ElementType? type1 = _grid[x, y - 1]?.Type;
                ElementType? type2 = _grid[x, y - 2]?.Type;
                if (type1 != null && type1 == type2 && type1 != ElementType.None)
                {
                    exclude2 = type1.Value;
                }
            }

            List<ElementType> validTypes = [];
            foreach (ElementType t in allTypes)
            {
                if (t != exclude1 && t != exclude2)
                {
                    validTypes.Add(t);
                }
            }

            if (validTypes.Count == 0)
            {
                validTypes.Add((ElementType)GD.RandRange(1, 4));
            }

            ElementType selectedType = validTypes[GD.RandRange(0, validTypes.Count - 1)];
            return new ElementData(selectedType, x, y);
        }

        /// <summary>
        /// Возвращает элемент сетки по координатам.
        /// </summary>
        /// <summary>
        /// Возвращает Element.
        /// </summary>
        public ElementData GetElement(int x, int y)
        {
            return !IsValidPosition(x, y) ? null : _grid[x, y];
        }

        /// <summary>
        /// Пытается поменять местами два элемента на сетке.
        /// Если обмен не приводит к совпадению, элементы возвращаются на исходные позиции.
        /// </summary>
        /// <summary>
        /// Пытается выполнить Swap.
        /// </summary>
        public bool TrySwap(int x1, int y1, int x2, int y2)
        {
            if (RemainingSwaps <= 0)
            {
                GD.Print("[GridManager] No swaps remaining");
                return false;
            }

            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
            {
                GD.Print("[GridManager] Invalid swap positions");
                return false;
            }

            int deltaX = Mathf.Abs(x2 - x1);
            int deltaY = Mathf.Abs(y2 - y1);
            if ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1))
            {
                SwapElements(x1, y1, x2, y2);

                List<MatchResult> matches = FindAllMatches();
                if (matches.Count > 0)
                {
                    RemainingSwaps--;
                    _ = EmitSignal(SignalName.SwapCompleted, true);
                    GD.Print($"[GridManager] Valid swap! Remaining: {RemainingSwaps}");
                    return true;
                }
                else
                {
                    SwapElements(x1, y1, x2, y2);
                    _ = EmitSignal(SignalName.SwapCompleted, false);
                    GD.Print("[GridManager] Invalid swap - no matches created");
                    return false;
                }
            }

            GD.Print("[GridManager] Elements not adjacent");
            return false;
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
        /// Сканирует всю сетку и находит все линии из 3 и более одинаковых элементов.
        /// </summary>
        /// <returns>Список найденных совпадений.</returns>
        /// <summary>
        /// Элемент FindAllMatches.
        /// </summary>
        public List<MatchResult> FindAllMatches()
        {
            List<MatchResult> allMatches = [];

            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize - 2; x++)
                {
                    MatchResult matches = CheckLineMatch(x, y, 1, 0); 
                    if (matches != null)
                    {
                        allMatches.Add(matches);
                        x += matches.MatchCount - 1; 
                    }
                }
            }

            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize - 2; y++)
                {
                    MatchResult matches = CheckLineMatch(x, y, 0, 1); 
                    if (matches != null)
                    {
                        allMatches.Add(matches);
                        y += matches.MatchCount - 1; 
                    }
                }
            }

            if (allMatches.Count > 0)
            {
                _ = EmitSignal(SignalName.MatchesFound, allMatches.Count);
                GD.Print($"[GridManager] Found {allMatches.Count} matches");
            }

            return allMatches;
        }

        private MatchResult CheckLineMatch(int startX, int startY, int deltaX, int deltaY)
        {
            ElementData startElement = _grid[startX, startY];
            if (startElement == null || startElement.Type == ElementType.None)
            {
                return null;
            }

            int matchCount = 1;
            List<(int, int)> matchedPositions = [(startX, startY)];

            for (int i = 1; i < GridSize; i++)
            {
                int x = startX + (i * deltaX);
                int y = startY + (i * deltaY);

                if (!IsValidPosition(x, y))
                {
                    break;
                }

                ElementData element = _grid[x, y];
                if (element == null || !startElement.CanMatchWith(element))
                {
                    break;
                }

                matchCount++;
                matchedPositions.Add((x, y));
            }

            if (matchCount >= GameConstants.MATCH_MIN_LENGTH)
            {
                MatchResult result = new(startElement.Type, matchCount, deltaX == 1)
                {
                    MatchedPositions = matchedPositions
                };
                return result;
            }

            return null;
        }

        /// <summary>
        /// Обрабатывает найденные совпадения: удаляет элементы, применяет гравитацию и заполняет пустые ячейки.
        /// </summary>
        /// <summary>
        /// Обрабатывает Matches.
        /// </summary>
        public void ProcessMatches(List<MatchResult> matches)
        {
            if (matches == null || matches.Count == 0)
            {
                return;
            }
            
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

            ApplyGravity();

            RefillGrid();
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
            GD.Print("[GridManager] Grid refilled");
            
            CheckAndReshuffleIfNeeded();
        }

        /// <summary>
        /// Сбрасывает количество свапов игрока в начале нового хода.
        /// </summary>
        /// <summary>
        /// Сбрасывает Swaps.
        /// </summary>
        public void ResetSwaps()
        {
            RemainingSwaps = GameConstants.PLAYER_SWAPS_PER_TURN;
            GD.Print($"[GridManager] Swaps reset to {RemainingSwaps}");
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GridSize && y >= 0 && y < GridSize;
        }

        /// <summary>
        /// Проверяет, существуют ли на доске возможные ходы (потенциальные совпадения).
        /// </summary>
        /// <summary>
        /// Проверяет наличие ValidMoves.
        /// </summary>
        public bool HasValidMoves()
        {
            for (int x = 0; x < GridSize - 1; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (WouldCreateMatch(x, y, x + 1, y))
                    {
                        return true;
                    }
                }
            }

            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize - 1; y++)
                {
                    if (WouldCreateMatch(x, y, x, y + 1))
                    {
                        return true;
                    }
                }
            }

            GD.Print("[GridManager] No valid moves available!");
            return false;
        }

        private bool WouldCreateMatch(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
            {
                return false;
            }

            ElementData elem1 = _grid[x1, y1];
            ElementData elem2 = _grid[x2, y2];

            if (elem1 == null || elem2 == null)
            {
                return false;
            }

            _grid[x1, y1] = elem2;
            _grid[x2, y2] = elem1;
            elem2.X = x1;
            elem2.Y = y1;
            elem1.X = x2;
            elem1.Y = y2;

            bool createsMatch = CheckMatchAtPosition(x1, y1) || CheckMatchAtPosition(x2, y2);

            _grid[x1, y1] = elem1;
            _grid[x2, y2] = elem2;
            elem1.X = x1;
            elem1.Y = y1;
            elem2.X = x2;
            elem2.Y = y2;

            return createsMatch;
        }

        private bool CheckMatchAtPosition(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return false;
            }

            ElementData element = _grid[x, y];
            if (element == null || element.Type == ElementType.None)
            {
                return false;
            }

            int horizontalCount = 1;

            for (int i = x - 1; i >= 0; i--)
            {
                if (_grid[i, y] != null && element.CanMatchWith(_grid[i, y]))
                {
                    horizontalCount++;
                }
                else
                {
                    break;
                }
            }

            for (int i = x + 1; i < GridSize; i++)
            {
                if (_grid[i, y] != null && element.CanMatchWith(_grid[i, y]))
                {
                    horizontalCount++;
                }
                else
                {
                    break;
                }
            }

            if (horizontalCount >= GameConstants.MATCH_MIN_LENGTH)
            {
                return true;
            }

            int verticalCount = 1;

            for (int i = y - 1; i >= 0; i--)
            {
                if (_grid[x, i] != null && element.CanMatchWith(_grid[x, i]))
                {
                    verticalCount++;
                }
                else
                {
                    break;
                }
            }

            for (int i = y + 1; i < GridSize; i++)
            {
                if (_grid[x, i] != null && element.CanMatchWith(_grid[x, i]))
                {
                    verticalCount++;
                }
                else
                {
                    break;
                }
            }

            return verticalCount >= GameConstants.MATCH_MIN_LENGTH;
        }

        /// <summary>
        /// Проверяет наличие доступных ходов, и если их нет — перетасовывает доску.
        /// </summary>
        /// <summary>
        /// Проверяет AndReshuffleIfNeeded.
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
                    GD.Print("[GridManager] Board reshuffled successfully");
                    return;
                }
                
                attempts++;
            }
            
            GD.PrintErr("[GridManager] Failed to reshuffle board after 3 attempts");
        }

        /// <summary>
        /// Возвращает текущее состояние двумерного массива сетки.
        /// </summary>
        /// <summary>
        /// Возвращает Grid.
        /// </summary>
        public ElementData[,] GetGrid()
        {
            return _grid;
        }
    }
}
