class_name BuildingData
extends RefCounted
## Data structure representing a settlement building's static definition
## (see design document, section 9). Every building is tied to a concrete
## gameplay mechanic — there are no purely decorative buildings.

## The building's type.
var type: GameEnums.BuildingType
## Localization key for the building's display name.
var name_key: String
## The highest level this building can be upgraded to.
var max_level: int
## Strategic resource cost to upgrade from level 1 to level 2. Scales
## linearly per level beyond that — see get_upgrade_cost(). Placeholder
## balance, not final.
var base_upgrade_cost: Dictionary[GameEnums.StrategicResource, int]

## Builds a building definition from its type, name, level cap and base
## upgrade cost.
func _init(
	type: GameEnums.BuildingType,
	name_key: String,
	max_level: int,
	base_upgrade_cost: Dictionary[GameEnums.StrategicResource, int],
) -> void:
	self.type = type
	self.name_key = name_key
	self.max_level = max_level
	self.base_upgrade_cost = base_upgrade_cost

## Computes the resource cost to upgrade from the given current level to
## the next one. Scales linearly with the current level — a placeholder
## curve, not final balance. Returns an empty cost at or above max_level.
func get_upgrade_cost(current_level: int) -> Dictionary[GameEnums.StrategicResource, int]:
	if current_level >= max_level:
		return {}

	var scaled: Dictionary[GameEnums.StrategicResource, int] = {}
	for resource: GameEnums.StrategicResource in base_upgrade_cost.keys():
		scaled[resource] = base_upgrade_cost[resource] * current_level
	return scaled
