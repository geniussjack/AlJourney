using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourney.Scripts.Match3
{
    public static class MatchDetector
    {
        public static List<MatchResult> FindAllMatches(ElementData[,] grid, int gridSize)
        {
            List<MatchResult> results = [];
            ScanMatchesHorizontal(grid, gridSize, results);
            ScanMatchesVertical(grid, gridSize, results);
            return results;
        }

        private static void ScanMatchesHorizontal(ElementData[,] grid, int gridSize, List<MatchResult> results)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize - 2; x++)
                {
                    MatchResult match = CheckLineMatch(grid, gridSize, x, y, 1, 0);
                    if (match != null)
                    {
                        results.Add(match);
                        x += match.MatchCount - 1;
                    }
                }
            }
        }

        private static void ScanMatchesVertical(ElementData[,] grid, int gridSize, List<MatchResult> results)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize - 2; y++)
                {
                    MatchResult match = CheckLineMatch(grid, gridSize, x, y, 0, 1);
                    if (match != null)
                    {
                        results.Add(match);
                        y += match.MatchCount - 1;
                    }
                }
            }
        }

        private static MatchResult CheckLineMatch(ElementData[,] grid, int gridSize, int startX, int startY, int deltaX, int deltaY)
        {
            ElementData startElement = grid[startX, startY];
            if (startElement == null || startElement.Type == ElementType.None)
            {
                return null;
            }

            int matchCount = 1;
            int currentX = startX + deltaX;
            int currentY = startY + deltaY;

            while (currentX < gridSize && currentY < gridSize)
            {
                ElementData currentElement = grid[currentX, currentY];
                if (currentElement != null && startElement.CanMatchWith(currentElement))
                {
                    matchCount++;
                }
                else
                {
                    break;
                }

                currentX += deltaX;
                currentY += deltaY;
            }

            if (matchCount >= GameConstants.MATCH_MIN_LENGTH)
            {
                MatchResult result = new(startElement.Type, matchCount, deltaX == 1);
                for (int i = 0; i < matchCount; i++)
                {
                    result.MatchedPositions.Add((startX + (i * deltaX), startY + (i * deltaY)));
                }
                return result;
            }

            return null;
        }
    }
}
