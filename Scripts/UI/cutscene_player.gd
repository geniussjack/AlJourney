class_name CutscenePlayer
extends Control
## Reusable full-screen cutscene player: shows a CutsceneData's slides one
## at a time and calls back when finished. Advances a slide on tap/click,
## and skips the whole cutscene when Enter is held down continuously for
## SKIP_HOLD_DURATION_SECONDS — polled directly via
## Input.is_physical_key_pressed rather than through a project InputMap
## action, so this works without touching project.godot. Built entirely in
## code, similar to BattleHUD/CampaignMapScene — the scene file only
## carries the root Control and this script.

const SKIP_HOLD_DURATION_SECONDS: float = 2.0

var _slide_image: TextureRect
var _slide_label: Label
var _skip_hint_label: Label
var _skip_progress_bar: ProgressBar

var _slides: Array[CutsceneSlide]
var _on_finished: Callable
var _current_slide_index: int = 0
var _skip_hold_time: float = 0.0
var _is_finished: bool = false

## Builds the (data-less) visual layout. Actual slide content is supplied
## afterwards via initialize().
func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	z_index = 100

	var background := ColorRect.new()
	background.color = Color(0, 0, 0, 0.92)
	background.set_anchors_preset(Control.PRESET_FULL_RECT)
	background.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(background)

	var center := CenterContainer.new()
	center.set_anchors_preset(Control.PRESET_FULL_RECT)
	center.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(center)

	var layout := VBoxContainer.new()
	layout.alignment = BoxContainer.ALIGNMENT_CENTER
	layout.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layout.add_theme_constant_override("separation", 24)
	center.add_child(layout)

	_slide_image = TextureRect.new()
	_slide_image.custom_minimum_size = Vector2(0, 280)
	_slide_image.expand_mode = TextureRect.EXPAND_FIT_WIDTH
	_slide_image.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	_slide_image.visible = false
	layout.add_child(_slide_image)

	_slide_label = Label.new()
	_slide_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_slide_label.autowrap_mode = TextServer.AUTOWRAP_WORD
	_slide_label.custom_minimum_size = Vector2(900, 0)
	_slide_label.add_theme_font_size_override("font_size", 28)
	layout.add_child(_slide_label)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 20)
	layout.add_child(spacer)

	_skip_hint_label = Label.new()
	_skip_hint_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_skip_hint_label.text = tr("UI_CUTSCENE_SKIP_HINT")
	_skip_hint_label.modulate = Color(1, 1, 1, 0.6)
	_skip_hint_label.add_theme_font_size_override("font_size", 16)
	layout.add_child(_skip_hint_label)

	_skip_progress_bar = ProgressBar.new()
	_skip_progress_bar.custom_minimum_size = Vector2(220, 6)
	_skip_progress_bar.show_percentage = false
	_skip_progress_bar.max_value = 1.0
	layout.add_child(_skip_progress_bar)

## Supplies the cutscene to play and the callback to invoke once it
## finishes (either by reaching the last slide or by being skipped).
## on_finished: invoked exactly once, after this node has removed itself.
func initialize(data: CutsceneData, on_finished: Callable) -> void:
	_slides = data.slides
	_on_finished = on_finished
	_current_slide_index = 0

	_show_current_slide()

## Advances one slide on a tap/click anywhere on the cutscene.
func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and not event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		_advance_slide()

## Polls the physical Enter key every frame: a continuous hold of
## SKIP_HOLD_DURATION_SECONDS skips the whole cutscene, while a shorter tap
## advances a single slide (detected on release, since a genuine hold and a
## tap are only distinguishable in hindsight).
func _process(delta: float) -> void:
	if _is_finished:
		return

	var is_holding_skip_key: bool = Input.is_physical_key_pressed(KEY_ENTER) or Input.is_physical_key_pressed(KEY_KP_ENTER)

	if is_holding_skip_key:
		_skip_hold_time += delta
		_skip_progress_bar.value = clampf(_skip_hold_time / SKIP_HOLD_DURATION_SECONDS, 0.0, 1.0)

		if _skip_hold_time >= SKIP_HOLD_DURATION_SECONDS:
			_finish()

		return

	var was_tap: bool = _skip_hold_time > 0.0 and _skip_hold_time < SKIP_HOLD_DURATION_SECONDS
	_skip_hold_time = 0.0
	_skip_progress_bar.value = 0.0

	if was_tap:
		_advance_slide()

## Shows the current slide's text and (if present) illustration.
func _show_current_slide() -> void:
	var slide: CutsceneSlide = _slides[_current_slide_index]
	_slide_label.text = tr(slide.text_key)

	if not slide.image_path.is_empty() and ResourceLoader.exists(slide.image_path):
		_slide_image.texture = load(slide.image_path)
		_slide_image.visible = true
	else:
		_slide_image.texture = null
		_slide_image.visible = false

## Moves to the next slide, or finishes the cutscene if it was the last one.
func _advance_slide() -> void:
	_current_slide_index += 1

	if _current_slide_index >= _slides.size():
		_finish()
		return

	_show_current_slide()

## Removes the player and invokes the finished callback exactly once.
func _finish() -> void:
	if _is_finished:
		return

	_is_finished = true

	var on_finished: Callable = _on_finished
	queue_free()
	if on_finished.is_valid():
		on_finished.call()

## Instantiates a CutscenePlayer as a child of parent and starts playing
## the given cutscene. This is the intended entry point — callers should
## not instantiate the scene directly.
## parent: the node the player is attached to (typically the current UI screen).
## on_finished: invoked once the cutscene ends or is skipped.
static func play(parent: Node, data: CutsceneData, on_finished: Callable) -> CutscenePlayer:
	var scene: PackedScene = load("res://Scenes/UI/CutscenePlayer.tscn")
	var player: CutscenePlayer = scene.instantiate()

	parent.add_child(player)
	player.initialize(data, on_finished)

	return player
