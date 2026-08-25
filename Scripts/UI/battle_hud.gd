class_name BattleHUD
extends Control
## UI for the battle screen. Responsible for displaying hero health and
## shields, enemy info, the current wave, and available actions.

var _mage_name_label: Label
var _mage_health_bar: ProgressBar
var _mage_health_label: Label
var _mage_shield_label: Label

var _warrior_name_label: Label
var _warrior_health_bar: ProgressBar
var _warrior_health_label: Label
var _warrior_shield_label: Label

var _enemies_container: Container

var _coins_label: Label
var _wave_label: Label
var _party_level_label: Label
var _ultimate_charge_label: Label

var _hero_system: DualHeroSystem
var _battle_manager: BattleManager
var _enemy_health_bars: Array[EnemyHealthBar] = []
var _pause_menu: PauseMenu

var _mage_info_container: Control
var _warrior_info_container: Control
var _mage_status_container: HBoxContainer
var _warrior_status_container: HBoxContainer

var _mage_damage_flash: DamageFlash
var _warrior_damage_flash: DamageFlash

var _inventory_button: Button

## Sets up references to child UI elements and subscribes to game state and
## combo change events.
func _ready() -> void:
	_mage_name_label = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageName")
	_mage_health_bar = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageHealthBar")
	_mage_health_label = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageHealthLabel")
	_mage_shield_label = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MageText/MageShieldLabel")

	_warrior_name_label = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorName")
	_warrior_health_bar = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorHealthBar")
	_warrior_health_label = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorHealthLabel")
	_warrior_shield_label = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorText/WarriorShieldLabel")

	_mage_info_container = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MagePortraitContainer")
	_warrior_info_container = get_node("../DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorPortraitContainer")

	_enemies_container = get_node("../DecorativeLayer/RightPanel/MarginContainer/VBoxContainer")

	_wave_label = get_node("MarginContainer/VBoxContainer/BottomBar/WaveLabel")
	_coins_label = get_node("MarginContainer/VBoxContainer/BottomBar/CoinsContainer/CoinsLabel")

	var bottom_bar: HBoxContainer = get_node("MarginContainer/VBoxContainer/BottomBar")
	_ultimate_charge_label = Label.new()
	_ultimate_charge_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	bottom_bar.add_child(_ultimate_charge_label)
	_update_ultimate_charge(0, BattleManager.MAX_ULTIMATE_CHARGE)

	_party_level_label = Label.new()
	_party_level_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	bottom_bar.add_child(_party_level_label)
	_update_party_level(GameStateManager.party_level)

	_inventory_button = get_node("MarginContainer/VBoxContainer/TopBar/InventoryButton")
	_inventory_button.pressed.connect(_on_inventory_button_pressed)

	GameStateManager.coins_changed.connect(_on_coins_changed)
	GameStateManager.wave_changed.connect(_on_wave_changed)
	GameStateManager.party_leveled_up.connect(_on_party_leveled_up)

	var pause_scene: PackedScene = load("res://Scenes/UI/PauseMenu.tscn")
	if pause_scene != null:
		_pause_menu = pause_scene.instantiate()
		add_child(_pause_menu)
	else:
		printerr("[BattleHUD] Failed to load PauseMenu.tscn")

	print("[BattleHUD] Initialized for dual hero system")

