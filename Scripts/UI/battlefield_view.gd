class_name BattlefieldView
extends Control
## Grounded battle presentation and pointer gestures. Combat remains owned
## by BattleManager; opening or cancelling a picker never spends a turn.

const BLUE := Color("50a9ff")
const RED := Color("ef625d")
const GREEN := Color("73dd80")
const INK := Color("182936")
const DRAG_THRESHOLD: float = 12.0
const ANIMATED_ATLAS_CELL_SIZE := Vector2(128, 128)
const ACTION_ANIMATION_DURATION: float = 0.56
const IDLE_FRAME_DURATION: float = 0.28

var _party: DualHeroSystem
var _battle: BattleManager
var _units: Array[Character] = []
var _positions: Dictionary = {}
var _textures: Dictionary = {}
var _regions: Dictionary = {}
var _animation_states: Dictionary = {}
var _elapsed_time: float = 0.0
var _actor: PlayerCharacter
var _target: Character
var _pressed_at := Vector2.ZERO
var _dragging: bool = false
var _orb_drag: bool = false
var _locked: bool = false
var _choices: Array[AbilityData] = []
var _preview: AbilityData
var _menu: PanelContainer
var _buttons: VBoxContainer
var _pause: PauseMenu
var _font: Font
var _inventory: Control

## Builds only contextual controls; there is no permanent action toolbar.
func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	process_mode = Node.PROCESS_MODE_ALWAYS
	_font = ThemeDB.fallback_font
	_menu = PanelContainer.new()
	_menu.custom_minimum_size = Vector2(300, 0)
	_buttons = VBoxContainer.new()
	_menu.add_child(_buttons)
	add_child(_menu)
	_menu.hide()
	var inventory_button := Button.new()
	inventory_button.text = tr("UI_INVENTORY")
	inventory_button.set_anchors_and_offsets_preset(Control.PRESET_TOP_RIGHT)
	inventory_button.offset_left = -200
	inventory_button.offset_right = -20
	inventory_button.offset_top = 12
	inventory_button.offset_bottom = 54
	inventory_button.pressed.connect(_open_inventory)
	add_child(inventory_button)
	_pause = preload("res://Scenes/UI/PauseMenu.tscn").instantiate()
	add_child(_pause)
	_pause.set_process_input(false)
	get_window().focus_exited.connect(cancel_gesture)

## Binds the live party after its optional companion has been created.
func initialize(party: DualHeroSystem, battle: BattleManager) -> void:
	_party = party
	_battle = battle
	_battle.phase_changed.connect(_on_phase_changed)
	_battle.wave_advanced.connect(_on_wave_changed)
	_battle.battle_ended.connect(_on_battle_ended)
	_battle.turn_state_changed.connect(_on_turn_changed)
	_battle.ability_resolved.connect(_on_ability_resolved)
	_battle.enemy_attack_resolved.connect(_on_enemy_attack_resolved)
	_rebuild_units()

## Preserves the battle scene's existing enemy setup entry point.
func setup_enemies(_enemies: Array[Enemy]) -> void:
	cancel_gesture()
	_rebuild_units()

## Refreshes summons as well as wave changes without binding stale actors.
func _process(delta: float) -> void:
	if _battle == null or get_tree().paused:
		return
	_elapsed_time += delta
	_update_animations(delta)
	if _units.size() != _party.get_party_members().size() + _battle.enemies.size():
		_rebuild_units()
	_layout_units()
	if _actor != null and not _battle.can_actor_act(_actor):
		cancel_gesture()
	if _target != null and (not is_instance_valid(_target) or not _target.is_alive):
		cancel_gesture()
	queue_redraw()

## Records one visual for each combat group, including summoned groups.
func _rebuild_units() -> void:
	_units.clear()
	_units.append_array(_party.get_party_members())
	_units.append_array(_battle.enemies)
	for unit: Character in _units:
		var path: String = BattleSpriteCatalog.get_player_texture_path(unit, _party) if unit is PlayerCharacter else BattleSpriteCatalog.get_enemy_texture_path(unit)
		if not _textures.has(path):
			_textures[path] = load(path)
			_regions[path] = Rect2(Vector2.ZERO, ANIMATED_ATLAS_CELL_SIZE) if BattleSpriteCatalog.is_animated_texture(path) else (_textures[path] as Texture2D).get_image().get_used_rect()
		unit.set_meta("battle_texture", path)
		if not _animation_states.has(unit):
			_animation_states[unit] = {"row": 0, "elapsed": 0.0, "locked": false}
	for animated_unit: Variant in _animation_states.keys():
		if not _units.has(animated_unit):
			_animation_states.erase(animated_unit)
	_layout_units()

