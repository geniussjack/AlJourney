class_name LevelDefinition
extends RefCounted
## Describes a single campaign map level: its location, position within it,
## unlock requirement, and the curated (predetermined) sequence of waves
## that play out back-to-back without leaving combat within a single
## attempt at the level.

## The level's unique identifier.
var id: String
## The location this level belongs to.
var location: GameEnums.LocationId
## The level's order within its location (for display on the map).
var order_in_location: int
## The level's curated wave sequence.
var waves: Array[WaveDefinition]
## The level's numeric difficulty, used in place of a wave number as the
## input for ScalingSystem (scaling of enemy stats, rewards and shop prices).
var difficulty_rating: int
## True if the level is an optional branch off the main line (a source of
## resources and, in the future, rarity catalysts) rather than part of the
## mandatory linear chain to the necromancer.
var is_branch: bool
## The id of the level that must be completed to unlock this one, or an
## empty string for the very first level of the campaign.
var required_level_id: String

## Builds a level definition from its location, order, waves and unlock rule.
func _init(
	id: String,
	location: GameEnums.LocationId,
	order_in_location: int,
	waves: Array[WaveDefinition],
	difficulty_rating: int,
	is_branch: bool = false,
	required_level_id: String = "",
) -> void:
	self.id = id
	self.location = location
	self.order_in_location = order_in_location
	self.waves = waves
	self.difficulty_rating = difficulty_rating
	self.is_branch = is_branch
	self.required_level_id = required_level_id
