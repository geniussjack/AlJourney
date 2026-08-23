extends Control
## UI for the settings menu. Manages video and audio setting changes.

var _resolution_dropdown: OptionButton
var _window_mode_dropdown: OptionButton
var _language_dropdown: OptionButton
var _fps_limit_dropdown: OptionButton

var _master_volume_slider: HSlider
var _master_volume_label: Label
var _music_volume_slider: HSlider
var _music_volume_label: Label
var _sfx_volume_slider: HSlider
var _sfx_volume_label: Label

var _apply_button: Button
var _reset_button: Button
var _back_button: Button

var _has_unsaved_changes: bool = false

const RESOLUTIONS: Array[Vector2i] = [
	Vector2i(1280, 720),
	Vector2i(1920, 1080),
	Vector2i(2560, 1440),
	Vector2i(3840, 2160),
]

const FPS_LIMITS: Array[int] = [30, 60, 120, 144, 240, 0]

## Sets up references to the controls, subscribes to their events, and
## loads the current settings.
func _ready() -> void:
	_resolution_dropdown = get_node("SettingsMenu/Panel/VBoxContainer/VideoSettings/ResolutionDropdown")
	_window_mode_dropdown = get_node("SettingsMenu/Panel/VBoxContainer/VideoSettings/WindowModeDropdown")
	_language_dropdown = get_node("SettingsMenu/Panel/VBoxContainer/VideoSettings/LanguageDropdown")
	_fps_limit_dropdown = get_node("SettingsMenu/Panel/VBoxContainer/VideoSettings/FpsLimitDropdown")

	_master_volume_slider = get_node("SettingsMenu/Panel/VBoxContainer/AudioSettings/MasterVolumeSlider")
	_master_volume_label = get_node("SettingsMenu/Panel/VBoxContainer/AudioSettings/MasterVolumeContainer/MasterVolumeLabel")
	_music_volume_slider = get_node("SettingsMenu/Panel/VBoxContainer/AudioSettings/MusicVolumeSlider")
	_music_volume_label = get_node("SettingsMenu/Panel/VBoxContainer/AudioSettings/MusicVolumeContainer/MusicVolumeLabel")
	_sfx_volume_slider = get_node("SettingsMenu/Panel/VBoxContainer/AudioSettings/SfxVolumeSlider")
	_sfx_volume_label = get_node("SettingsMenu/Panel/VBoxContainer/AudioSettings/SfxVolumeContainer/SfxVolumeLabel")

	_apply_button = get_node("SettingsMenu/Panel/VBoxContainer/ButtonsContainer/ApplyButton")
	_reset_button = get_node("SettingsMenu/Panel/VBoxContainer/ButtonsContainer/ResetButton")
	_back_button = get_node("SettingsMenu/Panel/VBoxContainer/ButtonsContainer/BackButton")

	_setup_resolution_dropdown()
	_setup_fps_dropdown()
	_setup_language_dropdown()
	_setup_window_mode_dropdown()

	_window_mode_dropdown.item_selected.connect(_on_window_mode_selected)
	_language_dropdown.item_selected.connect(_on_language_selected)
	_resolution_dropdown.item_selected.connect(_on_resolution_selected)
	_fps_limit_dropdown.item_selected.connect(_on_fps_limit_selected)

	_master_volume_slider.value_changed.connect(_on_master_volume_changed)
	_music_volume_slider.value_changed.connect(_on_music_volume_changed)
	_sfx_volume_slider.value_changed.connect(_on_sfx_volume_changed)

	_apply_button.pressed.connect(_on_apply_pressed)
	_reset_button.pressed.connect(_on_reset_pressed)
	_back_button.pressed.connect(_on_back_pressed)

	_load_current_settings()

	print("[SettingsMenuUI] Initialized")

## Rebuilds the language-dependent dropdowns and reloads values whenever
## the active locale changes.
func _notification(what: int) -> void:
	if what == NOTIFICATION_TRANSLATION_CHANGED:
		_setup_language_dropdown()
		_setup_window_mode_dropdown()
		_load_current_settings()

## Populates the language dropdown's translated entries.
func _setup_language_dropdown() -> void:
	if _language_dropdown == null:
		return

	_language_dropdown.clear()
	_language_dropdown.add_item(tr("UI_ENGLISH"), 0)
	_language_dropdown.add_item(tr("UI_RUSSIAN"), 1)

## Populates the window mode dropdown's translated entries.
func _setup_window_mode_dropdown() -> void:
	if _window_mode_dropdown == null:
		return

	_window_mode_dropdown.clear()
	_window_mode_dropdown.add_item(tr("UI_FULLSCREEN"), 0)
	_window_mode_dropdown.add_item(tr("UI_BORDERLESS"), 1)
	_window_mode_dropdown.add_item(tr("UI_WINDOWED"), 2)