## Advances one-shot action states and returns living units to their idle row.
func _update_animations(delta: float) -> void:
	for unit: Character in _animation_states.keys():
		var state: Dictionary = _animation_states[unit]
		if state["locked"]:
			continue
		if state["row"] != 0:
			state["elapsed"] += delta
			if state["elapsed"] >= ACTION_ANIMATION_DURATION:
				state["row"] = 0
				state["elapsed"] = 0.0

## Uses staggered floor anchors, leaving the middle free for combat effects.
func _layout_units() -> void:
	_positions.clear()
	var allies: Array[PlayerCharacter] = _party.get_party_members()
	var anchors: Array[Vector2] = [Vector2(0.17, 0.68), Vector2(0.36, 0.79), Vector2(0.28, 0.50)]
	for i: int in range(allies.size()):
		_positions[allies[i]] = size * anchors[i]
	for i: int in range(_battle.enemies.size()):
		var enemy_anchors: Array[Vector2] = [Vector2(0.73, 0.50), Vector2(0.86, 0.65), Vector2(0.66, 0.80)]
		if _battle.enemies.size() <= 3:
			_positions[_battle.enemies[i]] = size * enemy_anchors[i]
		else:
			var rows: int = ceili(_battle.enemies.size() / 3.0)
			_positions[_battle.enemies[i]] = size * Vector2(0.62 + (i % 3) * 0.13, 0.48 + (i / 3) * (0.34 / maxf(1, rows - 1)))
	_battle.visual_positions = _positions.duplicate()

## Draws provisional existing sprites on the floor, with live health and rings.
func _draw() -> void:
	if _battle == null:
		return
	var ordered: Array[Character] = _units.duplicate()
	ordered.sort_custom(func(a: Character, b: Character) -> bool: return _positions.get(a, Vector2.ZERO).y < _positions.get(b, Vector2.ZERO).y)
	for unit: Character in ordered:
		_draw_unit(unit)
	_draw_orb()
	if _actor != null or _orb_drag:
		var color: Color = BLUE
		if _target != null:
			color = GREEN if _target is PlayerCharacter else RED
		if not _locked:
			_draw_ring(get_local_mouse_position(), Vector2(42, 42), color)
	draw_string(_font, Vector2(28, 36), "%s %d  |  %s %d" % [tr("UI_BATTLE_WAVE"), _battle.current_wave_index + 1, tr("UI_PARTY_LEVEL"), GameStateManager.party_level], HORIZONTAL_ALIGNMENT_LEFT, -1, 22, INK)
	draw_string(_font, Vector2(28, size.y - 24), tr("UI_BATTLE_GESTURE_HELP"), HORIZONTAL_ALIGNMENT_LEFT, size.x - 56, 18, INK)

