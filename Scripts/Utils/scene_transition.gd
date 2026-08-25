class_name SceneTransition
extends CanvasLayer
## Component that creates smooth transitions between scenes. Uses a
## full-screen rectangle to create fade-out, fade-in and flash effects.
## Helps avoid jarring frame changes in the game.

var _fade_rect: ColorRect
var _is_transitioning: bool = false

## Creates a full-screen ColorRect, makes it fully transparent, and sets it
## up so it doesn't intercept mouse events.
func _ready() -> void:
	_fade_rect = ColorRect.new()
	_fade_rect.color = Color.BLACK
	_fade_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_fade_rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(_fade_rect)

	_fade_rect.modulate = Color(1, 1, 1, 0)

	print("[SceneTransition] Initialized")

## Performs a smooth transition to the given scene. First fades the screen
## to black, then loads the new scene and fades back in.
## duration: the total transition time, in seconds.
func transition_to_scene(scene_path: String, duration: float = 0.5) -> void:
	if _is_transitioning:
		printerr("[SceneTransition] Already transitioning!")
		return

	_is_transitioning = true
	print("[SceneTransition] Transitioning to: %s" % scene_path)

	var tween: Tween = create_tween()
	tween.tween_property(_fade_rect, "modulate:a", 1.0, duration / 2)
	tween.tween_callback(func() -> void:
		get_tree().change_scene_to_file(scene_path)

		var fade_tween: Tween = create_tween()
		fade_tween.tween_property(_fade_rect, "modulate:a", 0.0, duration / 2)
		fade_tween.tween_callback(func() -> void: _is_transitioning = false)
	)

## Smoothly fades the screen out to fully opaque.
## duration: the fade-out duration, in seconds.
func fade_out(duration: float = 0.3) -> void:
	var tween: Tween = create_tween()
	tween.tween_property(_fade_rect, "modulate:a", 1.0, duration)

## Smoothly fades the screen in to fully transparent.
## duration: the fade-in duration, in seconds.
func fade_in(duration: float = 0.3) -> void:
	var tween: Tween = create_tween()
	tween.tween_property(_fade_rect, "modulate:a", 0.0, duration)

## Creates a brief screen flash of the given color. Useful for visualizing
## heavy damage, critical hits, or other significant events.
func flash(color: Color, duration: float = 0.2) -> void:
	var original_color: Color = _fade_rect.color
	_fade_rect.color = color

	var tween: Tween = create_tween()
	tween.tween_property(_fade_rect, "modulate:a", 0.7, duration / 2)
	tween.tween_property(_fade_rect, "modulate:a", 0.0, duration / 2)
	tween.tween_callback(func() -> void: _fade_rect.color = original_color)
