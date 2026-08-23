extends Node
## Global (autoload) legacy ability system manager. Responsible for
## managing unlocking, equipping and applying ability effects for
## characters. Does not participate in the heroes' actual combat logic —
## see design document, section 4.

var _ability_templates: Dictionary[String, AbilityData] = AbilityDatabase.templates
var _equipped_abilities: Dictionary[GameEnums.CharacterClass, Array] = {}

## Gets the list of every ability available to the given character class.
func get_available_abilities(hero_class: GameEnums.CharacterClass) -> Array[AbilityData]:
	var result: Array[AbilityData] = []
	for ability: AbilityData in _ability_templates.values():
		if _is_ability_for_hero(ability, hero_class):
			result.append(ability)
	return result

## Gets the list of abilities currently equipped by the given character
## class, or an empty list if none are equipped.
func get_equipped_abilities(hero_class: GameEnums.CharacterClass) -> Array[AbilityData]:
	var abilities: Array = _equipped_abilities.get(hero_class, [])
	var result: Array[AbilityData] = []
	result.assign(abilities)
	return result

## Unlocks the given ability for the specified character in exchange for
## in-game currency.
## Returns true if the ability was successfully unlocked; false if there
## weren't enough coins.
func unlock_ability(hero: GameEnums.CharacterClass, ability: AbilityData) -> bool:
	if GameStateManager.coins < ability.unlock_cost:
		return false

	GameStateManager.spend_coins(ability.unlock_cost)

	if not _equipped_abilities.has(hero):
		_equipped_abilities[hero] = []

	(_equipped_abilities[hero] as Array).append(ability)
	print("[AbilitySystem] Unlocked ability %s for %s" % [ability.name, GameEnums.CharacterClass.keys()[hero]])
	return true

## Equips the given ability to the specified character. A single character
## cannot have more than 3 abilities equipped.
## Returns true if the ability was successfully equipped; false if the
## 3-ability limit was reached.
func equip_ability(hero: GameEnums.CharacterClass, ability: AbilityData) -> bool:
	if not _equipped_abilities.has(hero):
		_equipped_abilities[hero] = []

	var abilities: Array = _equipped_abilities[hero]
	if abilities.size() >= 3:
		return false

	abilities.append(ability)
	print("[AbilitySystem] Equipped ability %s for %s" % [ability.name, GameEnums.CharacterClass.keys()[hero]])
	return true

## Computes and returns the total value of a specific effect across all of
## the character's equipped abilities.
func get_ability_effect(hero: GameEnums.CharacterClass, effect_name: String) -> int:
	if not _equipped_abilities.has(hero):
		return 0

	var total: int = 0
	for ability: AbilityData in _equipped_abilities[hero]:
		total += ability.get_effect(effect_name)
	return total

## Gets a dictionary with the total value of every stat granted by the
## given character's equipped abilities, keyed by stat name.
func get_total_ability_stats(hero: GameEnums.CharacterClass) -> Dictionary[String, int]:
	var total_stats: Dictionary[String, int] = {}

	if not _equipped_abilities.has(hero):
		return total_stats

	for ability: AbilityData in _equipped_abilities[hero]:
		for effect_name: String in ability.effects.keys():
			if total_stats.has(effect_name):
				total_stats[effect_name] += ability.effects[effect_name]
			else:
				total_stats[effect_name] = ability.effects[effect_name]

	return total_stats

## Whether the given ability's element belongs to the given hero class's category.
static func _is_ability_for_hero(ability: AbilityData, hero_class: GameEnums.CharacterClass) -> bool:
	return (
		(hero_class == GameEnums.CharacterClass.MAGE and (ability.element == GameEnums.AbilityElement.FIRE or ability.element == GameEnums.AbilityElement.HEAL))
		or (hero_class == GameEnums.CharacterClass.WARRIOR and (ability.element == GameEnums.AbilityElement.SWORD or ability.element == GameEnums.AbilityElement.SHIELD))
	)
