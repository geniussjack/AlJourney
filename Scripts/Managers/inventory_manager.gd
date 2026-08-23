extends Node
## Global (autoload) inventory manager. Responsible for storing items,
## equipping heroes, and upgrading equipment.

var _inventory: Array[EquipmentData] = []
var _hero_equipment: Dictionary[GameEnums.CharacterClass, Dictionary] = {}

## Adds a list of items to the player's shared inventory.
func add_items(items: Array[EquipmentData]) -> void:
	for item: EquipmentData in items:
		_inventory.append(item)
		print("[InventoryManager] Added: %s" % item.name)

## Equips the given item to the selected hero. If the slot is already
## occupied, the old item is unequipped and returned to the inventory.
## Returns true if the item was successfully equipped.
func equip_item(hero: GameEnums.CharacterClass, item: EquipmentData) -> bool:
	if not _hero_equipment.has(hero):
		_hero_equipment[hero] = {}

	var hero_slots: Dictionary = _hero_equipment[hero]
	if hero_slots.has(item.slot):
		unequip_item(hero, item.slot)

	hero_slots[item.slot] = item
	print("[InventoryManager] Equipped %s to %s" % [item.name, GameEnums.CharacterClass.keys()[hero]])
	return true

## Unequips the item from the given slot on the selected hero and returns
## it to the shared inventory.
## Returns the unequipped item, or null if the slot was empty.
func unequip_item(hero: GameEnums.CharacterClass, slot: GameEnums.EquipmentSlot) -> EquipmentData:
	if not _hero_equipment.has(hero):
		return null

	var hero_slots: Dictionary = _hero_equipment[hero]
	if not hero_slots.has(slot):
		return null

	var item: EquipmentData = hero_slots[slot]
	hero_slots.erase(slot)
	_inventory.append(item)
	print("[InventoryManager] Unequipped %s from %s" % [item.name, GameEnums.CharacterClass.keys()[hero]])
	return item

## Upgrades an equipment item for coins, if there are enough funds.
## Returns true if the item was successfully upgraded.
func upgrade_equipment(item: EquipmentData) -> bool:
	var wave_number: int = GameStateManager.current_wave
	var cost: int = item.get_upgrade_cost(wave_number)

	if cost == 0 or GameStateManager.coins < cost:
		print("[InventoryManager] Not enough coins to upgrade %s. Need: %d, Have: %d" % [item.name, cost, GameStateManager.coins])
		return false

	GameStateManager.spend_coins(cost)
	var upgraded_item: EquipmentData = item.upgrade()

	var inventory_index: int = _inventory.find(item)
	if inventory_index >= 0:
		_inventory[inventory_index] = upgraded_item
	else:
		for hero_slots: Dictionary in _hero_equipment.values():
			var found_slot: Variant = null
			for slot: GameEnums.EquipmentSlot in hero_slots.keys():
				if hero_slots[slot] == item:
					found_slot = slot
					break
			if found_slot != null:
				hero_slots[found_slot] = upgraded_item
				break

	print("[InventoryManager] Upgraded %s to level %d" % [item.name, upgraded_item.current_level])
	return true

## Returns the list of every item currently in the player's inventory.
func get_inventory() -> Array[EquipmentData]:
	return _inventory.duplicate()

## Saves the current state of the inventory and hero equipment into a save
## data object.
func save_to_data(data: SaveData) -> void:
	data.inventory = _inventory.duplicate()
	data.hero_equipment = {}
	for hero: GameEnums.CharacterClass in _hero_equipment.keys():
		data.hero_equipment[hero] = _hero_equipment[hero].duplicate()

## Loads the state of the inventory and hero equipment from a save data object.
func load_from_data(data: SaveData) -> void:
	_inventory.clear()
	if data.inventory != null:
		_inventory.append_array(data.inventory)

	_hero_equipment.clear()
	if data.hero_equipment != null:
		for hero: GameEnums.CharacterClass in data.hero_equipment.keys():
			_hero_equipment[hero] = data.hero_equipment[hero].duplicate()

	print("[InventoryManager] Loaded %d items and equipment for %d heroes from save." % [_inventory.size(), _hero_equipment.size()])

## Gets all equipment currently equipped by the given hero, keyed by slot.
func get_hero_equipment(hero: GameEnums.CharacterClass) -> Dictionary:
	return _hero_equipment.get(hero, {})

## Gets the item equipped in the given slot for the given hero, or null if empty.
func get_equipped_item(hero: GameEnums.CharacterClass, slot: GameEnums.EquipmentSlot) -> EquipmentData:
	return get_hero_equipment(hero).get(slot, null)

## Gets the list of inventory items filtered by their rarity.
func get_equipment_by_rarity(rarity: GameEnums.EquipmentRarity) -> Array[EquipmentData]:
	var result: Array[EquipmentData] = []
	for item: EquipmentData in _inventory:
		if item.rarity == rarity:
			result.append(item)
	return result

## Gets the list of inventory items filtered by the equipment slot they are
## intended for.
func get_equipment_by_slot(slot: GameEnums.EquipmentSlot) -> Array[EquipmentData]:
	var result: Array[EquipmentData] = []
	for item: EquipmentData in _inventory:
		if item.slot == slot:
			result.append(item)
	return result
