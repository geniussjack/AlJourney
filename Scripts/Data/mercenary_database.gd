class_name MercenaryDatabase
extends RefCounted
## Static catalog of mercenary subclasses (see design document, section 4).
## Two archetypes (Mage/Warrior), five classes each (Healer/Warden/Pyro/
## Cryo/Storm) — a working roster agreed for the first playable version,
## explicitly not final; revisit once the game can be balance-tested.
##
## Attack-class subclasses apply their signature status effect directly as
## part of the ability itself (CombatEffectProcessor reads
## AbilityData.effects the same way it reads an equipped weapon's stats),
## rather than through equipment — mercenaries don't share the two heroes'
## equipment pool.

## Healer (Mage): heals an ally, or the whole party for less each.
static var healer_mage_ability_one: AbilityData = AbilityData.new(
	"merc_healer_mage_1", "ABILITY_MERC_HEALER_MAGE_1", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.HEAL,
	"", "ABILITY_MERC_HEALER_MAGE_1_DESC", 0, {"heal": 24} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF
)
static var healer_mage_ability_two: AbilityData = AbilityData.new(
	"merc_healer_mage_2", "ABILITY_MERC_HEALER_MAGE_2", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.HEAL,
	"", "ABILITY_MERC_HEALER_MAGE_2_DESC", 0, {"heal": 14} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF, true
)

## Healer (Warrior/Field Medic): same role, different flavor.
static var healer_warrior_ability_one: AbilityData = AbilityData.new(
	"merc_healer_warrior_1", "ABILITY_MERC_HEALER_WARRIOR_1", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.HEAL,
	"", "ABILITY_MERC_HEALER_WARRIOR_1_DESC", 0, {"heal": 24} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF
)
static var healer_warrior_ability_two: AbilityData = AbilityData.new(
	"merc_healer_warrior_2", "ABILITY_MERC_HEALER_WARRIOR_2", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.HEAL,
	"", "ABILITY_MERC_HEALER_WARRIOR_2_DESC", 0, {"heal": 14} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF, true
)

## Warden (Mage): shields an ally, or shields and cleanses negative effects.
static var warden_mage_ability_one: AbilityData = AbilityData.new(
	"merc_warden_mage_1", "ABILITY_MERC_WARDEN_MAGE_1", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.SHIELD,
	"", "ABILITY_MERC_WARDEN_MAGE_1_DESC", 0, {"shield": 26} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF
)
static var warden_mage_ability_two: AbilityData = AbilityData.new(
	"merc_warden_mage_2", "ABILITY_MERC_WARDEN_MAGE_2", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.SHIELD,
	"", "ABILITY_MERC_WARDEN_MAGE_2_DESC", 0, {"shield": 12, "cleanse": 1} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF
)

## Warden (Warrior/Shieldbearer): same role, different flavor.
static var warden_warrior_ability_one: AbilityData = AbilityData.new(
	"merc_warden_warrior_1", "ABILITY_MERC_WARDEN_WARRIOR_1", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.SHIELD,
	"", "ABILITY_MERC_WARDEN_WARRIOR_1_DESC", 0, {"shield": 26} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF
)
static var warden_warrior_ability_two: AbilityData = AbilityData.new(
	"merc_warden_warrior_2", "ABILITY_MERC_WARDEN_WARRIOR_2", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.SHIELD,
	"", "ABILITY_MERC_WARDEN_WARRIOR_2_DESC", 0, {"shield": 12, "cleanse": 1} as Dictionary[String, int], GameEnums.AbilityTargetType.ALLY_OR_SELF
)

## Pyro (Mage/Pyromancer): fire damage with a burn stacked on top.
static var pyro_mage_ability_one: AbilityData = AbilityData.new(
	"merc_pyro_mage_1", "ABILITY_MERC_PYRO_MAGE_1", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.FIRE,
	"", "ABILITY_MERC_PYRO_MAGE_1_DESC", 0, {"damage": 20, "burn_damage": 3} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)
static var pyro_mage_ability_two: AbilityData = AbilityData.new(
	"merc_pyro_mage_2", "ABILITY_MERC_PYRO_MAGE_2", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.FIRE,
	"", "ABILITY_MERC_PYRO_MAGE_2_DESC", 0, {"damage": 14, "burn_damage": 4} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)