## Draws a body independently of its readiness and target indicators.
func _draw_unit(unit: Character) -> void:
	var foot: Vector2 = _positions[unit]
	var path: String = unit.get_meta("battle_texture")
	var region: Rect2 = _regions[path]
	var height: float = 220.0
	if unit is Enemy and _battle.enemies.size() > 3:
		height = minf(height, 320.0 / ceili(_battle.enemies.size() / 3.0))
	if unit is Enemy and unit.enemy_type == GameEnums.EnemyType.SLIME:
		height = 125.0
	var extent: Vector2 = region.size * (height / maxf(1, region.size.y))
	var body := Rect2(foot - Vector2(extent.x / 2, height), extent)
	var tint: Color = Color.WHITE if unit.is_alive else Color(0.5, 0.5, 0.5, 0.4)
	draw_set_transform(foot, 0, Vector2(1.0, 0.25))
	draw_circle(Vector2.ZERO, 65, Color(0.05, 0.1, 0.06, 0.3))
	draw_set_transform(Vector2.ZERO)
	if unit is PlayerCharacter and _battle.can_actor_act(unit) and unit != _actor:
		_draw_ring(foot, Vector2(78, 22), BLUE)
	var texture: Texture2D = _textures[path]
	if BattleSpriteCatalog.is_animated_texture(path):
		region.position = _get_animation_frame(unit) * ANIMATED_ATLAS_CELL_SIZE
	draw_texture_rect_region(texture, body, region, tint)
	var maximum: int = unit.get_total_max_health()
	draw_rect(Rect2(foot + Vector2(-67, 17), Vector2(134, 16)), INK)
	draw_rect(Rect2(foot + Vector2(-63, 21), Vector2(126 * clampf(float(unit.current_health) / maxi(1, maximum), 0, 1), 8)), GREEN)
	var display_name: String = tr(unit.get_character_name())
	if unit == _party.mage:
		display_name = tr("UI_BATTLE_ALTARION")
	elif unit == _party.warrior:
		display_name = tr("UI_BATTLE_ALDRIC")
	draw_string(_font, foot + Vector2(-100, 54), display_name, HORIZONTAL_ALIGNMENT_CENTER, 200, 18, INK)
	draw_string(_font, foot + Vector2(-100, 77), "%d/%d  %s %d" % [unit.current_health, maximum, tr("UI_BATTLE_SHIELD"), unit.current_shield], HORIZONTAL_ALIGNMENT_CENTER, 200, 16, INK)
	var statuses: String = ""
	for effect: StatusEffectData in unit.get_active_effects():
		statuses += "%s:%d " % [tr("STATUS_" + GameEnums.StatusEffect.keys()[effect.type]), effect.duration]
	draw_string(_font, foot + Vector2(-120, 97), statuses, HORIZONTAL_ALIGNMENT_CENTER, 240, 14, INK)
	if (_actor != null or _orb_drag) and unit.is_alive:
		if not _is_valid_target(unit):
			var center: Vector2 = foot - Vector2(0, 130)
			_draw_ring(center, Vector2(28, 28), RED)
			draw_line(center - Vector2(19, 19), center + Vector2(19, 19), RED, 5)
		elif unit == _target or (_preview != null and _preview.is_aoe and ((unit is PlayerCharacter) == (_target is PlayerCharacter))):
			_draw_ring(foot, Vector2(82, 26), GREEN if unit is PlayerCharacter else RED)

## Returns the atlas column and row for a unit's current presentation state.
func _get_animation_frame(unit: Character) -> Vector2:
	var state: Dictionary = _animation_states.get(unit, {"row": 0, "elapsed": 0.0, "locked": false})
	var row: int = state["row"]
	var column: int
	if state["locked"]:
		column = 3
	elif row == 0:
		column = int(_elapsed_time / IDLE_FRAME_DURATION) % 4
	else:
		column = mini(3, int(state["elapsed"] / ACTION_ANIMATION_DURATION * 4.0))
	return Vector2(column, row)

## Starts attack, hit and defeat rows after combat logic has resolved an ability.
func _on_ability_resolved(caster: PlayerCharacter, ability: AbilityData, targets: Array[Character]) -> void:
	if ability.is_attack_ability:
		_play_animation(caster, 1)
		for target: Character in targets:
			_play_animation(target, 3 if not target.is_alive else 2, not target.is_alive)

## Assigns a one-shot row, or freezes a defeated combatant on its final frame.
func _play_animation(unit: Character, row: int, locked: bool = false) -> void:
	if not _animation_states.has(unit):
		return
	_animation_states[unit] = {"row": row, "elapsed": 0.0, "locked": locked}

## Animates a standard enemy strike and the resulting player hit or defeat.
func _on_enemy_attack_resolved(caster: Enemy, target: PlayerCharacter) -> void:
	_play_animation(caster, 3 if not caster.is_alive else 1, not caster.is_alive)
	_play_animation(target, 3 if not target.is_alive else 2, not target.is_alive)

