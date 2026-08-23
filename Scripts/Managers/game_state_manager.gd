extends Node
## Global (autoload) game state manager. Responsible for managing the
## global state, saving data, campaign levels and resources.

## Raised when the global game state changes.
signal state_changed(new_state: GameEnums.GameState)
## Raised when the current wave changes.
signal wave_changed(wave_number: int)
## Raised when the player's coin count changes.
signal coins_changed(new_amount: int)
## Raised when the heroes' stats are updated.
signal hero_stats_changed

## The current global game state. Only ever changed through change_state().
var current_state: GameEnums.GameState = GameEnums.GameState.MAIN_MENU

## The current game save data.
var current_save: SaveData = null

## Indicates whether a game session is currently active.
var is_game_active: bool = false

## The current campaign difficulty rating the player has reached — as of
## the campaign map, this is LevelDefinition.difficulty_rating, not an
## endless wave counter — used as-is by ScalingSystem (rewards, shop
## prices, enemy stats).
var current_wave: int:
	get:
		return current_save.current_wave if current_save != null else 1

## Id of the campaign map level the player is currently on or should
## attempt next.
var current_level_id: String:
	get:
		return current_save.current_level_id if current_save != null else CampaignDatabase.FIRST_LEVEL_ID

## Ids of every campaign level already completed.
var completed_level_ids: Array[String]:
	get:
		return current_save.completed_level_ids if current_save != null else []

## The current number of coins the player has.
var coins: int:
	get:
		return current_save.coins if current_save != null else 0

## Initializes the state manager when it is added to the scene tree.
func _ready() -> void:
	current_state = GameEnums.GameState.MAIN_MENU
	current_save = SaveData.new()

	print("[GameStateManager] Initialized")

## Starts a new game, resetting progress and setting initial values.
func start_new_game() -> void:
	current_save = SaveData.create_new()
	is_game_active = true
	current_state = GameEnums.GameState.MAP

	wave_changed.emit(current_save.current_wave)
	coins_changed.emit(current_save.coins)
	hero_stats_changed.emit()

	InventoryManager.load_from_data(current_save)

	print("[GameStateManager] New game started with dual heroes - Wave 1")

## Loads the game state from the provided save data.
func load_game(save_data: SaveData) -> void:
	current_save = save_data
	is_game_active = true
	current_state = GameEnums.GameState.MAP

	wave_changed.emit(current_save.current_wave)
	coins_changed.emit(current_save.coins)
	hero_stats_changed.emit()

	InventoryManager.load_from_data(current_save)

	print("[GameStateManager] Game loaded - Wave %d" % current_save.current_wave)

## Marks the level selected on the campaign map as the current one, without
## starting it right away (unlike start_level()) — used by the map screen
## before transitioning to the battle scene, which calls start_level()
## itself on start.
func select_level(level_id: String) -> void:
	if current_save == null or level_id.is_empty():
		return

	current_save.current_level_id = level_id

## Begins an attempt at the given campaign map level: records it as the
## current one and carries its difficulty_rating over into current_wave for
## the existing scaling scale (enemy stats, rewards, shop prices).
func start_level(level: LevelDefinition) -> void:
	if current_save == null or level == null:
		return

	current_save.current_level_id = level.id
	current_save.current_wave = level.difficulty_rating

	if current_save.current_wave > current_save.highest_wave:
		current_save.highest_wave = current_save.current_wave

	wave_changed.emit(current_save.current_wave)

	print("[GameStateManager] Started level %s (difficulty %d)" % [level.id, level.difficulty_rating])

## Marks a level as completed. For main-line levels, automatically advances
## progress to the next level on the line (see
## CampaignDatabase.get_next_main_level); completing a branch does not move
## main-line progress.
func complete_level(level_id: String) -> void:
	if current_save == null or level_id.is_empty():
		return

	if not current_save.completed_level_ids.has(level_id):
		current_save.completed_level_ids.append(level_id)

	var next_level: LevelDefinition = CampaignDatabase.get_next_main_level(level_id)
	if next_level != null:
		current_save.current_level_id = next_level.id

	print("[GameStateManager] Completed level %s%s" % [level_id, (", next: " + next_level.id) if next_level != null else ""])

## Adds the given number of coins to the current save.
func add_coins(amount: int) -> void:
	if current_save == null or amount <= 0:
		return

	current_save.coins += amount
	coins_changed.emit(current_save.coins)

	print("[GameStateManager] Added %d coins. Total: %d" % [amount, current_save.coins])

## Spends the given number of coins, if there are enough available.
## Returns true if the coins were successfully spent.
func spend_coins(amount: int) -> bool:
	if current_save == null or amount <= 0 or current_save.coins < amount:
		return false

	current_save.coins -= amount
	coins_changed.emit(current_save.coins)

	print("[GameStateManager] Spent %d coins. Remaining: %d" % [amount, current_save.coins])
	return true

## Updates the base stats of both heroes in the save data.
func update_hero_stats(
	mage_health: int, mage_max_health: int, mage_damage: int, mage_defense: int,
	warrior_health: int, warrior_max_health: int, warrior_damage: int, warrior_defense: int,
) -> void:
	if current_save == null:
		return

	current_save.mage_health = mage_health
	current_save.mage_max_health = mage_max_health
	current_save.mage_damage = mage_damage
	current_save.mage_defense = mage_defense

	current_save.warrior_health = warrior_health
	current_save.warrior_max_health = warrior_max_health
	current_save.warrior_damage = warrior_damage
	current_save.warrior_defense = warrior_defense

	hero_stats_changed.emit()

## Fully restores both heroes' health in the current save. Used when the
## player exits to the campaign map after a defeat: the party keeps its
## progress but starts the next attempt at full health rather than being
## permanently punished for the loss.
func heal_party_to_full() -> void:
	if current_save == null:
		return

	current_save.mage_health = current_save.mage_max_health
	current_save.warrior_health = current_save.warrior_max_health

	hero_stats_changed.emit()

	print("[GameStateManager] Party healed to full")

## Changes the current global game state and emits state_changed if it
## actually changed. Prints on every call, matching the public ChangeState
## API — internal state transitions that shouldn't log this way use
## _set_current_state() directly instead.
func change_state(new_state: GameEnums.GameState) -> void:
	_set_current_state(new_state)
	print("[GameStateManager] State changed to %s" % GameEnums.GameState.keys()[new_state])

## Assigns current_state and emits state_changed, but only if the state
## actually changed — mirrors the private CurrentState property setter from
## the original C# implementation.
func _set_current_state(new_state: GameEnums.GameState) -> void:
	if current_state == new_state:
		return

	current_state = new_state
	state_changed.emit(new_state)

## Ends the current game, transitioning it into a victory or defeat state.
func end_game(is_victory: bool) -> void:
	is_game_active = false
	_set_current_state(GameEnums.GameState.VICTORY if is_victory else GameEnums.GameState.GAME_OVER)

	print("[GameStateManager] Game ended - %s" % ("Victory" if is_victory else "Defeat"))

## Returns the game to the main menu, resetting the active session.
func return_to_main_menu() -> void:
	is_game_active = false
	current_save = null
	_set_current_state(GameEnums.GameState.MAIN_MENU)

	print("[GameStateManager] Returned to main menu")
