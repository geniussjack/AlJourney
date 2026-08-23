class_name AbilityDatabase
extends RefCounted
## Database of character abilities. Altarion (Mage) and Aldric (Warrior)
## each have one attack and one defensive/support ability, plus a unique
## ultimate ability each. This is the final ability set for the main heroes
## themselves (not a placeholder) — they are not tied to equipment and are
## not unlocked with coins, unlike future mercenaries, whose abilities will
## be determined by their equipment subclass/type.

## Altarion's attack ability: single-target damage to an enemy.
static var altarion_attack: AbilityData = AbilityData.new(
	"altarion_fireball", "ABILITY_ALTARION_FIREBALL", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.FIRE,
	"res://Resources/Sprites/Abilities/fireball.png",
	"ABILITY_ALTARION_FIREBALL_DESC",
	0,
	{"damage": 22} as Dictionary[String, int],
	GameEnums.AbilityTargetType.ENEMY
)

## Altarion's support ability: heals himself or an ally.
static var altarion_support: AbilityData = AbilityData.new(
	"altarion_healing_light", "ABILITY_ALTARION_HEALING_LIGHT", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.HEAL,
	"res://Resources/Sprites/Abilities/healing_light.png",
	"ABILITY_ALTARION_HEALING_LIGHT_DESC",
	0,
	{"heal": 18} as Dictionary[String, int],
	GameEnums.AbilityTargetType.ALLY_OR_SELF
)

## Altarion's ultimate ability: a firestorm hitting every living enemy (AoE).
static var altarion_ultimate: AbilityData = AbilityData.new(
	"altarion_meteor_storm", "ABILITY_ALTARION_METEOR_STORM", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.FIRE,
	"res://Resources/Sprites/Abilities/meteor_storm.png",
	"ABILITY_ALTARION_METEOR_STORM_DESC",
	0,
	{"damage": 40} as Dictionary[String, int],
	GameEnums.AbilityTargetType.ENEMY,
	true,
	true
)

## Aldric's attack ability: single-target strike against an enemy.
static var aldric_attack: AbilityData = AbilityData.new(
	"aldric_sword_strike", "ABILITY_ALDRIC_SWORD_STRIKE", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.SWORD,
	"res://Resources/Sprites/Abilities/sword_strike.png",
	"ABILITY_ALDRIC_SWORD_STRIKE_DESC",
	0,
	{"damage": 26} as Dictionary[String, int],
	GameEnums.AbilityTargetType.ENEMY
)

## Aldric's support ability: shields himself or an ally.
static var aldric_support: AbilityData = AbilityData.new(
	"aldric_shield_wall", "ABILITY_ALDRIC_SHIELD_WALL", GameEnums.AbilityType.SUPPORT, GameEnums.AbilityElement.SHIELD,
	"res://Resources/Sprites/Abilities/shield_wall.png",
	"ABILITY_ALDRIC_SHIELD_WALL_DESC",
	0,
	{"shield": 22} as Dictionary[String, int],
	GameEnums.AbilityTargetType.ALLY_OR_SELF
)

## Aldric's ultimate ability: a crushing blow against the enemy with the
## highest current HP. The target is selected automatically (see
## AbilityTargetingRules.select_highest_health_target), without player
## confirmation.
static var aldric_ultimate: AbilityData = AbilityData.new(
	"aldric_crushing_blow", "ABILITY_ALDRIC_CRUSHING_BLOW", GameEnums.AbilityType.ATTACK, GameEnums.AbilityElement.SWORD,
	"res://Resources/Sprites/Abilities/crushing_blow.png",
	"ABILITY_ALDRIC_CRUSHING_BLOW_DESC",
	0,
	{"damage": 70} as Dictionary[String, int],
	GameEnums.AbilityTargetType.ENEMY,
	false,
	true
)

## Flat registry of every ability by id. Only used by the legacy ability
## unlock/equip system (AbilitySystem), which does not participate in the
## heroes' combat logic (see design document, section 4) and has been left
## untouched outside the scope of the GDScript rewrite.
static var templates: Dictionary[String, AbilityData] = {
	altarion_attack.id: altarion_attack,
	altarion_support.id: altarion_support,
	aldric_attack.id: aldric_attack,
	aldric_support.id: aldric_support,
}

## Returns the fixed pair of abilities (attack, support) for the main hero
## of the given class. Only applies to Altarion/Aldric — mercenaries'
## abilities will be determined by their equipment subclass, not by this
## method (see design document, section 4).
static func get_hero_abilities(hero_class: GameEnums.CharacterClass) -> Array[AbilityData]:
	match hero_class:
		GameEnums.CharacterClass.WARRIOR:
			return [aldric_attack, aldric_support]
		_:
			return [altarion_attack, altarion_support]

## Returns the unique ultimate ability of the main hero of the given class.
static func get_hero_ultimate(hero_class: GameEnums.CharacterClass) -> AbilityData:
	match hero_class:
		GameEnums.CharacterClass.WARRIOR:
			return aldric_ultimate
		_:
			return altarion_ultimate
