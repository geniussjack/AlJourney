class_name ComboParticles
extends Node2D
## Helper class for creating visual effects and floating text when combo
## effects trigger, damage is taken, healing occurs, or shields are applied.

## Creates and starts a particle effect at the location of a triggered
## combo. The particle color depends on the element type, and the amount
## depends on the combo level. Particles are automatically removed once the
## animation finishes.
## parent: the node the particles will be attached to.
## position: the position where the particles appear.
## element: the ability's element, used to determine the effect color.
## combo_level: the effect's level, determining the number of particles.
static func spawn_combo_effect(parent: Node, position: Vector2, element: GameEnums.AbilityElement, combo_level: int) -> void:
	var particles := CpuParticles2D.new()
	particles.position = position
	particles.emitting = true
	particles.one_shot = true
	particles.amount = _get_particle_amount(combo_level)
	particles.lifetime = 0.5
	particles.explosiveness = 0.8
	particles.spread = 360.0
	particles.initial_velocity_min = 50.0
	particles.initial_velocity_max = 150.0
	particles.scale_amount_min = 0.5
	particles.scale_amount_max = 1.5
	particles.color = _get_element_color(element)

	parent.add_child(particles)

	var timer: SceneTreeTimer = parent.get_tree().create_timer(particles.lifetime + 0.1)
	timer.timeout.connect(particles.queue_free)

	print("[ComboParticles] Spawned %s particles (combo %d)" % [GameEnums.AbilityElement.keys()[element], combo_level])

## Returns the particle count for a given combo level.
static func _get_particle_amount(combo_level: int) -> int:
	match combo_level:
		1:
			return 10
		2:
			return 20
		3:
			return 30
		_:
			return 10

## Returns the particle color associated with an ability element.
static func _get_element_color(element: GameEnums.AbilityElement) -> Color:
	match element:
		GameEnums.AbilityElement.FIRE:
			return Color(1.0, 0.3, 0.0)
		GameEnums.AbilityElement.HEAL:
			return Color(0.0, 1.0, 0.3)
		GameEnums.AbilityElement.SWORD:
			return Color(1.0, 0.6, 0.0)
		GameEnums.AbilityElement.SHIELD:
			return Color(0.2, 0.5, 1.0)
		GameEnums.AbilityElement.ICE:
			return Color(0.6, 0.9, 1.0)
		GameEnums.AbilityElement.LIGHTNING:
			return Color(1.0, 1.0, 0.3)
		GameEnums.AbilityElement.BLEED:
			return Color(0.7, 0.0, 0.1)
		GameEnums.AbilityElement.PIERCE:
			return Color(0.75, 0.75, 0.75)
		_:
			return Color.WHITE

## Creates animated floating text that rises upward and gradually fades
## out. The text is automatically removed once the animation finishes.
static func spawn_floating_text(parent: Node, position: Vector2, text: String, color: Color) -> void:
	var label := Label.new()
	label.position = position
	label.text = text
	label.modulate = color
	label.add_theme_font_size_override("font_size", 24)

	parent.add_child(label)

	var tween: Tween = label.create_tween()
	tween.set_parallel(true)
	tween.tween_property(label, "position:y", position.y - 50, 1.0)
	tween.tween_property(label, "modulate:a", 0.0, 1.0)
	tween.chain().tween_callback(label.queue_free)

## Creates red floating text to display damage taken.
static func spawn_damage_number(parent: Node, position: Vector2, damage: int) -> void:
	spawn_floating_text(parent, position, "-%d" % damage, Color(1.0, 0.3, 0.3))

## Creates green floating text to display healing received.
static func spawn_heal_number(parent: Node, position: Vector2, healing: int) -> void:
	spawn_floating_text(parent, position, "+%d" % healing, Color(0.3, 1.0, 0.3))

## Creates blue floating text to display shield gained.
static func spawn_shield_number(parent: Node, position: Vector2, shield: int) -> void:
	spawn_floating_text(parent, position, "+%d Shield" % shield, Color(0.3, 0.5, 1.0))
