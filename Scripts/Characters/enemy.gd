class_name Enemy
extends Character
## Enemy class. Inherits from the base Character class. Handles enemy
## types, their base stats, wave scaling and type-specific attacks.

## The abilities available to the Necromancer boss.
enum NecromancerAbility {
	NONE,
	SUMMON_SKELETON,
	DARK_BOLT,
	WEAKENING_DARKNESS,
}

## Base stats for every enemy type: [name_key, hp, damage, defense, attack_type, coin_reward].
const BASE_STATS_MAP: Dictionary = {
	GameEnums.EnemyType.SKELETON_WARRIOR: ["ENEMY_SKELETON_WARRIOR", GameConstants.SKELETON_WARRIOR_HP, GameConstants.SKELETON_WARRIOR_DAMAGE, GameConstants.SKELETON_WARRIOR_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_BASIC_ENEMY],
	GameEnums.EnemyType.SKELETON_ARCHER: ["ENEMY_SKELETON_ARCHER", GameConstants.SKELETON_ARCHER_HP, GameConstants.SKELETON_ARCHER_DAMAGE, GameConstants.SKELETON_ARCHER_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_BASIC_ENEMY],
	GameEnums.EnemyType.ZOMBIE: ["ENEMY_ZOMBIE", GameConstants.ZOMBIE_HP, GameConstants.ZOMBIE_DAMAGE, GameConstants.ZOMBIE_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_BASIC_ENEMY],
	GameEnums.EnemyType.SLIME: ["ENEMY_SLIME", GameConstants.SLIME_HP, GameConstants.SLIME_DAMAGE, GameConstants.SLIME_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_BASIC_ENEMY],
	GameEnums.EnemyType.DRAUGR_WARRIOR: ["ENEMY_DRAUGR_WARRIOR", GameConstants.DRAUGR_WARRIOR_HP, GameConstants.DRAUGR_WARRIOR_DAMAGE, GameConstants.DRAUGR_WARRIOR_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_BASIC_ENEMY],
	GameEnums.EnemyType.DRAUGR_DEFENDER: ["ENEMY_DRAUGR_DEFENDER", GameConstants.DRAUGR_DEFENDER_HP, GameConstants.DRAUGR_DEFENDER_DAMAGE, GameConstants.DRAUGR_DEFENDER_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_BASIC_ENEMY],
	GameEnums.EnemyType.DRAUGR_CASTER: ["ENEMY_DRAUGR_CASTER", GameConstants.DRAUGR_CASTER_HP, GameConstants.DRAUGR_CASTER_DAMAGE, GameConstants.DRAUGR_CASTER_DEFENSE, GameEnums.AttackType.MAGICAL, GameConstants.COINS_PER_BASIC_ENEMY],
	GameEnums.EnemyType.GENERAL_OF_DRAUGR: ["ENEMY_GENERAL_OF_DRAUGR", GameConstants.GENERAL_DRAUGR_HP, GameConstants.GENERAL_DRAUGR_DAMAGE, GameConstants.GENERAL_DRAUGR_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_MINIBOSS],
	GameEnums.EnemyType.ARHISKELETON: ["ENEMY_ARHISKELETON", GameConstants.ARHISKELETON_HP, GameConstants.ARHISKELETON_DAMAGE, GameConstants.ARHISKELETON_DEFENSE, GameEnums.AttackType.PHYSICAL, GameConstants.COINS_PER_MINIBOSS],
	GameEnums.EnemyType.NECROMANCER: ["ENEMY_NECROMANCER", GameConstants.NECROMANCER_HP, GameConstants.NECROMANCER_DAMAGE, GameConstants.NECROMANCER_DEFENSE, GameEnums.AttackType.MAGICAL, GameConstants.COINS_PER_BOSS],
}

var _wave_number: int

## This enemy's type.
var enemy_type: GameEnums.EnemyType

## The gold coin reward for defeating this enemy.
var coin_reward: int

## The shared party XP reward for defeating this enemy.
var xp_reward: int

## The original number of creatures in the stack, as set at creation.
var _stack_count_raw: int = 1

## The current number of creatures still represented in the stack, derived
## from the remaining HP proportion of the original stack size.
var stack_count: int:
	get:
		if _stack_count_raw > 0:
			return ceili(current_health / (float(get_total_max_health()) / _stack_count_raw))
		return 1

## Whether this enemy is a miniboss.
var is_miniboss: bool:
	get:
		return enemy_type == GameEnums.EnemyType.GENERAL_OF_DRAUGR or enemy_type == GameEnums.EnemyType.ARHISKELETON

