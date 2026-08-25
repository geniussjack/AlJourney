extends Control
## UI for the settlement screen: shows every building's current level and
## lets the player spend strategic resources to upgrade them (see design
## document, section 9). Built entirely in code, similar to
## CampaignMapScene/ShopUI — the scene file only carries the root Control.

var _resources_label: Label
var _building_rows: Dictionary[GameEnums.BuildingType, Dictionary] = {}
var _workers_summary_label: Label
var _worker_rows: Dictionary[GameEnums.StrategicResource, Dictionary] = {}
var _mercenary_rows: Dictionary[String, Dictionary] = {}
var _potion_rows: Dictionary[String, Dictionary] = {}
var _defense_count_label: Label
var _defense_minus_button: Button
var _defense_plus_button: Button
var _last_raid_label: Label
var _forge_rows: Dictionary[String, Dictionary] = {}
var _catalysts_label: Label

## Builds the whole settlement layout and subscribes to state changes.
func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)

	var root := VBoxContainer.new()
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	root.add_theme_constant_override("separation", 12)
	add_child(root)

	root.add_child(_build_top_bar())

	_resources_label = Label.new()
	root.add_child(_resources_label)

	root.add_child(_build_workers_section())

	var scroll := ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(scroll)

	var buildings_container := VBoxContainer.new()
	buildings_container.add_theme_constant_override("separation", 8)
	scroll.add_child(buildings_container)

	for building: GameEnums.BuildingType in GameEnums.BuildingType.values():
		buildings_container.add_child(_build_building_row(building))

	root.add_child(_build_forge_section())
	root.add_child(_build_mercenaries_section())
	root.add_child(_build_potions_section())

	GameStateManager.strategic_resource_changed.connect(_on_strategic_resource_changed)
	GameStateManager.building_upgraded.connect(_on_building_upgraded)
	GameStateManager.worker_assignment_changed.connect(_on_worker_assignment_changed)
	GameStateManager.active_mercenary_changed.connect(_on_active_mercenary_changed)
	GameStateManager.mercenary_recovery_changed.connect(_on_mercenary_recovery_changed)
	GameStateManager.potion_count_changed.connect(_on_potion_count_changed)
	GameStateManager.defense_workers_changed.connect(_on_defense_workers_changed)
	GameStateManager.raid_resolved.connect(_on_raid_resolved)
	GameStateManager.catalyst_count_changed.connect(_on_catalyst_count_changed)
	GameStateManager.coins_changed.connect(_on_coins_changed)

	_refresh_resources_label()
	for building: GameEnums.BuildingType in _building_rows.keys():
		_refresh_building_row(building)
	_refresh_workers_section()
	for key: String in _mercenary_rows.keys():
		_refresh_mercenary_row(key)
	for id: String in _potion_rows.keys():
		_refresh_potion_row(id)
	_refresh_all_forge_rows()
	_refresh_catalysts_label()

	print("[SettlementUI] Initialized")

## Builds the Forge panel: one row per currently-equipped item (across both
## heroes), each with an "upgrade level" action (existing coin-based
## upgrade, previously not exposed in any UI) and an "upgrade rarity"
## action (new — see design document, section 10), plus a summary line of
## owned catalysts.
func _build_forge_section() -> VBoxContainer:
	var section := VBoxContainer.new()
	section.add_theme_constant_override("separation", 4)

	var title := Label.new()
	title.text = tr("UI_SETTLEMENT_FORGE_TITLE")
	section.add_child(title)

	_catalysts_label = Label.new()
	section.add_child(_catalysts_label)

	for archetype: GameEnums.CharacterClass in GameEnums.CharacterClass.values():
		for slot: GameEnums.EquipmentSlot in InventoryManager.get_hero_equipment(archetype).keys():
			section.add_child(_build_forge_item_row(archetype, slot))

	return section

