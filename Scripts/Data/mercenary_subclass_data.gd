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
## Localization key for the subclass's display name.
var name_key: String
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

## Builds a mercenary subclass definition.
func _init(
	archetype: GameEnums.CharacterClass,
	mercenary_class: GameEnums.MercenaryClass,
	name_key: String,
	base_hp: int,
	base_damage: int,
	base_defense: int,
	ability_one: AbilityData,
	ability_two: AbilityData,
) -> void:
	self.archetype = archetype
	self.mercenary_class = mercenary_class
	self.name_key = name_key
	self.base_hp = base_hp
	self.base_damage = base_damage
	self.base_defense = base_defense
	self.ability_one = ability_one
	self.ability_two = ability_two
