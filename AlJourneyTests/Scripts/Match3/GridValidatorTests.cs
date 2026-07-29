namespace AlJourneyTests.Scripts.Match3
{
    public class GridValidatorTests
    {
        [Fact]
        public void IsValidPosition_ReturnsTrue_ForValidCoordinates()
        {
            // Arrange
            const int gridSize = 8;

            // Act & Assert
            Assert.True(GridValidator.IsValidPosition(0, 0, gridSize));
            Assert.True(GridValidator.IsValidPosition(7, 7, gridSize));
            Assert.True(GridValidator.IsValidPosition(3, 4, gridSize));
        }

        [Fact]
        public void IsValidPosition_ReturnsFalse_ForInvalidCoordinates()
        {
            // Arrange
            const int gridSize = 8;

            // Act & Assert
            Assert.False(GridValidator.IsValidPosition(-1, 0, gridSize));
            Assert.False(GridValidator.IsValidPosition(0, -1, gridSize));
            Assert.False(GridValidator.IsValidPosition(8, 0, gridSize));
            Assert.False(GridValidator.IsValidPosition(0, 8, gridSize));
        }

        [Fact]
        public void HasValidMoves_ReturnsTrue_WhenHorizontalMoveExists()
        {
            // Arrange
            const int gridSize = 4;
            ElementData[,] grid = new ElementData[gridSize, gridSize];
            // Создаем поле без совпадений
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = new ElementData((ElementType)(1 + ((x + y) % 4)), x, y);
                }
            }

            // Искусственно создаем ситуацию, где возможен горизонтальный ход
            // 0,0: Sword
            // 1,0: Fire
            // 2,0: Sword
            // 3,0: Sword
            // Свап 0,0 и 1,0 создаст линию 1,0, 2,0, 3,0 из Sword
            grid[0, 0] = new ElementData(ElementType.Sword, 0, 0);
            grid[1, 0] = new ElementData(ElementType.Fire, 1, 0);
            grid[2, 0] = new ElementData(ElementType.Sword, 2, 0);
            grid[3, 0] = new ElementData(ElementType.Sword, 3, 0);

            // Act
            bool result = GridValidator.HasValidMoves(grid, gridSize);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasValidMoves_ReturnsTrue_WhenVerticalMoveExists()
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

            // Искусственно создаем ситуацию, где возможен вертикальный ход
            grid[0, 0] = new ElementData(ElementType.Heal, 0, 0);
            grid[0, 1] = new ElementData(ElementType.Shield, 0, 1);
            grid[0, 2] = new ElementData(ElementType.Heal, 0, 2);
            grid[0, 3] = new ElementData(ElementType.Heal, 0, 3);

            // Act
            bool result = GridValidator.HasValidMoves(grid, gridSize);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasValidMoves_ReturnsFalse_WhenNoMovesExist()
        {
            // Arrange
            const int gridSize = 4;
            ElementData[,] grid = new ElementData[gridSize, gridSize];
            // Идеальное "безопасное" поле без возможных ходов (диагональное распределение)
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    // 1, 2, 3, 4 повторяются, не создавая рядов
                    grid[x, y] = new ElementData((ElementType)(1 + ((x + y) % 4)), x, y);
                }
            }

            // Act
            bool result = GridValidator.HasValidMoves(grid, gridSize);

            // Assert
            Assert.False(result);
        }
    }
}
