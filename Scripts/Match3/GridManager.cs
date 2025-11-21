using AlJourney.Scripts.Core;
using Godot;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    /// <summary>
    /// Manages the match-3 grid logic including swaps, matches, and cascades.
    /// </summary>
    public partial class GridManager : Node
    {
        [Signal]
        public delegate void GridInitializedEventHandler();

        [Signal]
        public delegate void SwapCompletedEventHandler(bool wasValid);

        [Signal]
        public delegate void MatchesFoundEventHandler(int matchCount);

        [Signal]
        public delegate void GridRefillCompletedEventHandler();

        private ElementData[,] _grid;

        /// <summary>
        /// Current grid size (5x5).
        /// </summary>
        public int GridSize { get; private set; }

        /// <summary>
        /// Remaining swaps for current turn.
        /// </summary>
        public int RemainingSwaps { get; private set; }

        public override void _Ready()
        {
            GridSize = GameConstants.GRID_SIZE;
            _grid = new ElementData[GridSize, GridSize];
            RemainingSwaps = 0;

            GD.Print("[GridManager] Initialized");
        }

        /// <summary>
        /// Initializes the grid with random elements ensuring no initial matches.
        /// </summary>
        public void InitializeGrid()
        {
            // Fill grid with random elements
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

        /// <summary>
        /// Generates an element that won't create immediate matches.
        /// </summary>
        private ElementData GenerateSafeElement(int x, int y)
        {
            List<ElementType> availableTypes =
            [
                ElementType.Fire,
                ElementType.Heal,
                ElementType.Sword,
                ElementType.Shield
            ];

            // Remove types that would create horizontal match
            if (x >= 2)
            {
                ElementType? type1 = _grid[x - 1, y]?.Type;
                ElementType? type2 = _grid[x - 2, y]?.Type;
                if (type1 != null && type1 == type2 && type1 != ElementType.None)
                {
                    _ = availableTypes.Remove(type1.Value);
                }
            }

            // Remove types that would create vertical match
            if (y >= 2)
            {
                ElementType? type1 = _grid[x, y - 1]?.Type;
                ElementType? type2 = _grid[x, y - 2]?.Type;
                if (type1 != null && type1 == type2 && type1 != ElementType.None)
                {
                    _ = availableTypes.Remove(type1.Value);
                }
            }

            // If all types removed (rare), pick random
            if (availableTypes.Count == 0)
            {
                availableTypes.Add((ElementType)GD.RandRange(1, 4));
            }

            ElementType selectedType = availableTypes[GD.RandRange(0, availableTypes.Count - 1)];
            return new ElementData(selectedType, x, y);
        }

        /// <summary>
        /// Gets element at position.
        /// </summary>
        public ElementData GetElement(int x, int y)
        {
            return !IsValidPosition(x, y) ? null : _grid[x, y];
        }

        /// <summary>
        /// Attempts to swap two adjacent elements.
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

            // Check if adjacent
            int deltaX = Mathf.Abs(x2 - x1);
            int deltaY = Mathf.Abs(y2 - y1);
            if ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1))
            {
                // Perform swap
                SwapElements(x1, y1, x2, y2);

                // Check if swap creates matches
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
                    // Revert swap
                    SwapElements(x1, y1, x2, y2);
                    _ = EmitSignal(SignalName.SwapCompleted, false);
                    GD.Print("[GridManager] Invalid swap - no matches created");
                    return false;
                }
            }

            GD.Print("[GridManager] Elements not adjacent");
            return false;
        }

        /// <summary>
        /// Swaps two elements in the grid.
        /// </summary>
        private void SwapElements(int x1, int y1, int x2, int y2)
        {
            (_grid[x1, y1], _grid[x2, y2]) = (_grid[x2, y2], _grid[x1, y1]);

            // Update positions
            _grid[x1, y1].X = x1;
            _grid[x1, y1].Y = y1;
            _grid[x2, y2].X = x2;
            _grid[x2, y2].Y = y2;
        }

        /// <summary>
        /// Finds all current matches on the grid.
        /// </summary>
        public List<MatchResult> FindAllMatches()
        {
            List<MatchResult> allMatches = [];

            // Check horizontal matches
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize - 2; x++)
                {
                    MatchResult matches = CheckLineMatch(x, y, 1, 0); // horizontal
                    if (matches != null)
                    {
                        allMatches.Add(matches);
                        x += matches.MatchCount - 1; // Skip matched elements
                    }
                }
            }

            // Check vertical matches
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize - 2; y++)
                {
                    MatchResult matches = CheckLineMatch(x, y, 0, 1); // vertical
                    if (matches != null)
                    {
                        allMatches.Add(matches);
                        y += matches.MatchCount - 1; // Skip matched elements
                    }
                }
            }

            if (allMatches.Count > 0)
            {
                // FIX: Передаём только количество, а не List
                _ = EmitSignal(SignalName.MatchesFound, allMatches.Count);
                GD.Print($"[GridManager] Found {allMatches.Count} matches");
            }

            return allMatches;
        }

        /// <summary>
        /// Checks for matches in a specific direction.
        /// </summary>
        private MatchResult CheckLineMatch(int startX, int startY, int deltaX, int deltaY)
        {
            ElementData startElement = _grid[startX, startY];
            if (startElement == null || startElement.Type == ElementType.None)
            {
                return null;
            }

            int matchCount = 1;
            List<(int, int)> matchedPositions = [(startX, startY)];

            // Count consecutive matches
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

            // Valid match is 3 or more
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
        /// Removes matched elements and applies gravity.
        /// </summary>
        public void ProcessMatches(List<MatchResult> matches)
        {
            // Mark matched elements
            foreach (MatchResult match in matches)
            {
                foreach ((int x, int y) in match.MatchedPositions)
                {
                    if (IsValidPosition(x, y))
                    {
                        _grid[x, y].IsMatched = true;
                    }
                }
            }

            // Remove matched elements
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (_grid[x, y].IsMatched)
                    {
                        _grid[x, y] = null;
                    }
                }
            }

            // Apply gravity
            ApplyGravity();

            // Refill empty spaces
            RefillGrid();
        }

        /// <summary>
        /// Applies gravity to make elements fall down.
        /// </summary>
        private void ApplyGravity()
        {
            for (int x = 0; x < GridSize; x++)
            {
                // Start from bottom
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

        /// <summary>
        /// Fills empty spaces with new random elements.
        /// </summary>
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
        }

        /// <summary>
        /// Resets swaps for new turn.
        /// </summary>
        public void ResetSwaps()
        {
            RemainingSwaps = GameConstants.PLAYER_SWAPS_PER_TURN;
            GD.Print($"[GridManager] Swaps reset to {RemainingSwaps}");
        }

        /// <summary>
        /// Checks if position is valid on grid.
        /// </summary>
        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GridSize && y >= 0 && y < GridSize;
        }

        /// <summary>
        /// Gets the entire grid (for debugging/visualization).
        /// </summary>
        public ElementData[,] GetGrid()
        {
            return _grid;
        }
    }
}
