class_name PotionDatabase
extends RefCounted
## Static catalog of Herbalist-brewable battle potions (see design
## document, section 9). Recipes unlock progressively with Herbalist level.
## Costs and effect values are placeholder balance, not final.

## Heals the party member who drinks it. Always available (Herbalist
## starts at level 1, same as every other building).
static var single_heal: PotionData = PotionData.new(
	"single_heal", "POTION_SINGLE_HEAL", GameEnums.PotionType.SINGLE_HEAL, 40,
	{GameEnums.StrategicResource.WOOD: 5, GameEnums.StrategicResource.SILVER: 2} as Dictionary[GameEnums.StrategicResource, int], 1
)

## Heals every living party member for less each than the single-target potion.
static var party_heal: PotionData = PotionData.new(
	"party_heal", "POTION_PARTY_HEAL", GameEnums.PotionType.PARTY_HEAL, 25,
	{GameEnums.StrategicResource.WOOD: 10, GameEnums.StrategicResource.SILVER: 5} as Dictionary[GameEnums.StrategicResource, int], 2
)

## Fills half of the party's shared ultimate charge instantly.
static var ultimate_fill: PotionData = PotionData.new(
	"ultimate_fill", "POTION_ULTIMATE_FILL", GameEnums.PotionType.ULTIMATE_FILL, 50,
	{GameEnums.StrategicResource.SILVER: 8, GameEnums.StrategicResource.GOLD: 4} as Dictionary[GameEnums.StrategicResource, int], 3
)

## Every potion recipe, keyed by id.
static var potions: Dictionary[String, PotionData] = {
	"single_heal": single_heal,
	"party_heal": party_heal,
	"ultimate_fill": ultimate_fill,
}

## Returns the potion definition for the given id, or null if unknown.
static func get_potion(id: String) -> PotionData:
	return potions.get(id)

## Returns every potion recipe, in declaration order.
static func get_all_potions() -> Array[PotionData]:
	var result: Array[PotionData] = []
	result.assign(potions.values())
	return result
