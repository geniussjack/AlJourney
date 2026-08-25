class_name RarityCatalystDatabase
extends RefCounted
## Static catalog of rarity-upgrade catalysts, their resource costs, and
## which campaign levels grant them (see design document, section 10).
## Sourced from the campaign's three miniboss branch encounters plus the
## final boss, matching the natural difficulty curve: catalyst rarity
## rises with how far into the campaign its source is, not with which
## specific miniboss species it is (General of Draugr appears twice, at
## two different points). Costs and item flavor are a first pass, to be
## revisited once new enemies/balance are introduced.

static var draugr_rune_shard: RarityCatalystData = RarityCatalystData.new(
	"draugr_rune_shard", "CATALYST_DRAUGR_RUNE_SHARD", GameEnums.EquipmentRarity.UNCOMMON, GameEnums.CharacterClass.MAGE
)
static var draugr_chain_link: RarityCatalystData = RarityCatalystData.new(
	"draugr_chain_link", "CATALYST_DRAUGR_CHAIN_LINK", GameEnums.EquipmentRarity.UNCOMMON, GameEnums.CharacterClass.WARRIOR
)
static var archskeleton_skull_fragment: RarityCatalystData = RarityCatalystData.new(
	"archskeleton_skull_fragment", "CATALYST_ARCHSKELETON_SKULL_FRAGMENT", GameEnums.EquipmentRarity.RARE, GameEnums.CharacterClass.MAGE
)
static var archskeleton_rib_bone: RarityCatalystData = RarityCatalystData.new(
	"archskeleton_rib_bone", "CATALYST_ARCHSKELETON_RIB_BONE", GameEnums.EquipmentRarity.RARE, GameEnums.CharacterClass.WARRIOR
)
static var frozen_draugr_heart: RarityCatalystData = RarityCatalystData.new(
	"frozen_draugr_heart", "CATALYST_FROZEN_DRAUGR_HEART", GameEnums.EquipmentRarity.EPIC, GameEnums.CharacterClass.MAGE
)
static var frozen_draugr_gauntlet: RarityCatalystData = RarityCatalystData.new(
	"frozen_draugr_gauntlet", "CATALYST_FROZEN_DRAUGR_GAUNTLET", GameEnums.EquipmentRarity.EPIC, GameEnums.CharacterClass.WARRIOR
)
static var necromancer_soul_shard: RarityCatalystData = RarityCatalystData.new(
	"necromancer_soul_shard", "CATALYST_NECROMANCER_SOUL_SHARD", GameEnums.EquipmentRarity.LEGENDARY, GameEnums.CharacterClass.MAGE
)
static var necromancer_bone_plate: RarityCatalystData = RarityCatalystData.new(
	"necromancer_bone_plate", "CATALYST_NECROMANCER_BONE_PLATE", GameEnums.EquipmentRarity.LEGENDARY, GameEnums.CharacterClass.WARRIOR
)

## Every catalyst, keyed by id.
static var catalysts: Dictionary[String, RarityCatalystData] = {
	"draugr_rune_shard": draugr_rune_shard,
	"draugr_chain_link": draugr_chain_link,
	"archskeleton_skull_fragment": archskeleton_skull_fragment,
	"archskeleton_rib_bone": archskeleton_rib_bone,
	"frozen_draugr_heart": frozen_draugr_heart,
	"frozen_draugr_gauntlet": frozen_draugr_gauntlet,
	"necromancer_soul_shard": necromancer_soul_shard,
	"necromancer_bone_plate": necromancer_bone_plate,
}

## Strategic resource cost to upgrade equipment to the given rarity tier
## (on top of spending the matching catalyst). No entry for COMMON — it's
## the starting tier, nothing upgrades into it.
static var upgrade_resource_cost: Dictionary[GameEnums.EquipmentRarity, Dictionary] = {
	GameEnums.EquipmentRarity.UNCOMMON: {GameEnums.StrategicResource.WOOD: 10, GameEnums.StrategicResource.STONE: 10},
	GameEnums.EquipmentRarity.RARE: {GameEnums.StrategicResource.WOOD: 20, GameEnums.StrategicResource.STONE: 20, GameEnums.StrategicResource.IRON: 10},
	GameEnums.EquipmentRarity.EPIC: {GameEnums.StrategicResource.IRON: 30, GameEnums.StrategicResource.SILVER: 15},
	GameEnums.EquipmentRarity.LEGENDARY: {GameEnums.StrategicResource.SILVER: 40, GameEnums.StrategicResource.GOLD: 20},
}

## Which campaign level ids grant a catalyst on defeating their
## miniboss/boss, and which rarity tier that catalyst is for. Both
## archetypes' catalysts of that rarity are granted together, rather than
## rolling only one at random — these are rare, one-per-encounter
## materials, not regular loot.
static var level_catalyst_rarity: Dictionary[String, GameEnums.EquipmentRarity] = {
	"dark_forest_branch_1": GameEnums.EquipmentRarity.UNCOMMON,
	"buried_catacombs_branch_1": GameEnums.EquipmentRarity.RARE,
	"frozen_wastes_branch_1": GameEnums.EquipmentRarity.EPIC,
	"necromancer_lair_3": GameEnums.EquipmentRarity.LEGENDARY,
}

## Returns the catalyst for the given archetype/rarity pairing, or null if
## none exists (e.g. rarity == COMMON).
static func get_catalyst(archetype: GameEnums.CharacterClass, rarity: GameEnums.EquipmentRarity) -> RarityCatalystData:
	for catalyst: RarityCatalystData in catalysts.values():
		if catalyst.archetype == archetype and catalyst.rarity == rarity:
			return catalyst
	return null

## Returns every catalyst in the catalog.
static func get_all_catalysts() -> Array[RarityCatalystData]:
	var result: Array[RarityCatalystData] = []
	result.assign(catalysts.values())
	return result

## Returns the strategic resource cost to upgrade equipment to the given
## rarity, or an empty dictionary if that rarity has no cost defined (e.g. COMMON).
static func get_resource_cost(target_rarity: GameEnums.EquipmentRarity) -> Dictionary[GameEnums.StrategicResource, int]:
	var result: Dictionary[GameEnums.StrategicResource, int] = {}
	if upgrade_resource_cost.has(target_rarity):
		for resource: GameEnums.StrategicResource in (upgrade_resource_cost[target_rarity] as Dictionary).keys():
			result[resource] = upgrade_resource_cost[target_rarity][resource]
	return result