## Populates the resolution dropdown's entries.
func _setup_resolution_dropdown() -> void:
	_resolution_dropdown.clear()
	for i: int in range(RESOLUTIONS.size()):
		var res: Vector2i = RESOLUTIONS[i]
		_resolution_dropdown.add_item("%d x %d" % [res.x, res.y], i)

## Populates the FPS limit dropdown's entries.
func _setup_fps_dropdown() -> void:
	_fps_limit_dropdown.clear()
	for i: int in range(FPS_LIMITS.size()):
		var fps: int = FPS_LIMITS[i]
		var label: String = "Unlimited" if fps == 0 else "%d FPS" % fps
		_fps_limit_dropdown.add_item(label, i)

## Loads every control's value from SettingsManager's current state. Can
## run before _ready() (SettingsManager applying its loaded settings
## broadcasts NOTIFICATION_TRANSLATION_CHANGED to every Control already in
## the tree, including this one, before its own _ready() has assigned the
## control references below) — bail out until this node is actually ready.
func _load_current_settings() -> void:
	if _window_mode_dropdown == null:
		return

	_window_mode_dropdown.selected = SettingsManager.window_mode
	_language_dropdown.selected = 1 if SettingsManager.language == "ru" else 0

	var current_res: Vector2i = SettingsManager.resolution
	for i: int in range(RESOLUTIONS.size()):
		if RESOLUTIONS[i] == current_res:
			_resolution_dropdown.selected = i
			break

	var current_fps: int = SettingsManager.max_fps
	for i: int in range(FPS_LIMITS.size()):
		if FPS_LIMITS[i] == current_fps:
			_fps_limit_dropdown.selected = i
			break

	_master_volume_slider.value = SettingsManager.master_volume
	_music_volume_slider.value = SettingsManager.music_volume
	_sfx_volume_slider.value = SettingsManager.sfx_volume

	_update_volume_labels()
	_has_unsaved_changes = false

## Refreshes the percentage text next to each volume slider.
func _update_volume_labels() -> void:
	_master_volume_label.text = "%s %.0f%%" % [tr("UI_SETTINGS_MASTER"), _master_volume_slider.value * 100]
	_music_volume_label.text = "%s %.0f%%" % [tr("UI_SETTINGS_MUSIC"), _music_volume_slider.value * 100]
	_sfx_volume_label.text = "%s %.0f%%" % [tr("UI_SETTINGS_SFX"), _sfx_volume_slider.value * 100]

func _on_window_mode_selected(index: int) -> void:
	SettingsManager.set_window_mode(index, false)
	_has_unsaved_changes = true

func _on_language_selected(index: int) -> void:
	var lang: String = "ru" if index == 1 else "en"
	SettingsManager.set_language(lang)
	_has_unsaved_changes = true

func _on_resolution_selected(index: int) -> void:
	SettingsManager.set_resolution(RESOLUTIONS[index], false)
	_has_unsaved_changes = true

func _on_fps_limit_selected(index: int) -> void:
	SettingsManager.set_max_fps(FPS_LIMITS[index], false)
	_has_unsaved_changes = true

func _on_master_volume_changed(value: float) -> void:
	SettingsManager.set_master_volume(value)
	_update_volume_labels()
	_has_unsaved_changes = true

func _on_music_volume_changed(value: float) -> void:
	SettingsManager.set_music_volume(value)
	_update_volume_labels()
	_has_unsaved_changes = true

func _on_sfx_volume_changed(value: float) -> void:
	SettingsManager.set_sfx_volume(value)
	_update_volume_labels()
	_has_unsaved_changes = true

	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")

func _on_apply_pressed() -> void:
	print("[SettingsMenuUI] Apply pressed")

	SettingsManager.apply_video_settings()
	SettingsManager.save_settings()
	_has_unsaved_changes = false

	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")

	if get_parent() is MainMenuUI:
		(get_parent() as MainMenuUI).on_back_to_main_menu()

func _on_reset_pressed() -> void:
	print("[SettingsMenuUI] Reset pressed")

	SettingsManager.reset_to_defaults()

	_load_current_settings()

	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")

func _on_back_pressed() -> void:
	print("[SettingsMenuUI] Back pressed")
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")

	if not _has_unsaved_changes:
		if get_parent() is MainMenuUI:
			(get_parent() as MainMenuUI).on_back_to_main_menu()
		return

	var dialog := ConfirmationDialog.new()
	dialog.title = tr("UI_SETTINGS_CANCEL_TITLE")
	dialog.dialog_text = tr("UI_SETTINGS_CANCEL_PROMPT")
	dialog.theme = theme

	dialog.confirmed.connect(func() -> void:
		SettingsManager.load_settings()
		SettingsManager.apply_video_settings()
		_load_current_settings()

		if get_parent() is MainMenuUI:
			(get_parent() as MainMenuUI).on_back_to_main_menu()
		dialog.queue_free()
	)

	dialog.canceled.connect(dialog.queue_free)

	add_child(dialog)
	dialog.popup_centered()
