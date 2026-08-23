extends Control
## UI for the victory screen. Displays final stats when the game is
## completed successfully.

var _final_wave_label: Label
var _total_coins_label: Label
var _total_enemies_label: Label
var _survival_time_label: Label
var _main_menu_button: Button
var _new_game_button: Button

## Initializes references to labels and navigation buttons, subscribes to
## their events, and displays the victory stats.
func _ready() -> void:
	_final_wave_label = get_node("CenterContainer/VBoxContainer/StatsContainer/WaveLabel")
	_total_coins_label = get_node("CenterContainer/VBoxContainer/StatsContainer/CoinsLabel")
	_total_enemies_label = get_node("CenterContainer/VBoxContainer/StatsContainer/EnemiesLabel")
	_survival_time_label = get_node("CenterContainer/VBoxContainer/StatsContainer/TimeLabel")
	_main_menu_button = get_node("CenterContainer/VBoxContainer/ButtonsContainer/MainMenuButton")
	_new_game_button = get_node("CenterContainer/VBoxContainer/ButtonsContainer/NewGameButton")

	_main_menu_button.pressed.connect(_on_main_menu_pressed)
	_new_game_button.pressed.connect(_on_new_game_pressed)

	_main_menu_button.text = "UI_VICTORY_MAIN_MENU"
	_new_game_button.text = "UI_MAIN_MENU_NEW_GAME"
	get_node("CenterContainer/VBoxContainer/TitleLabel").text = "UI_VICTORY_TITLE"

	_display_stats()

	print("[VictoryUI] Initialized")

## Fills in the wave/coins/enemies-defeated stat labels.
func _display_stats() -> void:
	var save_data: SaveData = GameStateManager.current_save

	if save_data != null:
		var final_wave: int = save_data.current_wave
		var total_coins: int = save_data.coins
		var enemies_defeated: int = _calculate_enemies_defeated(final_wave)

		_final_wave_label.text = "%s %d" % [tr("UI_GAMEOVER_WAVE_REACHED"), final_wave]
		_total_coins_label.text = "%s %d" % [tr("UI_GAMEOVER_COINS"), total_coins]
		_total_enemies_label.text = "%s %d" % [tr("UI_GAMEOVER_ENEMIES_DEFEATED"), enemies_defeated]
		_survival_time_label.text = tr("UI_VICTORY_TITLE")

		print("[VictoryUI] Victory! Wave: %d, Coins: %d, Enemies: %d" % [final_wave, total_coins, enemies_defeated])
	else:
		_final_wave_label.text = "%s 1" % tr("UI_GAMEOVER_WAVE_REACHED")
		_total_coins_label.text = "%s 0" % tr("UI_GAMEOVER_COINS")
		_total_enemies_label.text = "%s 0" % tr("UI_GAMEOVER_ENEMIES_DEFEATED")
		_survival_time_label.text = tr("UI_VICTORY_TITLE")

## Rough display-only estimate of enemies defeated, derived from the wave reached.
static func _calculate_enemies_defeated(wave: int) -> int:
	return wave * 4

func _on_main_menu_pressed() -> void:
	print("[VictoryUI] Returning to main menu")
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")
	SaveSystem.delete_save()
	SceneManager.go_to_main_menu()

func _on_new_game_pressed() -> void:
	print("[VictoryUI] Starting new game")
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")
	SaveSystem.delete_save()
	GameStateManager.start_new_game()
	SceneManager.load_scene(GameEnums.GameState.MAP)
