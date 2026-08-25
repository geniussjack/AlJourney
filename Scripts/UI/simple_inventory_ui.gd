extends Control
## Simplified inventory UI. Shows the item list, current equipment, and
## details for the selected item.

var _coins_label: Label
var _close_button: Button
var _content_hbox: HBoxContainer

var _hero_toggle_btn: TextureButton
var _hero_name_label: Label

var _weapon_icon: TextureRect
var _weapon_name_label: Label
var _weapon_stats_label: Label
var _weapon_desc_label: Label

var _prev_weapon_btn: Button
var _next_weapon_btn: Button
var _upgrade_btn: Button

var _selected_hero: GameEnums.CharacterClass = GameEnums.CharacterClass.MAGE
var _available_weapons: Array[EquipmentData] = []
var _selected_weapon_index: int = 0

func _ready() -> void:
	_coins_label = get_node("MarginContainer/VBoxContainer/Header/CoinsLabel")
	_close_button = get_node("MarginContainer/VBoxContainer/Header/CloseButton")
	_close_button.text = tr("UI_CLOSE")
	_content_hbox = get_node("MarginContainer/VBoxContainer/ContentHBox")

	_close_button.pressed.connect(_on_close_pressed)

	# Clear old UI.
	for child: Node in _content_hbox.get_children():
		child.queue_free()

	_build_new_ui()
	_load_hero_data()

func _process(_delta: float) -> void:
	if Engine.get_physics_frames() % 60 == 0:
		_coins_label.text = "%s: %d" % [tr("UI_COINS"), GameStateManager.coins]

## Builds the whole inventory layout in code.
func _build_new_ui() -> void:
	var main_vbox := VBoxContainer.new()
	main_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	main_vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	_content_hbox.add_child(main_vbox)

	# Hero Toggle.
	_hero_name_label = Label.new()
	_hero_name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	main_vbox.add_child(_hero_name_label)

	_hero_toggle_btn = TextureButton.new()
	_hero_toggle_btn.stretch_mode = TextureButton.STRETCH_KEEP_ASPECT_CENTERED
	_hero_toggle_btn.custom_minimum_size = Vector2(48, 48)
	_hero_toggle_btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	_hero_toggle_btn.pressed.connect(_on_hero_toggled)
	main_vbox.add_child(_hero_toggle_btn)

	# Spacer.
	var spacer1 := Control.new()
	spacer1.custom_minimum_size = Vector2(0, 20)
	main_vbox.add_child(spacer1)

	var equipment_title := Label.new()
	equipment_title.text = tr("UI_EQUIPMENT")
	equipment_title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	main_vbox.add_child(equipment_title)

	# Weapon Selector.
	var weapon_hbox := HBoxContainer.new()
	weapon_hbox.alignment = BoxContainer.ALIGNMENT_CENTER
	main_vbox.add_child(weapon_hbox)

	_prev_weapon_btn = Button.new()
	_prev_weapon_btn.text = "<"
	_prev_weapon_btn.pressed.connect(func() -> void: _cycle_weapon(-1))
	weapon_hbox.add_child(_prev_weapon_btn)

	_weapon_icon = TextureRect.new()
	_weapon_icon.expand_mode = TextureRect.EXPAND_FIT_WIDTH
	_weapon_icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	_weapon_icon.custom_minimum_size = Vector2(64, 64)
	weapon_hbox.add_child(_weapon_icon)

	_next_weapon_btn = Button.new()
	_next_weapon_btn.text = ">"
	_next_weapon_btn.pressed.connect(func() -> void: _cycle_weapon(1))
	weapon_hbox.add_child(_next_weapon_btn)

	_weapon_name_label = Label.new()
	_weapon_name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	main_vbox.add_child(_weapon_name_label)

	_weapon_stats_label = Label.new()
	_weapon_stats_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	main_vbox.add_child(_weapon_stats_label)

	_weapon_desc_label = Label.new()
	_weapon_desc_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_weapon_desc_label.autowrap_mode = TextServer.AUTOWRAP_WORD
	_weapon_desc_label.custom_minimum_size = Vector2(250, 0)
	main_vbox.add_child(_weapon_desc_label)

	# Spacer.
	var spacer2 := Control.new()
	spacer2.custom_minimum_size = Vector2(0, 20)
	main_vbox.add_child(spacer2)

	_upgrade_btn = Button.new()
	_upgrade_btn.text = tr("UI_UPGRADE")
	_upgrade_btn.pressed.connect(_on_upgrade_pressed)
	main_vbox.add_child(_upgrade_btn)