## Initializes the HUD for the hero party and battle manager: sets up
## initial health/shield values, damage-taken effects, and portrait clicks
## for confirming an ability's target.
func initialize(hero_system: DualHeroSystem, battle_manager: BattleManager) -> void:
	_hero_system = hero_system
	_battle_manager = battle_manager

	_hero_system.hero_health_changed.connect(_on_hero_health_changed)
	_hero_system.hero_shield_changed.connect(_on_hero_shield_changed)

	_battle_manager.turn_state_changed.connect(_refresh_target_highlights)
	_battle_manager.phase_changed.connect(_on_battle_phase_changed)
	_battle_manager.ultimate_charge_changed.connect(_on_ultimate_charge_changed)

	_mage_info_container.mouse_filter = Control.MOUSE_FILTER_STOP
	_warrior_info_container.mouse_filter = Control.MOUSE_FILTER_STOP
	_mage_info_container.gui_input.connect(func(event: InputEvent) -> void: _on_ally_portrait_gui_input(event, _hero_system.mage))
	_warrior_info_container.gui_input.connect(func(event: InputEvent) -> void: _on_ally_portrait_gui_input(event, _hero_system.warrior))

	_hero_system.mage.shield_changed.connect(func(shield: int) -> void: _update_hero_shield(GameEnums.CharacterClass.MAGE, shield))
	_hero_system.warrior.shield_changed.connect(func(shield: int) -> void: _update_hero_shield(GameEnums.CharacterClass.WARRIOR, shield))

	_mage_name_label.text = tr("UI_BATTLE_ALTARION")
	_warrior_name_label.text = tr("UI_BATTLE_ALDRIC")

	_update_hero_health(GameEnums.CharacterClass.MAGE, _hero_system.mage.current_health, _hero_system.mage.max_health)
	_update_hero_health(GameEnums.CharacterClass.WARRIOR, _hero_system.warrior.current_health, _hero_system.warrior.max_health)
	_update_hero_shield(GameEnums.CharacterClass.MAGE, _hero_system.mage.current_shield)
	_update_hero_shield(GameEnums.CharacterClass.WARRIOR, _hero_system.warrior.current_shield)

	_update_wave(GameStateManager.current_wave)
	_update_coins(GameStateManager.coins)

	_mage_damage_flash = DamageFlash.new()
	_mage_info_container.add_child(_mage_damage_flash)
	_hero_system.mage.damage_taken.connect(func(_amount: int) -> void: _mage_damage_flash.flash_damage())
	_hero_system.mage.healed.connect(func(_amount: int) -> void: _mage_damage_flash.flash_heal())

	_warrior_damage_flash = DamageFlash.new()
	_warrior_info_container.add_child(_warrior_damage_flash)
	_hero_system.warrior.damage_taken.connect(func(_amount: int) -> void: _warrior_damage_flash.flash_damage())
	_hero_system.warrior.healed.connect(func(_amount: int) -> void: _warrior_damage_flash.flash_heal())

	_mage_status_container = HBoxContainer.new()
	_mage_status_container.alignment = BoxContainer.ALIGNMENT_CENTER
	_mage_info_container.add_child(_mage_status_container)
	_warrior_status_container = HBoxContainer.new()
	_warrior_status_container.alignment = BoxContainer.ALIGNMENT_CENTER
	_warrior_info_container.add_child(_warrior_status_container)

	_hero_system.mage.status_effect_added.connect(func(_type: GameEnums.StatusEffect, _duration: int, _power: int) -> void: _update_hero_status_effects(GameEnums.CharacterClass.MAGE))
	_hero_system.mage.status_effect_removed.connect(func(_type: GameEnums.StatusEffect) -> void: _update_hero_status_effects(GameEnums.CharacterClass.MAGE))
	_hero_system.warrior.status_effect_added.connect(func(_type: GameEnums.StatusEffect, _duration: int, _power: int) -> void: _update_hero_status_effects(GameEnums.CharacterClass.WARRIOR))
	_hero_system.warrior.status_effect_removed.connect(func(_type: GameEnums.StatusEffect) -> void: _update_hero_status_effects(GameEnums.CharacterClass.WARRIOR))

	print("[BattleHUD] Initialized for %s and %s" % [_hero_system.mage.get_character_name(), _hero_system.warrior.get_character_name()])

