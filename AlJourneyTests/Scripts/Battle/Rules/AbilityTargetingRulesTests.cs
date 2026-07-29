using AlJourney.Scripts.Battle.Rules;
using AlJourney.Scripts.Core;
using System.Collections.Generic;

namespace AlJourneyTests.Scripts.Battle.Rules
{
    /// <summary>
    /// Лёгкий двойник цели для юнит-тестов, не зависящий от Godot.Node.
    /// </summary>
    public class FakeCombatant(string name, bool isAlive)
    {
        public string Name { get; } = name;
        public bool IsAlive { get; set; } = isAlive;
    }

    public class AbilityTargetingRulesTests
    {
        private static bool IsAlive(FakeCombatant c) => c.IsAlive;

        [Fact]
        public void GetValidTargets_EnemyAbility_ReturnsOnlyAliveEnemies()
        {
            List<FakeCombatant> allies = [new("Eltarion", true)];
            List<FakeCombatant> enemies = [new("Skeleton", true), new("Zombie", false)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.GetValidTargets(
                AbilityTargetType.Enemy, allies, enemies, IsAlive);

            Assert.Single(result);
            Assert.Equal("Skeleton", result[0].Name);
        }

        [Fact]
        public void GetValidTargets_AllyAbility_ReturnsOnlyAliveAllies()
        {
            List<FakeCombatant> allies = [new("Eltarion", true), new("Eldric", false)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.GetValidTargets(
                AbilityTargetType.AllyOrSelf, allies, enemies, IsAlive);

            Assert.Single(result);
            Assert.Equal("Eltarion", result[0].Name);
        }

        [Fact]
        public void GetValidTargets_NoAliveTargetsInPool_ReturnsEmpty()
        {
            List<FakeCombatant> allies = [new("Eltarion", true)];
            List<FakeCombatant> enemies = [new("Skeleton", false)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.GetValidTargets(
                AbilityTargetType.Enemy, allies, enemies, IsAlive);

            Assert.Empty(result);
        }

        [Fact]
        public void ResolveEffectTargets_SingleTarget_ReturnsOnlyChosenTarget()
        {
            List<FakeCombatant> allies = [new("Eltarion", true), new("Eldric", true)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];
            FakeCombatant chosen = enemies[0];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.Enemy, isAoE: false, chosen, allies, enemies, IsAlive);

            Assert.Single(result);
            Assert.Same(chosen, result[0]);
        }

        [Fact]
        public void ResolveEffectTargets_SingleTarget_ChosenTargetDead_ReturnsEmpty()
        {
            List<FakeCombatant> allies = [new("Eltarion", true)];
            FakeCombatant deadEnemy = new("Skeleton", false);
            List<FakeCombatant> enemies = [deadEnemy];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.Enemy, isAoE: false, deadEnemy, allies, enemies, IsAlive);

            Assert.Empty(result);
        }

        [Fact]
        public void ResolveEffectTargets_SingleTarget_NullChosenTarget_ReturnsEmpty()
        {
            List<FakeCombatant> allies = [new("Eltarion", true)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.Enemy, isAoE: false, null, allies, enemies, IsAlive);

            Assert.Empty(result);
        }

        [Fact]
        public void ResolveEffectTargets_AoEEnemyAbility_HitsAllAliveEnemiesIgnoringChosenTarget()
        {
            List<FakeCombatant> allies = [new("Eltarion", true)];
            FakeCombatant chosen = new("Skeleton", true);
            List<FakeCombatant> enemies = [chosen, new("Zombie", true), new("Slime", false)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.Enemy, isAoE: true, chosen, allies, enemies, IsAlive);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "Skeleton");
            Assert.Contains(result, c => c.Name == "Zombie");
        }

        [Fact]
        public void ResolveEffectTargets_AoEAllyAbility_HitsWholeAliveParty()
        {
            List<FakeCombatant> allies = [new("Eltarion", true), new("Eldric", true), new("Companion", false)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.AllyOrSelf, isAoE: true, allies[0], allies, enemies, IsAlive);

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, c => c.Name == "Companion");
        }
    }
}
