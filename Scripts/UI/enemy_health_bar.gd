class_name EnemyHealthBar
extends VBoxContainer
## UI component representing a single enemy's health bar. Displays the name
## and current health, and reacts to damage or healing.

var _name_label: Label
var _health_bar: ProgressBar
var _health_label: Label
var _battle_manager: BattleManager
var _is_selectable: bool = false
var _damage_flash: DamageFlash
var _portrait: TextureRect
var _status_container: HBoxContainer

## The enemy this health bar is bound to.
var enemy: Enemy

## Creates and configures the visual elements of the health bar: name, the
## bar itself, and the health text.
func _init() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	gui_input.connect(_on_gui_input)

	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 10)
	add_child(row)

	var portrait_container := Control.new()
	portrait_container.custom_minimum_size = Vector2(96, 96)
	row.add_child(portrait_container)

	_portrait = TextureRect.new()
	_portrait.set_anchors_preset(Control.PRESET_CENTER)
	_portrait.grow_horizontal = Control.GROW_DIRECTION_BOTH
	_portrait.grow_vertical = Control.GROW_DIRECTION_BOTH
	_portrait.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_portrait.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	_portrait.custom_minimum_size = Vector2(96, 96)
	portrait_container.add_child(_portrait)

	var text_container := VBoxContainer.new()
	text_container.custom_minimum_size = Vector2(150, 0)
	text_container.add_theme_constant_override("separation", 2)
	row.add_child(text_container)

	_name_label = Label.new()
	_name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_LEFT
	_name_label.clip_text = true
	_name_label.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
	text_container.add_child(_name_label)

	_health_bar = ProgressBar.new()
	_health_bar.custom_minimum_size = Vector2(150, 20)
	_health_bar.show_percentage = false
	text_container.add_child(_health_bar)

	_health_label = Label.new()
	_health_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	text_container.add_child(_health_label)

## Initializes the health bar with a specific enemy's data, subscribes to
## its health-change and death events, and sets the color based on the
## enemy type.
func initialize(bound_enemy: Enemy, battle_manager: BattleManager) -> void:
	enemy = bound_enemy
	_battle_manager = battle_manager
	enemy.health_changed.connect(_on_health_changed)
	enemy.character_died.connect(_on_enemy_died)

	_name_label.text = enemy.get_character_name()

	var sprite_path: String
	match enemy.enemy_type:
		GameEnums.EnemyType.SLIME:
			sprite_path = "res://Resources/Sprites/Characters/slime_sprite.png"
		_:
			sprite_path = "res://Resources/Sprites/Characters/skeleton_sprite.png"
	_portrait.texture = load(sprite_path)
	_animate_portrait()
	_update_health(enemy.current_health, enemy.get_total_max_health())

	if enemy.is_boss:
		_health_bar.modulate = Color.PURPLE
	elif enemy.is_miniboss:
		_health_bar.modulate = Color.ORANGE
	else:
		_health_bar.modulate = Color.RED

	_damage_flash = DamageFlash.new()
	add_child(_damage_flash)
	enemy.damage_taken.connect(func(_amount: int) -> void: _damage_flash.flash_damage())
	enemy.healed.connect(func(_amount: int) -> void: _damage_flash.flash_heal())

	_status_container = HBoxContainer.new()
	_status_container.alignment = BoxContainer.ALIGNMENT_CENTER
	# Add _status_container to the text container below the health label.
	var text_container: Node = _health_label.get_parent()
	text_container.add_child(_status_container)

	enemy.status_effect_added.connect(func(_type: GameEnums.StatusEffect, _duration: int, _power: int) -> void: _update_status_effects())
	enemy.status_effect_removed.connect(func(_type: GameEnums.StatusEffect) -> void: _update_status_effects())
	_update_status_effects()

## Rebuilds the row of status effect icons from the enemy's active effects.
func _update_status_effects() -> void:
	for child: Node in _status_container.get_children():
		child.queue_free()

	for effect: StatusEffectData in enemy.get_active_effects():
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
		_status_container.add_child(icon)

## Starts the idle bob/scale animation for the portrait.
func _animate_portrait() -> void:
	_portrait.pivot_offset = Vector2(48, 48)  # 96x96 default size.

	var tween: Tween = create_tween()
	tween.set_loops()
	tween.set_trans(Tween.TRANS_SINE)
	tween.set_ease(Tween.EASE_IN_OUT)

	var delay: float = randf() * 0.5
	var dur1: float = 1.0 + (randf() * 0.2)
	var dur2: float = 1.0 + (randf() * 0.2)

	tween.tween_interval(delay)
	tween.tween_property(_portrait, "scale", Vector2(1.1, 1.1), dur1)
	tween.parallel().tween_property(_portrait, "position", _portrait.position - Vector2(0, 4), dur1)
	tween.tween_property(_portrait, "scale", Vector2(1.0, 1.0), dur2)
	tween.parallel().tween_property(_portrait, "position", _portrait.position, dur2)

func _on_health_changed(current_health: int, max_health: int) -> void:
	_update_health(current_health, max_health)

## Refreshes the bar value and text to match the given health.
func _update_health(current_health: int, max_health: int) -> void:
	_health_bar.max_value = max_health
	_health_bar.value = current_health
	_health_label.text = "%d/%d" % [current_health, max_health]
	_name_label.text = enemy.get_character_name()

## Fades out and frees the health bar when its enemy dies.
func _on_enemy_died() -> void:
	var tween: Tween = create_tween()
	tween.tween_property(self, "modulate:a", 0.0, 0.5)
	tween.tween_callback(queue_free)

## Marks this enemy as a valid (or invalid) target for the currently
## selected ability and highlights the health bar accordingly.
func set_selectable(selectable: bool) -> void:
	_is_selectable = selectable
	modulate = Color(1.3, 1.3, 0.6) if selectable else Color.WHITE

func _on_gui_input(event: InputEvent) -> void:
	if _is_selectable and event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if _battle_manager != null:
			_battle_manager.confirm_target(enemy)

## Unsubscribes from the associated enemy's events.
func _exit_tree() -> void:
	if enemy != null:
		enemy.health_changed.disconnect(_on_health_changed)
		enemy.character_died.disconnect(_on_enemy_died)