## Rebuilds the row of status effect icons for the given hero.
func _update_hero_status_effects(hero_class: GameEnums.CharacterClass) -> void:
	var container: HBoxContainer = _mage_status_container if hero_class == GameEnums.CharacterClass.MAGE else _warrior_status_container
	var hero: Character = _hero_system.mage if hero_class == GameEnums.CharacterClass.MAGE else _hero_system.warrior

	for child: Node in container.get_children():
		child.queue_free()

	for effect: StatusEffectData in hero.get_active_effects():
		var rect_color: Color = Color.WHITE
		var icon_emoji: String = "❓"
		match effect.type:
			GameEnums.StatusEffect.BURNING:
				icon_emoji = "🔥"
				rect_color = Color.ORANGE
			GameEnums.StatusEffect.BLEEDING:
				icon_emoji = "🩸"
				rect_color = Color.RED
			GameEnums.StatusEffect.FREEZE:
				icon_emoji = "❄️"
				rect_color = Color.AQUA
			GameEnums.StatusEffect.SHOCK:
				icon_emoji = "⚡"
				rect_color = Color.YELLOW
			GameEnums.StatusEffect.VULNERABLE:
				icon_emoji = "💔"
				rect_color = Color.PURPLE
			GameEnums.StatusEffect.STUNNED:
				icon_emoji = "💫"
				rect_color = Color.GRAY
			GameEnums.StatusEffect.WEAKENED:
				icon_emoji = "📉"
				rect_color = Color.BROWN
			GameEnums.StatusEffect.SHIELD_REFLECT:
				icon_emoji = "🛡️"
				rect_color = Color.LIGHT_BLUE
			GameEnums.StatusEffect.IMMUNITY:
				icon_emoji = "✨"
				rect_color = Color.GOLD
			GameEnums.StatusEffect.REGENERATION:
				icon_emoji = "💚"
				rect_color = Color.GREEN

		var icon := Label.new()
		icon.text = icon_emoji
		icon.modulate = rect_color
		icon.tooltip_text = "%s (%s %d)" % [GameEnums.StatusEffect.keys()[effect.type], tr("UI_STATUS_DURATION_LABEL"), effect.duration]
		icon.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		icon.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		icon.add_theme_font_size_override("font_size", 24)
		container.add_child(icon)

func _on_hero_health_changed(hero_class: GameEnums.CharacterClass, current_health: int, max_health: int) -> void:
	_update_hero_health(hero_class, current_health, max_health)

## Updates a hero's health bar value, label text and color band.
func _update_hero_health(hero_class: GameEnums.CharacterClass, current_health: int, max_health: int) -> void:
	var health_bar: ProgressBar = _mage_health_bar if hero_class == GameEnums.CharacterClass.MAGE else _warrior_health_bar
	var health_label: Label = _mage_health_label if hero_class == GameEnums.CharacterClass.MAGE else _warrior_health_label

	health_bar.max_value = max_health
	health_bar.value = current_health
	health_label.text = "%d / %d" % [current_health, max_health]

	var health_percent: float = float(current_health) / max_health
	if health_percent > 0.5:
		health_bar.modulate = Color.GREEN
	elif health_percent > 0.25:
		health_bar.modulate = Color.YELLOW
	else:
		health_bar.modulate = Color.RED

func _on_hero_shield_changed(hero_class: GameEnums.CharacterClass, shield_amount: int) -> void:
	_update_hero_shield(hero_class, shield_amount)

## Shows/hides and updates a hero's shield label.
func _update_hero_shield(hero_class: GameEnums.CharacterClass, shield_amount: int) -> void:
	var shield_label: Label = _mage_shield_label if hero_class == GameEnums.CharacterClass.MAGE else _warrior_shield_label

	if shield_amount > 0:
		shield_label.show()
		shield_label.text = "%s %d" % [tr("UI_BATTLE_SHIELD"), shield_amount]
	else:
		shield_label.hide()

