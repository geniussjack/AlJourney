class_name AbilityData
extends RefCounted
## Data structure representing a character ability. Stores the id, name,
## type, element, icon path, description, unlock cost and effects of the
## ability.

## Unique identifier for the ability.
var id: String
## Localization key for the ability's display name.
var name: String
## Whether the ability is an attack or a support ability.
var type: GameEnums.AbilityType
## The elemental flavor of the ability, used for color coding in the UI.
var element: GameEnums.AbilityElement
## Path to the ability's icon texture.
var icon_path: String
## Localization key for the ability's description.
var description: String
## Coin cost to unlock the ability (legacy ability system only).
var unlock_cost: int
## Named numeric effects the ability applies (e.g. "damage", "heal").
var effects: Dictionary[String, int]
## Who the ability can be targeted at: an enemy, or the caster/an ally.
var target_type: GameEnums.AbilityTargetType
## Whether the ability hits every valid target at once instead of one.
var is_aoe: bool
## Whether this is a hero's unique ultimate ability.
var is_ultimate: bool

## Whether this ability is an attack ability.
var is_attack_ability: bool:
	get:
		return type == GameEnums.AbilityType.ATTACK

## Whether this ability is a support ability.
var is_support_ability: bool:
	get:
		return type == GameEnums.AbilityType.SUPPORT

## Builds an ability from its id, name, type, element, presentation and effects.
func _init(
	id: String,
	name: String,
	type: GameEnums.AbilityType,
	element: GameEnums.AbilityElement,
	icon_path: String,
	description: String,
	unlock_cost: int,
	effects: Dictionary[String, int],
	target_type: GameEnums.AbilityTargetType,
	is_aoe: bool = false,
	is_ultimate: bool = false,
) -> void:
	self.id = id
	self.name = name
	self.type = type
	self.element = element
	self.icon_path = icon_path
	self.description = description
	self.unlock_cost = unlock_cost
	self.effects = effects
	self.target_type = target_type
	self.is_aoe = is_aoe
	self.is_ultimate = is_ultimate

## Returns the color associated with this ability's element. Used for color
## coding in the user interface.
func get_element_color() -> Color:
	match element:
		GameEnums.AbilityElement.FIRE:
			return Color.ORANGE
		GameEnums.AbilityElement.HEAL:
			return Color.GREEN
		GameEnums.AbilityElement.SWORD:
			return Color.RED
		GameEnums.AbilityElement.SHIELD:
			return Color.BLUE
		_:
			return Color.WHITE

## Returns the value of the first effect in the ability's effect dictionary.
## Convenient for abilities that only have a single numeric effect.
func get_primary_effect() -> int:
	if effects.is_empty():
		return 0
	return effects.values()[0]

## Returns the value of a specific effect by its name, or 0 if not found.
func get_effect(effect_name: String) -> int:
	return effects.get(effect_name, 0)

## Returns a string representation of the ability, including its name, type
## and element.
func _to_string() -> String:
	return "%s (%s - %s)" % [name, GameEnums.AbilityType.keys()[type], GameEnums.AbilityElement.keys()[element]]

## Serializes this ability into a plain Dictionary suitable for JSON storage.
func to_dict() -> Dictionary:
	return {
		"id": id,
		"name": name,
		"type": GameEnums.AbilityType.keys()[type],
		"element": GameEnums.AbilityElement.keys()[element],
		"icon_path": icon_path,
		"description": description,
		"unlock_cost": unlock_cost,
		"effects": effects,
		"target_type": GameEnums.AbilityTargetType.keys()[target_type],
		"is_aoe": is_aoe,
		"is_ultimate": is_ultimate,
	}

## Rebuilds an ability from a Dictionary previously produced by to_dict().
static func from_dict(data: Dictionary) -> AbilityData:
	var effects: Dictionary[String, int] = {}
	for key: String in (data.get("effects", {}) as Dictionary).keys():
		effects[key] = int(data["effects"][key])

	return AbilityData.new(
		data.get("id", ""),
		data.get("name", ""),
		GameEnums.AbilityType[data.get("type", "ATTACK")],
		GameEnums.AbilityElement[data.get("element", "FIRE")],
		data.get("icon_path", ""),
		data.get("description", ""),
		int(data.get("unlock_cost", 0)),
		effects,
		GameEnums.AbilityTargetType[data.get("target_type", "ENEMY")],
		bool(data.get("is_aoe", false)),
		bool(data.get("is_ultimate", false)),
	)
