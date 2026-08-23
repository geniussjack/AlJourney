class_name StatusEffectData
extends RefCounted
## Structure describing a status effect applied to a character or enemy.

## The kind of status effect.
var type: GameEnums.StatusEffect
## Remaining number of turns the effect stays active.
var duration: int
## Magnitude of the effect (damage per tick, shield amount, etc.).
var power: int
## Extra numeric parameter some effects need beyond power (e.g. a percentage).
var extra_data: float

## Builds a status effect instance from its type, duration and power.
func _init(type: GameEnums.StatusEffect, duration: int, power: int, extra_data: float = 0.0) -> void:
	self.type = type
	self.duration = duration
	self.power = power
	self.extra_data = extra_data

## Returns a copy of this status effect with its duration reduced by 1.
func tick_duration() -> StatusEffectData:
	return StatusEffectData.new(type, duration - 1, power, extra_data)

## Whether this status effect should be removed.
var should_remove: bool:
	get:
		return duration <= 0
