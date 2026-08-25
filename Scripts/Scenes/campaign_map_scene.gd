extends Control
## The campaign map screen — the hub between levels. Shows locations in
## playthrough order (from the village ruins to the necromancer's lair),
## the main line's sequentially unlocking levels and their branches, and
## provides access to the settlement shop. Built entirely in code, similar
## to BattleHUD/TurnActionPanel — doesn't use any editor-authored child
## nodes besides the root Control.

var _save_indicator_label: Label

## Initializes the map screen: builds the list of locations and levels
## based on the current save progress (GameStateManager.completed_level_ids),
## and autosaves.
##
## The map is the single point where progress gets persisted: every "exit"
## flow (battle won, battle lost, shop closed) funnels back here rather
## than each saving individually — see _on_save_completed() for the small
## indicator this shows the player.
func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)

	var root := VBoxContainer.new()
	root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	root.add_theme_constant_override("separation", 12)
	add_child(root)

	root.add_child(_build_top_bar())

	var scroll := ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(scroll)

	var locations_container := VBoxContainer.new()
	locations_container.add_theme_constant_override("separation", 16)
	scroll.add_child(locations_container)

	var completed_level_ids: Array[String] = GameStateManager.completed_level_ids

	for location: GameEnums.LocationId in GameEnums.LocationId.values():
		locations_container.add_child(_build_location_section(location, completed_level_ids))

	SaveSystem.save_completed.connect(_on_save_completed)
	SaveSystem.auto_save()

	print("[CampaignMapScene] Initialized")

## Unsubscribes from the save system when leaving the map, to avoid
## updating a freed label.
func _exit_tree() -> void:
	if SaveSystem.save_completed.is_connected(_on_save_completed):
		SaveSystem.save_completed.disconnect(_on_save_completed)

## Briefly shows a "Saved"/"Save failed" indicator next to the map title in
## response to the autosave triggered in _ready(), fading it out
## automatically on success.
func _on_save_completed(success: bool) -> void:
	_save_indicator_label.modulate = Color.LIGHT_GREEN if success else Color.ORANGE_RED
	_save_indicator_label.text = tr("UI_MAP_SAVED") if success else tr("UI_MAP_SAVE_FAILED")
	_save_indicator_label.visible = true

	if not success:
		return

	var tween: Tween = create_tween()
	tween.tween_interval(1.2)
	tween.tween_property(_save_indicator_label, "modulate:a", 0.0, 0.6)
	tween.tween_callback(func() -> void: _save_indicator_label.visible = false)

## Builds the title/save-indicator/shop/main-menu top bar.
func _build_top_bar() -> HBoxContainer:
	var top_bar := HBoxContainer.new()
	top_bar.add_theme_constant_override("separation", 12)

	var title := Label.new()
	title.text = tr("UI_MAP_TITLE")
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	top_bar.add_child(title)

	var party_level_label := Label.new()
	party_level_label.text = "%s %d" % [tr("UI_PARTY_LEVEL"), GameStateManager.party_level]
	top_bar.add_child(party_level_label)

	_save_indicator_label = Label.new()
	_save_indicator_label.visible = false
	top_bar.add_child(_save_indicator_label)

	var shop_button := Button.new()
	shop_button.text = tr("UI_MAP_SHOP")
	shop_button.pressed.connect(SceneManager.go_to_shop)
	top_bar.add_child(shop_button)

	var main_menu_button := Button.new()
	main_menu_button.text = tr("UI_MAP_MAIN_MENU")
	main_menu_button.pressed.connect(SceneManager.go_to_main_menu)
	top_bar.add_child(main_menu_button)

	return top_bar

## Builds one location's header and row of level buttons.
func _build_location_section(location: GameEnums.LocationId, completed_level_ids: Array[String]) -> VBoxContainer:
	var section := VBoxContainer.new()
	section.add_theme_constant_override("separation", 6)

	var header := Label.new()
	header.text = tr(CampaignDatabase.get_location_name_key(location))
	section.add_child(header)

	var levels_row := HBoxContainer.new()
	levels_row.add_theme_constant_override("separation", 8)
	section.add_child(levels_row)

	var levels_in_location: Array[LevelDefinition] = []
	for level: LevelDefinition in CampaignDatabase.levels:
		if level.location == location:
			levels_in_location.append(level)
	levels_in_location.sort_custom(func(a: LevelDefinition, b: LevelDefinition) -> bool:
		if a.is_branch != b.is_branch:
			return not a.is_branch
		return a.order_in_location < b.order_in_location
	)

	for level: LevelDefinition in levels_in_location:
		levels_row.add_child(_build_level_button(level, completed_level_ids))

	return section

## Builds a single level's button, disabled and dimmed while locked.
func _build_level_button(level: LevelDefinition, completed_level_ids: Array[String]) -> Button:
	var is_unlocked: bool = level.required_level_id.is_empty() or completed_level_ids.has(level.required_level_id)
	var is_completed: bool = completed_level_ids.has(level.id)

	var label: String = tr("UI_MAP_BRANCH") if level.is_branch else "%s %d" % [tr("UI_MAP_LEVEL"), level.order_in_location]
	if is_completed:
		label += " (%s)" % tr("UI_MAP_COMPLETED")
	elif not is_unlocked:
		label += " (%s)" % tr("UI_MAP_LOCKED")

	var button := Button.new()
	button.text = label
	button.disabled = not is_unlocked
	button.modulate = Color.WHITE if is_unlocked else Color(1, 1, 1, 0.45)

	if is_unlocked:
		button.pressed.connect(func() -> void: _on_level_selected(level))

	return button

## Selects the level on the map and transitions into the battle scene.
static func _on_level_selected(level: LevelDefinition) -> void:
	print("[CampaignMapScene] Selected level %s" % level.id)
	GameStateManager.select_level(level.id)
	SceneManager.load_scene(GameEnums.GameState.BATTLE)
