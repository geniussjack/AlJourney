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

	root.add_child(_build_mercenaries_section())

	GameStateManager.strategic_resource_changed.connect(_on_strategic_resource_changed)
	GameStateManager.building_upgraded.connect(_on_building_upgraded)
	GameStateManager.worker_assignment_changed.connect(_on_worker_assignment_changed)
	GameStateManager.active_mercenary_changed.connect(_on_active_mercenary_changed)
	GameStateManager.mercenary_recovery_changed.connect(_on_mercenary_recovery_changed)

	_refresh_resources_label()
	for building: GameEnums.BuildingType in _building_rows.keys():
		_refresh_building_row(building)
	_refresh_workers_section()
	for key: String in _mercenary_rows.keys():
		_refresh_mercenary_row(key)

	print("[SettlementUI] Initialized")

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

	return section

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

func _on_building_upgraded(building: GameEnums.BuildingType, _new_level: int) -> void:
	_refresh_building_row(building)
	if building == GameEnums.BuildingType.HOUSES:
		_refresh_workers_section()
	elif building == GameEnums.BuildingType.WAREHOUSE:
		_refresh_resources_label()

func _on_worker_assignment_changed(_resource: GameEnums.StrategicResource, _worker_count: int) -> void:
	_refresh_workers_section()

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
