class_name SaveData
extends RefCounted
## Data structure used to save and load player progress. Stores hero stat
## state, inventory, equipment, unlocked abilities and campaign progress.

## Save data schema version. Used to migrate old saves when the game is updated.
var schema_version: int = 1

## The current campaign difficulty rating, carried over from LevelDefinition
## on StartLevel — used as the input to ScalingSystem.
var current_wave: int = 1
## The highest difficulty rating reached so far.
var highest_wave: int = 1
## Id of the campaign map level the player is currently on or should
## attempt next. See CampaignDatabase.
var current_level_id: String = CampaignDatabase.FIRST_LEVEL_ID
## Ids of every campaign level already completed (main line and branches).
var completed_level_ids: Array[String] = []

## The number of coins the player has.
var coins: int = 0

## The party's shared level, grown by combat XP (see
## GameStateManager.add_party_xp). Increases both heroes' base stats on
## level-up — a separate growth path from shop-purchased upgrades.
var party_level: int = 1
## XP accumulated toward the party's next level (resets on level-up, not cumulative).
var party_xp: int = 0

## Strategic resources currently stored in the settlement (see design
## document, section 9), keyed by resource type. Missing keys mean 0, not
## an error — a resource only appears here once it's been gathered at
## least once. Uncapped for now — the Warehouse building will introduce a
## storage limit once it exists.
var strategic_resources: Dictionary[GameEnums.StrategicResource, int] = {}

## Current level of each settlement building, keyed by type. Buildings
## start at level 1 (already exist in a basic form) rather than unbuilt —
## upgrading raises their level and unlocks better bonuses/recipes.
## Missing keys mean level 1, same as a fresh save.
var building_levels: Dictionary[GameEnums.BuildingType, int] = {}

## Villagers currently assigned to gather each strategic resource (see
## design document, section 9). Total assigned across all resources is
## capped by GameStateManager.get_worker_capacity() (Houses level).
var worker_assignments: Dictionary[GameEnums.StrategicResource, int] = {}
## Unix timestamp (seconds) of the last time worker-gathered resources
## were credited — used to compute both real-time ticks while playing and
## catch-up gains after time spent offline. 0 means never initialized yet.
var last_resource_tick_unix_time: int = 0

## Villagers currently assigned to defend the settlement against undead
## raids instead of gathering a resource. Shares the same total worker
## capacity pool as worker_assignments (see GameStateManager.
## get_total_assigned_workers()).
var defense_workers: int = 0
## Unix timestamp (seconds) of the last undead raid check — same
## real-time/offline-catch-up pattern as last_resource_tick_unix_time.
## 0 means never initialized yet.
var last_raid_check_unix_time: int = 0
## Whether the most recent raid was repelled. Meaningless (ignore) if no
## raid has happened yet — see last_raid_unix_time.
var last_raid_succeeded: bool = false
## Unix timestamp (seconds) of the most recent raid, or 0 if none has
## happened yet.
var last_raid_unix_time: int = 0

## The mercenary currently filling the party's third slot, identified by
## MercenarySubclassData.get_key() (e.g. "MAGE_HEALER"), or "" if the slot
## is empty. See design document, section 9.
var active_mercenary_key: String = ""
## Battles remaining before each mercenary (keyed by get_key()) becomes
## available again after being benched — see GameStateManager.
## on_battle_completed(). Missing keys mean 0 (available), same as a
## mercenary who has never been used.
var mercenary_recovery: Dictionary[String, int] = {}

## Number of each Herbalist-brewed potion currently owned, keyed by
## PotionData.id. Missing keys mean 0, same as a potion never brewed.
var potion_counts: Dictionary[String, int] = {}

## The Mage's current health.
var mage_health: int = 0
## The Mage's maximum health.
var mage_max_health: int = 0
## The Mage's damage.
var mage_damage: int = 0
## The Mage's defense.
var mage_defense: int = 0

## The Warrior's current health.
var warrior_health: int = 0
## The Warrior's maximum health.
var warrior_max_health: int = 0
## The Warrior's damage.
var warrior_damage: int = 0
## The Warrior's defense.
var warrior_defense: int = 0

## Reserved for future permanent bonuses. Always empty today — nothing in
## the codebase writes to it yet.
var permanent_upgrades: Dictionary[String, int] = {}
## Reserved for a future artifact system. Always empty today — nothing in
## the codebase writes to it yet.
var active_artifacts: Array[String] = []

## Equipment currently worn by each hero, keyed by hero class then slot.
var hero_equipment: Dictionary[GameEnums.CharacterClass, Dictionary] = {}
## Every equipment item currently in the player's shared inventory.
var inventory: Array[EquipmentData] = []

## Reserved for the legacy ability unlock/equip system's persistence. Always
## empty today — AbilitySystem keeps its own in-memory state instead of
## reading or writing this field (see design document, section 4).
var unlocked_abilities: Dictionary[GameEnums.CharacterClass, Array] = {}
## Reserved for the legacy ability unlock/equip system's persistence. Always
## empty today — see unlocked_abilities.
var equipped_abilities: Dictionary[GameEnums.CharacterClass, Array] = {}

