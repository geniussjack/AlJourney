class_name WaveDefinition
extends RefCounted
## Describes a single wave pass within a level — a set of spawns that appear
## at the same time.

## The list of spawns for this wave.
var enemies: Array[EnemySpawnDefinition]

## Builds a wave from its list of spawns.
func _init(enemies: Array[EnemySpawnDefinition]) -> void:
	self.enemies = enemies
