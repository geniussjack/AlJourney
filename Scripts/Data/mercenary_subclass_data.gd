class_name MercenarySubclassData
extends RefCounted
## Data structure representing one mercenary subclass: a specific
## (archetype, class) pairing — e.g. Mage archetype + Healer class =
## "Целитель" subclass, Warrior archetype + Healer class = "Полевой
## лекарь" subclass. See design document, section 4.

## The archetype this subclass belongs to (Mage or Warrior).
var archetype: GameEnums.CharacterClass
## The abstract class/role this subclass specializes in.
var mercenary_class: GameEnums.MercenaryClass
## Localization key for the subclass/role's display name (e.g. "Целитель").
var name_key: String
## Localization key for this mercenary's personal name (e.g. "Лира") — see
## design document, section 9: mercenaries are named survivors, not an
## anonymous pool. v1 simplification: exactly one named mercenary per
## subclass, so the personal identity lives directly on the subclass
## definition instead of a separate roster-entry class.
var character_name_key: String
## Base maximum health, before ScalingSystem/level bonuses.
var base_hp: int
## Base damage, before ScalingSystem/level bonuses.
var base_damage: int
## Base defense, before ScalingSystem/level bonuses.
var base_defense: int
## The subclass's two abilities — both the same type (attack or support),
## per design document, section 4.
var ability_one: AbilityData
var ability_two: AbilityData
## Barracks level required to unlock this mercenary, or 0 if unlocked by
## story progress instead (see required_level_id). Exactly one of the two
## unlock conditions is set per mercenary.
var required_barracks_level: int
## Campaign level id that must be completed (or be the current level, for
## the always-available first level) to unlock this mercenary, or ""
## if unlocked by Barracks level instead (see required_barracks_level).
var required_level_id: String

## Builds a mercenary subclass definition.
func _init(
	archetype: GameEnums.CharacterClass,
	mercenary_class: GameEnums.MercenaryClass,
	name_key: String,
	character_name_key: String,
	base_hp: int,
	base_damage: int,
	base_defense: int,
	ability_one: AbilityData,
	ability_two: AbilityData,
	required_barracks_level: int = 0,
	required_level_id: String = "",
) -> void:
	self.archetype = archetype
	self.mercenary_class = mercenary_class
	self.name_key = name_key
	self.character_name_key = character_name_key
	self.base_hp = base_hp
	self.base_damage = base_damage
	self.base_defense = base_defense
	self.ability_one = ability_one
	self.ability_two = ability_two
	self.required_barracks_level = required_barracks_level
	self.required_level_id = required_level_id

## Returns a stable string identifier for this subclass, used to key
## roster/recovery state in GameStateManager and SaveData (e.g. "MAGE_HEALER").
func get_key() -> String:
	return "%s_%s" % [GameEnums.CharacterClass.keys()[archetype], GameEnums.MercenaryClass.keys()[mercenary_class]]
