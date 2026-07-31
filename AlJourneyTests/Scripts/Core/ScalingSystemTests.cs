using AlJourney.Scripts.Core;

namespace AlJourneyTests.Scripts.Core
{
    public class ScalingSystemTests
    {
        // Тестовые значения подобраны так, чтобы "чистый" математический результат (baseStat * множитель)
        // не попадал точно на целое число: из-за погрешности float такое пограничное значение может
        // случайным образом округлиться CeilToInt в большую сторону и сделать тест хрупким.
        [Theory]
        [InlineData(10, 0, 10)]   // wave 0 -> множитель ровно 1.0, без плавающей погрешности
        [InlineData(13, 1, 15)]   // 13 * 1.10 = 14.3 -> Ceil = 15
        [InlineData(13, 5, 20)]   // 13 * 1.50 = 19.5 -> Ceil = 20
        public void ScaleEnemyStat_AppliesWaveCoefficientAndRoundsUp(int baseStat, int wave, int expected)
        {
            int result = ScalingSystem.ScaleEnemyStat(baseStat, wave);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100, 0, 100)]
        [InlineData(37, 1, 41)]   // 37 * 1.10 = 40.7 -> Ceil = 41
        [InlineData(37, 3, 49)]   // 37 * 1.30 = 48.1 -> Ceil = 49
        public void ScaleReward_AppliesWaveCoefficientAndRoundsUp(int baseReward, int wave, int expected)
        {
            int result = ScalingSystem.ScaleReward(baseReward, wave);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100, 0, 100)]
        [InlineData(37, 1, 39)]   // 37 * 1.05 = 38.85 -> Ceil = 39
        [InlineData(37, 10, 56)]  // 37 * 1.50 = 55.5 -> Ceil = 56
        public void ScaleCost_AppliesWaveCoefficientAndRoundsUp(int baseCost, int wave, int expected)
        {
            int result = ScalingSystem.ScaleCost(baseCost, wave);

            Assert.Equal(expected, result);
        }
    }
}
