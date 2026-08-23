class_name CampaignDatabase
extends RefCounted
## Static campaign map database: locations, the main linear level chain from
## the village ruins to the necromancer's lair, and branches with minibosses.
## This is a working, easily adjustable level set — the exact wave
## composition will be tuned separately during balancing.

## The first campaign level, available with no unlock conditions.
const FIRST_LEVEL_ID: String = "village_ruins_1"

## Every campaign level in declaration order (main line and branches
## interleaved), in the order they are declared below in _build_levels().
static var levels: Array[LevelDefinition] = _build_levels()

## Builds the full, curated list of campaign levels for every location.
static func _build_levels() -> Array[LevelDefinition]:
	var result: Array[LevelDefinition] = []

	# --- Location 1: Village Ruins ---
	# Starting location. Simplest enemies: slimes and zombies.
	_add_main_level(result, GameEnums.LocationId.VILLAGE_RUINS, 1, 1, "",
		[_wave([_spawn(GameEnums.EnemyType.SLIME, 2)])])
	_add_main_level(result, GameEnums.LocationId.VILLAGE_RUINS, 2, 2, "",
		[_wave([_spawn(GameEnums.EnemyType.SLIME, 3)])])
	_add_main_level(result, GameEnums.LocationId.VILLAGE_RUINS, 3, 3, "",
		[_wave([_spawn(GameEnums.EnemyType.ZOMBIE, 1), _spawn(GameEnums.EnemyType.SLIME, 2)])])
	_add_main_level(result, GameEnums.LocationId.VILLAGE_RUINS, 4, 4, "",
		[
			_wave([_spawn(GameEnums.EnemyType.ZOMBIE, 2)]),
			_wave([_spawn(GameEnums.EnemyType.ZOMBIE, 1), _spawn(GameEnums.EnemyType.SLIME, 2)]),
		])

	# --- Location 2: Dark Forest ---
	# Skeletons (warrior and archer) are introduced; zombies remain a filler enemy.
	_add_main_level(result, GameEnums.LocationId.DARK_FOREST, 1, 5, _level_id(GameEnums.LocationId.VILLAGE_RUINS, 4),
		[_wave([_spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 2)])])
	_add_main_level(result, GameEnums.LocationId.DARK_FOREST, 2, 6, "",
		[_wave([_spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 1), _spawn(GameEnums.EnemyType.SKELETON_ARCHER, 2)])])
	_add_main_level(result, GameEnums.LocationId.DARK_FOREST, 3, 7, "",
		[
			_wave([_spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 2), _spawn(GameEnums.EnemyType.ZOMBIE, 1)]),
			_wave([_spawn(GameEnums.EnemyType.SKELETON_ARCHER, 2)]),
		])
	_add_main_level(result, GameEnums.LocationId.DARK_FOREST, 4, 8, "",
		[
			_wave([_spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 2), _spawn(GameEnums.EnemyType.SKELETON_ARCHER, 2)]),
			_wave([_spawn(GameEnums.EnemyType.ZOMBIE, 2)]),
		])
	# Branch: the first miniboss, the General of Draugr.
	_add_branch_level(result, GameEnums.LocationId.DARK_FOREST, "dark_forest_branch_1", 7,
		_level_id(GameEnums.LocationId.DARK_FOREST, 1),
		[
			_wave([_spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 2)]),
			_wave([_spawn(GameEnums.EnemyType.GENERAL_OF_DRAUGR)]),
		])

	# --- Location 3: Buried Catacombs ---
	# The Draugr trio is introduced; skeletons remain a filler enemy.
	_add_main_level(result, GameEnums.LocationId.BURIED_CATACOMBS, 1, 9, _level_id(GameEnums.LocationId.DARK_FOREST, 4),
		[_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2)])])
	_add_main_level(result, GameEnums.LocationId.BURIED_CATACOMBS, 2, 10, "",
		[_wave([_spawn(GameEnums.EnemyType.DRAUGR_DEFENDER, 1), _spawn(GameEnums.EnemyType.SKELETON_ARCHER, 2)])])
	_add_main_level(result, GameEnums.LocationId.BURIED_CATACOMBS, 3, 11, "",
		[
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_CASTER, 2), _spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 1)]),
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2)]),
		])
	_add_main_level(result, GameEnums.LocationId.BURIED_CATACOMBS, 4, 12, "",
		[
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 1), _spawn(GameEnums.EnemyType.DRAUGR_DEFENDER, 1), _spawn(GameEnums.EnemyType.DRAUGR_CASTER, 1)]),
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2), _spawn(GameEnums.EnemyType.DRAUGR_CASTER, 1)]),
		])
	# Branch: the second miniboss, the Archskeleton.
	_add_branch_level(result, GameEnums.LocationId.BURIED_CATACOMBS, "buried_catacombs_branch_1", 11,
		_level_id(GameEnums.LocationId.BURIED_CATACOMBS, 1),
		[
			_wave([_spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 2), _spawn(GameEnums.EnemyType.SKELETON_ARCHER, 1)]),
			_wave([_spawn(GameEnums.EnemyType.ARHISKELETON)]),
		])

	# --- Location 4: Frozen Wastes ---
	# The heaviest "regular" mixed waves before the necromancer's lair.
	_add_main_level(result, GameEnums.LocationId.FROZEN_WASTES, 1, 13, _level_id(GameEnums.LocationId.BURIED_CATACOMBS, 4),
		[_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2), _spawn(GameEnums.EnemyType.SKELETON_ARCHER, 1)])])
	_add_main_level(result, GameEnums.LocationId.FROZEN_WASTES, 2, 14, "",
		[_wave([_spawn(GameEnums.EnemyType.DRAUGR_DEFENDER, 2), _spawn(GameEnums.EnemyType.DRAUGR_CASTER, 1)])])
	_add_main_level(result, GameEnums.LocationId.FROZEN_WASTES, 3, 15, "",
		[
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2), _spawn(GameEnums.EnemyType.DRAUGR_CASTER, 2)]),
			_wave([_spawn(GameEnums.EnemyType.SKELETON_WARRIOR, 2), _spawn(GameEnums.EnemyType.SKELETON_ARCHER, 2)]),
		])
	_add_main_level(result, GameEnums.LocationId.FROZEN_WASTES, 4, 16, "",
		[
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2), _spawn(GameEnums.EnemyType.DRAUGR_DEFENDER, 1)]),
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_CASTER, 2), _spawn(GameEnums.EnemyType.SKELETON_ARCHER, 1)]),
		])
	# Branch: a third encounter with the miniboss (General of Draugr) with reinforced guards.
	_add_branch_level(result, GameEnums.LocationId.FROZEN_WASTES, "frozen_wastes_branch_1", 15,
		_level_id(GameEnums.LocationId.FROZEN_WASTES, 1),
		[
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2), _spawn(GameEnums.EnemyType.DRAUGR_DEFENDER, 1)]),
			_wave([_spawn(GameEnums.EnemyType.GENERAL_OF_DRAUGR)]),
		])

	# --- Location 5: Necromancer's Lair ---
	# Final heavy mixed waves and the fight against the main boss.
	_add_main_level(result, GameEnums.LocationId.NECROMANCER_LAIR, 1, 17, _level_id(GameEnums.LocationId.FROZEN_WASTES, 4),
		[_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2), _spawn(GameEnums.EnemyType.DRAUGR_CASTER, 2)])])
	_add_main_level(result, GameEnums.LocationId.NECROMANCER_LAIR, 2, 18, "",
		[
			_wave([_spawn(GameEnums.EnemyType.ARHISKELETON)]),
			_wave([_spawn(GameEnums.EnemyType.DRAUGR_WARRIOR, 2), _spawn(GameEnums.EnemyType.DRAUGR_DEFENDER, 1)]),
		])
	_add_main_level(result, GameEnums.LocationId.NECROMANCER_LAIR, 3, 20, "",
		[_wave([_spawn(GameEnums.EnemyType.NECROMANCER)])])

	return result

