class_name PauseMenu
extends Control
## UI component for the pause menu. Handles pause menu logic, pausing the
## game, and returning to the main menu. Works together with PauseMenu.tscn.

var _title_label: Label
var _resume_button: Button
var _save_button: Button
var _main_menu_button: Button

## Sets up the resume, save and quit buttons, hides the menu by default,
## and sets the process mode to Always.
func _ready() -> void:
	_title_label = get_node("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/TitleLabel")
	_resume_button = get_node("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ResumeButton")
	_save_button = get_node("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/SaveButton")
	_main_menu_button = get_node("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MainMenuButton")

	_title_label.text = "UI_PAUSE_TITLE"
	_resume_button.text = "UI_PAUSE_RESUME"
	_save_button.text = "UI_PAUSE_SAVE"
	_main_menu_button.text = "UI_PAUSE_MAIN_MENU"

	_resume_button.pressed.connect(_on_resume_pressed)
	_save_button.pressed.connect(_on_save_pressed)
	_main_menu_button.pressed.connect(_on_main_menu_pressed)

	hide()

	process_mode = Node.PROCESS_MODE_ALWAYS

	print("[PauseMenu] Initialized")

## Pressing Esc/Accept toggles the pause state.
func _input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel") or event.is_action_pressed("ui_accept"):
		if visible:
			resume()
		else:
			pause_game()

		get_viewport().set_input_as_handled()

## Pauses the game and shows the menu with a fade-in animation.
func pause_game() -> void:
	show()
	get_tree().paused = true

	modulate = Color(1, 1, 1, 0)
	var tween: Tween = create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.tween_property(self, "modulate:a", 1.0, 0.15)

	print("[PauseMenu] Paused")

## Unpauses the game with a fade-out animation.
func resume() -> void:
	var tween: Tween = create_tween()
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.tween_property(self, "modulate:a", 0.0, 0.15)
	tween.tween_callback(func() -> void:
		hide()
		get_tree().paused = false
		print("[PauseMenu] Resumed")
	)

func _on_resume_pressed() -> void:
	resume()

func _on_save_pressed() -> void:
	SaveSystem.save_game()
	print("[PauseMenu] Game saved")

func _on_main_menu_pressed() -> void:
	get_tree().paused = false
	SceneManager.go_to_main_menu()
