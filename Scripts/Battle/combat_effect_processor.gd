class_name CombatEffectProcessor
extends RefCounted
## Service for applying ability effects during turn-based combat: dealing
## damage with attack abilities and healing/shielding with support
## abilities.

## Applies an attack ability to every resolved target (a single target, or
## every enemy for AoE).
static func apply_attack_ability(ability: AbilityData, caster: PlayerCharacter, targets: Array[Character], battle_manager: BattleManager, camera_shake: CameraShake) -> void:
	if targets.is_empty():
		return

	var damage: int = caster.calculate_damage(ability.get_effect("damage"))
	var is_aoe: bool = targets.size() > 1

	AudioManager.play_attack_sound()
	if camera_shake != null:
		if is_aoe:
			camera_shake.shake_strong()
		else:
			camera_shake.shake_medium()

	ComboParticles.spawn_combo_effect(battle_manager, Vector2(640, 200 if is_aoe else 300), ability.element, 1)

	for target: Character in targets:
		_deal_damage(target, damage, caster, ability, is_aoe, battle_manager)

## Applies damage to a single target and any reflection back at the caster.
static func _deal_damage(target: Character, damage: int, caster: PlayerCharacter, ability: AbilityData, is_aoe: bool, battle_manager: BattleManager) -> void:
	var reflected: int = target.take_damage(damage, caster.attack_type, true)
	var particle_pos: Vector2 = Vector2(400, 200) if is_aoe else Vector2(640, 250)

	AudioManager.play_hit_sound()
	ComboParticles.spawn_damage_number(battle_manager, particle_pos, damage)

	if target.is_alive:
		_apply_weapon_status_effects(caster, target)
		_apply_status_effects_from_stats(ability.effects, target)

	if reflected > 0:
		caster.take_damage(reflected, target.attack_type, false)

## Duration (in turns) applied to weapon-driven damage-over-time effects
## (Burning/Bleeding) on hit.
const WEAPON_DOT_DURATION: int = 3
## Duration (in turns) applied to weapon-driven debuffs (Weakened/Shock/
## Vulnerable) on hit.
const WEAPON_DEBUFF_DURATION: int = 2

## Reads the caster's equipped weapon for status-inducing stats
## (burn_damage, bleed_damage, weaken_amount, shock_amount,
## vulnerable_amount — see EquipmentDatabase) and applies the matching
## status effect to the target that was just hit. Only the weapon slot is
## checked, since these are elemental weapon stats, not general equipment
## bonuses.
static func _apply_weapon_status_effects(caster: PlayerCharacter, target: Character) -> void:
	var weapon: EquipmentData = InventoryManager.get_equipped_item(caster.character_class, GameEnums.EquipmentSlot.WEAPON)
	if weapon == null:
		return

	_apply_status_effects_from_stats(weapon.get_total_stats(), target)

## Reads a stats dictionary — either an equipped weapon's total stats, or
## an ability's own intrinsic effects — for status-inducing keys and
## applies the matching status effect to the target. Shared by
## weapon-driven status effects (see EquipmentDatabase) and
## ability-intrinsic ones (see MercenaryDatabase, whose subclass attack
## abilities apply their own status effects directly rather than through
## equipment, since mercenaries don't share the two heroes' equipment pool).
static func _apply_status_effects_from_stats(stats: Dictionary[String, int], target: Character) -> void:
	if stats.has("burn_damage") and not _has_immunity(target, "burn"):
		target.apply_status_effect(StatusEffectData.new(GameEnums.StatusEffect.BURNING, WEAPON_DOT_DURATION, stats["burn_damage"]))

	if stats.has("bleed_damage") and not _has_immunity(target, "bleed"):
		target.apply_status_effect(StatusEffectData.new(GameEnums.StatusEffect.BLEEDING, WEAPON_DOT_DURATION, stats["bleed_damage"]))

	if stats.has("weaken_amount") and not _has_immunity(target, "weaken"):
		target.apply_status_effect(StatusEffectData.new(GameEnums.StatusEffect.WEAKENED, WEAPON_DEBUFF_DURATION, 0, stats["weaken_amount"] / 100.0))

	if stats.has("shock_amount") and not _has_immunity(target, "shock"):
		target.apply_status_effect(StatusEffectData.new(GameEnums.StatusEffect.SHOCK, WEAPON_DEBUFF_DURATION, 0, stats["shock_amount"] / 100.0))

	if stats.has("vulnerable_amount") and not _has_immunity(target, "vulnerable"):
		target.apply_status_effect(StatusEffectData.new(GameEnums.StatusEffect.VULNERABLE, WEAPON_DEBUFF_DURATION, 0, stats["vulnerable_amount"] / 100.0))

## Whether the target's equipped gear grants immunity to the given status
## key (e.g. Dragon Scales' immunity_burn). Only PlayerCharacter targets
## can have equipment; enemies never carry immunity stats today.
static func _has_immunity(target: Character, status_key: String) -> bool:
	return target is PlayerCharacter and (target as PlayerCharacter).has_equipment_immunity(status_key)

## Applies a support ability to every resolved target (a single target, or
## the whole party for AoE). Supports healing and/or shielding depending on
## the ability's effects.
static func apply_support_ability(ability: AbilityData, targets: Array[Character], hero_system: DualHeroSystem, battle_manager: BattleManager, camera_shake: CameraShake) -> void:
	if targets.is_empty():
		return

	if camera_shake != null:
		camera_shake.shake_light()
	ComboParticles.spawn_combo_effect(battle_manager, Vector2(640, 360), ability.element, 1)

	var heal: int = ability.get_effect("heal")
	var shield: int = ability.get_effect("shield")
	var cleanse: bool = ability.get_effect("cleanse") > 0

	for target: Character in targets:
		var position: Vector2 = get_ally_vfx_position(target, hero_system)

		if heal > 0:
			var healed_amount: int = PlayerCharacter.calculate_healing(heal)
			target.heal(healed_amount)
			ComboParticles.spawn_heal_number(battle_manager, position, healed_amount)

		if shield > 0:
			var shield_amount: int = PlayerCharacter.calculate_shield(shield)
			target.add_shield(shield_amount)
			ComboParticles.spawn_shield_number(battle_manager, position, shield_amount)

		if cleanse:
			target.clear_negative_effects()

## Returns the on-screen position for visual effects above the given party
## member. Used both by heroes' combat abilities and by enemy attacks
## against the party.
static func get_ally_vfx_position(member: Character, hero_system: DualHeroSystem) -> Vector2:
	if member == hero_system.mage:
		return Vector2(200, 100)

	if member == hero_system.warrior:
		return Vector2(1000, 100)

	# Reserved for the mercenary (Companion) slot, not yet used.
	return Vector2(600, 100)
