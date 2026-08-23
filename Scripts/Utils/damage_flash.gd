class_name DamageFlash
extends Node
## Component that briefly changes the color of the parent CanvasItem. Used
## to create a "flash" effect when taking damage, healing, or on other events.

var _target: CanvasItem
var _original_modulate: Color

## Looks for a parent node of type CanvasItem and stores its original color
## so it can be correctly restored after the flash animation ends.
func _ready() -> void:
	_target = get_parent() as CanvasItem

	if _target != null:
		_original_modulate = _target.modulate
		print("[DamageFlash] Initialized for %s" % _target.name)
	else:
		printerr("[DamageFlash] No CanvasItem parent found!")

## Starts an animation changing the parent node's color to the given one,
## then smoothly returns it to its original state.
## flash_color: the color the object will flash.
## duration: the total duration of the effect, in seconds.
func flash(flash_color: Color, duration: float = 0.15) -> void:
	if _target == null:
		return

	var tween: Tween = create_tween()
	tween.tween_property(_target, "modulate", flash_color, duration / 2)
	tween.tween_property(_target, "modulate", _original_modulate, duration / 2)

## Starts a red flash. Intended to visualize taking damage.
func flash_damage() -> void:
	flash(Color(1.5, 0.5, 0.5, 1.0), 0.2)

## Starts a green flash. Intended to visualize a healing effect.
func flash_heal() -> void:
	flash(Color(0.5, 1.5, 0.5, 1.0), 0.2)

## Starts a blue flash. Intended to visualize gaining a shield or magical protection.
func flash_shield() -> void:
	flash(Color(0.5, 0.5, 1.5, 1.0), 0.2)

## Starts a bright white flash. Intended to visualize critical hits or
## other powerful events.
func flash_critical() -> void:
	flash(Color(2.0, 2.0, 2.0, 1.0), 0.15)

## Starts a flash with a custom color and the given parameters. Allows
## flexible use of the effect for non-standard situations.
func flash_custom(color: Color, duration: float = 0.15) -> void:
	flash(color, duration)
