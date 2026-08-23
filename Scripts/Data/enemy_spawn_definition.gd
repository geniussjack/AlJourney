class_name EnemySpawnDefinition
extends RefCounted
## Describes a single spawn within a wave: the enemy type and stack size
## (see Enemy's spawner factory).

## The enemy type.
var type: GameEnums.EnemyType
## The number of creatures in the stack.
var count: int

## Builds a spawn definition from its enemy type and stack size.
func _init(type: GameEnums.EnemyType, count: int = 1) -> void:
	self.type = type
	self.count = count
