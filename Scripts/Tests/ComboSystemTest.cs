using AlJourney.Scripts.Match3;
using AlJourney.Scripts.Data;
using AlJourney.Scripts.Core;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace AlJourney.Tests
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
            var match1 = new MatchResult(ElementType.Fire, 3, true); // Level 1
            var match2 = new MatchResult(ElementType.Fire, 4, true); // Level 2
            var match3 = new MatchResult(ElementType.Fire, 5, true); // Level 3

            var effects = _comboSystem.ProcessMatches(new List<MatchResult> { match1, match2, match3 });

            effects.Count.ShouldBe(3);
            
            var e1 = effects.First(e => e.ComboLevel == 1);
            e1.Damage.ShouldBe(GameConstants.FIRE_3_DAMAGE);
            e1.IsAoE.ShouldBeFalse();

            var e2 = effects.First(e => e.ComboLevel == 2);
            e2.Damage.ShouldBe(GameConstants.FIRE_4_DAMAGE);
            e2.StatusEffect.Type.ShouldBe(StatusEffect.Burning);

            var e3 = effects.First(e => e.ComboLevel == 3);
            e3.Damage.ShouldBe(GameConstants.FIRE_5_DAMAGE);
            e3.IsAoE.ShouldBeTrue();
        }

        [Test]
        public void ProcessMatchesHandlesCascadeBonus()
        {
            var match = new MatchResult(ElementType.Sword, 3, true);

            // First non-cascade
            var effects1 = _comboSystem.ProcessMatches(new List<MatchResult> { match });
            int baseDamage = effects1[0].Damage;
            _comboSystem.GetCascadeLevel().ShouldBe(0);

            // Second cascade (level 1)
            var effects2 = _comboSystem.ProcessMatches(new List<MatchResult> { match }, true);
            _comboSystem.GetCascadeLevel().ShouldBe(1);
            effects2[0].Damage.ShouldBeGreaterThan(baseDamage);

            // Reset cascade
            _comboSystem.ResetCascade();
            _comboSystem.GetCascadeLevel().ShouldBe(0);
        }

        [Test]
        public void ProcessMatchesReturnsNullForSmallMatches()
        {
            var match = new MatchResult(ElementType.Shield, 2, true); // Only 2
            var effects = _comboSystem.ProcessMatches(new List<MatchResult> { match });
            effects.ShouldBeEmpty();
        }

        [Test]
        public void ProcessHealAndShieldCombosWork()
        {
            var healMatch = new MatchResult(ElementType.Heal, 4, true);
            var shieldMatch = new MatchResult(ElementType.Shield, 5, true);

            var effects = _comboSystem.ProcessMatches(new List<MatchResult> { healMatch, shieldMatch });

            effects.Count.ShouldBe(2);

            var healEff = effects.First(e => e.ElementType == ElementType.Heal);
            healEff.Healing.ShouldBe(GameConstants.HEAL_4_AMOUNT);

            var shieldEff = effects.First(e => e.ElementType == ElementType.Shield);
            shieldEff.Shield.ShouldBe(GameConstants.SHIELD_5_AMOUNT);
            shieldEff.StatusEffect.Type.ShouldBe(StatusEffect.Immunity);
        }
    }
}
