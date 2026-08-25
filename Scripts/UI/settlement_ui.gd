extends Control
## UI for the settlement screen: shows every building's current level and
## lets the player spend strategic resources to upgrade them (see design
## document, section 9). Built entirely in code, similar to
## CampaignMapScene/ShopUI — the scene file only carries the root Control.

var _resources_label: Label
var _building_rows: Dictionary[GameEnums.BuildingType, Dictionary] = {}

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

	var scroll := ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(scroll)

	var buildings_container := VBoxContainer.new()
	buildings_container.add_theme_constant_override("separation", 8)
	scroll.add_child(buildings_container)

	for building: GameEnums.BuildingType in GameEnums.BuildingType.values():
		buildings_container.add_child(_build_building_row(building))

	GameStateManager.strategic_resource_changed.connect(_on_strategic_resource_changed)
	GameStateManager.building_upgraded.connect(_on_building_upgraded)

	_refresh_resources_label()
	for building: GameEnums.BuildingType in _building_rows.keys():
		_refresh_building_row(building)

	print("[SettlementUI] Initialized")

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
	var parts: Array[String] = []
	for resource: GameEnums.StrategicResource in GameEnums.StrategicResource.values():
		parts.append("%s: %d" % [tr(_resource_name_key(resource)), GameStateManager.get_strategic_resource(resource)])
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