## Returns the localization key for the location's display name (see
## Data/Languages/translations.csv).
static func get_location_name_key(location: GameEnums.LocationId) -> String:
	match location:
		GameEnums.LocationId.VILLAGE_RUINS:
			return "LOCATION_VILLAGE_RUINS"
		GameEnums.LocationId.DARK_FOREST:
			return "LOCATION_DARK_FOREST"
		GameEnums.LocationId.BURIED_CATACOMBS:
			return "LOCATION_BURIED_CATACOMBS"
		GameEnums.LocationId.FROZEN_WASTES:
			return "LOCATION_FROZEN_WASTES"
		GameEnums.LocationId.NECROMANCER_LAIR:
			return "LOCATION_NECROMANCER_LAIR"
		_:
			return "LOCATION_VILLAGE_RUINS"

## Returns the campaign level with the given id, or null if no such level exists.
static func get_level(level_id: String) -> LevelDefinition:
	for level: LevelDefinition in levels:
		if level.id == level_id:
			return level
	return null

## Returns the level that comes next on the main line after the given one,
## within the same location, or the first level of the next location if the
## given level was the last one in its location. Branches are not part of
## the main sequence. Returns null after the final campaign level.
static func get_next_main_level(completed_level_id: String) -> LevelDefinition:
	var completed: LevelDefinition = get_level(completed_level_id)
	if completed == null or completed.is_branch:
		return null

	var best: LevelDefinition = null
	for level: LevelDefinition in levels:
		if level.is_branch or level.location != completed.location or level.order_in_location <= completed.order_in_location:
			continue

		if best == null or level.order_in_location < best.order_in_location:
			best = level

	if best != null:
		return best

	var next_location: int = completed.location + 1
	if next_location > GameEnums.LocationId.NECROMANCER_LAIR:
		return null
	return _get_first_level_of_location(next_location as GameEnums.LocationId)