## Renders stepped rings without filtered raster textures.
func _draw_ring(center: Vector2, radius: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for i: int in range(33):
		var angle: float = TAU * i / 32.0
		points.append((center + Vector2(cos(angle), sin(angle)) * radius).snapped(Vector2(3, 3)))
	draw_polyline(points, color, 4.0, false)

## Gives the shared charge a globe silhouette and a visible fill level.
func _draw_orb() -> void:
	var center: Vector2 = _orb_position()
	draw_rect(Rect2(center + Vector2(-43, 47), Vector2(86, 12)), Color("926e3f"))
	draw_circle(center, 52, INK)
	var fraction: float = float(_battle.ultimate_charge) / BattleManager.MAX_ULTIMATE_CHARGE
	for y: int in range(-45, 46, 3):
		var half_width: float = floorf(sqrt(45 * 45 - y * y) / 3) * 3
		var color: Color = Color("8064d9") if y >= 45 - 90 * fraction else Color("b0d6dd")
		draw_rect(Rect2(center + Vector2(-half_width, y), Vector2(half_width * 2, 3)), color)
	_draw_ring(center, Vector2(50, 50), GREEN if _battle.is_ultimate_ready else BLUE)
	draw_string(_font, center + Vector2(-50, 83), "%d/%d" % [_battle.ultimate_charge, BattleManager.MAX_ULTIMATE_CHARGE], HORIZONTAL_ALIGNMENT_CENTER, 100, 20, INK)

## Keeps the orb separate from character hit areas.
func _orb_position() -> Vector2:
	return size * Vector2(0.5, 0.87)

## Processes cancellation before the pause menu can consume Escape.
func _input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):
		if is_instance_valid(_inventory):
			_inventory.queue_free()
			_inventory = null
		elif _actor != null or _orb_drag:
			cancel_gesture()
		elif _pause.visible:
			_pause.resume()
		else:
			_pause.pause_game()
		get_viewport().set_input_as_handled()

## Handles press, preview and release; contextual buttons own their clicks.
func _unhandled_input(event: InputEvent) -> void:
	_handle_pointer(event, get_local_mouse_position())

## Resolves local pointer coordinates independently of window scaling.
func _handle_pointer(event: InputEvent, mouse: Vector2) -> void:
	if get_tree().paused or is_instance_valid(_inventory) or _battle == null or _battle.current_phase != GameEnums.BattlePhase.PLAYER_TURN:
		return
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_RIGHT:
		cancel_gesture()
		var unit: Character = _unit_at(mouse)
		if unit is PlayerCharacter and _battle.can_actor_act(unit):
			_actor = unit
			_target = unit
			_locked = true
			_show_potions()
		return
	if event is InputEventMouseMotion and (_actor != null or _orb_drag) and not _locked:
		_dragging = _dragging or mouse.distance_to(_pressed_at) >= DRAG_THRESHOLD
		var hovered: Character = _unit_at(mouse)
		if hovered != _target:
			_target = hovered if _is_valid_target(hovered) else null
			_show_choices(false)
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		if event.pressed:
			cancel_gesture()
			_pressed_at = mouse
			if mouse.distance_to(_orb_position()) <= 55 and _battle.is_ultimate_ready:
				_orb_drag = true
			else:
				var unit: Character = _unit_at(mouse)
				if unit is PlayerCharacter and _battle.can_actor_act(unit):
					_actor = unit
		elif _actor != null or _orb_drag:
			var unit: Character = _unit_at(mouse)
			if not _dragging and not _orb_drag:
				unit = _actor
			_target = unit if _is_valid_target(unit) else null
			if _target == null:
				cancel_gesture()
			elif _orb_drag:
				_battle.select_actor(_target as PlayerCharacter)
				_battle.select_ability(AbilityDatabase.get_hero_ultimate((_target as PlayerCharacter).character_class))
				cancel_gesture()
			else:
				_choices = _matching_abilities(_target)
				if _choices.size() == 1:
					_execute(_choices[0])
				else:
					_locked = true
					_show_choices(true)

## Picks frontmost bodies, not health bars or decorative shadows.
func _unit_at(point: Vector2) -> Character:
	var best: Character
	for unit: Character in _units:
		if unit.is_alive and Rect2(_positions[unit] - Vector2(105, 240), Vector2(210, 250)).has_point(point):
			if best == null or _positions[unit].y > _positions[best].y:
				best = unit
	return best

## Uses the existing target rules to identify both options for specialists.
func _matching_abilities(target: Character) -> Array[AbilityData]:
	var result: Array[AbilityData] = []
	if _actor == null or target == null or not target.is_alive:
		return result
	for ability: AbilityData in _battle.get_actor_abilities(_actor):
		if (ability.target_type == GameEnums.AbilityTargetType.ENEMY) == (target is Enemy):
			result.append(ability)
	return result

