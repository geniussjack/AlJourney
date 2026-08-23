class_name AbilityTargetingRules
extends RefCounted
## Pure target-selection rules for abilities: which targets are valid to aim
## at, and who the effect actually applies to once a target is confirmed
## (accounting for AoE). Has no dependency on the scene tree — operates
## purely on Character (and its PlayerCharacter/Enemy subclasses) so the
## rules stay reusable and easy to reason about in isolation.

## Returns the list of targets that can, in principle, be aimed at with an
## ability of the given targeting type. Attack abilities target enemies;
## defensive/support abilities target allies (including the caster). Dead
## characters are never a valid target.
## allies: every ally, including the caster themselves.
## enemies: every enemy on the battlefield.
static func get_valid_targets(
	target_type: GameEnums.AbilityTargetType,
	allies: Array[Character],
	enemies: Array[Character],
) -> Array[Character]:
	var pool: Array[Character] = enemies if target_type == GameEnums.AbilityTargetType.ENEMY else allies
	var result: Array[Character] = []
	for candidate: Character in pool:
		if candidate.is_alive:
			result.append(candidate)
	return result

## Returns the final list of targets the ability's effect applies to once
## the player has aimed at a specific target. For single-target abilities,
## this is the chosen target itself (if it's still valid). For AoE
## abilities, the effect spreads to the entire target pool for the matching
## targeting type (every living enemy, or the whole living party).
## chosen_target: the target chosen by the player (may be null if not yet confirmed).
static func resolve_effect_targets(
	target_type: GameEnums.AbilityTargetType,
	is_aoe: bool,
	chosen_target: Character,
	allies: Array[Character],
	enemies: Array[Character],
) -> Array[Character]:
	if not is_aoe:
		if chosen_target != null and chosen_target.is_alive:
			return [chosen_target]
		return []

	return get_valid_targets(target_type, allies, enemies)

## Automatically selects the living target with the highest current health
## from the candidate list. Used for single-target ultimate abilities the
## player doesn't manually aim, e.g. "strike the enemy with the highest HP".
## Returns the living target with the highest current health, or null if
## there are no living candidates.
static func select_highest_health_target(candidates: Array[Character]) -> Character:
	var best: Character = null
	for candidate: Character in candidates:
		if not candidate.is_alive:
			continue
		if best == null or candidate.current_health > best.current_health:
			best = candidate
	return best
