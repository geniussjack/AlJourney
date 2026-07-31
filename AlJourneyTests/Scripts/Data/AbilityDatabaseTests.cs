using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;

namespace AlJourneyTests.Scripts.Data
{
    public class AbilityDatabaseTests
    {
        [Fact]
        public void AltarionAttack_IsSingleTargetFireAttackOnEnemies()
        {
            AbilityData ability = AbilityDatabase.AltarionAttack;

            Assert.Equal(AbilityType.Attack, ability.Type);
            Assert.Equal(AbilityElement.Fire, ability.Element);
            Assert.Equal(AbilityTargetType.Enemy, ability.TargetType);
            Assert.False(ability.IsAoE);
            Assert.False(ability.IsUltimate);
            Assert.Equal(22, ability.GetEffect("damage"));
        }

        [Fact]
        public void AltarionSupport_IsHealingAbilityForAllies()
        {
            AbilityData ability = AbilityDatabase.AltarionSupport;

            Assert.Equal(AbilityType.Support, ability.Type);
            Assert.Equal(AbilityElement.Heal, ability.Element);
            Assert.Equal(AbilityTargetType.AllyOrSelf, ability.TargetType);
            Assert.Equal(18, ability.GetEffect("heal"));
        }

        [Fact]
        public void AltarionUltimate_IsAoEFireAttackOnEnemies()
        {
            AbilityData ability = AbilityDatabase.AltarionUltimate;

            Assert.True(ability.IsAoE);
            Assert.True(ability.IsUltimate);
            Assert.Equal(AbilityTargetType.Enemy, ability.TargetType);
            Assert.Equal(40, ability.GetEffect("damage"));
        }

        [Fact]
        public void AldricAttack_IsSingleTargetSwordAttackOnEnemies()
        {
            AbilityData ability = AbilityDatabase.AldricAttack;

            Assert.Equal(AbilityType.Attack, ability.Type);
            Assert.Equal(AbilityElement.Sword, ability.Element);
            Assert.Equal(AbilityTargetType.Enemy, ability.TargetType);
            Assert.Equal(26, ability.GetEffect("damage"));
        }

        [Fact]
        public void AldricSupport_IsShieldAbilityForAllies()
        {
            AbilityData ability = AbilityDatabase.AldricSupport;

            Assert.Equal(AbilityType.Support, ability.Type);
            Assert.Equal(AbilityElement.Shield, ability.Element);
            Assert.Equal(AbilityTargetType.AllyOrSelf, ability.TargetType);
            Assert.Equal(22, ability.GetEffect("shield"));
        }

        [Fact]
        public void AldricUltimate_IsSingleTargetSwordUltimateOnEnemies()
        {
            AbilityData ability = AbilityDatabase.AldricUltimate;

            Assert.False(ability.IsAoE);
            Assert.True(ability.IsUltimate);
            Assert.Equal(AbilityTargetType.Enemy, ability.TargetType);
            Assert.Equal(70, ability.GetEffect("damage"));
        }

        [Fact]
        public void Templates_ContainsOnlyTheFourNonUltimateHeroAbilities()
        {
            Assert.Equal(4, AbilityDatabase.Templates.Count);
            Assert.Same(AbilityDatabase.AltarionAttack, AbilityDatabase.Templates[AbilityDatabase.AltarionAttack.Id]);
            Assert.Same(AbilityDatabase.AltarionSupport, AbilityDatabase.Templates[AbilityDatabase.AltarionSupport.Id]);
            Assert.Same(AbilityDatabase.AldricAttack, AbilityDatabase.Templates[AbilityDatabase.AldricAttack.Id]);
            Assert.Same(AbilityDatabase.AldricSupport, AbilityDatabase.Templates[AbilityDatabase.AldricSupport.Id]);
            Assert.DoesNotContain(AbilityDatabase.AltarionUltimate.Id, AbilityDatabase.Templates.Keys);
            Assert.DoesNotContain(AbilityDatabase.AldricUltimate.Id, AbilityDatabase.Templates.Keys);
        }

        [Fact]
        public void GetHeroAbilities_Mage_ReturnsAltarionAttackAndSupport()
        {
            (AbilityData attack, AbilityData support) = AbilityDatabase.GetHeroAbilities(CharacterClass.Mage);

            Assert.Same(AbilityDatabase.AltarionAttack, attack);
            Assert.Same(AbilityDatabase.AltarionSupport, support);
        }

        [Fact]
        public void GetHeroAbilities_Warrior_ReturnsAldricAttackAndSupport()
        {
            (AbilityData attack, AbilityData support) = AbilityDatabase.GetHeroAbilities(CharacterClass.Warrior);

            Assert.Same(AbilityDatabase.AldricAttack, attack);
            Assert.Same(AbilityDatabase.AldricSupport, support);
        }

        [Fact]
        public void GetHeroAbilities_UnknownClass_FallsBackToAltarion()
        {
            (AbilityData attack, AbilityData support) = AbilityDatabase.GetHeroAbilities((CharacterClass)999);

            Assert.Same(AbilityDatabase.AltarionAttack, attack);
            Assert.Same(AbilityDatabase.AltarionSupport, support);
        }

        [Fact]
        public void GetHeroUltimate_Mage_ReturnsAltarionUltimate()
        {
            Assert.Same(AbilityDatabase.AltarionUltimate, AbilityDatabase.GetHeroUltimate(CharacterClass.Mage));
        }

        [Fact]
        public void GetHeroUltimate_Warrior_ReturnsAldricUltimate()
        {
            Assert.Same(AbilityDatabase.AldricUltimate, AbilityDatabase.GetHeroUltimate(CharacterClass.Warrior));
        }

        [Fact]
        public void GetHeroUltimate_UnknownClass_FallsBackToAltarionUltimate()
        {
            Assert.Same(AbilityDatabase.AltarionUltimate, AbilityDatabase.GetHeroUltimate((CharacterClass)999));
        }
    }
}