## Timestamp of the last successful save, formatted "yyyy-MM-dd HH:mm:ss".
var last_save_time: String = ""

## Builds a save profile with every field at its zeroed default. Prefer
## create_new() for a playable starting profile.
func _init() -> void:
	last_save_time = Time.get_datetime_string_from_system(false, true)

## Factory method that creates a new save profile with default starting
## values. Sets initial stats for the Mage and Warrior, starting weapon
## inventory and equipment, and resets progress back to the first level.
static func create_new() -> SaveData:
	var save := SaveData.new()
	save.current_wave = 1
	save.highest_wave = 1
	save.current_level_id = CampaignDatabase.FIRST_LEVEL_ID
	save.completed_level_ids = []
	save.coins = 0
	save.party_level = 1
	save.party_xp = 0

	save.mage_max_health = GameConstants.MAGE_BASE_HP
	save.mage_health = GameConstants.MAGE_BASE_HP
	save.mage_damage = GameConstants.MAGE_BASE_DAMAGE
	save.mage_defense = GameConstants.MAGE_BASE_DEFENSE

	save.warrior_max_health = GameConstants.WARRIOR_BASE_HP
	save.warrior_health = GameConstants.WARRIOR_BASE_HP
	save.warrior_damage = GameConstants.WARRIOR_BASE_DAMAGE
	save.warrior_defense = GameConstants.WARRIOR_BASE_DEFENSE

	var starting_weapons: Array[String] = ["fireball", "iceball", "electroball", "sword", "axe", "spear"]
	for weapon_id: String in starting_weapons:
		var weapon_data: EquipmentData = EquipmentDatabase.templates.get(weapon_id)
		if weapon_data == null:
			continue

		save.inventory.append(weapon_data)

		if weapon_id == "fireball":
			save.hero_equipment[GameEnums.CharacterClass.MAGE] = {GameEnums.EquipmentSlot.WEAPON: weapon_data}
		elif weapon_id == "sword":
			save.hero_equipment[GameEnums.CharacterClass.WARRIOR] = {GameEnums.EquipmentSlot.WEAPON: weapon_data}

	return save

## Adapts data from older game versions into the current save structure. If
## the schema version is outdated, the data is converted to ensure
## compatibility.
## Returns the migrated SaveData object, or null if migration failed.
static func migrate(old_data: SaveData) -> SaveData:
	if old_data.schema_version == 1:
		return old_data

	print("[SaveData] Migrating from schema version %d to 1" % old_data.schema_version)

	return null

## Serializes this save into a plain Dictionary suitable for JSON storage.
func to_dict() -> Dictionary:
	var hero_equipment_dict: Dictionary = {}
	for hero_class: GameEnums.CharacterClass in hero_equipment.keys():
		var slots_dict: Dictionary = {}
		var slots: Dictionary = hero_equipment[hero_class]
		for slot: GameEnums.EquipmentSlot in slots.keys():
			var item: EquipmentData = slots[slot]
			slots_dict[GameEnums.EquipmentSlot.keys()[slot]] = item.to_dict()
		hero_equipment_dict[GameEnums.CharacterClass.keys()[hero_class]] = slots_dict

	var inventory_list: Array = []
	for item: EquipmentData in inventory:
		inventory_list.append(item.to_dict())

	var strategic_resources_dict: Dictionary = {}
	for resource: GameEnums.StrategicResource in strategic_resources.keys():
		strategic_resources_dict[GameEnums.StrategicResource.keys()[resource]] = strategic_resources[resource]

	var building_levels_dict: Dictionary = {}
	for building: GameEnums.BuildingType in building_levels.keys():
		building_levels_dict[GameEnums.BuildingType.keys()[building]] = building_levels[building]

	var worker_assignments_dict: Dictionary = {}
	for resource: GameEnums.StrategicResource in worker_assignments.keys():
		worker_assignments_dict[GameEnums.StrategicResource.keys()[resource]] = worker_assignments[resource]

	return {
		"schemaVersion": schema_version,
		"currentWave": current_wave,
		"highestWave": highest_wave,
		"currentLevelId": current_level_id,
		"completedLevelIds": completed_level_ids,
		"coins": coins,
		"strategicResources": strategic_resources_dict,
		"buildingLevels": building_levels_dict,
		"workerAssignments": worker_assignments_dict,
		"lastResourceTickUnixTime": last_resource_tick_unix_time,
		"defenseWorkers": defense_workers,
		"lastRaidCheckUnixTime": last_raid_check_unix_time,
		"lastRaidSucceeded": last_raid_succeeded,
		"lastRaidUnixTime": last_raid_unix_time,
		"activeMercenaryKey": active_mercenary_key,
		"mercenaryRecovery": mercenary_recovery.duplicate(),
		"potionCounts": potion_counts.duplicate(),
		"partyLevel": party_level,
		"partyXp": party_xp,
		"mageHealth": mage_health,
		"mageMaxHealth": mage_max_health,
		"mageDamage": mage_damage,
		"mageDefense": mage_defense,
		"warriorHealth": warrior_health,
		"warriorMaxHealth": warrior_max_health,
		"warriorDamage": warrior_damage,
		"warriorDefense": warrior_defense,
		"permanentUpgrades": permanent_upgrades,
		"activeArtifacts": active_artifacts,
		"heroEquipment": hero_equipment_dict,
		"inventory": inventory_list,
		"unlockedAbilities": {},
		"equippedAbilities": {},
		"lastSaveTime": last_save_time,
	}

