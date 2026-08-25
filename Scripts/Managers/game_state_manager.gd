extends Node
## Global (autoload) game state manager. Responsible for managing the
## global state, saving data, campaign levels and resources.

## Raised when the global game state changes.
signal state_changed(new_state: GameEnums.GameState)
## Raised when the current wave changes.
signal wave_changed(wave_number: int)
## Raised when the player's coin count changes.
signal coins_changed(new_amount: int)
## Raised when a strategic resource's stored amount changes (settlement
## economy — see design document, section 9).
signal strategic_resource_changed(resource: GameEnums.StrategicResource, new_amount: int)
## Raised when a settlement building's level increases.
signal building_upgraded(building: GameEnums.BuildingType, new_level: int)
## Raised when a villager is assigned to or unassigned from gathering a
## strategic resource.
signal worker_assignment_changed(resource: GameEnums.StrategicResource, worker_count: int)
## Raised when the heroes' stats are updated.
signal hero_stats_changed
## Raised when the shared party level increases. Carries the new level —
## if multiple levels were gained from a single XP grant, this only fires
## once, already at the final level.
signal party_leveled_up(new_level: int)
## Raised when the party's third slot is filled or emptied, carrying the
## new active mercenary's key (see MercenarySubclassData.get_key()), or ""
## if the slot is now empty.
signal active_mercenary_changed(key: String)
## Raised when a mercenary's recovery countdown changes, carrying its key
## and the number of battles remaining (0 means available again).
signal mercenary_recovery_changed(key: String, battles_remaining: int)

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

## The party's current shared level.
var party_level: int:
	get:
		return current_save.party_level if current_save != null else 1

var _resource_tick_check_accumulator: float = 0.0

## Initializes the state manager when it is added to the scene tree.
func _ready() -> void:
	current_state = GameEnums.GameState.MAIN_MENU
	current_save = SaveData.new()

	print("[GameStateManager] Initialized")

## Checks for elapsed worker-gathered resource ticks at most once per real
## second — cheap enough to run continuously while the game is open, and
## the same code path handles offline catch-up on load (see
## _apply_elapsed_resource_gains()).
func _process(delta: float) -> void:
	_resource_tick_check_accumulator += delta
	if _resource_tick_check_accumulator < 1.0:
		return

	_resource_tick_check_accumulator = 0.0
	_apply_elapsed_resource_gains()

## Starts a new game, resetting progress and setting initial values.
func start_new_game() -> void:
	current_save = SaveData.create_new()
	current_save.last_resource_tick_unix_time = Time.get_unix_time_from_system()
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
	_apply_elapsed_resource_gains()

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

## Returns the current stored amount of the given strategic resource. A
## resource that has never been gathered reads as 0, not an error.
func get_strategic_resource(resource: GameEnums.StrategicResource) -> int:
	return current_save.strategic_resources.get(resource, 0) if current_save != null else 0

## Maximum amount of any single strategic resource that can be stored,
## based on the Warehouse building's level (see design document, section 9).
func get_storage_cap() -> int:
	var warehouse_level: int = get_building_level(GameEnums.BuildingType.WAREHOUSE)
	return GameConstants.WAREHOUSE_BASE_STORAGE_CAP + ((warehouse_level - 1) * GameConstants.WAREHOUSE_STORAGE_CAP_PER_LEVEL)

## Adds the given amount of a strategic resource to storage, clamped at the
## Warehouse's storage cap (see get_storage_cap()) — any amount that would
## overflow it is simply lost, not carried over.
func add_strategic_resource(resource: GameEnums.StrategicResource, amount: int) -> void:
	if current_save == null or amount <= 0:
		return

	var new_amount: int = mini(get_strategic_resource(resource) + amount, get_storage_cap())
	if new_amount == get_strategic_resource(resource):
		return

	current_save.strategic_resources[resource] = new_amount
	strategic_resource_changed.emit(resource, new_amount)

	print("[GameStateManager] Added %d %s. Total: %d" % [amount, GameEnums.StrategicResource.keys()[resource], new_amount])

## Attempts to spend a combination of strategic resources atomically:
## either every listed cost can be afforded and all are deducted, or
## nothing is spent at all.
## costs: resource type -> amount required.
## Returns true if the resources were successfully spent.
func spend_strategic_resources(costs: Dictionary[GameEnums.StrategicResource, int]) -> bool:
	if current_save == null:
		return false

	for resource: GameEnums.StrategicResource in costs.keys():
		if get_strategic_resource(resource) < costs[resource]:
			return false

	for resource: GameEnums.StrategicResource in costs.keys():
		var new_amount: int = get_strategic_resource(resource) - costs[resource]
		current_save.strategic_resources[resource] = new_amount
		strategic_resource_changed.emit(resource, new_amount)

	print("[GameStateManager] Spent strategic resources: %s" % costs)
	return true

## Returns the current level of the given settlement building. A building
## that has never been upgraded reads as level 1 (buildings start already
## built in a basic form — see SaveData.building_levels).
func get_building_level(building: GameEnums.BuildingType) -> int:
	return current_save.building_levels.get(building, 1) if current_save != null else 1