## Builds one equipped item's Forge row. Rows are keyed by (archetype,
## slot) rather than the EquipmentData instance itself, since both upgrade
## actions replace the item with a new instance (EquipmentData.upgrade()/
## upgrade_rarity()) — looking the current item up fresh by slot on every
## refresh/click avoids holding a stale reference.
func _build_forge_item_row(archetype: GameEnums.CharacterClass, slot: GameEnums.EquipmentSlot) -> HBoxContainer:
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 12)

	var name_label := Label.new()
	name_label.custom_minimum_size = Vector2(200, 0)
	row.add_child(name_label)

	var level_status_label := Label.new()
	row.add_child(level_status_label)

	var level_button := Button.new()
	level_button.text = tr("UI_SETTLEMENT_UPGRADE_LEVEL")
	level_button.pressed.connect(func() -> void:
		var current_item: EquipmentData = InventoryManager.get_equipped_item(archetype, slot)
		if current_item != null:
			InventoryManager.upgrade_equipment(current_item)
		_refresh_forge_item_row(archetype, slot)
	)
	row.add_child(level_button)

	var rarity_status_label := Label.new()
	rarity_status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(rarity_status_label)

	var rarity_button := Button.new()
	rarity_button.text = tr("UI_SETTLEMENT_UPGRADE_RARITY")
	rarity_button.pressed.connect(func() -> void:
		var current_item: EquipmentData = InventoryManager.get_equipped_item(archetype, slot)
		if current_item != null:
			InventoryManager.upgrade_rarity(current_item, archetype)
		_refresh_forge_item_row(archetype, slot)
		_refresh_catalysts_label()
	)
	row.add_child(rarity_button)

	_forge_rows[_forge_row_key(archetype, slot)] = {
		"archetype": archetype,
		"slot": slot,
		"name_label": name_label,
		"level_status_label": level_status_label,
		"level_button": level_button,
		"rarity_status_label": rarity_status_label,
		"rarity_button": rarity_button,
	}

	return row

static func _forge_row_key(archetype: GameEnums.CharacterClass, slot: GameEnums.EquipmentSlot) -> String:
	return "%s_%s" % [GameEnums.CharacterClass.keys()[archetype], GameEnums.EquipmentSlot.keys()[slot]]

func _refresh_all_forge_rows() -> void:
	for row: Dictionary in _forge_rows.values():
		_refresh_forge_item_row(row["archetype"], row["slot"])

## Refreshes one Forge row's name/level/rarity status text and both
## buttons' enabled state, reading the current item fresh from
## InventoryManager (see _build_forge_item_row).
func _refresh_forge_item_row(archetype: GameEnums.CharacterClass, slot: GameEnums.EquipmentSlot) -> void:
	var key: String = _forge_row_key(archetype, slot)
	if not _forge_rows.has(key):
		return

	var row: Dictionary = _forge_rows[key]
	var name_label: Label = row["name_label"]
	var level_status_label: Label = row["level_status_label"]
	var level_button: Button = row["level_button"]
	var rarity_status_label: Label = row["rarity_status_label"]
	var rarity_button: Button = row["rarity_button"]

	var item: EquipmentData = InventoryManager.get_equipped_item(archetype, slot)
	if item == null:
		name_label.text = ""
		level_status_label.text = ""
		level_button.disabled = true
		rarity_status_label.text = ""
		rarity_button.disabled = true
		return

	name_label.text = "%s (%s)" % [item.name, tr(item.get_rarity_name_key())]
	name_label.modulate = item.get_rarity_color()

	var level_cost: int = item.get_upgrade_cost(GameStateManager.current_wave)
	if level_cost <= 0:
		level_status_label.text = tr("UI_SETTLEMENT_MAX_LEVEL")
		level_button.disabled = true
	else:
		level_status_label.text = "%s %d/%d (%d)" % [tr("UI_SETTLEMENT_LEVEL"), item.current_level, item.max_level, level_cost]
		level_button.disabled = GameStateManager.coins < level_cost

	if item.rarity >= GameEnums.EquipmentRarity.LEGENDARY:
		rarity_status_label.text = tr("UI_SETTLEMENT_MAX_RARITY")
		rarity_button.disabled = true
		return

	var target_rarity: GameEnums.EquipmentRarity = (item.rarity + 1) as GameEnums.EquipmentRarity
	var catalyst: RarityCatalystData = RarityCatalystDatabase.get_catalyst(archetype, target_rarity)
	var cost: Dictionary[GameEnums.StrategicResource, int] = RarityCatalystDatabase.get_resource_cost(target_rarity)

	var cost_parts: Array[String] = []
	var can_afford: bool = true
	for resource: GameEnums.StrategicResource in cost.keys():
		cost_parts.append("%s: %d" % [tr(_resource_name_key(resource)), cost[resource]])
		if GameStateManager.get_strategic_resource(resource) < cost[resource]:
			can_afford = false

	var has_catalyst: bool = catalyst != null and GameStateManager.get_catalyst_count(catalyst.id) > 0
	var missing_note: String = "" if has_catalyst else " (%s)" % tr("UI_SETTLEMENT_MISSING_CATALYST")
	rarity_status_label.text = "%s: %s%s" % [tr(catalyst.name_key) if catalyst != null else "?", ", ".join(cost_parts), missing_note]
	rarity_button.disabled = not can_afford or not has_catalyst

