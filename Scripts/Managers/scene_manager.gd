extends Node
## Global (autoload) scene manager. Responsible for loading and switching
## game scenes based on the game state.

## Raised before a new scene starts loading.
signal scene_load_started(scene_name: String)
## Raised after a new scene has finished loading.
signal scene_load_completed(scene_name: String)

var _scene_paths: Dictionary[GameEnums.GameState, String] = {
	GameEnums.GameState.MAIN_MENU: "res://Scenes/UI/MainMenu.tscn",
	GameEnums.GameState.MAP: "res://Scenes/UI/CampaignMapScene.tscn",
	GameEnums.GameState.BATTLE: "res://Scenes/Battle/BattleScene.tscn",
	GameEnums.GameState.SHOP: "res://Scenes/UI/ShopScene.tscn",
	GameEnums.GameState.GAME_OVER: "res://Scenes/UI/GameOverScreen.tscn",
	GameEnums.GameState.VICTORY: "res://Scenes/UI/VictoryScreen.tscn",
}

var _current_scene: Node
var _is_transitioning: bool = false

## Initializes the scene manager when added to the tree. Locates the
## currently active scene.
func _ready() -> void:
	_is_transitioning = false

	var root: Window = get_tree().root
	_current_scene = root.get_child(root.get_child_count() - 1)

	print("[SceneManager] Initialized")

## Starts loading the scene that corresponds to the given game state.
func load_scene(state: GameEnums.GameState) -> void:
	if not _scene_paths.has(state):
		printerr("[SceneManager] No scene path defined for state: %s" % GameEnums.GameState.keys()[state])
		return
	load_scene_by_path(_scene_paths[state])

## Begins loading the scene at the given path. The switch happens on a deferred call.
func load_scene_by_path(scene_path: String) -> void:
	if _is_transitioning:
		printerr("[SceneManager] Scene transition already in progress")
		return

	_deferred_scene_change.call_deferred(scene_path)

## Deferred method that safely replaces the current scene with a new one.
## Removes the old scene and adds the new one to the tree root.
func _deferred_scene_change(scene_path: String) -> void:
	_is_transitioning = true
	scene_load_started.emit(scene_path)

	if _current_scene != null:
		_current_scene.queue_free()

	var new_scene_resource: PackedScene = load(scene_path)
	if new_scene_resource == null:
		printerr("[SceneManager] Failed to load scene: %s" % scene_path)
		_is_transitioning = false
		return

	_current_scene = new_scene_resource.instantiate()
	get_tree().root.add_child(_current_scene)
	get_tree().current_scene = _current_scene

	_is_transitioning = false
	scene_load_completed.emit(scene_path)

	print("[SceneManager] Scene loaded: %s" % scene_path)

## Reloads the currently active scene.
func reload_current_scene() -> void:
	if _current_scene == null:
		return

	var scene_path: String = _current_scene.scene_file_path
	if not scene_path.is_empty():
		load_scene_by_path(scene_path)

## Navigates to the main menu.
func go_to_main_menu() -> void:
	GameStateManager.return_to_main_menu()
	load_scene(GameEnums.GameState.MAIN_MENU)

## Starts a new game and navigates to the campaign map.
func start_new_game() -> void:
	GameStateManager.start_new_game()
	load_scene(GameEnums.GameState.MAP)

## Loads a save file and resumes the game on the campaign map.
func continue_game() -> void:
	var save_data: SaveData = SaveSystem.load_game()
	if save_data != null:
		GameStateManager.load_game(save_data)
		load_scene(GameEnums.GameState.MAP)
	else:
		printerr("[SceneManager] No save data to continue from")

## Instantiates the scene at the given path as an overlay on top of the
## currently active scene (e.g. the shop or game-over screen over the battle
## scene), rather than replacing it outright.
func show_overlay(scene_path: String) -> void:
	var new_scene_resource: PackedScene = load(scene_path)
	if new_scene_resource == null:
		return

	var overlay: Control = new_scene_resource.instantiate()
	# Make sure the overlay renders on top of everything.
	overlay.z_index = 100

	# Add it to the current scene (BattleScene).
	if _current_scene != null and is_instance_valid(_current_scene):
		var canvas: CanvasLayer = _current_scene.get_node_or_null("CanvasLayer")
		if canvas != null:
			canvas.add_child(overlay)
			return

	get_tree().root.add_child(overlay)

## Navigates to the campaign map — the hub between levels, from which the
## shop and the next level selection are accessible.
func go_to_map() -> void:
	GameStateManager.change_state(GameEnums.GameState.MAP)
	load_scene(GameEnums.GameState.MAP)

## Navigates to the shop scene.
func go_to_shop() -> void:
	GameStateManager.change_state(GameEnums.GameState.SHOP)
	show_overlay("res://Scenes/UI/ShopScene.tscn")

## Shows the "Game Over" screen after a defeat. Unlike a true
## GameStateManager.end_game(), a defeat no longer ends the session:
## progress is kept, and the player heals up and returns to the campaign
## map from the Game Over screen — so only the display state changes here,
## GameStateManager.is_game_active stays true.
func game_over() -> void:
	GameStateManager.change_state(GameEnums.GameState.GAME_OVER)
	show_overlay("res://Scenes/UI/GameOverScreen.tscn")

## Shows the "Victory" screen after defeating the Necromancer. Unlike
## game_over(), this is a true GameStateManager.end_game(true) — the run is
## over; VictoryUI's own buttons start a new game or return to the main menu.
func go_to_victory() -> void:
	GameStateManager.end_game(true)
	show_overlay("res://Scenes/UI/VictoryScreen.tscn")
