using AlJourney.Scripts.Core;
using AlJourney.Scripts.Match3;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Scripts.Tests
{
    public class ComboSystemTest : TestClass
    {
        private ComboSystem _comboSystem;

        public ComboSystemTest(Node testScene) : base(testScene) { }

        [Setup]
        public void Setup()
        {
            _comboSystem = new ComboSystem();
        }

        [Cleanup]
        public void Cleanup()
        {
            _comboSystem.QueueFree();
        }

        [Test]
        public void ProcessMatchesCalculatesCorrectEffectsForFire()
        {
            MatchResult match1 = new(ElementType.Fire, 3, true); // Level 1
            MatchResult match2 = new(ElementType.Fire, 4, true); // Level 2
            MatchResult match3 = new(ElementType.Fire, 5, true); // Level 3

            List<ComboEffect> effects = _comboSystem.ProcessMatches([match1, match2, match3]);

            effects.Count.ShouldBe(3);

            ComboEffect e1 = effects.First(e => e.ComboLevel == 1);
            e1.Damage.ShouldBe(GameConstants.FIRE_3_DAMAGE);
            e1.IsAoE.ShouldBeFalse();

            ComboEffect e2 = effects.First(e => e.ComboLevel == 2);
            e2.Damage.ShouldBe(GameConstants.FIRE_4_DAMAGE);
            e2.StatusEffect.Type.ShouldBe(StatusEffect.Burning);

            ComboEffect e3 = effects.First(e => e.ComboLevel == 3);
            e3.Damage.ShouldBe(GameConstants.FIRE_5_DAMAGE);
            e3.IsAoE.ShouldBeTrue();
        }

        [Test]
        public void ProcessMatchesHandlesCascadeBonus()
        {
            MatchResult match = new(ElementType.Sword, 3, true);

            // First non-cascade
            List<ComboEffect> effects1 = _comboSystem.ProcessMatches([match]);
            int baseDamage = effects1[0].Damage;
            _comboSystem.GetCascadeLevel().ShouldBe(0);

            // Second cascade (level 1)
            List<ComboEffect> effects2 = _comboSystem.ProcessMatches([match], true);
            _comboSystem.GetCascadeLevel().ShouldBe(1);
            effects2[0].Damage.ShouldBeGreaterThan(baseDamage);

            // Reset cascade
            _comboSystem.ResetCascade();
            _comboSystem.GetCascadeLevel().ShouldBe(0);
        }

        [Test]
        public void ProcessMatchesReturnsNullForSmallMatches()
        {
            MatchResult match = new(ElementType.Shield, 2, true); // Only 2
            List<ComboEffect> effects = _comboSystem.ProcessMatches([match]);
            effects.ShouldBeEmpty();
        }

        [Test]
        public void ProcessHealAndShieldCombosWork()
        {
            MatchResult healMatch = new(ElementType.Heal, 4, true);
            MatchResult shieldMatch = new(ElementType.Shield, 5, true);

            List<ComboEffect> effects = _comboSystem.ProcessMatches([healMatch, shieldMatch]);

            effects.Count.ShouldBe(2);

            ComboEffect healEff = effects.First(e => e.ElementType == ElementType.Heal);
            healEff.Healing.ShouldBe(GameConstants.HEAL_4_AMOUNT);

            ComboEffect shieldEff = effects.First(e => e.ElementType == ElementType.Shield);
            shieldEff.Shield.ShouldBe(GameConstants.SHIELD_5_AMOUNT);
            shieldEff.StatusEffect.Type.ShouldBe(StatusEffect.Immunity);
        }
    }
}
