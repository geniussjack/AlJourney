extends Node
## Global (autoload) manager for saving and loading progress. Responsible
## for serializing game data to JSON and reading it back from local storage.

## Raised after a save operation completes.
signal save_completed(success: bool)
## Raised after a load operation completes.
signal load_completed(success: bool)

var _save_path: String

## Initializes the singleton and creates the save directory if it doesn't exist.
func _ready() -> void:
	_save_path = GameConstants.SAVE_DIRECTORY + GameConstants.SAVE_FILE_NAME

	if DirAccess.make_dir_recursive_absolute(GameConstants.SAVE_DIRECTORY) == OK:
		print("[SaveSystem] Initialized. Save path: %s" % _save_path)
	else:
		printerr("[SaveSystem] Failed to create save directory: %s" % GameConstants.SAVE_DIRECTORY)

## Saves the current game state to a file. Includes wave progress, hero
## stats and inventory state.
## Returns true if the save completed successfully.
func save_game() -> bool:
	var save_data: SaveData = GameStateManager.current_save
	if save_data == null:
		printerr("[SaveSystem] No active save data to save")
		save_completed.emit(false)
		return false

	InventoryManager.save_to_data(save_data)
	save_data.last_save_time = Time.get_datetime_string_from_system(false, true)

	var json_data: String = JSON.stringify(save_data.to_dict(), "\t")

	var file: FileAccess = FileAccess.open(_save_path, FileAccess.WRITE)
	if file == null:
		printerr("[SaveSystem] Failed to open save file: %s" % error_string(FileAccess.get_open_error()))
		save_completed.emit(false)
		return false

	file.store_string(json_data)
	file.close()

	print("[SaveSystem] Game saved successfully - Wave %d" % save_data.current_wave)
	save_completed.emit(true)
	return true

## Reads and deserializes the save file. If the structure is outdated,
## attempts to migrate it. Validates the integrity of the loaded data.
## Returns the loaded SaveData object, or null on failure.
func load_game() -> SaveData:
	if not FileAccess.file_exists(_save_path):
		print("[SaveSystem] No save file found")
		load_completed.emit(false)
		return null

	var file: FileAccess = FileAccess.open(_save_path, FileAccess.READ)
	if file == null:
		printerr("[SaveSystem] Failed to open save file: %s" % error_string(FileAccess.get_open_error()))
		load_completed.emit(false)
		return null

	var json_data: String = file.get_as_text()
	file.close()

	if json_data.strip_edges().is_empty():
		printerr("[SaveSystem] Save file is empty")
		load_completed.emit(false)
		return null

	var save_data: SaveData = _deserialize_and_migrate(json_data)

	if save_data == null or not _validate_save_data(save_data):
		load_completed.emit(false)
		return null

	print("[SaveSystem] Game loaded successfully - Wave %d" % save_data.current_wave)
	load_completed.emit(true)
	return save_data

## Parses the raw JSON, builds a SaveData from it and migrates it to the
## current schema version if needed.
func _deserialize_and_migrate(json_data: String) -> SaveData:
	var parsed: Variant = JSON.parse_string(json_data)
	if parsed == null or not (parsed is Dictionary):
		printerr("[SaveSystem] JSON deserialization failed: save file may be corrupted")
		return null

	var save_data: SaveData = SaveData.from_dict(parsed)

	if save_data.schema_version != 1:
		print("[SaveSystem] Outdated save schema (v%d), attempting migration" % save_data.schema_version)
		save_data = SaveData.migrate(save_data)

		if save_data == null:
			printerr("[SaveSystem] Save migration failed")
			return null

	return save_data

## Checks whether the save file physically exists.
func save_file_exists() -> bool:
	return FileAccess.file_exists(_save_path)

## Deletes the current save file, without a way to recover it.
func delete_save() -> bool:
	if not FileAccess.file_exists(_save_path):
		print("[SaveSystem] No save file to delete")
		return true

	var err: Error = DirAccess.remove_absolute(_save_path)
	if err != OK:
		printerr("[SaveSystem] Failed to delete save: %s" % error_string(err))
		return false

	print("[SaveSystem] Save file deleted")
	return true

## Validates that the loaded save data is internally consistent.
func _validate_save_data(data: SaveData) -> bool:
	if not _validate_progression(data) or not _validate_hero_stats(data):
		return false

	print("[SaveSystem] Save data validation passed")
	return true

## Validates wave progress and coin count.
func _validate_progression(data: SaveData) -> bool:
	if data.current_wave < 1 or data.highest_wave < 1 or data.highest_wave < data.current_wave:
		printerr("[SaveSystem] Validation failed: Invalid wave progress (Current:%d, Highest:%d)" % [data.current_wave, data.highest_wave])
		return false

	if data.coins < 0:
		printerr("[SaveSystem] Validation failed: Invalid coins (%d)" % data.coins)
		return false

	return true

## Validates both heroes' stat ranges.
func _validate_hero_stats(data: SaveData) -> bool:
	return (
		_validate_hero(data.mage_health, data.mage_max_health, data.mage_damage, data.mage_defense, "Mage")
		and _validate_hero(data.warrior_health, data.warrior_max_health, data.warrior_damage, data.warrior_defense, "Warrior")
	)

## Validates a single hero's stat range.
func _validate_hero(health: int, max_health: int, damage: int, defense: int, hero_name: String) -> bool:
	if max_health <= 0 or health < 0 or health > max_health:
		printerr("[SaveSystem] Validation failed: Invalid %s health (%d/%d)" % [hero_name, health, max_health])
		return false

	if damage < 0 or defense < 0:
		printerr("[SaveSystem] Validation failed: Invalid %s stats (Dmg:%d, Def:%d)" % [hero_name, damage, defense])
		return false

	return true

## Performs a save if the game is currently active.
func auto_save() -> void:
	if GameStateManager.is_game_active:
		save_game()
		print("[SaveSystem] Auto-save completed")