## Whether this enemy is the main boss.
var is_boss: bool:
	get:
		return enemy_type == GameEnums.EnemyType.NECROMANCER

## Returns the character's display name, translated, with a stack-size
## suffix ("x3") when more than one creature remains in the stack.
func get_character_name() -> String:
	return "%s x%d" % [tr(_name), stack_count] if stack_count > 1 else tr(_name)

## Factory method that creates and initializes a new enemy of the given type.
## wave_number: the current wave number, used to scale the enemy's stats.
## stack_count: the number of enemies in the stack.
static func create(enemy_type: GameEnums.EnemyType, wave_number: int, stack_count: int = 1) -> Enemy:
	var enemy := Enemy.new()
	enemy.enemy_type = enemy_type
	enemy._wave_number = wave_number
	enemy._stack_count_raw = stack_count

	var stats: Array = _get_enemy_base_stats(enemy_type)
	var name_key: String = stats[0]
	var hp: int = stats[1]
	var damage: int = stats[2]
	var defense: int = stats[3]
	var attack_type: GameEnums.AttackType = stats[4]
	var coin_reward: int = stats[5]

	var scaled_hp: int = ScalingSystem.scale_enemy_stat(hp, wave_number)
	var scaled_dmg: int = ScalingSystem.scale_enemy_stat(damage, wave_number)
	var scaled_defense: int = ScalingSystem.scale_enemy_stat(defense, wave_number)
	var scaled_reward: int = ScalingSystem.scale_reward(coin_reward, wave_number)

	var total_hp: int = scaled_hp * stack_count
	# Damage and defense are accounted for by the methods below, factoring in stack_count.

	enemy.initialize(name_key, total_hp, scaled_dmg, scaled_defense, attack_type)
	enemy.coin_reward = scaled_reward * stack_count

	var xp_base: int
	if enemy.is_boss:
		xp_base = GameConstants.XP_PER_BOSS
	elif enemy.is_miniboss:
		xp_base = GameConstants.XP_PER_MINIBOSS
	else:
		xp_base = GameConstants.XP_PER_BASIC_ENEMY
	enemy.xp_reward = ScalingSystem.scale_reward(xp_base, wave_number) * stack_count

	print("[Enemy] Created %s x%d (Wave %d) - Total HP: %d, Base DMG: %d, Base DEF: %d, Reward: %d, XP: %d" % [name_key, stack_count, wave_number, total_hp, scaled_dmg, scaled_defense, enemy.coin_reward, enemy.xp_reward])
	return enemy

## Looks up an enemy type's base stats, falling back to a generic weak
## enemy if the type is somehow missing from BASE_STATS_MAP.
static func _get_enemy_base_stats(type: GameEnums.EnemyType) -> Array:
	return BASE_STATS_MAP.get(type, ["Unknown Enemy", 10, 5, 0, GameEnums.AttackType.PHYSICAL, 1])

## Computes and returns the damage this enemy deals on the current turn.
## Accounts for certain enemy types' special behavior.
func perform_attack() -> int:
	if not is_alive or is_stunned:
		return 0

	var damage: int = _base_damage * stack_count

	match enemy_type:
		GameEnums.EnemyType.ARHISKELETON:
			damage = _base_damage * GameConstants.ARHISKELETON_ARROWS_PER_TURN
			print("[%s] fires %d arrows for %d damage!" % [_name, GameConstants.ARHISKELETON_ARROWS_PER_TURN, damage])

		GameEnums.EnemyType.GENERAL_OF_DRAUGR:
			if randf() < 0.25:
				damage = ceili(_base_damage * 1.5)
				print("[%s] uses magic attack for %d damage!" % [_name, damage])
			else:
				print("[%s] attacks for %d damage!" % [_name, damage])

		GameEnums.EnemyType.NECROMANCER:
			print("[%s] prepares dark magic..." % _name)

		_:
			print("[%s] attacks for %d damage!" % [_name, damage])

	if has_status_effect(GameEnums.StatusEffect.FREEZE):
		damage = ceili(damage * 0.7)
		print("[%s] Damage reduced by Freeze status: %d" % [_name, damage])

	return damage

## Determines which ability the Necromancer will use on the current turn.
## The Necromancer cycles through its abilities.
func get_necromancer_ability(turn_number: int) -> NecromancerAbility:
	if enemy_type != GameEnums.EnemyType.NECROMANCER:
		return NecromancerAbility.NONE
	return (turn_number % 3) + 1

## Plays a fade-out tween before freeing the enemy node on death.
func _on_death() -> void:
	super._on_death()
	var tween: Tween = create_tween()
	tween.tween_property(self, "modulate:a", 0.0, 0.5)
	tween.tween_callback(queue_free)
