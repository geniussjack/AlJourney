using AlJourney.Scripts.Battle.Rules;
using AlJourney.Scripts.Core;

namespace AlJourneyTests.Scripts.Battle.Rules
{
    /// <summary>
    /// Лёгкий двойник цели для юнит-тестов, не зависящий от Godot.Node.
    /// </summary>
    public class FakeCombatant(string name, bool isAlive, int currentHealth = 0)
    {
        public string Name { get; } = name;
        public bool IsAlive { get; set; } = isAlive;
        public int CurrentHealth { get; set; } = currentHealth;
    }

    public class AbilityTargetingRulesTests
    {
        private static bool IsAlive(FakeCombatant c)
        {
            return c.IsAlive;
        }

        private static int CurrentHealth(FakeCombatant c)
        {
            return c.CurrentHealth;
        }

        [Fact]
        public void GetValidTargets_EnemyAbility_ReturnsOnlyAliveEnemies()
        {
            List<FakeCombatant> allies = [new("Altarion", true)];
            List<FakeCombatant> enemies = [new("Skeleton", true), new("Zombie", false)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.GetValidTargets(
                AbilityTargetType.Enemy, allies, enemies, IsAlive);

            _ = Assert.Single(result);
            Assert.Equal("Skeleton", result[0].Name);
        }

        [Fact]
        public void GetValidTargets_AllyAbility_ReturnsOnlyAliveAllies()
        {
            List<FakeCombatant> allies = [new("Altarion", true), new("Aldric", false)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.GetValidTargets(
                AbilityTargetType.AllyOrSelf, allies, enemies, IsAlive);

            _ = Assert.Single(result);
            Assert.Equal("Altarion", result[0].Name);
        }

        [Fact]
        public void GetValidTargets_NoAliveTargetsInPool_ReturnsEmpty()
        {
            List<FakeCombatant> allies = [new("Altarion", true)];
            List<FakeCombatant> enemies = [new("Skeleton", false)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.GetValidTargets(
                AbilityTargetType.Enemy, allies, enemies, IsAlive);

            Assert.Empty(result);
        }

        [Fact]
        public void ResolveEffectTargets_SingleTarget_ReturnsOnlyChosenTarget()
        {
            List<FakeCombatant> allies = [new("Altarion", true), new("Aldric", true)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];
            FakeCombatant chosen = enemies[0];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.Enemy, isAoE: false, chosen, allies, enemies, IsAlive);

            _ = Assert.Single(result);
            Assert.Same(chosen, result[0]);
        }

        [Fact]
        public void ResolveEffectTargets_SingleTarget_ChosenTargetDead_ReturnsEmpty()
        {
            List<FakeCombatant> allies = [new("Altarion", true)];
            FakeCombatant deadEnemy = new("Skeleton", false);
            List<FakeCombatant> enemies = [deadEnemy];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.Enemy, isAoE: false, deadEnemy, allies, enemies, IsAlive);

            Assert.Empty(result);
        }

        [Fact]
        public void ResolveEffectTargets_SingleTarget_NullChosenTarget_ReturnsEmpty()
        {
            List<FakeCombatant> allies = [new("Altarion", true)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.Enemy, isAoE: false, null, allies, enemies, IsAlive);

            Assert.Empty(result);
        }

        [Fact]
        public void ResolveEffectTargets_AoEEnemyAbility_HitsAllAliveEnemiesIgnoringChosenTarget()
        {
            List<FakeCombatant> allies = [new("Altarion", true)];
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
            List<FakeCombatant> allies = [new("Altarion", true), new("Aldric", true), new("Companion", false)];
            List<FakeCombatant> enemies = [new("Skeleton", true)];

            IReadOnlyList<FakeCombatant> result = AbilityTargetingRules.ResolveEffectTargets(
                AbilityTargetType.AllyOrSelf, isAoE: true, allies[0], allies, enemies, IsAlive);

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, c => c.Name == "Companion");
        }

        [Fact]
        public void SelectHighestHealthTarget_ReturnsAliveCandidateWithMostHealth()
        {
            List<FakeCombatant> candidates =
            [
                new("Skeleton", true, currentHealth: 30),
                new("Zombie", true, currentHealth: 80),
                new("Slime", true, currentHealth: 50)
            ];

            FakeCombatant? result = AbilityTargetingRules.SelectHighestHealthTarget(candidates, CurrentHealth, IsAlive);

            Assert.Same(candidates[1], result);
        }

        [Fact]
        public void SelectHighestHealthTarget_IgnoresDeadCandidatesEvenWithMoreHealth()
        {
            List<FakeCombatant> candidates =
            [
                new("Skeleton", true, currentHealth: 30),
                new("Zombie", false, currentHealth: 999)
            ];

            FakeCombatant? result = AbilityTargetingRules.SelectHighestHealthTarget(candidates, CurrentHealth, IsAlive);

            Assert.Same(candidates[0], result);
        }

        [Fact]
        public void SelectHighestHealthTarget_NoAliveCandidates_ReturnsNull()
        {
            List<FakeCombatant> candidates = [new("Skeleton", false, currentHealth: 30)];

            FakeCombatant? result = AbilityTargetingRules.SelectHighestHealthTarget(candidates, CurrentHealth, IsAlive);

            Assert.Null(result);
        }

        [Fact]
        public void GetValidTargets_NullAllies_ThrowsArgumentNullException()
        {
            _ = Assert.Throws<ArgumentNullException>(
                () => AbilityTargetingRules.GetValidTargets<FakeCombatant>(AbilityTargetType.Enemy, null!, [], IsAlive));
        }

        [Fact]
        public void GetValidTargets_NullEnemies_ThrowsArgumentNullException()
        {
            _ = Assert.Throws<ArgumentNullException>(
                () => AbilityTargetingRules.GetValidTargets<FakeCombatant>(AbilityTargetType.Enemy, [], null!, IsAlive));
        }

        [Fact]
        public void GetValidTargets_NullIsAlivePredicate_ThrowsArgumentNullException()
        {
            _ = Assert.Throws<ArgumentNullException>(
                () => AbilityTargetingRules.GetValidTargets<FakeCombatant>(AbilityTargetType.Enemy, [], [], null!));
        }

        [Fact]
        public void SelectHighestHealthTarget_NullCandidates_ThrowsArgumentNullException()
        {
            _ = Assert.Throws<ArgumentNullException>(
                () => AbilityTargetingRules.SelectHighestHealthTarget<FakeCombatant>(null!, CurrentHealth, IsAlive));
        }

        [Fact]
        public void SelectHighestHealthTarget_NullCurrentHealthSelector_ThrowsArgumentNullException()
        {
            _ = Assert.Throws<ArgumentNullException>(
                () => AbilityTargetingRules.SelectHighestHealthTarget<FakeCombatant>([], null!, IsAlive));
        }

        [Fact]
        public void SelectHighestHealthTarget_NullIsAlivePredicate_ThrowsArgumentNullException()
        {
            _ = Assert.Throws<ArgumentNullException>(
                () => AbilityTargetingRules.SelectHighestHealthTarget<FakeCombatant>([], CurrentHealth, null!));
        }
    }
}
