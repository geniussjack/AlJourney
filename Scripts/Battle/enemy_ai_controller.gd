class_name EnemyAIController
extends RefCounted
## Service for controlling enemy AI and processing their actions. Works
## with an arbitrary composition of the player's living party (2 heroes +
## an optional mercenary), rather than a hardcoded Mage/Warrior pair.

## Fraction of an enemy's max HP below which it becomes eligible to be
## healed instead of attacked (a wounded ally, or itself). Formalizes the
## previously-undefined "when does an enemy support instead of attack"
## decision (see design document, section 3) — open to rebalance.
const SUPPORT_HP_THRESHOLD: float = 0.5
## Chance a non-boss enemy skips its attack in favor of healing the most
## wounded eligible ally when one exists, rather than always healing.
const SUPPORT_ACTION_CHANCE: float = 0.4
## Fraction of the healed target's max HP restored by a support action.
const SUPPORT_HEAL_PERCENT: float = 0.20

## Performs one enemy's action for the current turn: a standard attack, a
## defensive/support action (healing the most wounded living ally,
## including itself), or — for the Necromancer boss, which has its own
## turn-selection logic — one of its special abilities.
static func perform_enemy_action(enemy: Enemy, battle_manager: BattleManager, camera_shake: CameraShake) -> void:
	if enemy.is_stunned:
		return

	var alive_members: Array[PlayerCharacter] = battle_manager.hero_system.get_alive_members()
	if alive_members.is_empty():
		return

	if enemy.is_boss:
		var boss_target: PlayerCharacter = _select_target(alive_members, enemy)
		_perform_necromancer_action(enemy, boss_target, battle_manager)
		return

	var living_allies: Array[Enemy] = []
	for candidate: Enemy in battle_manager.enemies:
		if candidate.is_alive:
			living_allies.append(candidate)

	var support_target: Enemy = _select_support_target(living_allies)
	if support_target != null and randf() < SUPPORT_ACTION_CHANCE:
		_execute_support_action(enemy, support_target)
		return

	var target: PlayerCharacter = _select_target(alive_members, enemy)
	_execute_standard_enemy_attack(enemy, target, battle_manager, camera_shake)

## Picks the most critically wounded living ally-enemy (including the
## acting enemy itself) below SUPPORT_HP_THRESHOLD, or null if none
## qualifies.
static func _select_support_target(living_allies: Array[Enemy]) -> Enemy:
	var most_wounded: Enemy = null
	var lowest_ratio: float = 1.0

	for ally: Enemy in living_allies:
		var max_hp: int = ally.get_total_max_health()
		if max_hp <= 0:
			continue

		var ratio: float = float(ally.current_health) / max_hp
		if ratio < SUPPORT_HP_THRESHOLD and ratio < lowest_ratio:
			most_wounded = ally
			lowest_ratio = ratio

	return most_wounded

## Heals the chosen ally instead of attacking this turn.
static func _execute_support_action(enemy: Enemy, target: Enemy) -> void:
	var heal_amount: int = ceili(target.get_total_max_health() * SUPPORT_HEAL_PERCENT)
	print("[%s] supports %s for %d HP" % [enemy.get_character_name(), target.get_character_name(), heal_amount])
	target.heal(heal_amount)

## Executes a standard (non-boss) enemy attack against the chosen target.
static func _execute_standard_enemy_attack(enemy: Enemy, target: PlayerCharacter, battle_manager: BattleManager, camera_shake: CameraShake) -> void:
	var damage: int = enemy.perform_attack()
	if damage <= 0:
		return

	AudioManager.play_attack_sound()
	if camera_shake != null:
		camera_shake.shake_light()
	var reflected: int = target.take_damage(damage, enemy.attack_type, true)
	battle_manager.add_ultimate_charge(BattleManager.ULTIMATE_CHARGE_PER_ACTION)

	AudioManager.play_hit_sound()
	var target_pos: Vector2 = CombatEffectProcessor.get_ally_vfx_position(target, battle_manager.hero_system)
	ComboParticles.spawn_damage_number(battle_manager, target_pos, damage)

	if reflected > 0:
		enemy.take_damage(reflected, target.attack_type, false)

## Picks the enemy's target: the most critically wounded (below 30% HP)
## living party member if one exists, otherwise a boss/miniboss targets the
## lowest-defense member and a regular enemy picks at random.
static func _select_target(alive_members: Array[PlayerCharacter], enemy: Enemy) -> PlayerCharacter:
	var wounded: PlayerCharacter = null
	for member: PlayerCharacter in alive_members:
		if member.current_health < member.max_health * 0.3:
			if wounded == null or member.current_health < wounded.current_health:
				wounded = member

	if wounded != null:
		return wounded

	if enemy.is_miniboss or enemy.is_boss:
		var lowest_defense: PlayerCharacter = alive_members[0]
		for member: PlayerCharacter in alive_members:
			if member.base_defense < lowest_defense.base_defense:
				lowest_defense = member
		return lowest_defense

	return alive_members[randi_range(0, alive_members.size() - 1)]

## Dispatches the Necromancer's turn to whichever special ability it cycles to.
static func _perform_necromancer_action(necromancer: Enemy, target: PlayerCharacter, battle_manager: BattleManager) -> void:
	if necromancer.is_stunned:
		return

	battle_manager.increment_necromancer_turn_count()
	var ability: Enemy.NecromancerAbility = necromancer.get_necromancer_ability(battle_manager.necromancer_turn_count)

	if ability == Enemy.NecromancerAbility.SUMMON_SKELETON:
		_execute_necromancer_summon(battle_manager)
	elif ability == Enemy.NecromancerAbility.DARK_BOLT:
		_execute_necromancer_dark_bolt(necromancer, target, battle_manager)
	elif ability == Enemy.NecromancerAbility.WEAKENING_DARKNESS:
		_execute_necromancer_weaken(battle_manager)

## Summons an extra Skeleton Warrior onto the field, up to the per-wave cap.
static func _execute_necromancer_summon(battle_manager: BattleManager) -> void:
	if battle_manager.enemies.size() < GameConstants.MAX_ENEMIES_PER_WAVE:
		var skeleton: Enemy = EnemySpawner.spawn_enemy(GameEnums.EnemyType.SKELETON_WARRIOR, battle_manager.current_wave)
		skeleton.character_died.connect(func() -> void: battle_manager.on_enemy_died(skeleton))

		battle_manager.enemies.append(skeleton)
		battle_manager.add_child(skeleton)

## A direct magical strike against the target.
static func _execute_necromancer_dark_bolt(necromancer: Enemy, target: PlayerCharacter, battle_manager: BattleManager) -> void:
	var damage: int = necromancer.perform_attack()
	var reflected: int = target.take_damage(damage, GameEnums.AttackType.MAGICAL, true)
	battle_manager.add_ultimate_charge(BattleManager.ULTIMATE_CHARGE_PER_ACTION)

	if reflected > 0:
		necromancer.take_damage(reflected, target.attack_type, false)

## Applies a brief Weakened status to the entire living party.
static func _execute_necromancer_weaken(battle_manager: BattleManager) -> void:
	var weaken_effect := StatusEffectData.new(GameEnums.StatusEffect.WEAKENED, 1, 0)
	for member: PlayerCharacter in battle_manager.hero_system.get_alive_members():
		member.apply_status_effect(weaken_effect)