## Attempts to upgrade the given building by one level: computes its cost
## via BuildingDatabase from the current level, spends the resources
## atomically, and raises the level on success.
## Returns true if the building was successfully upgraded.
func upgrade_building(building: GameEnums.BuildingType) -> bool:
	if current_save == null:
		return false

	var data: BuildingData = BuildingDatabase.get_building(building)
	if data == null:
		return false

	var current_level: int = get_building_level(building)
	if current_level >= data.max_level:
		print("[GameStateManager] %s is already at max level" % GameEnums.BuildingType.keys()[building])
		return false

	var cost: Dictionary[GameEnums.StrategicResource, int] = data.get_upgrade_cost(current_level)
	if not spend_strategic_resources(cost):
		print("[GameStateManager] Cannot afford to upgrade %s" % GameEnums.BuildingType.keys()[building])
		return false

	var new_level: int = current_level + 1
	current_save.building_levels[building] = new_level
	building_upgraded.emit(building, new_level)

	print("[GameStateManager] Upgraded %s to level %d" % [GameEnums.BuildingType.keys()[building], new_level])
	return true

## Total villagers that can be assigned to gather resources or defend the
## settlement at once, based on the Houses building's level.
func get_worker_capacity() -> int:
	var houses_level: int = get_building_level(GameEnums.BuildingType.HOUSES)
	return GameConstants.HOUSES_BASE_WORKER_CAPACITY + ((houses_level - 1) * GameConstants.HOUSES_WORKER_CAPACITY_PER_LEVEL)

## Number of villagers currently assigned to gather the given resource.
func get_assigned_workers(resource: GameEnums.StrategicResource) -> int:
	return current_save.worker_assignments.get(resource, 0) if current_save != null else 0

## Total villagers currently assigned across every resource.
func get_total_assigned_workers() -> int:
	var total: int = 0
	for resource: GameEnums.StrategicResource in GameEnums.StrategicResource.values():
		total += get_assigned_workers(resource)
	return total

## Assigns one more villager to gather the given resource, if capacity allows.
## Returns true if a villager was assigned.
func assign_worker(resource: GameEnums.StrategicResource) -> bool:
	if current_save == null or get_total_assigned_workers() >= get_worker_capacity():
		return false

	var new_count: int = get_assigned_workers(resource) + 1
	current_save.worker_assignments[resource] = new_count
	worker_assignment_changed.emit(resource, new_count)

	print("[GameStateManager] Assigned a worker to %s (now %d)" % [GameEnums.StrategicResource.keys()[resource], new_count])
	return true

## Removes one villager from gathering the given resource, if any are assigned.
## Returns true if a villager was unassigned.
func unassign_worker(resource: GameEnums.StrategicResource) -> bool:
	if current_save == null:
		return false

	var current_count: int = get_assigned_workers(resource)
	if current_count <= 0:
		return false

	var new_count: int = current_count - 1
	current_save.worker_assignments[resource] = new_count
	worker_assignment_changed.emit(resource, new_count)

	print("[GameStateManager] Unassigned a worker from %s (now %d)" % [GameEnums.StrategicResource.keys()[resource], new_count])
	return true

## Whether the given mercenary subclass's unlock condition has been met —
## either the required Barracks level or the required campaign level (see
## MercenarySubclassData). Exactly one of the two conditions is set per
## subclass, so only the relevant one is checked.
func is_mercenary_unlocked(subclass: MercenarySubclassData) -> bool:
	if current_save == null:
		return false

	if subclass.required_barracks_level > 0:
		return get_building_level(GameEnums.BuildingType.BARRACKS) >= subclass.required_barracks_level

	return current_save.current_level_id == subclass.required_level_id or current_save.completed_level_ids.has(subclass.required_level_id)

## Returns every mercenary subclass currently unlocked, in roster order.
func get_unlocked_mercenaries() -> Array[MercenarySubclassData]:
	var result: Array[MercenarySubclassData] = []
	for subclass: MercenarySubclassData in MercenaryDatabase.get_all_subclasses():
		if is_mercenary_unlocked(subclass):
			result.append(subclass)
	return result

## Battles remaining before the given mercenary (by
## MercenarySubclassData.get_key()) is available again after being
## benched. 0 means it isn't in recovery, but doesn't by itself mean
## available — see is_mercenary_available().
func get_battles_until_available(key: String) -> int:
	return current_save.mercenary_recovery.get(key, 0) if current_save != null else 0

## Whether the given mercenary can be set as the active companion right
## now: unlocked and not still recovering from a previous battle.
func is_mercenary_available(subclass: MercenarySubclassData) -> bool:
	return is_mercenary_unlocked(subclass) and get_battles_until_available(subclass.get_key()) <= 0

## The raw key of the mercenary currently filling the party's third slot
## (see MercenarySubclassData.get_key()), or "" if empty.
func get_active_mercenary_key() -> String:
	return current_save.active_mercenary_key if current_save != null else ""