## Pyro (Warrior/Axeman): physical damage with a bleed stacked on top.
static var pyro_warrior_ability_one: AbilityData = AbilityData.new(
	"merc_pyro_warrior_1", "ABILITY_MERC_PYRO_WARRIOR_1", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.BLEED,
	"", "ABILITY_MERC_PYRO_WARRIOR_1_DESC", 0, {"damage": 22, "bleed_damage": 3} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)
static var pyro_warrior_ability_two: AbilityData = AbilityData.new(
	"merc_pyro_warrior_2", "ABILITY_MERC_PYRO_WARRIOR_2", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.BLEED,
	"", "ABILITY_MERC_PYRO_WARRIOR_2_DESC", 0, {"damage": 16, "bleed_damage": 4} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)

## Cryo (Mage/Cryomancer): ice damage that weakens the target's defense.
static var cryo_mage_ability_one: AbilityData = AbilityData.new(
	"merc_cryo_mage_1", "ABILITY_MERC_CRYO_MAGE_1", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.ICE,
	"", "ABILITY_MERC_CRYO_MAGE_1_DESC", 0, {"damage": 16, "weaken_amount": 30} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)
static var cryo_mage_ability_two: AbilityData = AbilityData.new(
	"merc_cryo_mage_2", "ABILITY_MERC_CRYO_MAGE_2", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.ICE,
	"", "ABILITY_MERC_CRYO_MAGE_2_DESC", 0, {"damage": 10, "weaken_amount": 40} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)

## Cryo (Warrior/Spearman): physical damage that leaves the target vulnerable.
static var cryo_warrior_ability_one: AbilityData = AbilityData.new(
	"merc_cryo_warrior_1", "ABILITY_MERC_CRYO_WARRIOR_1", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.PIERCE,
	"", "ABILITY_MERC_CRYO_WARRIOR_1_DESC", 0, {"damage": 18, "vulnerable_amount": 30} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)
static var cryo_warrior_ability_two: AbilityData = AbilityData.new(
	"merc_cryo_warrior_2", "ABILITY_MERC_CRYO_WARRIOR_2", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.PIERCE,
	"", "ABILITY_MERC_CRYO_WARRIOR_2_DESC", 0, {"damage": 12, "vulnerable_amount": 40} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)

## Storm (Mage/Stormcaller): the glass-cannon class — highest raw damage
## among mercenaries, at the cost of low defense (see base stats below).
static var storm_mage_ability_one: AbilityData = AbilityData.new(
	"merc_storm_mage_1", "ABILITY_MERC_STORM_MAGE_1", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.LIGHTNING,
	"", "ABILITY_MERC_STORM_MAGE_1_DESC", 0, {"damage": 26, "shock_amount": 30} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)
static var storm_mage_ability_two: AbilityData = AbilityData.new(
	"merc_storm_mage_2", "ABILITY_MERC_STORM_MAGE_2", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.LIGHTNING,
	"", "ABILITY_MERC_STORM_MAGE_2_DESC", 0, {"damage": 30} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)

## Storm (Warrior/Berserker): the physical glass-cannon — same role as
## Stormcaller, pure damage instead of an elemental status effect.
static var storm_warrior_ability_one: AbilityData = AbilityData.new(
	"merc_storm_warrior_1", "ABILITY_MERC_STORM_WARRIOR_1", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.SWORD,
	"", "ABILITY_MERC_STORM_WARRIOR_1_DESC", 0, {"damage": 28} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)
static var storm_warrior_ability_two: AbilityData = AbilityData.new(
	"merc_storm_warrior_2", "ABILITY_MERC_STORM_WARRIOR_2", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.SWORD,
	"", "ABILITY_MERC_STORM_WARRIOR_2_DESC", 0, {"damage": 32} as Dictionary[String, int], GameEnums.AbilityTargetType.ENEMY
)