## Returns the first main-line level of the given location, in display order.
static func _get_first_level_of_location(location: GameEnums.LocationId) -> LevelDefinition:
	var first: LevelDefinition = null
	for level: LevelDefinition in levels:
		if level.is_branch or level.location != location:
			continue

		if first == null or level.order_in_location < first.order_in_location:
			first = level

	return first

## Builds the standard main-line level identifier from a location and order number.
static func _level_id(location: GameEnums.LocationId, order_in_location: int) -> String:
	return "%s_%d" % [_to_snake_case(location), order_in_location]

## Appends a main-line level to the campaign, defaulting its unlock
## requirement to the previous level of the same location when not given.
static func _add_main_level(
	result: Array[LevelDefinition],
	location: GameEnums.LocationId,
	order_in_location: int,
	difficulty: int,
	required_level_id: String,
	waves: Array[WaveDefinition],
) -> void:
	var resolved_required_id: String = required_level_id
	if resolved_required_id.is_empty() and order_in_location > 1:
		resolved_required_id = _level_id(location, order_in_location - 1)

	result.append(LevelDefinition.new(_level_id(location, order_in_location), location, order_in_location, waves, difficulty, false, resolved_required_id))

## Appends an optional branch level to the campaign. Branches use a negative
## order so they don't participate in computing the next main-line level
## (see get_next_main_level/_get_first_level_of_location), while still
## staying tied to their location for display purposes on the map.
static func _add_branch_level(
	result: Array[LevelDefinition],
	location: GameEnums.LocationId,
	id: String,
	difficulty: int,
	required_level_id: String,
	waves: Array[WaveDefinition],
) -> void:
	result.append(LevelDefinition.new(id, location, -1, waves, difficulty, true, required_level_id))

## Convenience constructor for a wave made of the given spawns.
static func _wave(enemies: Array[EnemySpawnDefinition]) -> WaveDefinition:
	return WaveDefinition.new(enemies)

## Convenience constructor for a single enemy spawn.
static func _spawn(type: GameEnums.EnemyType, count: int = 1) -> EnemySpawnDefinition:
	return EnemySpawnDefinition.new(type, count)

## Converts a location id into its snake_case identifier fragment.
static func _to_snake_case(location: GameEnums.LocationId) -> String:
	match location:
		GameEnums.LocationId.VILLAGE_RUINS:
			return "village_ruins"
		GameEnums.LocationId.DARK_FOREST:
			return "dark_forest"
		GameEnums.LocationId.BURIED_CATACOMBS:
			return "buried_catacombs"
		GameEnums.LocationId.FROZEN_WASTES:
			return "frozen_wastes"
		GameEnums.LocationId.NECROMANCER_LAIR:
			return "necromancer_lair"
		_:
			return GameEnums.LocationId.keys()[location].to_lower()