## Rebuilds a save from a Dictionary previously produced by to_dict().
static func from_dict(data: Dictionary) -> SaveData:
	var save := SaveData.new()
	save.schema_version = int(data.get("schemaVersion", 1))
	save.current_wave = int(data.get("currentWave", 1))
	save.highest_wave = int(data.get("highestWave", 1))
	save.current_level_id = data.get("currentLevelId", CampaignDatabase.FIRST_LEVEL_ID)

	save.completed_level_ids = []
	for level_id: String in (data.get("completedLevelIds", []) as Array):
		save.completed_level_ids.append(level_id)

	save.coins = int(data.get("coins", 0))

	save.strategic_resources = {}
	var strategic_resources_dict: Dictionary = data.get("strategicResources", {})
	for resource_key: String in strategic_resources_dict.keys():
		save.strategic_resources[GameEnums.StrategicResource[resource_key]] = int(strategic_resources_dict[resource_key])

	save.building_levels = {}
	var building_levels_dict: Dictionary = data.get("buildingLevels", {})
	for building_key: String in building_levels_dict.keys():
		save.building_levels[GameEnums.BuildingType[building_key]] = int(building_levels_dict[building_key])

	save.worker_assignments = {}
	var worker_assignments_dict: Dictionary = data.get("workerAssignments", {})
	for resource_key: String in worker_assignments_dict.keys():
		save.worker_assignments[GameEnums.StrategicResource[resource_key]] = int(worker_assignments_dict[resource_key])
	save.last_resource_tick_unix_time = int(data.get("lastResourceTickUnixTime", 0))

	save.defense_workers = int(data.get("defenseWorkers", 0))
	save.last_raid_check_unix_time = int(data.get("lastRaidCheckUnixTime", 0))
	save.last_raid_succeeded = bool(data.get("lastRaidSucceeded", false))
	save.last_raid_unix_time = int(data.get("lastRaidUnixTime", 0))

	save.active_mercenary_key = data.get("activeMercenaryKey", "")
	save.mercenary_recovery = {}
	var mercenary_recovery_dict: Dictionary = data.get("mercenaryRecovery", {})
	for key: String in mercenary_recovery_dict.keys():
		save.mercenary_recovery[key] = int(mercenary_recovery_dict[key])

	save.potion_counts = {}
	var potion_counts_dict: Dictionary = data.get("potionCounts", {})
	for key: String in potion_counts_dict.keys():
		save.potion_counts[key] = int(potion_counts_dict[key])

	save.party_level = int(data.get("partyLevel", 1))
	save.party_xp = int(data.get("partyXp", 0))

	save.mage_health = int(data.get("mageHealth", 0))
	save.mage_max_health = int(data.get("mageMaxHealth", 0))
	save.mage_damage = int(data.get("mageDamage", 0))
	save.mage_defense = int(data.get("mageDefense", 0))

	save.warrior_health = int(data.get("warriorHealth", 0))
	save.warrior_max_health = int(data.get("warriorMaxHealth", 0))
	save.warrior_damage = int(data.get("warriorDamage", 0))
	save.warrior_defense = int(data.get("warriorDefense", 0))

	save.permanent_upgrades = {}
	for key: String in (data.get("permanentUpgrades", {}) as Dictionary).keys():
		save.permanent_upgrades[key] = int(data["permanentUpgrades"][key])

	save.active_artifacts = []
	for artifact: String in (data.get("activeArtifacts", []) as Array):
		save.active_artifacts.append(artifact)

	save.hero_equipment = {}
	var hero_equipment_dict: Dictionary = data.get("heroEquipment", {})
	for hero_class_key: String in hero_equipment_dict.keys():
		var slots: Dictionary = {}
		var slots_dict: Dictionary = hero_equipment_dict[hero_class_key]
		for slot_key: String in slots_dict.keys():
			slots[GameEnums.EquipmentSlot[slot_key]] = EquipmentData.from_dict(slots_dict[slot_key])
		save.hero_equipment[GameEnums.CharacterClass[hero_class_key]] = slots

	save.inventory = []
	for item_dict: Dictionary in (data.get("inventory", []) as Array):
		save.inventory.append(EquipmentData.from_dict(item_dict))

	save.unlocked_abilities = {}
	save.equipped_abilities = {}

	save.last_save_time = data.get("lastSaveTime", "")

	return save