## Creates and configures health bars for the given list of enemies, first
## clearing any old data.
func setup_enemies(enemies: Array[Enemy]) -> void:
	_clear_enemies()

	for enemy: Enemy in enemies:
		var enemy_bar := EnemyHealthBar.new()
		enemy_bar.initialize(enemy, _battle_manager)
		_enemies_container.add_child(enemy_bar)
		_enemy_health_bars.append(enemy_bar)

	print("[BattleHUD] Setup %d enemy health bars" % enemies.size())

## Frees every existing enemy health bar.
func _clear_enemies() -> void:
	for bar: EnemyHealthBar in _enemy_health_bars:
		bar.queue_free()
	_enemy_health_bars.clear()

func _on_wave_changed(wave_number: int) -> void:
	_update_wave(wave_number)

func _update_wave(wave_number: int) -> void:
	_wave_label.text = "%s %d" % [tr("UI_BATTLE_WAVE"), wave_number]

func _on_coins_changed(coins: int) -> void:
	_update_coins(coins)

func _update_coins(coins: int) -> void:
	_coins_label.text = "%d" % coins

func _on_party_leveled_up(new_level: int) -> void:
	_update_party_level(new_level)

func _update_party_level(level: int) -> void:
	_party_level_label.text = "%s %d" % [tr("UI_PARTY_LEVEL"), level]

func _on_inventory_button_pressed() -> void:
	var inventory_scene: PackedScene = load("res://Scenes/UI/InventoryUI.tscn")
	if inventory_scene != null:
		var inventory: Control = inventory_scene.instantiate()
		add_child(inventory)
	else:
		printerr("[BattleHUD] Failed to load InventoryUI.tscn")

func _on_pause_pressed() -> void:
	print("[BattleHUD] Pause pressed")
	if _pause_menu != null:
		_pause_menu.pause_game()

func _on_battle_phase_changed(_new_phase: GameEnums.BattlePhase) -> void:
	_refresh_target_highlights()

## Updates the highlight and clickability of ally portraits and enemy
## health bars to match the current list of valid targets for the selected
## ability.
func _refresh_target_highlights() -> void:
	if _battle_manager == null:
		return

	var valid_targets: Array[Character] = _battle_manager.get_valid_targets()

	_set_ally_selectable(_mage_info_container, valid_targets.has(_hero_system.mage))
	_set_ally_selectable(_warrior_info_container, valid_targets.has(_hero_system.warrior))

	for bar: EnemyHealthBar in _enemy_health_bars:
		bar.set_selectable(valid_targets.has(bar.enemy))

static func _set_ally_selectable(container: Control, selectable: bool) -> void:
	container.modulate = Color(1.2, 1.2, 0.5) if selectable else Color.WHITE

func _on_ally_portrait_gui_input(event: InputEvent, member: PlayerCharacter) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if _battle_manager != null:
			_battle_manager.confirm_target(member)

func _on_ultimate_charge_changed(charge: int, max_charge: int) -> void:
	_update_ultimate_charge(charge, max_charge)

func _update_ultimate_charge(charge: int, max_charge: int) -> void:
	_ultimate_charge_label.text = "%s %d/%d" % [tr("UI_BATTLE_ULTIMATE_CHARGE"), charge, max_charge]
	_ultimate_charge_label.modulate = Color.GOLD if charge >= max_charge else Color.WHITE

## Unsubscribes from all global and local events to prevent memory leaks.
func _exit_tree() -> void:
	if _hero_system != null:
		_hero_system.hero_health_changed.disconnect(_on_hero_health_changed)
		_hero_system.hero_shield_changed.disconnect(_on_hero_shield_changed)

	if _battle_manager != null:
		_battle_manager.turn_state_changed.disconnect(_refresh_target_highlights)
		_battle_manager.phase_changed.disconnect(_on_battle_phase_changed)
		_battle_manager.ultimate_charge_changed.disconnect(_on_ultimate_charge_changed)

	GameStateManager.coins_changed.disconnect(_on_coins_changed)
	GameStateManager.wave_changed.disconnect(_on_wave_changed)
	GameStateManager.party_leveled_up.disconnect(_on_party_leveled_up)