## Refreshes the summary line of every rarity-upgrade catalyst currently owned.
func _refresh_catalysts_label() -> void:
	var parts: Array[String] = []
	for catalyst: RarityCatalystData in RarityCatalystDatabase.get_all_catalysts():
		var count: int = GameStateManager.get_catalyst_count(catalyst.id)
		if count > 0:
			parts.append("%s: %d" % [tr(catalyst.name_key), count])

	_catalysts_label.text = "%s: %s" % [tr("UI_SETTLEMENT_CATALYSTS_TITLE"), ", ".join(parts) if not parts.is_empty() else "-"]

func _on_catalyst_count_changed(_id: String, _new_count: int) -> void:
	_refresh_catalysts_label()
	_refresh_all_forge_rows()

func _on_coins_changed(_new_amount: int) -> void:
	_refresh_all_forge_rows()

## Builds the Herbalist brewing panel: one row per potion recipe, showing
## owned count, brew cost, and a brew button (locked ones show their
## required Herbalist level instead).
func _build_potions_section() -> VBoxContainer:
	var section := VBoxContainer.new()
	section.add_theme_constant_override("separation", 4)

	var title := Label.new()
	title.text = tr("UI_SETTLEMENT_POTIONS_TITLE")
	section.add_child(title)

	for potion: PotionData in PotionDatabase.get_all_potions():
		section.add_child(_build_potion_row(potion))

	return section

## Builds a single potion's row: name, owned count/cost, and a brew button.
func _build_potion_row(potion: PotionData) -> HBoxContainer:
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 12)

	var name_label := Label.new()
	name_label.text = tr(potion.name_key)
	name_label.custom_minimum_size = Vector2(180, 0)
	row.add_child(name_label)

	var status_label := Label.new()
	status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(status_label)

	var brew_button := Button.new()
	brew_button.text = tr("UI_SETTLEMENT_BREW")
	brew_button.pressed.connect(func() -> void: GameStateManager.brew_potion(potion))
	row.add_child(brew_button)

	_potion_rows[potion.id] = {
		"status_label": status_label,
		"brew_button": brew_button,
	}

	return row

