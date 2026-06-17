using AlJourney.Scripts.Core;
using Godot;

namespace AlJourney.Scripts.Match3
{
    public static class GridValidator
    {
        public static bool IsValidPosition(int x, int y, int gridSize)
        {
            return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
        }

        public static bool HasValidMoves(ElementData[,] grid, int gridSize)
        {
            return CheckHorizontalPotentialMoves(grid, gridSize) || CheckVerticalPotentialMoves(grid, gridSize);
        }

        private static bool CheckHorizontalPotentialMoves(ElementData[,] grid, int gridSize)
        {
            for (int x = 0; x < gridSize - 1; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (WouldCreateMatch(grid, gridSize, x, y, x + 1, y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool CheckVerticalPotentialMoves(ElementData[,] grid, int gridSize)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize - 1; y++)
                {
                    if (WouldCreateMatch(grid, gridSize, x, y, x, y + 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool WouldCreateMatch(ElementData[,] grid, int gridSize, int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1, gridSize) || !IsValidPosition(x2, y2, gridSize))
            {
                return false;
            }

            ElementData elem1 = grid[x1, y1];
            ElementData elem2 = grid[x2, y2];

            if (elem1 == null || elem2 == null)
            {
                return false;
            }

            // Swap virtually
            grid[x1, y1] = elem2;
            grid[x2, y2] = elem1;

            bool createsMatch = CheckMatchAtPosition(grid, gridSize, x1, y1) || CheckMatchAtPosition(grid, gridSize, x2, y2);

            // Revert
            grid[x1, y1] = elem1;
            grid[x2, y2] = elem2;

            return createsMatch;
        }

        private static bool CheckMatchAtPosition(ElementData[,] grid, int gridSize, int x, int y)
        {
            if (!IsValidPosition(x, y, gridSize))
            {
                return false;
            }

            ElementData element = grid[x, y];
            return element != null && element.Type != ElementType.None && (CheckHorizontalLength(grid, gridSize, x, y, element) >= GameConstants.MATCH_MIN_LENGTH ||
                   CheckVerticalLength(grid, gridSize, x, y, element) >= GameConstants.MATCH_MIN_LENGTH);
        }

        private static int CheckHorizontalLength(ElementData[,] grid, int gridSize, int startX, int y, ElementData element)
        {
            int count = 1;
            for (int i = startX - 1; i >= 0 && grid[i, y] != null && element.CanMatchWith(grid[i, y]); i--)
            {
                count++;
            }

            for (int i = startX + 1; i < gridSize && grid[i, y] != null && element.CanMatchWith(grid[i, y]); i++)
            {
                count++;
            }

            return count;
        }

        private static int CheckVerticalLength(ElementData[,] grid, int gridSize, int x, int startY, ElementData element)
        {
            int count = 1;
            for (int i = startY - 1; i >= 0 && grid[x, i] != null && element.CanMatchWith(grid[x, i]); i--)
            {
                count++;
            }

            for (int i = startY + 1; i < gridSize && grid[x, i] != null && element.CanMatchWith(grid[x, i]); i++)
            {
                count++;
            }

            return count;
        }
    }
}
