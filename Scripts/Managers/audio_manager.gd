extends Node
## Global (autoload) audio system manager. Responsible for playing music
## and sound effects, and managing their volume.

const SFX_POOL_SIZE: int = 8

var _music_player: AudioStreamPlayer
var _sfx_players: Array[AudioStreamPlayer] = []
var _missing_resource_warnings: Dictionary[String, bool] = {}

## Overall volume for all sounds in the game. Ranges from 0.0 to 1.0.
var master_volume: float = 1.0:
	set(value):
		master_volume = clampf(value, 0.0, 1.0)
		_apply_bus_volume("Master", master_volume)

## Background music volume. Ranges from 0.0 to 1.0.
var music_volume: float = 0.7:
	set(value):
		music_volume = clampf(value, 0.0, 1.0)
		_apply_bus_volume("Music", music_volume)

## Sound effects volume. Ranges from 0.0 to 1.0.
var sfx_volume: float = 0.8:
	set(value):
		sfx_volume = clampf(value, 0.0, 1.0)
		_apply_bus_volume("SFX", sfx_volume)

## Applies a linear volume value to the given Godot audio bus, converting it
## to decibels. Does nothing if no bus with that name is found (e.g. with a
## custom audio configuration).
func _apply_bus_volume(bus_name: String, linear_volume: float) -> void:
	var bus_index: int = AudioServer.get_bus_index(bus_name)
	if bus_index >= 0:
		AudioServer.set_bus_volume_db(bus_index, linear_to_db(linear_volume))

## Initializes the music and sound effect audio players when added to the
## scene tree. Sets up the sound player pools.
func _ready() -> void:
	_music_player = AudioStreamPlayer.new()
	_music_player.name = "MusicPlayer"
	_music_player.bus = "Music"
	add_child(_music_player)

	_sfx_players = []
	for i: int in range(SFX_POOL_SIZE):
		var sfx_player := AudioStreamPlayer.new()
		sfx_player.name = "SFXPlayer_%d" % i
		sfx_player.bus = "SFX"
		add_child(sfx_player)
		_sfx_players.append(sfx_player)

	# Initialize bus volumes with the values set by the field initializers.
	_apply_bus_volume("Master", master_volume)
	_apply_bus_volume("Music", music_volume)
	_apply_bus_volume("SFX", sfx_volume)

	get_tree().node_added.connect(_on_node_added)
	_hook_existing_nodes(get_tree().root)

	print("[AudioManager] Initialized")

## Plays background music from the given resource path.
func play_music(music_path: String, loop: bool = true) -> void:
	try_play_music(music_path, loop)

## Attempts to load and play background music. Prints a warning if the
## resource isn't found.
## Returns true if the music was successfully loaded and started playing.
func try_play_music(music_path: String, loop: bool = true) -> bool:
	var stream: AudioStream = load(music_path)
	if stream == null:
		_warn_missing_resource_once("music", music_path)
		return false

	_music_player.stream = stream

	if stream is AudioStreamOggVorbis:
		(stream as AudioStreamOggVorbis).loop = loop
	elif stream is AudioStreamWAV:
		(stream as AudioStreamWAV).loop_mode = AudioStreamWAV.LOOP_FORWARD if loop else AudioStreamWAV.LOOP_DISABLED
	elif stream is AudioStreamMP3:
		(stream as AudioStreamMP3).loop = loop

	_music_player.play()
	print("[AudioManager] Playing music: %s" % music_path)
	return true

## Stops the currently playing background music.
func stop_music() -> void:
	_music_player.stop()

## Plays a sound effect from the given path.
## pitch_variation: random pitch variation for sound variety.
func play_sfx(sfx_path: String, pitch_variation: float = 0.0) -> void:
	try_play_sfx(sfx_path, pitch_variation)