## Orb gestures only accept a living main hero with an unused action.
func _is_valid_target(target: Character) -> bool:
	if target == null or not target.is_alive:
		return false
	if _orb_drag:
		return target is PlayerCharacter and not target.is_mercenary and _battle.can_actor_act(target)
	return not _matching_abilities(target).is_empty()

## Shows a non-interactive preview until release locks the target.
func _show_choices(interactive: bool) -> void:
	for child: Node in _buttons.get_children():
		_buttons.remove_child(child)
		child.queue_free()
	_choices = _matching_abilities(_target)
	_menu.hide()
	if _choices.size() < 2:
		return
	_menu.mouse_filter = Control.MOUSE_FILTER_STOP if interactive else Control.MOUSE_FILTER_IGNORE
	_buttons.mouse_filter = Control.MOUSE_FILTER_IGNORE
	for ability: AbilityData in _choices:
		var button := Button.new()
		button.text = tr(ability.name)
		button.tooltip_text = tr(ability.description)
		button.custom_minimum_size = Vector2(300, 48)
		button.mouse_filter = Control.MOUSE_FILTER_STOP if interactive else Control.MOUSE_FILTER_IGNORE
		button.pressed.connect(_execute.bind(ability))
		button.mouse_entered.connect(func() -> void: _preview = ability)
		button.mouse_exited.connect(func() -> void: _preview = null)
		_buttons.add_child(button)
	_menu.reset_size()
	var anchor: Vector2 = _positions[_target] - Vector2(150, 355)
	_menu.position = anchor.clamp(Vector2(8, 48), size - Vector2(320, 130))
	_menu.show()

## Revalidates the action after menu selection before changing combat state.
func _execute(ability: AbilityData) -> void:
	if _battle.can_actor_act(_actor) and _matching_abilities(_target).has(ability):
		_battle.select_actor(_actor)
		_battle.select_ability(ability)
		_battle.confirm_target(_target)
	cancel_gesture()

## Keeps existing brewed potions accessible through an actor context menu.
func _show_potions() -> void:
	for child: Node in _buttons.get_children():
		_buttons.remove_child(child)
		child.queue_free()
	_menu.mouse_filter = Control.MOUSE_FILTER_STOP
	for potion: PotionData in PotionDatabase.get_all_potions():
		var button := Button.new()
		var count: int = GameStateManager.get_potion_count(potion.id)
		button.text = "%s ×%d" % [tr(potion.name_key), count]
		button.custom_minimum_size = Vector2(300, 48)
		button.disabled = count <= 0
		button.pressed.connect(_use_potion.bind(potion))
		_buttons.add_child(button)
	_menu.reset_size()
	_menu.position = (_positions[_actor] - Vector2(150, 400)).clamp(Vector2(8, 48), size - Vector2(320, 190))
	_menu.show()

## Consuming a potion follows the existing one-action cost in BattleManager.
func _use_potion(potion: PotionData) -> void:
	if _battle.can_actor_act(_actor):
		_battle.select_actor(_actor)
		_battle.use_potion(potion)
	cancel_gesture()

## Preserves access to the existing inventory without an ability toolbar.
func _open_inventory() -> void:
	cancel_gesture()
	if is_instance_valid(_inventory):
		return
	_inventory = preload("res://Scenes/UI/InventoryUI.tscn").instantiate()
	add_child(_inventory)

## Drops only UI state; cancellation never calls a combat action.
func cancel_gesture() -> void:
	_actor = null
	_target = null
	_preview = null
	_dragging = false
	_orb_drag = false
	_locked = false
	if _menu != null:
		_menu.hide()
	queue_redraw()

## Invalidates pointer state whenever combat changes ownership of the turn.
func _on_phase_changed(_phase: GameEnums.BattlePhase) -> void:
	cancel_gesture()

## Prevents old-wave targets from surviving a transition.
func _on_wave_changed(_index: int, _total: int) -> void:
	setup_enemies(_battle.enemies)

## Discards any selection after party defeat.
func _on_battle_ended(_won: bool) -> void:
	cancel_gesture()

## A resolved action invalidates a menu even if other party members remain.
func _on_turn_changed() -> void:
	queue_redraw()