## Refreshes one potion row's status text and the brew button's enabled state.
func _refresh_potion_row(id: String) -> void:
	var potion: PotionData = PotionDatabase.get_potion(id)
	var row: Dictionary = _potion_rows[id]
	var status_label: Label = row["status_label"]
	var brew_button: Button = row["brew_button"]

	if not GameStateManager.is_potion_unlocked(potion):
		status_label.text = "%s: %d" % [tr("UI_SETTLEMENT_REQUIRES_HERBALIST_LEVEL"), potion.required_herbalist_level]
		brew_button.disabled = true
		return

	var owned: int = GameStateManager.get_potion_count(id)
	var cost_parts: Array[String] = []
	var can_afford: bool = true
	for resource: GameEnums.StrategicResource in potion.brew_cost.keys():
		cost_parts.append("%s: %d" % [tr(_resource_name_key(resource)), potion.brew_cost[resource]])
		if GameStateManager.get_strategic_resource(resource) < potion.brew_cost[resource]:
			can_afford = false

	status_label.text = "%s: %d | %s" % [tr("UI_SETTLEMENT_OWNED"), owned, ", ".join(cost_parts)]
	brew_button.disabled = not can_afford

func _on_potion_count_changed(id: String, _new_count: int) -> void:
	if _potion_rows.has(id):
		_refresh_potion_row(id)

## Builds the Barracks recruitment panel: one row per mercenary subclass in
## the whole roster (locked ones are shown greyed out with their unlock
## condition, rather than hidden, so the player knows what's coming).
func _build_mercenaries_section() -> VBoxContainer:
	var section := VBoxContainer.new()
	section.add_theme_constant_override("separation", 4)

	var title := Label.new()
	title.text = tr("UI_SETTLEMENT_MERCENARIES_TITLE")
	section.add_child(title)

	var scroll := ScrollContainer.new()
	scroll.custom_minimum_size = Vector2(0, 160)
	section.add_child(scroll)

	var rows := VBoxContainer.new()
	rows.add_theme_constant_override("separation", 4)
	scroll.add_child(rows)

	for subclass: MercenarySubclassData in MercenaryDatabase.get_all_subclasses():
		rows.add_child(_build_mercenary_row(subclass))

	return section

## Builds a single mercenary's row: name/role, status, and a select/clear button.
func _build_mercenary_row(subclass: MercenarySubclassData) -> HBoxContainer:
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 12)

	var name_label := Label.new()
	name_label.text = "%s — %s" % [tr(subclass.character_name_key), tr(subclass.name_key)]
	name_label.custom_minimum_size = Vector2(220, 0)
	row.add_child(name_label)

	var status_label := Label.new()
	status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(status_label)

	var action_button := Button.new()
	action_button.pressed.connect(func() -> void: _on_mercenary_action_pressed(subclass))
	row.add_child(action_button)

	_mercenary_rows[subclass.get_key()] = {
		"status_label": status_label,
		"action_button": action_button,
	}

	return row

## Refreshes one mercenary row's status text and action button, based on
## whether it's locked, recovering, active, or available to select.
func _refresh_mercenary_row(key: String) -> void:
	var subclass: MercenarySubclassData = MercenaryDatabase.get_by_key(key)
	var row: Dictionary = _mercenary_rows[key]
	var status_label: Label = row["status_label"]
	var action_button: Button = row["action_button"]

	if not GameStateManager.is_mercenary_unlocked(subclass):
		status_label.text = _lock_condition_text(subclass)
		action_button.text = tr("UI_MERCENARY_SELECT")
		action_button.disabled = true
		return

	if GameStateManager.get_active_mercenary_key() == key:
		status_label.text = tr("UI_MERCENARY_ACTIVE")
		action_button.text = tr("UI_MERCENARY_CLEAR")
		action_button.disabled = false
		return

	var battles_left: int = GameStateManager.get_battles_until_available(key)
	if battles_left > 0:
		status_label.text = "%s: %d" % [tr("UI_MERCENARY_RECOVERING"), battles_left]
		action_button.text = tr("UI_MERCENARY_SELECT")
		action_button.disabled = true
		return

	status_label.text = ""
	action_button.text = tr("UI_MERCENARY_SELECT")
	action_button.disabled = false

