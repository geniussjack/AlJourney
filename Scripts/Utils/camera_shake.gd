class_name CameraShake
extends Node
## Component that creates a camera shake effect. Must be added as a child
## node of a Camera2D. Used to reinforce visual feedback when taking
## damage, on powerful attacks, or during explosions.

var _camera: Camera2D
var _original_offset: Vector2
var _shake_intensity: float
var _shake_duration: float
var _shake_timer: float
var _is_shaking: bool = false

## Checks for a parent camera and stores its original offset so it can be
## restored afterward.
func _ready() -> void:
	_camera = get_parent() as Camera2D

	if _camera != null:
		_original_offset = _camera.offset
		print("[CameraShake] Initialized for Camera2D")
	else:
		printerr("[CameraShake] No Camera2D found! Add this as child of Camera2D.")

## If shaking is active, applies a random offset to the parent camera based
## on the current intensity. Once the timer runs out, restores the camera
## to its original position.
func _process(delta: float) -> void:
	if not _is_shaking or _camera == null:
		return

	_shake_timer -= delta

	if _shake_timer <= 0:
		_is_shaking = false
		_camera.offset = _original_offset
	else:
		var current_intensity: float = _shake_intensity * (_shake_timer / _shake_duration)
		var random_offset := Vector2(
			(randf() * current_intensity * 2) - current_intensity,
			(randf() * current_intensity * 2) - current_intensity
		)
		_camera.offset = _original_offset + random_offset

## Starts the camera shake effect with the given parameters.
## intensity: shake intensity, in pixels.
## duration: effect duration, in seconds.
func shake(intensity: float = 10.0, duration: float = 0.3) -> void:
	if _camera == null:
		return

	_shake_intensity = intensity
	_shake_duration = duration
	_shake_timer = duration
	_is_shaking = true

	print("[CameraShake] Shake started - Intensity: %s, Duration: %s" % [intensity, duration])

## Starts a light camera shake. Suited for weak hits or minor events.
func shake_light() -> void:
	shake(5.0, 0.2)

## Starts a medium camera shake. Suited for regular attacks or standard effects.
func shake_medium() -> void:
	shake(10.0, 0.3)

## Starts a strong camera shake. Suited for critical hits, explosions, or
## powerful spells.
func shake_strong() -> void:
	shake(20.0, 0.5)

## Immediately stops the camera shake and returns it to its original position.
func stop_shake() -> void:
	_is_shaking = false
	if _camera != null:
		_camera.offset = _original_offset
