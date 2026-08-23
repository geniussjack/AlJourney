class_name MainMenuUI
extends Control
## UI for the game's main menu. Handles navigation between sections:
## continue/new game, settings, credits and quitting the game.

var _new_game_button: Button
var _continue_button: Button
var _settings_button: Button
var _credits_button: Button
var _quit_button: Button

var _main_menu_panel: Control
var _settings_panel: Control
var _credits_panel: Control

static var _has_played_game_start: bool = false

## Initializes references to buttons and panels, subscribes to press
## events, and shows the main menu screen.
func _ready() -> void:
	_main_menu_panel = get_node("MainMenuPanel")

	_new_game_button = get_node("MainMenuPanel/VBoxContainer/NewGameButton")
	_continue_button = get_node("MainMenuPanel/VBoxContainer/ContinueButton")
	_settings_button = get_node("MainMenuPanel/VBoxContainer/SettingsButton")
	_credits_button = get_node("MainMenuPanel/VBoxContainer/CreditsButton")
	_quit_button = get_node("MainMenuPanel/VBoxContainer/QuitButton")

	_settings_panel = get_node("SettingsPanel")
	_credits_panel = get_node("CreditsPanel")

	_new_game_button.pressed.connect(_on_new_game_pressed)
	_continue_button.pressed.connect(_on_continue_pressed)
	_settings_button.pressed.connect(_on_settings_pressed)
	_credits_button.pressed.connect(_on_credits_pressed)
	_quit_button.pressed.connect(_on_quit_pressed)

	_new_game_button.text = "UI_MAIN_MENU_NEW_GAME"
	_continue_button.text = "UI_MAIN_MENU_CONTINUE"
	_settings_button.text = "UI_MAIN_MENU_SETTINGS"
	_credits_button.text = "UI_MAIN_MENU_CREDITS"
	_quit_button.text = "UI_MAIN_MENU_QUIT"

	_show_main_menu()

	if not _has_played_game_start:
		AudioManager.play_music("res://Resources/Audio/Music/game_start.mp3", false)
		_has_played_game_start = true
	else:
		AudioManager.stop_music()

	print("[MainMenuUI] Initialized")

## Shows the home screen and hides every secondary panel.
func _show_main_menu() -> void:
	_main_menu_panel.show()
	_settings_panel.hide()
	_credits_panel.hide()
	_update_continue_button_state()

## Enables/disables and dims the Continue button based on save existence.
func _update_continue_button_state() -> void:
	var has_save: bool = SaveSystem.save_file_exists()
	_continue_button.disabled = not has_save
	_continue_button.modulate = Color.WHITE if has_save else Color(1, 1, 1, 0.45)

## Handles the "New Game" button. If a save already exists, asks for
## confirmation first since starting over erases it; otherwise starts right away.
func _on_new_game_pressed() -> void:
	print("[MainMenuUI] New game pressed")

	if SaveSystem.save_file_exists():
		var dialog := ConfirmationDialog.new()
		dialog.title = tr("UI_MAIN_MENU_NEW_GAME_CONFIRM_TITLE")
		dialog.dialog_text = tr("UI_MAIN_MENU_NEW_GAME_CONFIRM_TEXT")
		dialog.theme = theme

		dialog.confirmed.connect(func() -> void:
			dialog.queue_free()
			_begin_new_game()
		)
		dialog.canceled.connect(dialog.queue_free)

		add_child(dialog)
		dialog.popup_centered()
		return

	_begin_new_game()

## Resets progress and starts a new game, playing the intro cutscene before
## handing off to the campaign map.
func _begin_new_game() -> void:
	AudioManager.play_new_game_sound()
	AudioManager.play_music("res://Resources/Audio/Music/main_theme.mp3", true)
	SaveSystem.delete_save()
	GameStateManager.start_new_game()

	CutscenePlayer.play(self, CutsceneDatabase.new_game_intro, func() -> void: SceneManager.load_scene(GameEnums.GameState.MAP))

## Loads the existing save and resumes on the campaign map.
func _on_continue_pressed() -> void:
	if SaveSystem.save_file_exists():
		AudioManager.play_music("res://Resources/Audio/Music/main_theme.mp3", true)
		print("[MainMenuUI] Save found - continuing game")
		SceneManager.continue_game()
	else:
		printerr("[MainMenuUI] Continue button pressed but no save found!")

## Switches from the home screen to the settings panel.
func _on_settings_pressed() -> void:
	print("[MainMenuUI] Settings pressed")
	_main_menu_panel.hide()
	_settings_panel.show()

## Switches from the home screen to the credits panel.
func _on_credits_pressed() -> void:
	print("[MainMenuUI] Credits pressed")
	_main_menu_panel.hide()
	_credits_panel.show()

## Quits the application.
func _on_quit_pressed() -> void:
	print("[MainMenuUI] Quit pressed")
	get_tree().quit()

## Hides every secondary panel and returns the user to the main menu's home screen.
func on_back_to_main_menu() -> void:
	print("[MainMenuUI] Back to main menu")
	_show_main_menu()