## Returns the display text explaining why a locked mercenary isn't
## available yet.
func _lock_condition_text(subclass: MercenarySubclassData) -> String:
	if subclass.required_barracks_level > 0:
		return "%s: %d" % [tr("UI_MERCENARY_LOCKED_BARRACKS"), subclass.required_barracks_level]
	return tr("UI_MERCENARY_LOCKED_STORY")

func _on_mercenary_action_pressed(subclass: MercenarySubclassData) -> void:
	if GameStateManager.get_active_mercenary_key() == subclass.get_key():
		GameStateManager.clear_active_mercenary()
	else:
		GameStateManager.set_active_mercenary(subclass)

func _on_active_mercenary_changed(_key: String) -> void:
	for row_key: String in _mercenary_rows.keys():
		_refresh_mercenary_row(row_key)

func _on_mercenary_recovery_changed(key: String, _battles_remaining: int) -> void:
	if _mercenary_rows.has(key):
		_refresh_mercenary_row(key)

## Builds the worker-assignment panel: capacity summary plus one
## assign/unassign row per strategic resource.
func _build_workers_section() -> VBoxContainer:
	var section := VBoxContainer.new()
	section.add_theme_constant_override("separation", 4)

	_workers_summary_label = Label.new()
	section.add_child(_workers_summary_label)

	var rows := HBoxContainer.new()
	rows.add_theme_constant_override("separation", 16)
	section.add_child(rows)

	for resource: GameEnums.StrategicResource in GameEnums.StrategicResource.values():
		rows.add_child(_build_worker_row(resource))

	rows.add_child(_build_defense_row())

	_last_raid_label = Label.new()
	section.add_child(_last_raid_label)

	return section

## Builds the settlement-defense worker assignment row — same shape as a
## resource row, but draws from the same shared capacity pool without
## producing a resource (see design document, section 9).
func _build_defense_row() -> HBoxContainer:
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 4)

	var name_label := Label.new()
	name_label.text = tr("UI_SETTLEMENT_DEFENSE")
	row.add_child(name_label)

	_defense_minus_button = Button.new()
	_defense_minus_button.text = "-"
	_defense_minus_button.pressed.connect(func() -> void: GameStateManager.unassign_defense_worker())
	row.add_child(_defense_minus_button)

	_defense_count_label = Label.new()
	row.add_child(_defense_count_label)

	_defense_plus_button = Button.new()
	_defense_plus_button.text = "+"
	_defense_plus_button.pressed.connect(func() -> void: GameStateManager.assign_defense_worker())
	row.add_child(_defense_plus_button)

	return row

## Builds one resource's worker assignment row: name, count, +/- buttons.
func _build_worker_row(resource: GameEnums.StrategicResource) -> HBoxContainer:
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 4)

	var name_label := Label.new()
	name_label.text = tr(_resource_name_key(resource))
	row.add_child(name_label)

	var minus_button := Button.new()
	minus_button.text = "-"
	minus_button.pressed.connect(func() -> void: GameStateManager.unassign_worker(resource))
	row.add_child(minus_button)

	var count_label := Label.new()
	row.add_child(count_label)

	var plus_button := Button.new()
	plus_button.text = "+"
	plus_button.pressed.connect(func() -> void: GameStateManager.assign_worker(resource))
	row.add_child(plus_button)

	_worker_rows[resource] = {
		"count_label": count_label,
		"minus_button": minus_button,
		"plus_button": plus_button,
	}

	return row

## Refreshes the capacity summary and every resource's worker count/button state.
func _refresh_workers_section() -> void:
	var assigned: int = GameStateManager.get_total_assigned_workers()
	var capacity: int = GameStateManager.get_worker_capacity()
	_workers_summary_label.text = "%s: %d/%d" % [tr("BUILDING_HOUSES"), assigned, capacity]

	for resource: GameEnums.StrategicResource in _worker_rows.keys():
		var row: Dictionary = _worker_rows[resource]
		var count: int = GameStateManager.get_assigned_workers(resource)
		(row["count_label"] as Label).text = "%d" % count
		(row["minus_button"] as Button).disabled = count <= 0
		(row["plus_button"] as Button).disabled = assigned >= capacity

	var defense_count: int = GameStateManager.get_defense_workers()
	_defense_count_label.text = "%d" % defense_count
	_defense_minus_button.disabled = defense_count <= 0
	_defense_plus_button.disabled = assigned >= capacity

	_refresh_last_raid_label()

