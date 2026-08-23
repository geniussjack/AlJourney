class_name EquipmentData
extends RefCounted
## Data structure representing an equipment item. Holds its type, rarity,
## upgrade level, and base stats and special abilities.

## Unique identifier for the item.
var id: String
## Localization key for the item's display name.
var name: String
## Localization key for the item's description, or an empty string if none.
var description_key: String
## The equipment slot this item occupies.
var slot: GameEnums.EquipmentSlot
## The item's rarity tier.
var rarity: GameEnums.EquipmentRarity
## The item's current upgrade level.
var current_level: int
## The item's maximum upgrade level.
var max_level: int
## Named numeric base stats the item grants (e.g. "damage", "defense").
var base_stats: Dictionary[String, int]
## Named special abilities the item grants. Currently always empty; reserved
## for the future subclass/ability-set system (see design document, section 4).
var special_abilities: Dictionary[String, String]

## Builds an equipment item from its id, name, slot, rarity, level range and stats.
func _init(
	id: String,
	name: String,
	description_key: String,
	slot: GameEnums.EquipmentSlot,
	rarity: GameEnums.EquipmentRarity,
	current_level: int,
	max_level: int,
	base_stats: Dictionary[String, int],
	special_abilities: Dictionary[String, String],
) -> void:
	self.id = id
	self.name = name
	self.description_key = description_key
	self.slot = slot
	self.rarity = rarity
	self.current_level = current_level
	self.max_level = max_level
	self.base_stats = base_stats
	self.special_abilities = special_abilities

## Returns the color associated with the item's rarity tier. Used to
## highlight the item in the inventory or UI.
func get_rarity_color() -> Color:
	match rarity:
		GameEnums.EquipmentRarity.COMMON:
			return Color.GRAY
		GameEnums.EquipmentRarity.UNCOMMON:
			return Color.GREEN
		GameEnums.EquipmentRarity.RARE:
			return Color.BLUE
		GameEnums.EquipmentRarity.EPIC:
			return Color.PURPLE
		GameEnums.EquipmentRarity.LEGENDARY:
			return Color.ORANGE
		_:
			return Color.WHITE

## Returns the drop chance of the item based on its rarity.
func get_drop_chance() -> float:
	match rarity:
		GameEnums.EquipmentRarity.COMMON:
			return 40.0
		GameEnums.EquipmentRarity.UNCOMMON:
			return 30.0
		GameEnums.EquipmentRarity.RARE:
			return 15.0
		GameEnums.EquipmentRarity.EPIC:
			return 10.0
		GameEnums.EquipmentRarity.LEGENDARY:
			return 5.0
		_:
			return 0.0

## Computes the cost to upgrade the item to its next level. The cost may
## scale based on the current wave.
## wave_number: current wave number used for the price markup; when 0, the
## base cost is returned.
## Returns the number of coins required to upgrade, or 0 at max level.
func get_upgrade_cost(wave_number: int = 0) -> int:
	if current_level >= max_level:
		return 0

	var base_cost: int
	match rarity:
		GameEnums.EquipmentRarity.COMMON:
			base_cost = 50
		GameEnums.EquipmentRarity.UNCOMMON:
			base_cost = 100
		GameEnums.EquipmentRarity.RARE:
			base_cost = 200
		GameEnums.EquipmentRarity.EPIC:
			base_cost = 400
		GameEnums.EquipmentRarity.LEGENDARY:
			base_cost = 800
		_:
			base_cost = 50

	var level_cost: int = base_cost * current_level

	return ScalingSystem.scale_cost(level_cost, wave_number) if wave_number > 0 else level_cost

## Creates and returns an upgraded copy of the item, raising its level and
## base stats. If the item has already reached its max level, returns self.
func upgrade() -> EquipmentData:
	if current_level >= max_level:
		return self

	var new_stats: Dictionary[String, int] = base_stats.duplicate()
	for stat: String in new_stats.keys():
		new_stats[stat] += 1

	return EquipmentData.new(id, name, description_key, slot, rarity, current_level + 1, max_level, new_stats, special_abilities.duplicate())

## Returns the item's total stats, accounting for its base values and
## current upgrade level.
func get_total_stats() -> Dictionary[String, int]:
	var total_stats: Dictionary[String, int] = base_stats.duplicate()
	for stat: String in total_stats.keys():
		total_stats[stat] += current_level - 1
	return total_stats

## Returns a string representation of the item, including its name, rarity
## and current level relative to the max.
func _to_string() -> String:
	return "%s (%s) - Level %d/%d" % [name, GameEnums.EquipmentRarity.keys()[rarity], current_level, max_level]

## Serializes this item into a plain Dictionary suitable for JSON storage.
func to_dict() -> Dictionary:
	return {
		"id": id,
		"name": name,
		"description_key": description_key,
		"slot": GameEnums.EquipmentSlot.keys()[slot],
		"rarity": GameEnums.EquipmentRarity.keys()[rarity],
		"current_level": current_level,
		"max_level": max_level,
		"base_stats": base_stats,
		"special_abilities": special_abilities,
	}

## Rebuilds an item from a Dictionary previously produced by to_dict().
static func from_dict(data: Dictionary) -> EquipmentData:
	var base_stats: Dictionary[String, int] = {}
	for key: String in (data.get("base_stats", {}) as Dictionary).keys():
		base_stats[key] = int(data["base_stats"][key])

	var special_abilities: Dictionary[String, String] = {}
	for key: String in (data.get("special_abilities", {}) as Dictionary).keys():
		special_abilities[key] = String(data["special_abilities"][key])

	return EquipmentData.new(
		data.get("id", ""),
		data.get("name", ""),
		data.get("description_key", ""),
		GameEnums.EquipmentSlot[data.get("slot", "WEAPON")],
		GameEnums.EquipmentRarity[data.get("rarity", "COMMON")],
		int(data.get("current_level", 1)),
		int(data.get("max_level", 1)),
		base_stats,
		special_abilities,
	)
