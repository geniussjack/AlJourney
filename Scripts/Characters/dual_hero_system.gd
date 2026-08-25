class_name DualHeroSystem
extends Node
## The system managing the player's party. Historically named
## "DualHeroSystem" (after the two main heroes), but represents a
## three-slot party: the Mage and Warrior (Altarion and Aldric, always
## present) and an optional third mercenary slot that will become
## available at the village-restoration stage (see design document,
## section 9). Responsible for initializing party members, tracking their
## state, and routing signals.

## Raised when one of the heroes' health changes. Passes the hero's class
## and their current and maximum health.
signal hero_health_changed(hero_class: GameEnums.CharacterClass, current_health: int, max_health: int)
## Raised when one of the heroes' shield strength changes. Passes the
## hero's class and their current shield value.
signal hero_shield_changed(hero_class: GameEnums.CharacterClass, shield_amount: int)
## Raised when one of the heroes dies. Passes the class of the fallen hero.
signal hero_died(hero_class: GameEnums.CharacterClass)
## Raised when the entire party is defeated. This event typically leads to
## the game ending.
signal party_defeated

## Reference to the Mage character (Altarion). Read-only from outside.
var mage: PlayerCharacter
## Reference to the Warrior character (Aldric). Read-only from outside.
var warrior: PlayerCharacter
## The party's third slot — a mercenary hired from the settlement. Always
## empty for now: hiring becomes available at the village-restoration
## stage. Included ahead of time so the party structure doesn't need to be
## reworked later.
var companion: PlayerCharacter = null

## Initializes the Mage and Warrior, adds them as child nodes, and
## subscribes to their signals.
func _ready() -> void:
	mage = PlayerCharacter.create(GameEnums.CharacterClass.MAGE)
	warrior = PlayerCharacter.create(GameEnums.CharacterClass.WARRIOR)

	add_child(mage)
	add_child(warrior)

	_connect_hero_signals(mage, GameEnums.CharacterClass.MAGE)
	_connect_hero_signals(warrior, GameEnums.CharacterClass.WARRIOR)

	print("[DualHeroSystem] Both heroes initialized")

## Forwards a hero's health/shield/death signals to this system's own
## party-wide signals, tagged with the hero's class.
func _connect_hero_signals(hero: PlayerCharacter, hero_class: GameEnums.CharacterClass) -> void:
	hero.health_changed.connect(func(current: int, maximum: int) -> void:
		hero_health_changed.emit(hero_class, current, maximum)
		_check_party_defeated()
	)

	hero.shield_changed.connect(func(shield: int) -> void:
		hero_shield_changed.emit(hero_class, shield)
	)

	hero.character_died.connect(func() -> void:
		hero_died.emit(hero_class)
		_check_party_defeated()
	)

## Emits party_defeated once no party member is left alive.
func _check_party_defeated() -> void:
	if get_alive_members().is_empty():
		party_defeated.emit()
		print("[DualHeroSystem] Entire party has fallen - Game Over!")

## Returns every member of the party: the two heroes and the mercenary, if
## one is assigned.
## Returns the list of party members in a fixed order (Mage, Warrior, Companion).
func get_party_members() -> Array[PlayerCharacter]:
	var members: Array[PlayerCharacter] = [mage, warrior]
	if companion != null:
		members.append(companion)
	return members

## Returns only the party members who are currently alive.
func get_alive_members() -> Array[PlayerCharacter]:
	var alive: Array[PlayerCharacter] = []
	for member: PlayerCharacter in get_party_members():
		if member.is_alive:
			alive.append(member)
	return alive

## Loads both heroes' state from save data.
func load_from_save(
	mage_health: int, mage_max_health: int, mage_damage: int, mage_defense: int,
	warrior_health: int, warrior_max_health: int, warrior_damage: int, warrior_defense: int,
) -> void:
	mage.initialize_from_save("Altarion", mage_max_health, mage_health, mage_damage, mage_defense, GameEnums.CharacterClass.MAGE)
	warrior.initialize_from_save("Aldric", warrior_max_health, warrior_health, warrior_damage, warrior_defense, GameEnums.CharacterClass.WARRIOR)

	print("[DualHeroSystem] Heroes loaded from save")

## Returns both heroes' combined stats as a Dictionary with keys matching
## GameStateManager.update_hero_stats()'s parameters.
func get_combined_stats() -> Dictionary:
	var mage_stats: Dictionary = mage.get_stats()
	var warrior_stats: Dictionary = warrior.get_stats()

	return {
		"mage_health": mage_stats["current_health"],
		"mage_max_health": mage_stats["max_health"],
		"mage_damage": mage_stats["damage"],
		"mage_defense": mage_stats["defense"],
		"warrior_health": warrior_stats["current_health"],
		"warrior_max_health": warrior_stats["max_health"],
		"warrior_damage": warrior_stats["damage"],
		"warrior_defense": warrior_stats["defense"],
	}

## Processes every active status effect for every party member.
func process_status_effects() -> void:
	for member: PlayerCharacter in get_party_members():
		member.process_status_effects()
