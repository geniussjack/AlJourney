class_name Character
extends Node
## Base class for every living creature in the game. Manages health, armor,
## shields, status effects and base stats.

## Raised when current or maximum health changes.
signal health_changed(current_health: int, max_health: int)
## Raised when direct damage is taken.
signal damage_taken(amount: int)
## Raised when health is successfully restored.
signal healed(amount: int)
## Raised when the magic shield's strength changes.
signal shield_changed(amount: int)
## Raised when a new status effect is applied to the character. Currently
## unused — nothing in the codebase emits or listens to it (see
## status_effect_added for the signal actually driving the status effect UI).
signal status_effect_applied(effect: GameEnums.StatusEffect)
## Raised when a status effect expires or is cleared.
signal status_effect_removed(effect: GameEnums.StatusEffect)
## Raised when the character's health reaches zero.
signal character_died
## Raised when a new status effect is applied, carrying its full data. Drives
## the status effect icons in BattleHUD.
signal status_effect_added(effect_type: GameEnums.StatusEffect, duration: int, power: int)

## The character's display name (a localization key).
var _name: String
var _max_health: int
var _current_health: int
var _base_damage: int
var _base_defense: int
var _current_shield: int = 0
var _attack_type: GameEnums.AttackType
var _active_effects: Array[StatusEffectData] = []

## Returns the character's display name. Overridden by Enemy to append a
## stack-size suffix and translate the underlying localization key.
func get_character_name() -> String:
	return _name

## Base maximum health.
var max_health: int:
	get:
		return _max_health

## Current health points.
var current_health: int:
	get:
		return _current_health

## Base damage dealt.
var base_damage: int:
	get:
		return _base_damage

## Base defense stat.
var base_defense: int:
	get:
		return _base_defense

## The magic shield's current strength. The shield absorbs any damage
## before it affects health.
var current_shield: int:
	get:
		return _current_shield

## The character's attack type.
var attack_type: GameEnums.AttackType:
	get:
		return _attack_type

## Whether the character is still alive.
var is_alive: bool:
	get:
		return current_health > 0

## Whether the character has a stun effect active.
var is_stunned: bool:
	get:
		return has_status_effect(GameEnums.StatusEffect.STUNNED)

## Total defense stat. Overridden by PlayerCharacter to account for equipment.
func get_total_defense() -> int:
	return _base_defense

## Total maximum health. Overridden by PlayerCharacter to account for equipment.
func get_total_max_health() -> int:
	return _max_health

## Sets the character's starting stats.
func initialize(character_display_name: String, initial_max_health: int, damage: int, defense: int, initial_attack_type: GameEnums.AttackType = GameEnums.AttackType.PHYSICAL) -> void:
	_name = character_display_name
	_max_health = initial_max_health
	_current_health = initial_max_health
	_base_damage = damage
	_base_defense = defense
	_current_shield = 0
	_attack_type = initial_attack_type
	_active_effects = []

	health_changed.emit(_current_health, get_total_max_health())

## Deals damage to the character, accounting for armor, shields and status
## effects.
## can_reflect: whether this damage can be reflected back at the attacker.
## Returns the amount of damage that was reflected back.
func take_damage(damage: int, incoming_attack_type: GameEnums.AttackType, can_reflect: bool = true) -> int:
	if not is_alive or has_status_effect(GameEnums.StatusEffect.IMMUNITY):
		return 0

	var final_damage: int = _calculate_final_damage(damage)
	final_damage = _absorb_with_shield(final_damage)
	_apply_health_damage(final_damage)

	return _handle_damage_reflection(damage) if can_reflect else 0

## Applies defense and status-effect multipliers to a raw incoming damage value.
func _calculate_final_damage(raw_damage: int) -> int:
	var effective_defense: int = get_total_defense()

	if has_status_effect(GameEnums.StatusEffect.WEAKENED):
		effective_defense = ceili(effective_defense * 0.7)
		print("[%s] Defense reduced by Weakened status: %d" % [_name, effective_defense])

	var final_damage: int = maxi(1, raw_damage - effective_defense)

	if has_status_effect(GameEnums.StatusEffect.SHOCK) or has_status_effect(GameEnums.StatusEffect.VULNERABLE):
		final_damage = ceili(final_damage * 1.5)
		print("[%s] Damage increased by Shock/Vulnerable status: %d" % [_name, final_damage])

	return final_damage

## Consumes shield strength first, returning the damage left over for health.
func _absorb_with_shield(damage: int) -> int:
	if _current_shield <= 0:
		return damage

	var shield_absorbed: int = mini(_current_shield, damage)
	_current_shield -= shield_absorbed

	shield_changed.emit(_current_shield)
	print("[%s] Shield absorbed %d damage. Remaining shield: %d" % [_name, shield_absorbed, _current_shield])

	return damage - shield_absorbed

## Applies leftover damage to health and triggers death if it reaches zero.
func _apply_health_damage(damage_to_health: int) -> void:
	if damage_to_health <= 0:
		return

	_current_health = maxi(0, _current_health - damage_to_health)
	damage_taken.emit(damage_to_health)
	health_changed.emit(_current_health, get_total_max_health())

	print("[%s] Took %d damage. HP: %d/%d" % [_name, damage_to_health, _current_health, get_total_max_health()])

	if not is_alive:
		_on_death()

