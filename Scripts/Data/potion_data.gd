class_name PotionData
extends RefCounted
## Data structure representing a Herbalist-brewed battle potion (see design
## document, section 9). Consuming one takes the acting party member's turn,
## the same as any other action — a v1 simplification chosen to avoid a
## separate item-use turn economy before the game is fully playable.

## Unique id used to key owned counts (SaveData.potion_counts) and brewing.
var id: String
## Localization key for the potion's display name.
var name_key: String
## What the potion does when used — see GameEnums.PotionType.
var potion_type: GameEnums.PotionType
## Heal amount (SINGLE_HEAL/PARTY_HEAL) or ultimate charge granted
## (ULTIMATE_FILL).
var effect_value: int
## Strategic resource cost to brew one unit of this potion.
var brew_cost: Dictionary[GameEnums.StrategicResource, int]
## Herbalist level required before this potion's recipe is available.
var required_herbalist_level: int

## Builds a potion definition.
func _init(
	id: String,
	name_key: String,
	potion_type: GameEnums.PotionType,
	effect_value: int,
	brew_cost: Dictionary[GameEnums.StrategicResource, int],
	required_herbalist_level: int,
) -> void:
	self.id = id
	self.name_key = name_key
	self.potion_type = potion_type
	self.effect_value = effect_value
	self.brew_cost = brew_cost
	self.required_herbalist_level = required_herbalist_level
