class_name PlayerCharacter
extends Character
## The main player character class, inheriting from Character. Manages base
## stats, applying equipment, abilities, and damage calculation.

## This character's class. Set once at creation, treated as read-only afterward.
var character_class: GameEnums.CharacterClass

## Factory method that creates and initializes a new character of the given class.
static func create(character_class: GameEnums.CharacterClass) -> PlayerCharacter:
	var player := PlayerCharacter.new()
	player.character_class = character_class

	match character_class:
		GameEnums.CharacterClass.MAGE:
			player.initialize(
				"CHARACTER_MAGE",
				GameConstants.MAGE_BASE_HP,
				GameConstants.MAGE_BASE_DAMAGE,
				GameConstants.MAGE_BASE_DEFENSE,
				GameEnums.AttackType.MAGICAL
			)
		GameEnums.CharacterClass.WARRIOR:
			player.initialize(
				"CHARACTER_WARRIOR",
				GameConstants.WARRIOR_BASE_HP,
				GameConstants.WARRIOR_BASE_DAMAGE,
				GameConstants.WARRIOR_BASE_DEFENSE,
				GameEnums.AttackType.PHYSICAL
			)

	print("[PlayerCharacter] Created %s (%s)" % [player._name, GameEnums.CharacterClass.keys()[character_class]])
	return player

## Initializes the character with data loaded from a save file.
func initialize_from_save(character_display_name: String, initial_max_health: int, initial_current_health: int, damage: int, defense: int, initial_character_class: GameEnums.CharacterClass) -> void:
	character_class = initial_character_class
	_name = character_display_name
	_max_health = initial_max_health
	_current_health = initial_current_health
	_base_damage = damage
	_base_defense = defense
	_current_shield = 0
	_attack_type = GameEnums.AttackType.MAGICAL if initial_character_class == GameEnums.CharacterClass.MAGE else GameEnums.AttackType.PHYSICAL

	health_changed.emit(_current_health, get_total_max_health())
	print("[PlayerCharacter] Loaded %s from save - HP: %d/%d" % [_name, _current_health, get_total_max_health()])

## Sums the given stat across every piece of equipment worn by this character.
func _get_equipment_stat(stat_name: String) -> int:
	var equipment: Dictionary = InventoryManager.get_hero_equipment(character_class)
	var total: int = 0
	for item: EquipmentData in equipment.values():
		total += item.get_total_stats().get(stat_name, 0)
	return total

## Whether any piece of equipped gear grants immunity to the given status
## effect's key (e.g. "burn" for Dragon Scales' immunity_burn stat). Used
## for narrow, per-effect immunity — unlike GameEnums.StatusEffect.IMMUNITY,
## which blocks all damage and effects outright.
func has_equipment_immunity(status_key: String) -> bool:
	return _get_equipment_stat("immunity_%s" % status_key) > 0

## Returns the given stat's total bonus from the legacy ability system's
## equipped abilities.
func _get_ability_stat(stat_name: String) -> int:
	return AbilitySystem.get_ability_effect(character_class, stat_name)

## The character's total defense, including base defense and bonuses from
## equipment and active abilities.
func get_total_defense() -> int:
	return _base_defense + _get_equipment_stat("defense") + _get_ability_stat("defense")

## The character's total maximum health, computed from base health plus
## both flat and percentage bonuses from equipment and abilities.
func get_total_max_health() -> int:
	var hp_bonus: int = _get_equipment_stat("hp") + _get_ability_stat("hp")
	var hp_percent: int = _get_equipment_stat("hp_percent") + _get_ability_stat("hp_percent")
	var base_hp: int = _max_health + hp_bonus
	return base_hp + (base_hp * hp_percent / 100)

## Computes the final attack damage, accounting for base damage, equipment
## and ability bonuses, and status effects.
## base_damage: the base damage dealt by the attack (the ability effect's value).
func calculate_damage(attack_base_damage: int) -> int:
	var equip_bonus: int = _get_equipment_stat("damage")
	var ability_bonus: int = _get_ability_stat("damage")
	var total_base_damage: int = _base_damage + equip_bonus + ability_bonus
	var final_damage: int = attack_base_damage + total_base_damage

	var weaken_effect: StatusEffectData = get_active_status_effect(GameEnums.StatusEffect.WEAKENED)
	if weaken_effect != null:
		var reduction: float = weaken_effect.extra_data if weaken_effect.extra_data > 0.0 else Character.DEFAULT_WEAKEN_REDUCTION
		final_damage = ceili(final_damage * (1.0 - reduction))
		print("[%s] Damage reduced by Weakened status: %d" % [_name, final_damage])

	return final_damage

## Computes the final healing value, which can be boosted by additional
## modifiers.
static func calculate_healing(base_healing: int) -> int:
	return base_healing

## Computes the final shield strength applied to the character.
static func calculate_shield(base_shield: int) -> int:
	return base_shield

## Gets the character's current stats: maximum health, current health,
## damage and defense, as a Dictionary with those keys.
func get_stats() -> Dictionary:
	var dmg: int = _base_damage + _get_equipment_stat("damage") + _get_ability_stat("damage")
	return {
		"max_health": get_total_max_health(),
		"current_health": _current_health,
		"damage": dmg,
		"defense": get_total_defense(),
	}