## The subclass currently filling the party's third slot, or null if empty.
func get_active_mercenary() -> MercenarySubclassData:
	if current_save == null or current_save.active_mercenary_key.is_empty():
		return null
	return MercenaryDatabase.get_by_key(current_save.active_mercenary_key)

## Sets the given mercenary as the active companion, filling the party's
## third slot. Returns true on success; fails if the mercenary isn't
## currently available (see is_mercenary_available()).
func set_active_mercenary(subclass: MercenarySubclassData) -> bool:
	if current_save == null or subclass == null or not is_mercenary_available(subclass):
		return false

	current_save.active_mercenary_key = subclass.get_key()
	active_mercenary_changed.emit(current_save.active_mercenary_key)

	print("[GameStateManager] Active mercenary set: %s" % subclass.get_key())
	return true

## Empties the party's third slot without sending the previous companion
## into recovery — unlike on_battle_completed(), which is what actually
## benches a mercenary after a fight. Used for the player choosing to
## leave the slot empty ahead of a battle.
func clear_active_mercenary() -> void:
	if current_save == null or current_save.active_mercenary_key.is_empty():
		return

	current_save.active_mercenary_key = ""
	active_mercenary_changed.emit("")

## Called once per finished battle, win or loss (see design document,
## section 9: "смерть в бою или провал набега даёт только временный
## откат" — a lost battle still advances recovery). Advances every
## recovering mercenary's countdown by one battle, and — if a companion was
## used this battle — sends it into recovery and empties the slot.
func on_battle_completed() -> void:
	if current_save == null:
		return

	var recovery: Dictionary[String, int] = current_save.mercenary_recovery
	for key: String in recovery.keys().duplicate():
		var remaining: int = maxi(0, recovery[key] - 1)
		if remaining <= 0:
			recovery.erase(key)
		else:
			recovery[key] = remaining
		mercenary_recovery_changed.emit(key, remaining)

	if not current_save.active_mercenary_key.is_empty():
		var used_key: String = current_save.active_mercenary_key
		var duration: int = _get_mercenary_recovery_duration()
		recovery[used_key] = duration
		mercenary_recovery_changed.emit(used_key, duration)
		clear_active_mercenary()

## Number of battles a mercenary spends in recovery after being used,
## reduced by Herbalist building upgrades (design document, section 9).
func _get_mercenary_recovery_duration() -> int:
	var herbalist_level: int = get_building_level(GameEnums.BuildingType.HERBALIST)
	return maxi(GameConstants.MERCENARY_MIN_RECOVERY_BATTLES, GameConstants.MERCENARY_BASE_RECOVERY_BATTLES - (herbalist_level - 1))

## Credits resources gathered by assigned workers for every full tick
## (GameConstants.SECONDS_PER_RESOURCE_TICK) of real time elapsed since
## the last check. The same code path handles both continuous ticking
## while the game is running (see _process()) and catching up on
## resources gathered while the player was offline (see load_game()) —
## a long elapsed gap just resolves to many ticks at once.
func _apply_elapsed_resource_gains() -> void:
	if current_save == null:
		return

	var now: int = Time.get_unix_time_from_system()

	if current_save.last_resource_tick_unix_time <= 0:
		current_save.last_resource_tick_unix_time = now
		return

	var elapsed: int = now - current_save.last_resource_tick_unix_time
	var ticks: int = int(float(elapsed) / GameConstants.SECONDS_PER_RESOURCE_TICK)
	if ticks <= 0:
		return

	for resource: GameEnums.StrategicResource in GameEnums.StrategicResource.values():
		var workers: int = get_assigned_workers(resource)
		if workers > 0:
			add_strategic_resource(resource, workers * GameConstants.RESOURCE_PER_WORKER_PER_TICK * ticks)

	current_save.last_resource_tick_unix_time += int(ticks * GameConstants.SECONDS_PER_RESOURCE_TICK)

## Grants the party shared XP, applying as many level-ups as the amount
## covers (capped at GameConstants.PARTY_LEVEL_MAX). Only updates the
## level/XP counters in the save — the caller is responsible for applying
## the resulting per-level stat bonuses to any live Character instances
## (see BattleManager.on_enemy_died), since this manager has no reference
## to the active DualHeroSystem during a battle.
## Returns the number of levels gained.
func add_party_xp(amount: int) -> int:
	if current_save == null or amount <= 0:
		return 0

	current_save.party_xp += amount
	var levels_gained: int = 0

	while current_save.party_level < GameConstants.PARTY_LEVEL_MAX:
		var required: int = _xp_to_next_party_level(current_save.party_level)
		if current_save.party_xp < required:
			break

		current_save.party_xp -= required
		current_save.party_level += 1
		levels_gained += 1

	if levels_gained > 0:
		party_leveled_up.emit(current_save.party_level)
		print("[GameStateManager] Party leveled up to %d (+%d)" % [current_save.party_level, levels_gained])

	return levels_gained

## XP required to advance from the given party level to the next one.
func _xp_to_next_party_level(level: int) -> int:
	return GameConstants.PARTY_LEVEL_BASE_XP + ((level - 1) * GameConstants.PARTY_LEVEL_XP_GROWTH)

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
