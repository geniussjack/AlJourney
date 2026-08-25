class_name BuildingDatabase
extends RefCounted
## Static catalog of the settlement's buildings (see design document,
## section 9). Level caps and upgrade costs are placeholder balance, not
## final — the building set and each building's gameplay purpose are the
## part that's actually settled.

## Crafts and upgrades equipment; will also host rarity upgrades (design
## document, section 10) once that system exists.
static var forge: BuildingData = BuildingData.new(
	GameEnums.BuildingType.FORGE, "BUILDING_FORGE", 5,
	{GameEnums.StrategicResource.IRON: 20, GameEnums.StrategicResource.GOLD: 10} as Dictionary[GameEnums.StrategicResource, int]
)

## Brews battle potions (single-target heal, party heal, ultimate charge
## fill) and speeds up mercenary recovery time.
static var herbalist: BuildingData = BuildingData.new(
	GameEnums.BuildingType.HERBALIST, "BUILDING_HERBALIST", 5,
	{GameEnums.StrategicResource.WOOD: 20, GameEnums.StrategicResource.SILVER: 10} as Dictionary[GameEnums.StrategicResource, int]
)

## Recruits and trains mercenaries for the party's third slot.
static var barracks: BuildingData = BuildingData.new(
	GameEnums.BuildingType.BARRACKS, "BUILDING_BARRACKS", 5,
	{GameEnums.StrategicResource.IRON: 25, GameEnums.StrategicResource.GOLD: 15} as Dictionary[GameEnums.StrategicResource, int]
)

## Raises the storage cap on strategic resources.
static var warehouse: BuildingData = BuildingData.new(
	GameEnums.BuildingType.WAREHOUSE, "BUILDING_WAREHOUSE", 5,
	{GameEnums.StrategicResource.WOOD: 15, GameEnums.StrategicResource.STONE: 15} as Dictionary[GameEnums.StrategicResource, int]
)

## Defends the settlement against undead raids.
static var wall: BuildingData = BuildingData.new(
	GameEnums.BuildingType.WALL, "BUILDING_WALL", 5,
	{GameEnums.StrategicResource.STONE: 20, GameEnums.StrategicResource.IRON: 10} as Dictionary[GameEnums.StrategicResource, int]
)

## Raises the cap on villagers that can be assigned to gather resources or
## defend the settlement.
static var houses: BuildingData = BuildingData.new(
	GameEnums.BuildingType.HOUSES, "BUILDING_HOUSES", 5,
	{GameEnums.StrategicResource.WOOD: 15, GameEnums.StrategicResource.STONE: 10} as Dictionary[GameEnums.StrategicResource, int]
)

## Every building, keyed by type.
static var buildings: Dictionary[GameEnums.BuildingType, BuildingData] = {
	GameEnums.BuildingType.FORGE: forge,
	GameEnums.BuildingType.HERBALIST: herbalist,
	GameEnums.BuildingType.BARRACKS: barracks,
	GameEnums.BuildingType.WAREHOUSE: warehouse,
	GameEnums.BuildingType.WALL: wall,
	GameEnums.BuildingType.HOUSES: houses,
}

## Returns the building definition for the given type.
static func get_building(type: GameEnums.BuildingType) -> BuildingData:
	return buildings.get(type)
