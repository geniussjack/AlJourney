extends Node
## Global (autoload) loot system manager. Responsible for generating
## equipment after defeating enemies. Determines item rarity and stats
## based on the current wave.

## Rarity roll table as [rarity, cumulative-weight percent] pairs.
const RARITY_WEIGHTS: Array = [
	[GameEnums.EquipmentRarity.COMMON, 40.0],
	[GameEnums.EquipmentRarity.UNCOMMON, 30.0],
	[GameEnums.EquipmentRarity.RARE, 15.0],
	[GameEnums.EquipmentRarity.EPIC, 10.0],
	[GameEnums.EquipmentRarity.LEGENDARY, 5.0],
]

## Slot roll table as [slot, cumulative-weight percent] pairs.
const SLOT_WEIGHTS: Array = [
	[GameEnums.EquipmentSlot.WEAPON, 25.0],
	[GameEnums.EquipmentSlot.HEAD, 15.0],
	[GameEnums.EquipmentSlot.BODY, 15.0],
	[GameEnums.EquipmentSlot.LEGS, 15.0],
	[GameEnums.EquipmentSlot.NECKLACE, 15.0],
	[GameEnums.EquipmentSlot.RING, 7.0],
	[GameEnums.EquipmentSlot.EARRING, 8.0],
]

var _equipment_templates: Dictionary[String, EquipmentData] = EquipmentDatabase.templates

## Generates an expanded list of items after defeating a boss.
## wave_number: the current wave number, used to scale rarity.
func generate_boss_loot(wave_number: int) -> Array[EquipmentData]:
	var drop_count: int = randi_range(3, 11)
	var loot: Array[EquipmentData] = []

	print("[LootSystem] Generating %d items for boss at wave %d" % [drop_count, wave_number])

	for i: int in range(drop_count):
		var rarity: GameEnums.EquipmentRarity = _determine_rarity()
		var slot: GameEnums.EquipmentSlot = _determine_slot()
		var item: EquipmentData = _generate_equipment(rarity, slot)

		if item != null:
			loot.append(item)

	return loot

## Generates a single item after defeating a normal enemy. The chance of
## high rarity is artificially lowered for balance.
## Returns the generated equipment item, or null on failure.
func generate_normal_loot(wave_number: int) -> EquipmentData:
	var rarity: GameEnums.EquipmentRarity = _determine_rarity()

	# Lower the rarity for normal enemies.
	if rarity == GameEnums.EquipmentRarity.LEGENDARY:
		rarity = GameEnums.EquipmentRarity.EPIC

	if rarity == GameEnums.EquipmentRarity.EPIC and randf() > 0.1:
		rarity = GameEnums.EquipmentRarity.RARE

	var slot: GameEnums.EquipmentSlot = _determine_slot()
	var item: EquipmentData = _generate_equipment(rarity, slot)

	print("[LootSystem] Generated normal loot: %s at wave %d" % [item.name, wave_number])
	return item

## Rolls a weighted random rarity from RARITY_WEIGHTS.
func _determine_rarity() -> GameEnums.EquipmentRarity:
	var roll: float = randf() * 100.0
	var cumulative: float = 0.0

	for pair: Array in RARITY_WEIGHTS:
		cumulative += pair[1]
		if roll <= cumulative:
			return pair[0]

	return GameEnums.EquipmentRarity.COMMON

## Rolls a weighted random equipment slot from SLOT_WEIGHTS.
func _determine_slot() -> GameEnums.EquipmentSlot:
	var roll: float = randf() * 100.0
	var cumulative: float = 0.0

	for pair: Array in SLOT_WEIGHTS:
		cumulative += pair[1]
		if roll <= cumulative:
			return pair[0]

	return GameEnums.EquipmentSlot.EARRING

## Picks a random matching template for the given rarity/slot, or falls
## back to a generated basic item if none exists.
func _generate_equipment(rarity: GameEnums.EquipmentRarity, slot: GameEnums.EquipmentSlot) -> EquipmentData:
	var templates: Array[EquipmentData] = []
	for item: EquipmentData in _equipment_templates.values():
		if item.slot == slot and item.rarity == rarity:
			templates.append(item)

	if templates.size() > 0:
		return templates[randi_range(0, templates.size() - 1)]
	return _generate_basic_equipment(rarity, slot)

## Returns the base stat dictionary for a procedurally generated item in
## the given slot.
static func _get_basic_stats(slot: GameEnums.EquipmentSlot) -> Dictionary[String, int]:
	match slot:
		GameEnums.EquipmentSlot.WEAPON:
			return {"damage": 1}
		GameEnums.EquipmentSlot.HEAD:
			return {"defense": 1}
		GameEnums.EquipmentSlot.BODY:
			return {"defense": 2}
		GameEnums.EquipmentSlot.LEGS:
			return {"defense": 1}
		GameEnums.EquipmentSlot.NECKLACE:
			return {"hp_percent": 5}
		GameEnums.EquipmentSlot.RING:
			return {"damage": 2}
		GameEnums.EquipmentSlot.EARRING:
			return {"defense": 1}
		_:
			return {}

## Procedurally generates a basic item for a rarity/slot combination with
## no curated template in EquipmentDatabase.
static func _generate_basic_equipment(rarity: GameEnums.EquipmentRarity, slot: GameEnums.EquipmentSlot) -> EquipmentData:
	var rarity_name: String = GameEnums.EquipmentRarity.keys()[rarity]
	var slot_name: String = GameEnums.EquipmentSlot.keys()[slot]
	var item_name: String = "%s %s" % [rarity_name, slot_name]
	var stats: Dictionary[String, int] = _get_basic_stats(slot)

	var max_level: int
	match rarity:
		GameEnums.EquipmentRarity.COMMON:
			max_level = 5
		GameEnums.EquipmentRarity.UNCOMMON:
			max_level = 10
		GameEnums.EquipmentRarity.RARE:
			max_level = 15
		GameEnums.EquipmentRarity.EPIC:
			max_level = 20
		GameEnums.EquipmentRarity.LEGENDARY:
			max_level = 25
		_:
			max_level = 5

	return EquipmentData.new(
		"%s_%s" % [rarity_name, slot_name],
		item_name,
		"",
		slot,
		rarity,
		1,
		max_level,
		stats,
		{} as Dictionary[String, String],
	)