## Refreshes the "last raid" status line: outcome and time since it happened.
func _refresh_last_raid_label() -> void:
	var save: SaveData = GameStateManager.current_save
	if save == null or save.last_raid_unix_time <= 0:
		_last_raid_label.text = tr("UI_SETTLEMENT_NO_RAIDS_YET")
		return

	var outcome_key: String = "UI_SETTLEMENT_RAID_REPELLED" if save.last_raid_succeeded else "UI_SETTLEMENT_RAID_FAILED"
	var seconds_ago: int = maxi(0, Time.get_unix_time_from_system() - save.last_raid_unix_time)
	_last_raid_label.text = "%s (%d %s)" % [tr(outcome_key), seconds_ago, tr("UI_SETTLEMENT_SECONDS_AGO")]

## Builds the title/back top bar.
func _build_top_bar() -> HBoxContainer:
	var top_bar := HBoxContainer.new()
	top_bar.add_theme_constant_override("separation", 12)

	var title := Label.new()
	title.text = tr("UI_SETTLEMENT_TITLE")
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	top_bar.add_child(title)

	var back_button := Button.new()
	back_button.text = tr("UI_SETTLEMENT_BACK")
	back_button.pressed.connect(_on_back_pressed)
	top_bar.add_child(back_button)

	return top_bar

## Builds a single building's row: name, level, upgrade cost and button.
## Caches the row's controls in _building_rows for later refreshes.
func _build_building_row(building: GameEnums.BuildingType) -> HBoxContainer:
	var data: BuildingData = BuildingDatabase.get_building(building)

	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 12)

	var name_label := Label.new()
	name_label.text = tr(data.name_key)
	name_label.custom_minimum_size = Vector2(200, 0)
	row.add_child(name_label)

	var level_label := Label.new()
	row.add_child(level_label)

	var cost_label := Label.new()
	cost_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(cost_label)

	var upgrade_button := Button.new()
	upgrade_button.text = tr("UI_SETTLEMENT_UPGRADE")
	upgrade_button.pressed.connect(func() -> void: _on_upgrade_pressed(building))
	row.add_child(upgrade_button)

	_building_rows[building] = {
		"level_label": level_label,
		"cost_label": cost_label,
		"upgrade_button": upgrade_button,
	}

	return row

## Refreshes the resource totals label.
func _refresh_resources_label() -> void:
	var cap: int = GameStateManager.get_storage_cap()
	var parts: Array[String] = []
	for resource: GameEnums.StrategicResource in GameEnums.StrategicResource.values():
		parts.append("%s: %d/%d" % [tr(_resource_name_key(resource)), GameStateManager.get_strategic_resource(resource), cap])
	_resources_label.text = " | ".join(parts)

## Refreshes one building row's level/cost text and the upgrade button's
## enabled state.
func _refresh_building_row(building: GameEnums.BuildingType) -> void:
	var data: BuildingData = BuildingDatabase.get_building(building)
	var row: Dictionary = _building_rows[building]
	var level_label: Label = row["level_label"]
	var cost_label: Label = row["cost_label"]
	var upgrade_button: Button = row["upgrade_button"]

	var current_level: int = GameStateManager.get_building_level(building)
	level_label.text = "%s %d/%d" % [tr("UI_SETTLEMENT_LEVEL"), current_level, data.max_level]

	if current_level >= data.max_level:
		cost_label.text = tr("UI_SETTLEMENT_MAX_LEVEL")
		upgrade_button.disabled = true
		return

	var cost: Dictionary[GameEnums.StrategicResource, int] = data.get_upgrade_cost(current_level)
	var cost_parts: Array[String] = []
	var can_afford: bool = true
	for resource: GameEnums.StrategicResource in cost.keys():
		cost_parts.append("%s: %d" % [tr(_resource_name_key(resource)), cost[resource]])
		if GameStateManager.get_strategic_resource(resource) < cost[resource]:
			can_afford = false
	cost_label.text = ", ".join(cost_parts)
	upgrade_button.disabled = not can_afford