## Every subclass, keyed by archetype then class. Base stats are a
## placeholder first pass, not final balance.
static var subclasses: Dictionary[GameEnums.CharacterClass, Dictionary] = {
	GameEnums.CharacterClass.MAGE: {
		GameEnums.MercenaryClass.HEALER: MercenarySubclassData.new(GameEnums.CharacterClass.MAGE, GameEnums.MercenaryClass.HEALER, "MERCENARY_HEALER_MAGE", 85, 6, 2, healer_mage_ability_one, healer_mage_ability_two),
		GameEnums.MercenaryClass.WARDEN: MercenarySubclassData.new(GameEnums.CharacterClass.MAGE, GameEnums.MercenaryClass.WARDEN, "MERCENARY_WARDEN_MAGE", 80, 6, 4, warden_mage_ability_one, warden_mage_ability_two),
		GameEnums.MercenaryClass.PYRO: MercenarySubclassData.new(GameEnums.CharacterClass.MAGE, GameEnums.MercenaryClass.PYRO, "MERCENARY_PYRO_MAGE", 75, 9, 2, pyro_mage_ability_one, pyro_mage_ability_two),
		GameEnums.MercenaryClass.CRYO: MercenarySubclassData.new(GameEnums.CharacterClass.MAGE, GameEnums.MercenaryClass.CRYO, "MERCENARY_CRYO_MAGE", 75, 8, 2, cryo_mage_ability_one, cryo_mage_ability_two),
		GameEnums.MercenaryClass.STORM: MercenarySubclassData.new(GameEnums.CharacterClass.MAGE, GameEnums.MercenaryClass.STORM, "MERCENARY_STORM_MAGE", 65, 11, 1, storm_mage_ability_one, storm_mage_ability_two),
	},
	GameEnums.CharacterClass.WARRIOR: {
		GameEnums.MercenaryClass.HEALER: MercenarySubclassData.new(GameEnums.CharacterClass.WARRIOR, GameEnums.MercenaryClass.HEALER, "MERCENARY_HEALER_WARRIOR", 125, 9, 4, healer_warrior_ability_one, healer_warrior_ability_two),
		GameEnums.MercenaryClass.WARDEN: MercenarySubclassData.new(GameEnums.CharacterClass.WARRIOR, GameEnums.MercenaryClass.WARDEN, "MERCENARY_WARDEN_WARRIOR", 120, 9, 7, warden_warrior_ability_one, warden_warrior_ability_two),
		GameEnums.MercenaryClass.PYRO: MercenarySubclassData.new(GameEnums.CharacterClass.WARRIOR, GameEnums.MercenaryClass.PYRO, "MERCENARY_PYRO_WARRIOR", 115, 13, 3, pyro_warrior_ability_one, pyro_warrior_ability_two),
		GameEnums.MercenaryClass.CRYO: MercenarySubclassData.new(GameEnums.CharacterClass.WARRIOR, GameEnums.MercenaryClass.CRYO, "MERCENARY_CRYO_WARRIOR", 115, 12, 3, cryo_warrior_ability_one, cryo_warrior_ability_two),
		GameEnums.MercenaryClass.STORM: MercenarySubclassData.new(GameEnums.CharacterClass.WARRIOR, GameEnums.MercenaryClass.STORM, "MERCENARY_STORM_WARRIOR", 100, 16, 2, storm_warrior_ability_one, storm_warrior_ability_two),
	},
}

## Returns the subclass definition for the given archetype/class pairing.
static func get_subclass(archetype: GameEnums.CharacterClass, mercenary_class: GameEnums.MercenaryClass) -> MercenarySubclassData:
	var archetype_subclasses: Dictionary = subclasses.get(archetype, {})
	return archetype_subclasses.get(mercenary_class)

## Returns every subclass available for the given archetype, in
## GameEnums.MercenaryClass declaration order.
static func get_subclasses_for_archetype(archetype: GameEnums.CharacterClass) -> Array[MercenarySubclassData]:
	var result: Array[MercenarySubclassData] = []
	for mercenary_class: GameEnums.MercenaryClass in GameEnums.MercenaryClass.values():
		var subclass: MercenarySubclassData = get_subclass(archetype, mercenary_class)
		if subclass != null:
			result.append(subclass)
	return result