## Computes reflected damage if a ShieldReflect status effect is active.
func _handle_damage_reflection(original_damage: int) -> int:
	var reflect_effect: StatusEffectData = null
	for effect: StatusEffectData in _active_effects:
		if effect.type == GameEnums.StatusEffect.SHIELD_REFLECT:
			reflect_effect = effect
			break

	if reflect_effect != null and original_damage > 0:
		var reflected_damage: int = ceili(original_damage * reflect_effect.extra_data)
		print("[%s] Reflected %d damage!" % [_name, reflected_damage])
		return reflected_damage
	return 0

## Restores the character's health, without exceeding the maximum.
func heal(amount: int) -> void:
	if not is_alive:
		return

	var actual_heal: int = mini(amount, get_total_max_health() - _current_health)
	if actual_heal > 0:
		_current_health += actual_heal
		healed.emit(actual_heal)
		health_changed.emit(_current_health, get_total_max_health())

		print("[%s] Healed %d HP. HP: %d/%d" % [_name, actual_heal, _current_health, get_total_max_health()])

## Applies a magic shield to the character. Shields can stack.
func add_shield(amount: int) -> void:
	if not is_alive:
		return

	_current_shield += amount
	shield_changed.emit(_current_shield)

	print("[%s] Gained %d shield. Total: %d" % [_name, amount, _current_shield])

## Applies a new status effect. If an effect of that type is already
## active, its duration is updated, provided the new duration is longer.
func apply_status_effect(effect: StatusEffectData) -> void:
	if not is_alive or has_status_effect(GameEnums.StatusEffect.IMMUNITY):
		return

	var existing_effect: StatusEffectData = null
	for active_effect: StatusEffectData in _active_effects:
		if active_effect.type == effect.type:
			existing_effect = active_effect
			break

	if existing_effect != null:
		if effect.duration > existing_effect.duration:
			_active_effects.erase(existing_effect)
			_active_effects.append(effect)
	else:
		_active_effects.append(effect)

	status_effect_added.emit(effect.type, effect.duration, effect.power)
	print("[%s] Applied status effect: %s for %d turns" % [_name, GameEnums.StatusEffect.keys()[effect.type], effect.duration])

## Clears every negative effect. Typically triggered by a powerful heal.
func clear_negative_effects() -> void:
	var negative_effects: Array[GameEnums.StatusEffect] = [
		GameEnums.StatusEffect.BURNING,
		GameEnums.StatusEffect.BLEEDING,
		GameEnums.StatusEffect.WEAKENED,
		GameEnums.StatusEffect.STUNNED,
	]
	var to_remove: Array[StatusEffectData] = []
	for effect: StatusEffectData in _active_effects:
		if negative_effects.has(effect.type):
			to_remove.append(effect)

	for effect: StatusEffectData in to_remove:
		_active_effects.erase(effect)
		status_effect_removed.emit(effect.type)
		print("[%s] Removed negative effect: %s" % [_name, GameEnums.StatusEffect.keys()[effect.type]])

## Called every turn. Processes damage/heal-over-time effects and ticks
## down effect duration counters.
func process_status_effects() -> void:
	if not is_alive:
		return

	for i: int in range(_active_effects.size() - 1, -1, -1):
		var effect: StatusEffectData = _active_effects[i]
		_apply_effect_tick(effect)

		var updated_effect: StatusEffectData = effect.tick_duration()
		if updated_effect.should_remove:
			_active_effects.remove_at(i)
			status_effect_removed.emit(effect.type)
			print("[%s] Status effect expired: %s" % [_name, GameEnums.StatusEffect.keys()[effect.type]])
		else:
			_active_effects[i] = updated_effect

	if not is_alive:
		_on_death()

## Applies a single turn's worth of a damage/heal-over-time effect.
func _apply_effect_tick(effect: StatusEffectData) -> void:
	match effect.type:
		GameEnums.StatusEffect.BURNING, GameEnums.StatusEffect.BLEEDING:
			var dot_damage: int = effect.power
			_current_health = maxi(0, _current_health - dot_damage)
			damage_taken.emit(dot_damage)
			health_changed.emit(_current_health, get_total_max_health())
			print("[%s] %s dealt %d damage. HP: %d/%d" % [_name, GameEnums.StatusEffect.keys()[effect.type], dot_damage, _current_health, get_total_max_health()])

		GameEnums.StatusEffect.REGENERATION:
			heal(effect.power)

## Checks whether a specific status effect is active on the character.
func has_status_effect(effect_type: GameEnums.StatusEffect) -> bool:
	for effect: StatusEffectData in _active_effects:
		if effect.type == effect_type:
			return true
	return false

## Returns the list of every effect currently active. A copy is returned to
## prevent accidental modification.
func get_active_effects() -> Array[StatusEffectData]:
	return _active_effects.duplicate()

## Called when the character dies.
func _on_death() -> void:
	character_died.emit()
	print("[%s] has died!" % _name)

## Permanently increases the character's base maximum health. Current
## health increases proportionally.
func increase_max_health(amount: int) -> void:
	_max_health += amount
	_current_health += amount
	health_changed.emit(_current_health, get_total_max_health())

## Permanently increases base damage.
func increase_damage(amount: int) -> void:
	_base_damage += amount

## Permanently increases base armor.
func increase_defense(amount: int) -> void:
	_base_defense += amount
