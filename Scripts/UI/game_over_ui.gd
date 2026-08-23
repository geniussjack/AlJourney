extends Control
## UI for the game over screen. Displays the run's stats and a single
## "Exit" button that returns to the campaign map — a defeat no longer
## wipes progress or ends the session (see SceneManager.game_over()).

var _wave_reached_label: Label
var _coins_collected_label: Label
var _enemies_defeated_label: Label
var _exit_button: Button

## Sets up references to labels and buttons, subscribes to press events,
## and displays the stats.
func _ready() -> void:
	_wave_reached_label = get_node("CenterContainer/VBoxContainer/StatsContainer/WaveLabel")
	_coins_collected_label = get_node("CenterContainer/VBoxContainer/StatsContainer/CoinsLabel")
	_enemies_defeated_label = get_node("CenterContainer/VBoxContainer/StatsContainer/EnemiesLabel")
	_exit_button = get_node("CenterContainer/VBoxContainer/ButtonsContainer/ExitButton")

	_exit_button.pressed.connect(_on_exit_pressed)

	_exit_button.text = tr("UI_GAMEOVER_EXIT")
	get_node("CenterContainer/VBoxContainer/Title").text = tr("UI_GAMEOVER_TITLE")
	get_node("CenterContainer/VBoxContainer/Subtitle").text = tr("UI_GAMEOVER_SUBTITLE")
	get_node("CenterContainer/VBoxContainer/StatsContainer/StatsTitle").text = tr("UI_GAMEOVER_STATS_TITLE")

	_display_stats()

	print("[GameOverUI] Initialized")

## Fills in the wave/coins/enemies-defeated stat labels.
func _display_stats() -> void:
	var save_data: SaveData = GameStateManager.current_save

	if save_data != null:
		var wave_reached: int = save_data.current_wave
		var coins_collected: int = save_data.coins
		var enemies_defeated: int = _calculate_enemies_defeated(wave_reached)

		_wave_reached_label.text = "%s %d" % [tr("UI_GAMEOVER_WAVE_REACHED"), wave_reached]
		_coins_collected_label.text = "%s %d" % [tr("UI_GAMEOVER_COINS"), coins_collected]
		_enemies_defeated_label.text = "%s %d" % [tr("UI_GAMEOVER_ENEMIES_DEFEATED"), enemies_defeated]

		print("[GameOverUI] Stats - Wave: %d, Coins: %d, Enemies: %d" % [wave_reached, coins_collected, enemies_defeated])
	else:
		_wave_reached_label.text = "%s 1" % tr("UI_GAMEOVER_WAVE_REACHED")
		_coins_collected_label.text = "%s 0" % tr("UI_GAMEOVER_COINS")
		_enemies_defeated_label.text = "%s 0" % tr("UI_GAMEOVER_ENEMIES_DEFEATED")

## Rough display-only estimate of enemies defeated, derived from the wave reached.
static func _calculate_enemies_defeated(wave: int) -> int:
	return wave * 4

## Exits the defeated battle back to the campaign map. Progress is kept —
## no save is deleted — and the party is healed to full so the player can
## immediately retry or pick a different level.
func _on_exit_pressed() -> void:
	print("[GameOverUI] Exiting to campaign map after defeat")
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")
	GameStateManager.heal_party_to_full()
	SceneManager.go_to_map()