## Attempts to find a free player and play the sound effect.
## Returns true if the effect was found and started playing.
func try_play_sfx(sfx_path: String, pitch_variation: float = 0.0) -> bool:
	if not ResourceLoader.exists(sfx_path):
		_warn_missing_resource_once("sfx", sfx_path)
		return false

	var stream: AudioStream = load(sfx_path)
	if stream == null:
		return false

	var available_player: AudioStreamPlayer = null
	for player: AudioStreamPlayer in _sfx_players:
		if not player.playing:
			available_player = player
			break

	if available_player == null:
		available_player = _sfx_players[0]

	available_player.stream = stream
	available_player.volume_db = 0.0  # Reset local volume, Bus handles the global volume.

	available_player.pitch_scale = 1.0 + ((randf() * pitch_variation * 2.0) - pitch_variation) if pitch_variation > 0.0 else 1.0

	available_player.play()
	return true

## Plays the sound effect for a correct/allowed choice (a pressed button).
func play_choice_right_sound() -> void:
	play_sfx("res://Resources/Audio/SFX/choice_right_sound.mp3", 0.05)

## Plays the sound effect for a disabled/blocked choice.
func play_choice_error_sound() -> void:
	play_sfx("res://Resources/Audio/SFX/choice_error_sound.mp3", 0.05)

## Recursively hooks up choice sounds to every existing button in the tree.
func _hook_existing_nodes(parent: Node) -> void:
	_on_node_added(parent)
	for child: Node in parent.get_children():
		_hook_existing_nodes(child)

## Hooks up choice sounds to a newly added button: a normal click plays the
## "right" sound, a click on a disabled button plays the "error" sound.
func _on_node_added(node: Node) -> void:
	if not (node is BaseButton):
		return

	var button: BaseButton = node
	# To avoid multiple connections if somehow called twice.
	if button.pressed.is_connected(play_choice_right_sound):
		return

	button.pressed.connect(play_choice_right_sound)
	button.gui_input.connect(func(event: InputEvent) -> void:
		if button.disabled and event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
			play_choice_error_sound()
	)

## Plays the swap sound effect.
func play_swap_sound() -> void:
	play_sfx("res://Resources/Audio/SFX/swap.wav", 0.1)

## Plays the match sound effect.
func play_match_sound() -> void:
	play_sfx("res://Resources/Audio/SFX/match.wav", 0.15)

## Plays the attack sound effect.
func play_attack_sound() -> void:
	play_sfx("res://Resources/Audio/SFX/attack.wav", 0.1)

## Plays the hit sound effect.
func play_hit_sound() -> void:
	play_sfx("res://Resources/Audio/SFX/hit.wav", 0.1)

## Plays the new-game sound effect.
func play_new_game_sound() -> void:
	play_sfx("res://Resources/Audio/SFX/new_game.wav")

## Smoothly fades the current music's volume down to silence, then stops it.
## duration: the fade duration, in seconds.
func fade_out_music(duration: float = 1.0) -> void:
	if _music_player == null or not _music_player.playing:
		return

	var tween: Tween = create_tween()
	tween.tween_property(_music_player, "volume_db", -80.0, duration)
	tween.tween_callback(_music_player.stop)

	print("[AudioManager] Fading out music over %ss" % duration)

## Smoothly raises the music's volume from silence up to its target level.
## duration: the fade-in duration, in seconds.
func fade_in_music(duration: float = 1.0) -> void:
	if _music_player == null or not _music_player.playing:
		return

	_music_player.volume_db = -80.0

	var tween: Tween = create_tween()
	tween.tween_property(_music_player, "volume_db", 0.0, duration)

	print("[AudioManager] Fading in music over %ss" % duration)

## Performs a smooth transition between the current music and a new track.
func crossfade_music(new_music_path: String, duration: float = 1.0, loop: bool = true) -> void:
	if _music_player.playing:
		fade_out_music(duration)

	get_tree().create_timer(duration).timeout.connect(func() -> void:
		play_music(new_music_path, loop)
		fade_in_music(duration)
	)

	print("[AudioManager] Crossfading to: %s" % new_music_path)

## Prints a warning for a missing audio resource at most once per path.
func _warn_missing_resource_once(resource_type: String, resource_path: String) -> void:
	var warning_key: String = "%s:%s" % [resource_type, resource_path]
	if _missing_resource_warnings.has(warning_key):
		return

	_missing_resource_warnings[warning_key] = true
	printerr("[AudioManager] Missing %s resource: %s" % [resource_type, resource_path])
