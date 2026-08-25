extends Control
## UI component for the credits screen. Responsible for displaying
## developer and technology credits.

var _back_button: Button
var _credits_text: RichTextLabel

## Initializes UI elements, subscribes to the "Back" button press, and
## fills in the credits text.
func _ready() -> void:
	_back_button = get_node("CreditsMenu/Panel/VBoxContainer/BackButton")
	_credits_text = get_node("CreditsMenu/Panel/VBoxContainer/ScrollContainer/CreditsText")

	_back_button.pressed.connect(_on_back_pressed)

	_setup_credits_content()

	print("[CreditsUI] Initialized")

## Fills in the BBCode credits text.
func _setup_credits_content() -> void:
	_credits_text.bbcode_enabled = true
	_credits_text.text = "[center][b][font_size=32]%s[/font_size][/b]\n\n[font_size=20]%s[/font_size]\n\n[font_size=16]-------------------------[/font_size]\n\n[b]%s[/b]\n\n[b]%s[/b]\n%s\n\n[b]%s[/b]\n%s\n\n[b]%s[/b]\n%s\n\n[font_size=16]-------------------------[/font_size]\n\n[b]%s[/b]\nGodot Engine Team\nCommunity Contributors\n\n[font_size=16]-------------------------[/font_size]\n\n[b]%s[/b]\nGodot Engine 4.7\nGDScript\n\n[font_size=14]%s[/font_size][/center]" % [
		tr("UI_CREDITS_GAME_TITLE"),
		tr("UI_CREDITS_SUBTITLE"),
		tr("UI_CREDITS_TEAM_TITLE"),
		tr("UI_CREDITS_PROGRAMMING"), tr("AUTHOR_NAME"),
		tr("UI_CREDITS_ART"), tr("AUTHOR_NAME"),
		tr("UI_CREDITS_AUDIO"), tr("AUTHOR_NAME"),
		tr("UI_CREDITS_THANKS"),
		tr("UI_CREDITS_BUILT_WITH"),
		tr("UI_CREDITS_COPYRIGHT"),
	]

## Closes the credits panel, returning to whichever parent opened it.
func _on_back_pressed() -> void:
	print("[CreditsUI] Back pressed")

	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")

	if get_parent() is MainMenuUI:
		(get_parent() as MainMenuUI).on_back_to_main_menu()
		return

	SceneManager.go_to_main_menu()
