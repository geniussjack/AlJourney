extends Node
## Global (autoload) settings manager. Responsible for saving, loading and
## applying the game's video and audio settings.

## Raised whenever any settings are changed and applied.
signal settings_changed

const SETTINGS_PATH: String = "user://settings.cfg"

var _resolution: Vector2i = Vector2i(1920, 1080)

## The current screen resolution.
var resolution: Vector2i:
	get:
		return _resolution

## Window mode (0 = Fullscreen, 1 = Borderless, 2 = Windowed).
var window_mode: int = 0

## The game's current language.
var language: String = "ru" if OS.get_locale_language() == "ru" else "en"

## The maximum frames per second.
var max_fps: int = 60

## The overall sound volume.
var master_volume: float = 1.0

## The background music volume.
var music_volume: float = 0.7

## The sound effects volume.
var sfx_volume: float = 0.8

## Initializes the settings manager when added to the scene tree. Loads and
## applies the saved settings.
func _ready() -> void:
	load_settings()
	_apply_settings()

	print("[SettingsManager] Initialized")

## Sets a new screen resolution.
## apply_immediately: if true, video settings are applied immediately.
func set_resolution(new_resolution: Vector2i, apply_immediately: bool = true) -> void:
	_resolution = new_resolution

	if apply_immediately:
		apply_video_settings()

	print("[SettingsManager] Resolution set to %s" % _resolution)

## Sets the window mode.
## mode: 0 - Fullscreen, 1 - Borderless, 2 - Windowed.
func set_window_mode(mode: int, apply_immediately: bool = true) -> void:
	window_mode = mode

	if apply_immediately:
		apply_video_settings()

	print("[SettingsManager] WindowMode: %d" % mode)

## Changes the game's language.
func set_language(lang: String, apply_immediately: bool = true) -> void:
	language = lang
	if apply_immediately:
		TranslationServer.set_locale(lang)
	print("[SettingsManager] Language: %s" % lang)

## Sets the maximum frame rate.
## apply_immediately: if true, video settings are applied immediately.
func set_max_fps(fps: int, apply_immediately: bool = true) -> void:
	max_fps = fps

	if apply_immediately:
		apply_video_settings()

	print("[SettingsManager] Max FPS: %d" % fps)

## Sets the overall volume and forwards it to AudioManager.
func set_master_volume(volume: float) -> void:
	master_volume = clampf(volume, 0.0, 1.0)
	AudioManager.master_volume = master_volume
	print("[SettingsManager] Master volume: %.2f" % master_volume)

## Sets the background music volume and forwards it to AudioManager.
func set_music_volume(volume: float) -> void:
	music_volume = clampf(volume, 0.0, 1.0)
	AudioManager.music_volume = music_volume
	print("[SettingsManager] Music volume: %.2f" % music_volume)

## Sets the sound effects volume and forwards it to AudioManager.
func set_sfx_volume(volume: float) -> void:
	sfx_volume = clampf(volume, 0.0, 1.0)
	AudioManager.sfx_volume = sfx_volume
	print("[SettingsManager] SFX volume: %.2f" % sfx_volume)

## Applies the current video settings to the engine and application window.
func apply_video_settings() -> void:
	var window: Window = get_window()

	if window_mode == 0:  # Fullscreen
		window.mode = Window.MODE_EXCLUSIVE_FULLSCREEN
		window.borderless = false
	elif window_mode == 1:  # Borderless
		var screen_id: int = window.current_screen
		var screen_pos: Vector2i = DisplayServer.screen_get_position(screen_id)
		var screen_size: Vector2i = DisplayServer.screen_get_size(screen_id)
		window.mode = Window.MODE_WINDOWED
		window.borderless = true
		window.size = _resolution
		window.position = screen_pos + ((screen_size - _resolution) / 2)
	else:  # Windowed
		var screen_id: int = window.current_screen
		var screen_pos: Vector2i = DisplayServer.screen_get_position(screen_id)
		var screen_size: Vector2i = DisplayServer.screen_get_size(screen_id)
		window.mode = Window.MODE_WINDOWED
		window.borderless = false

		var new_size: Vector2i = _resolution
		if new_size == screen_size:
			new_size.y -= 40  # Prevent Windows from auto-maximizing.
		window.size = new_size

		var centered: Vector2i = screen_pos + ((screen_size - new_size) / 2)
		if centered.y <= screen_pos.y:
			centered.y = screen_pos.y + 40

		window.position = centered

	Engine.max_fps = max_fps

	settings_changed.emit()
	print("[SettingsManager] Video settings applied")

## Applies every loaded setting: video, language and audio bus volumes.
func _apply_settings() -> void:
	apply_video_settings()
	TranslationServer.set_locale(language)
	AudioManager.master_volume = master_volume
	AudioManager.music_volume = music_volume
	AudioManager.sfx_volume = sfx_volume

## Saves the current settings to a configuration file on disk.
func save_settings() -> void:
	var config := ConfigFile.new()

	config.set_value("video", "resolution_x", _resolution.x)
	config.set_value("video", "resolution_y", _resolution.y)
	config.set_value("video", "window_mode", window_mode)
	config.set_value("video", "language", language)
	config.set_value("video", "max_fps", max_fps)

	config.set_value("audio", "master_volume", master_volume)
	config.set_value("audio", "music_volume", music_volume)
	config.set_value("audio", "sfx_volume", sfx_volume)

	var err: Error = config.save(SETTINGS_PATH)
	if err != OK:
		printerr("[SettingsManager] Failed to save settings: %s" % error_string(err))
	else:
		print("[SettingsManager] Settings saved")

## Loads settings from the configuration file on disk, if it exists.
func load_settings() -> void:
	var config := ConfigFile.new()
	var err: Error = config.load(SETTINGS_PATH)

	if err != OK:
		print("[SettingsManager] No settings file found, using defaults")
		return

	_resolution = Vector2i(
		int(config.get_value("video", "resolution_x", 1920)),
		int(config.get_value("video", "resolution_y", 1080))
	)
	window_mode = int(config.get_value("video", "window_mode", 0))
	language = config.get_value("video", "language", "ru" if OS.get_locale_language() == "ru" else "en")
	max_fps = int(config.get_value("video", "max_fps", 60))

	master_volume = float(config.get_value("audio", "master_volume", 1.0))
	music_volume = float(config.get_value("audio", "music_volume", 0.7))
	sfx_volume = float(config.get_value("audio", "sfx_volume", 0.8))

	print("[SettingsManager] Settings loaded")

## Resets all settings to their default values, applies and saves them.
func reset_to_defaults() -> void:
	_resolution = Vector2i(1920, 1080)
	window_mode = 0
	language = "ru" if OS.get_locale_language() == "ru" else "en"
	max_fps = 60
	master_volume = 1.0
	music_volume = 0.7
	sfx_volume = 0.8

	_apply_settings()
	save_settings()

	print("[SettingsManager] Settings reset to defaults")
