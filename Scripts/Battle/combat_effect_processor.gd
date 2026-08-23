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
		_deal_damage(target, damage, caster, is_aoe, battle_manager)

## Applies damage to a single target and any reflection back at the caster.
static func _deal_damage(target: Character, damage: int, caster: PlayerCharacter, is_aoe: bool, battle_manager: BattleManager) -> void:
	var reflected: int = target.take_damage(damage, caster.attack_type, true)
	var particle_pos: Vector2 = Vector2(400, 200) if is_aoe else Vector2(640, 250)

	AudioManager.play_hit_sound()
	ComboParticles.spawn_damage_number(battle_manager, particle_pos, damage)

	if reflected > 0:
		caster.take_damage(reflected, target.attack_type, false)

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