## Loads the selected hero's portrait and available weapons.
func _load_hero_data() -> void:
	_hero_name_label.text = tr("HERO_MAGE") if _selected_hero == GameEnums.CharacterClass.MAGE else tr("HERO_WARRIOR")
	var portrait_path: String = "res://Resources/Sprites/Characters/mage_sprite.png" if _selected_hero == GameEnums.CharacterClass.MAGE else "res://Resources/Sprites/Characters/warrior_sprite.png"

	_hero_toggle_btn.texture_normal = load(portrait_path) if ResourceLoader.exists(portrait_path) else null

	_available_weapons.clear()
	var all_items: Array[EquipmentData] = InventoryManager.get_inventory()
	for item: EquipmentData in all_items:
		if item.slot == GameEnums.EquipmentSlot.WEAPON and (
			(_selected_hero == GameEnums.CharacterClass.MAGE and (item.id.contains("ball") or item.id == "staff"))
			or (_selected_hero == GameEnums.CharacterClass.WARRIOR and (item.id == "sword" or item.id == "axe" or item.id == "spear"))
		):
			_available_weapons.append(item)

	var current_weapon: EquipmentData = InventoryManager.get_equipped_item(_selected_hero, GameEnums.EquipmentSlot.WEAPON)
	_selected_weapon_index = 0
	for i: int in range(_available_weapons.size()):
		if current_weapon != null and _available_weapons[i].id == current_weapon.id:
			_selected_weapon_index = i
			break

	_update_weapon_display()

## Cycles the selected weapon and equips it immediately.
func _cycle_weapon(direction: int) -> void:
	if _available_weapons.is_empty():
		return

	_selected_weapon_index += direction
	if _selected_weapon_index < 0:
		_selected_weapon_index = _available_weapons.size() - 1

	if _selected_weapon_index >= _available_weapons.size():
		_selected_weapon_index = 0

	var new_weapon: EquipmentData = _available_weapons[_selected_weapon_index]
	InventoryManager.equip_item(_selected_hero, new_weapon)
	_update_weapon_display()

## Refreshes the weapon icon, name, stats and upgrade button.
func _update_weapon_display() -> void:
	if _available_weapons.is_empty() or _selected_weapon_index < 0 or _selected_weapon_index >= _available_weapons.size():
		_weapon_name_label.text = tr("UI_INVENTORY_NO_WEAPON")
		_weapon_stats_label.text = ""
		_weapon_desc_label.text = ""
		_weapon_icon.texture = null
		_upgrade_btn.disabled = true
		return

	var weapon: EquipmentData = _available_weapons[_selected_weapon_index]
	_weapon_name_label.text = "%s (%s %d)" % [tr(weapon.name), tr("UI_INVENTORY_LEVEL_LABEL"), weapon.current_level]
	_weapon_name_label.modulate = weapon.get_rarity_color()

	var stats: String = ""
	for stat_name: String in weapon.base_stats.keys():
		var localized_stat_name: String = tr("STAT_%s" % stat_name.to_upper())
		stats += "%s: %d\n" % [localized_stat_name, weapon.base_stats[stat_name]]
	_weapon_stats_label.text = stats
	_weapon_desc_label.text = tr(weapon.description_key)

	var icon_path: String = "res://Resources/Sprites/Elements/%s_sprite.png" % weapon.id
	match weapon.id:
		"fireball":
			icon_path = "res://Resources/Sprites/Elements/fireball_sprite.png"
		"iceball":
			icon_path = "res://Resources/Sprites/Elements/iceball_sprite.png"
		"electroball":
			icon_path = "res://Resources/Sprites/Elements/electroball_sprite.png"
		"sword":
			icon_path = "res://Resources/Sprites/Elements/sword_icon.png"
		"axe":
			icon_path = "res://Resources/Sprites/Elements/axe_sprite.png"
		"spear":
			icon_path = "res://Resources/Sprites/Elements/spear_sprite.png"

	_weapon_icon.texture = load(icon_path) if ResourceLoader.exists(icon_path) else null

	var cost: int = weapon.get_upgrade_cost(GameStateManager.current_wave)
	_upgrade_btn.text = "%s (%d %s)" % [tr("UI_UPGRADE"), cost, tr("UI_COINS").to_lower()]
	_upgrade_btn.disabled = GameStateManager.coins < cost
	_coins_label.text = "%s: %d" % [tr("UI_COINS"), GameStateManager.coins]

func _on_hero_toggled() -> void:
	_selected_hero = GameEnums.CharacterClass.WARRIOR if _selected_hero == GameEnums.CharacterClass.MAGE else GameEnums.CharacterClass.MAGE
	_load_hero_data()

func _on_upgrade_pressed() -> void:
	if _available_weapons.is_empty():
		return

	var weapon: EquipmentData = _available_weapons[_selected_weapon_index]

	if InventoryManager.upgrade_equipment(weapon):
		_load_hero_data()

func _on_close_pressed() -> void:
	queue_free()