## Returns the localization key for a strategic resource's display name.
static func _resource_name_key(resource: GameEnums.StrategicResource) -> String:
	return "STRATEGIC_RESOURCE_%s" % GameEnums.StrategicResource.keys()[resource]

func _on_strategic_resource_changed(_resource: GameEnums.StrategicResource, _new_amount: int) -> void:
	_refresh_resources_label()
	for building: GameEnums.BuildingType in _building_rows.keys():
		_refresh_building_row(building)
	for id: String in _potion_rows.keys():
		_refresh_potion_row(id)
	_refresh_all_forge_rows()

func _on_building_upgraded(building: GameEnums.BuildingType, _new_level: int) -> void:
	_refresh_building_row(building)
	if building == GameEnums.BuildingType.HOUSES:
		_refresh_workers_section()
	elif building == GameEnums.BuildingType.WAREHOUSE:
		_refresh_resources_label()
	elif building == GameEnums.BuildingType.HERBALIST:
		for id: String in _potion_rows.keys():
			_refresh_potion_row(id)

func _on_worker_assignment_changed(_resource: GameEnums.StrategicResource, _worker_count: int) -> void:
	_refresh_workers_section()

func _on_defense_workers_changed(_worker_count: int) -> void:
	_refresh_workers_section()

func _on_raid_resolved(_succeeded: bool, _resources_lost: Dictionary) -> void:
	_refresh_resources_label()
	_refresh_last_raid_label()

func _on_upgrade_pressed(building: GameEnums.BuildingType) -> void:
	GameStateManager.upgrade_building(building)

func _on_back_pressed() -> void:
	queue_free()

## Unsubscribes from global state when the screen closes.
func _exit_tree() -> void:
	if GameStateManager.strategic_resource_changed.is_connected(_on_strategic_resource_changed):
		GameStateManager.strategic_resource_changed.disconnect(_on_strategic_resource_changed)
	if GameStateManager.building_upgraded.is_connected(_on_building_upgraded):
		GameStateManager.building_upgraded.disconnect(_on_building_upgraded)
	if GameStateManager.worker_assignment_changed.is_connected(_on_worker_assignment_changed):
		GameStateManager.worker_assignment_changed.disconnect(_on_worker_assignment_changed)
	if GameStateManager.active_mercenary_changed.is_connected(_on_active_mercenary_changed):
		GameStateManager.active_mercenary_changed.disconnect(_on_active_mercenary_changed)
	if GameStateManager.mercenary_recovery_changed.is_connected(_on_mercenary_recovery_changed):
		GameStateManager.mercenary_recovery_changed.disconnect(_on_mercenary_recovery_changed)
	if GameStateManager.potion_count_changed.is_connected(_on_potion_count_changed):
		GameStateManager.potion_count_changed.disconnect(_on_potion_count_changed)
	if GameStateManager.catalyst_count_changed.is_connected(_on_catalyst_count_changed):
		GameStateManager.catalyst_count_changed.disconnect(_on_catalyst_count_changed)
	if GameStateManager.coins_changed.is_connected(_on_coins_changed):
		GameStateManager.coins_changed.disconnect(_on_coins_changed)
	if GameStateManager.defense_workers_changed.is_connected(_on_defense_workers_changed):
		GameStateManager.defense_workers_changed.disconnect(_on_defense_workers_changed)
	if GameStateManager.raid_resolved.is_connected(_on_raid_resolved):
		GameStateManager.raid_resolved.disconnect(_on_raid_resolved)
