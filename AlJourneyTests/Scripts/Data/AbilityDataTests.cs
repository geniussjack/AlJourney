using AlJourney.Scripts.Core;
using AlJourney.Scripts.Data;
using Godot;

namespace AlJourneyTests.Scripts.Data
{
    public class AbilityDataTests
    {
        private static AbilityData CreateAbility(
            AbilityType type = AbilityType.Attack,
            AbilityElement element = AbilityElement.Fire,
            Dictionary<string, int>? effects = null)
        {
            return new AbilityData(
                "test_ability",
                "Test Ability",
                type,
                element,
                "res://icon.png",
                "Test description",
                0,
                effects ?? new Dictionary<string, int> { ["damage"] = 10 },
                AbilityTargetType.Enemy);
        }

        [Fact]
        public void Constructor_StoresAllSuppliedValues()
        {
            Dictionary<string, int> effects = new() { ["damage"] = 22 };
            AbilityData ability = new(
                "altarion_fireball", "ABILITY_ALTARION_FIREBALL", AbilityType.Attack, AbilityElement.Fire,
                "res://icon.png", "desc", 5, effects, AbilityTargetType.Enemy, IsAoE: true, IsUltimate: true);

            Assert.Equal("altarion_fireball", ability.Id);
            Assert.Equal("ABILITY_ALTARION_FIREBALL", ability.Name);
            Assert.Equal(AbilityType.Attack, ability.Type);
            Assert.Equal(AbilityElement.Fire, ability.Element);
            Assert.Equal("res://icon.png", ability.IconPath);
            Assert.Equal("desc", ability.Description);
            Assert.Equal(5, ability.UnlockCost);
            Assert.Same(effects, ability.Effects);
            Assert.Equal(AbilityTargetType.Enemy, ability.TargetType);
            Assert.True(ability.IsAoE);
            Assert.True(ability.IsUltimate);
        }

        [Fact]
        public void Constructor_DefaultsIsAoEAndIsUltimateToFalse()
        {
            AbilityData ability = CreateAbility();

            Assert.False(ability.IsAoE);
            Assert.False(ability.IsUltimate);
        }

        [Fact]
        public void GetElementColor_KnownElements_ReturnMatchingNamedColors()
        {
            Assert.Equal(Colors.Orange, CreateAbility(element: AbilityElement.Fire).GetElementColor());
            Assert.Equal(Colors.Green, CreateAbility(element: AbilityElement.Heal).GetElementColor());
            Assert.Equal(Colors.Red, CreateAbility(element: AbilityElement.Sword).GetElementColor());
            Assert.Equal(Colors.Blue, CreateAbility(element: AbilityElement.Shield).GetElementColor());
        }

        [Fact]
        public void GetElementColor_UnknownElement_ReturnsWhite()
        {
            AbilityData ability = CreateAbility(element: (AbilityElement)999);

            Color color = ability.GetElementColor();

            Assert.Equal(Colors.White, color);
        }

        [Fact]
        public void IsAttackAbility_AttackType_ReturnsTrue()
        {
            AbilityData ability = CreateAbility(type: AbilityType.Attack);

            Assert.True(ability.IsAttackAbility);
            Assert.False(ability.IsSupportAbility);
        }

        [Fact]
        public void IsSupportAbility_SupportType_ReturnsTrue()
        {
            AbilityData ability = CreateAbility(type: AbilityType.Support);

            Assert.True(ability.IsSupportAbility);
            Assert.False(ability.IsAttackAbility);
        }

        [Fact]
        public void GetPrimaryEffect_SingleEffect_ReturnsItsValue()
        {
            AbilityData ability = CreateAbility(effects: new Dictionary<string, int> { ["heal"] = 18 });

            Assert.Equal(18, ability.GetPrimaryEffect());
        }

        [Fact]
        public void GetPrimaryEffect_NoEffects_ReturnsZero()
        {
            AbilityData ability = CreateAbility(effects: []);

            Assert.Equal(0, ability.GetPrimaryEffect());
        }

        [Fact]
        public void GetEffect_ExistingKey_ReturnsValue()
        {
            AbilityData ability = CreateAbility(effects: new Dictionary<string, int> { ["shield"] = 22 });

            Assert.Equal(22, ability.GetEffect("shield"));
        }

        [Fact]
        public void GetEffect_MissingKey_ReturnsZero()
        {
            AbilityData ability = CreateAbility(effects: new Dictionary<string, int> { ["shield"] = 22 });

            Assert.Equal(0, ability.GetEffect("damage"));
        }

        [Fact]
        public void ToString_FormatsNameTypeAndElement()
        {
            AbilityData ability = CreateAbility(type: AbilityType.Attack, element: AbilityElement.Fire) with { Name = "Fireball" };

            Assert.Equal("Fireball (Attack - Fire)", ability.ToString());
        }
    }
}
