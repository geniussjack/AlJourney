using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourneyTests.Scripts.Data
{
    public class StatusEffectDataTests
    {
        [Fact]
        public void Constructor_DefaultsExtraDataToZero()
        {
            StatusEffectData effect = new(StatusEffect.Burning, 3, 5);

            Assert.Equal(0f, effect.ExtraData);
        }

        [Fact]
        public void Constructor_StoresSuppliedExtraData()
        {
            StatusEffectData effect = new(StatusEffect.ShieldReflect, 1, 0, 0.25f);

            Assert.Equal(StatusEffect.ShieldReflect, effect.Type);
            Assert.Equal(1, effect.Duration);
            Assert.Equal(0, effect.Power);
            Assert.Equal(0.25f, effect.ExtraData);
        }

        [Fact]
        public void TickDuration_DecreasesDurationByOneAndPreservesOtherFields()
        {
            StatusEffectData effect = new(StatusEffect.Bleeding, 3, 4, 0.1f);

            StatusEffectData result = effect.TickDuration();

            Assert.Equal(2, result.Duration);
            Assert.Equal(StatusEffect.Bleeding, result.Type);
            Assert.Equal(4, result.Power);
            Assert.Equal(0.1f, result.ExtraData);
        }

        [Theory]
        [InlineData(2, false)]
        [InlineData(1, false)]
        [InlineData(0, true)]
        [InlineData(-1, true)]
        public void ShouldRemove_ReflectsWhetherDurationHasExpired(int duration, bool expected)
        {
            StatusEffectData effect = new(StatusEffect.Weakened, duration, 1);

            Assert.Equal(expected, effect.ShouldRemove);
        }
    }
}
