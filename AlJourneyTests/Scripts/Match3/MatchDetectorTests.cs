using AlJourney.Scripts.Core;
using AlJourney.Scripts.Match3;

namespace AlJourneyTests.Scripts.Match3
{
    public class MatchDetectorTests
    {
        [Fact]
        public void FindAllMatches_ReturnsMatches_WhenHorizontalMatchExists()
        {
            // Arrange
            const int gridSize = 4;
            ElementData[,] grid = new ElementData[gridSize, gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = new ElementData((ElementType)(1 + ((x + y) % 4)), x, y);
                }
            }

            // Create horizontal match of 3 Fires at y=0
            grid[0, 0] = new ElementData(ElementType.Fire, 0, 0);
            grid[1, 0] = new ElementData(ElementType.Fire, 1, 0);
            grid[2, 0] = new ElementData(ElementType.Fire, 2, 0);

            // Act
            List<MatchResult> results = MatchDetector.FindAllMatches(grid, gridSize);

            // Assert
            _ = Assert.Single(results);
            Assert.Equal(ElementType.Fire, results[0].ElementType);
            Assert.Equal(3, results[0].MatchCount);
            Assert.True(results[0].IsHorizontal);
            Assert.Equal(3, results[0].MatchedPositions.Count);
            Assert.Contains((0, 0), results[0].MatchedPositions);
            Assert.Contains((1, 0), results[0].MatchedPositions);
            Assert.Contains((2, 0), results[0].MatchedPositions);
        }

        [Fact]
        public void FindAllMatches_ReturnsMatches_WhenVerticalMatchExists()
        {
            // Arrange
            const int gridSize = 5;
            ElementData[,] grid = new ElementData[gridSize, gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = new ElementData((ElementType)(1 + ((x + y) % 4)), x, y);
                }
            }

            // Create vertical match of 4 Shields at x=2
            grid[2, 0] = new ElementData(ElementType.Shield, 2, 0);
            grid[2, 1] = new ElementData(ElementType.Shield, 2, 1);
            grid[2, 2] = new ElementData(ElementType.Shield, 2, 2);
            grid[2, 3] = new ElementData(ElementType.Shield, 2, 3);

            // Act
            List<MatchResult> results = MatchDetector.FindAllMatches(grid, gridSize);

            // Assert
            _ = Assert.Single(results);
            Assert.Equal(ElementType.Shield, results[0].ElementType);
            Assert.Equal(4, results[0].MatchCount);
            Assert.False(results[0].IsHorizontal);
        }

        [Fact]
        public void FindAllMatches_ReturnsMultipleMatches_WhenCrossMatchExists()
        {
            // Arrange
            const int gridSize = 5;
            ElementData[,] grid = new ElementData[gridSize, gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = new ElementData((ElementType)(1 + ((x + y) % 4)), x, y);
                }
            }

            // Cross match of Heal at 2,2
            grid[1, 2] = new ElementData(ElementType.Heal, 1, 2);
            grid[2, 2] = new ElementData(ElementType.Heal, 2, 2);
            grid[3, 2] = new ElementData(ElementType.Heal, 3, 2);

            grid[2, 1] = new ElementData(ElementType.Heal, 2, 1);
            // 2,2 is already heal
            grid[2, 3] = new ElementData(ElementType.Heal, 2, 3);

            // Act
            List<MatchResult> results = MatchDetector.FindAllMatches(grid, gridSize);

            // Assert
            Assert.Equal(2, results.Count); // One horizontal, one vertical
        }

        [Fact]
        public void FindAllMatches_ReturnsEmpty_WhenNoMatchesExist()
        {
            // Arrange
            const int gridSize = 4;
            ElementData[,] grid = new ElementData[gridSize, gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = new ElementData((ElementType)(1 + ((x + y) % 4)), x, y);
                }
            }

            // Act
            List<MatchResult> results = MatchDetector.FindAllMatches(grid, gridSize);

            // Assert
            Assert.Empty(results);
        }
    }
}
