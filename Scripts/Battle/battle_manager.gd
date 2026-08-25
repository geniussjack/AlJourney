class_name BattleManager
extends Node
## Manager for the turn-based combat system, instantiated per battle scene
## (not an autoload). Manages the player party's turn queue (the player
## chooses which of the living combatants acts next, then their ability and
## target), enemy turns, enemy waves, and damage and loot payouts.

## Raised at the start of a new battle.
signal battle_started
## Raised when the current battle phase changes.
signal phase_changed(new_phase: GameEnums.BattlePhase)
## Raised on any change to the player's turn selection state (actor
## selection, ability selection, target resolution). Used by the
## target-selection UI to refresh itself.
signal turn_state_changed
## Raised when every wave of the current level has been cleared (the level
## is fully completed). A level can consist of several consecutive waves
## (see wave_advanced) — this signal only fires once the last one is cleared.
signal level_completed
## Raised when advancing to the next wave within the same level (not the
## last one) — combat continues without leaving the scene, but the UI (e.g.
## enemy health bars in BattleHUD) needs to refresh for the new enemies
## lineup.
## wave_index: the new current wave's index (zero-based) within the level.
signal wave_advanced(wave_index: int, total_waves: int)
## Raised when the battle ends.
## player_won: true if the player won.
signal battle_ended(player_won: bool)
## Raised every time one of the enemies dies.
signal enemy_defeated(enemy: Enemy)
## Raised when the party's shared ultimate charge changes.
signal ultimate_charge_changed(charge: int, max_charge: int)

## The maximum value of the party's shared ultimate charge.
const MAX_ULTIMATE_CHARGE: int = 100
## The fixed ultimate charge gained per successful action (a party attack
## landing on an enemy, or an enemy attack landing on the party).
const ULTIMATE_CHARGE_PER_ACTION: int = 25

## The Necromancer boss's turn counter, used to cycle through its abilities.
var necromancer_turn_count: int = 0

func increment_necromancer_turn_count() -> void:
	necromancer_turn_count += 1

var _camera_shake: CameraShake
var _battle_ended_signaled: bool = false
var _is_level_completed: bool = false
var _level: LevelDefinition
var _current_wave_index: int = 0

var _pending_actors: Array[PlayerCharacter] = []

## The party's current total ultimate charge (0..MAX_ULTIMATE_CHARGE).
var ultimate_charge: int = 0

## True if the ultimate charge is full and it's ready to use.
var is_ultimate_ready: bool:
	get:
		return ultimate_charge >= MAX_ULTIMATE_CHARGE

## The current battle phase.
var current_phase: GameEnums.BattlePhase = GameEnums.BattlePhase.PLAYER_TURN

## The current level's difficulty (see LevelDefinition.difficulty_rating),
## used as the input to ScalingSystem — shared by every wave of a single level.
var current_wave: int = 0

## The current wave's index (zero-based) within the level's waves.
var current_wave_index: int:
	get:
		return _current_wave_index

## The total number of waves in the current level.
var total_waves_in_level: int:
	get:
		return _level.waves.size() if _level != null else 0

## Reference to the hero party system.
var hero_system: DualHeroSystem

## The list of every enemy currently active on the field.
var enemies: Array[Enemy] = []

## Party members who have not yet acted in the current round.
var pending_actors: Array[PlayerCharacter]:
	get:
		return _pending_actors

## The actor selected by the player for the current turn.
var selected_actor: PlayerCharacter = null

## The ability selected for the current turn.
var selected_ability: AbilityData = null

## Initializes the battle manager.
func _ready() -> void:
	enemies = []
	current_phase = GameEnums.BattlePhase.PLAYER_TURN
	necromancer_turn_count = 0
	_battle_ended_signaled = false
	_is_level_completed = false
	ultimate_charge = 0

	print("[BattleManager] Initialized for party-based turn combat")

## Starts the battle for the given campaign map level with the provided
## party system. The level's waves spawn one after another as they're
## cleared, without leaving combat (see _on_enemies_cleared()).
func start_battle(party_hero_system: DualHeroSystem, level: LevelDefinition, camera_shake: CameraShake = null) -> void:
	hero_system = party_hero_system
	_level = level
	current_wave = level.difficulty_rating
	_current_wave_index = 0
	necromancer_turn_count = 0
	_camera_shake = camera_shake
	_battle_ended_signaled = false
	_is_level_completed = false
	ultimate_charge = 0

	hero_system.party_defeated.connect(_on_party_defeated)

	_spawn_current_wave()
	_start_player_turn()

	battle_started.emit()

## Begins the player's turn: processes party status effects and resets the
## turn-order queue.
func _start_player_turn() -> void:
	hero_system.process_status_effects()

	_pending_actors = hero_system.get_alive_members()
	selected_actor = null
	selected_ability = null

	_change_phase(GameEnums.BattlePhase.PLAYER_TURN)

## Sets the current phase and notifies subscribers.
func _change_phase(new_phase: GameEnums.BattlePhase) -> void:
	current_phase = new_phase
	phase_changed.emit(current_phase)

## Selects the actor who will take the next turn. The player determines the
## turn order among the living party members who haven't acted yet this round.
func select_actor(actor: PlayerCharacter) -> void:
	if current_phase != GameEnums.BattlePhase.PLAYER_TURN or actor == null or not _pending_actors.has(actor):
		return

	selected_actor = actor
	selected_ability = null
	turn_state_changed.emit()

## Selects the ability the chosen actor will use: attack, support, or (once
## fully charged) the ultimate ability. The ultimate resolves immediately,
## without a separate target confirmation — it either hits an area, or
## picks its own target by its own rules (see _resolve_ultimate()).
func select_ability(ability: AbilityData) -> void:
	if current_phase != GameEnums.BattlePhase.PLAYER_TURN or selected_actor == null or ability == null:
		return

	if ability.is_ultimate:
		if not is_ultimate_ready:
			return

		_resolve_ultimate(selected_actor, ability)
		return

	selected_ability = ability
	turn_state_changed.emit()

## Returns the list of valid targets for the selected ability's targeting.
func get_valid_targets() -> Array[Character]:
	if selected_ability == null:
		return []

	var allies: Array[Character] = []
	allies.assign(hero_system.get_alive_members())
	var alive_enemies: Array[Character] = _get_alive_enemies()

	return AbilityTargetingRules.get_valid_targets(selected_ability.target_type, allies, alive_enemies)

## Confirms the target and immediately resolves the selected ability's
## effect. If this was the last party member who hadn't acted yet, the
## enemy turn begins.
func confirm_target(target: Character) -> void:
	if current_phase != GameEnums.BattlePhase.PLAYER_TURN or selected_actor == null or selected_ability == null:
		return

	if not get_valid_targets().has(target):
		return

	_resolve_ability(selected_actor, selected_ability, target)
	_advance_turn_after_action(selected_actor)

## Resolves a chosen (non-ultimate) ability against its confirmed target(s).
func _resolve_ability(caster: PlayerCharacter, ability: AbilityData, primary_target: Character) -> void:
	var allies: Array[Character] = []
	allies.assign(hero_system.get_alive_members())
	var alive_enemies: Array[Character] = _get_alive_enemies()
	var targets: Array[Character] = AbilityTargetingRules.resolve_effect_targets(
		ability.target_type, ability.is_aoe, primary_target, allies, alive_enemies
	)

	if ability.is_attack_ability:
		CombatEffectProcessor.apply_attack_ability(ability, caster, targets, self, _camera_shake)
		if not targets.is_empty():
			add_ultimate_charge(ULTIMATE_CHARGE_PER_ACTION)
	else:
		CombatEffectProcessor.apply_support_ability(ability, targets, hero_system, self, _camera_shake)

## Immediately resolves the selected actor's ultimate ability and resets
## the party's shared charge. AoE ultimates hit every living enemy;
## single-target ones hit the enemy with the highest current HP
## (auto-selected, without player involvement).
func _resolve_ultimate(caster: PlayerCharacter, ultimate: AbilityData) -> void:
	var alive_enemies: Array[Character] = _get_alive_enemies()
	var targets: Array[Character]

	if ultimate.is_aoe:
		targets = alive_enemies
	else:
		var highest_health_enemy: Character = AbilityTargetingRules.select_highest_health_target(alive_enemies)
		targets = [] if highest_health_enemy == null else [highest_health_enemy]

	CombatEffectProcessor.apply_attack_ability(ultimate, caster, targets, self, _camera_shake)

	ultimate_charge = 0
	ultimate_charge_changed.emit(ultimate_charge, MAX_ULTIMATE_CHARGE)

	_advance_turn_after_action(caster)

## Removes an actor from the queue of party members who haven't acted this
## round, and either passes the turn to the next party member, or (if this
## was the last one) starts the enemy turn.
func _advance_turn_after_action(actor: PlayerCharacter) -> void:
	_pending_actors.erase(actor)
	selected_actor = null
	selected_ability = null

	if _pending_actors.is_empty():
		_start_enemy_turn()
	else:
		turn_state_changed.emit()

## Consumes a Herbalist-brewed potion on behalf of the current actor and
## applies its effect immediately (no target selection — see PotionData:
## SINGLE_HEAL always targets the actor who drank it). Ends the actor's
## turn, the same as using an ability.
## Returns true if the potion was available and used.
func use_potion(potion: PotionData) -> bool:
	if current_phase != GameEnums.BattlePhase.PLAYER_TURN or selected_actor == null or potion == null:
		return false

	if not GameStateManager.use_potion(potion.id):
		return false

	var actor: PlayerCharacter = selected_actor

	match potion.potion_type:
		GameEnums.PotionType.SINGLE_HEAL:
			actor.heal(PlayerCharacter.calculate_healing(potion.effect_value))
		GameEnums.PotionType.PARTY_HEAL:
			for member: PlayerCharacter in hero_system.get_alive_members():
				member.heal(PlayerCharacter.calculate_healing(potion.effect_value))
		GameEnums.PotionType.ULTIMATE_FILL:
			add_ultimate_charge(potion.effect_value)

	print("[BattleManager] %s used potion %s" % [actor.get_character_name(), potion.id])

	_advance_turn_after_action(actor)
	return true

## Increases (clamped) the party's shared ultimate charge and notifies
## subscribers. Called both for successful party attacks and for enemy
## attacks landing on the party.
func add_ultimate_charge(amount: int) -> void:
	var clamped: int = clampi(ultimate_charge + amount, 0, MAX_ULTIMATE_CHARGE)
	if clamped == ultimate_charge:
		return

	ultimate_charge = clamped
	ultimate_charge_changed.emit(ultimate_charge, MAX_ULTIMATE_CHARGE)

## Spawns the enemies for the level's current wave (_current_wave_index)
## using the curated composition from LevelDefinition.waves, replacing the
## previous enemies lineup.
func _spawn_current_wave() -> void:
	# The base wave's enemies aren't added to the scene tree, but creatures
	# summoned by a boss (see EnemyAIController._execute_necromancer_summon)
	# are — and they need to be explicitly freed here, otherwise on
	# advancing to the next wave within the level they'd remain hanging as
	# BattleManager child nodes until the end of the whole battle.
	for leftover: Enemy in enemies:
		if leftover.is_inside_tree():
			leftover.queue_free()
	enemies.clear()

	var wave: WaveDefinition = _level.waves[_current_wave_index]
	for spawn: EnemySpawnDefinition in wave.enemies:
		var enemy: Enemy = EnemySpawner.spawn_enemy(spawn.type, current_wave, spawn.count)
		enemy.character_died.connect(func() -> void: on_enemy_died(enemy))
		enemies.append(enemy)

## Plays out the enemy turn: every living enemy acts in sequence with a
## short pause between them, then hands control back to the player (or ends
## the wave if every enemy is now dead).
func _start_enemy_turn() -> void:
	_change_phase(GameEnums.BattlePhase.ENEMY_TURN)

	var active_enemies: Array[Enemy] = []
	for enemy: Enemy in enemies:
		if enemy.is_alive:
			active_enemies.append(enemy)

	for enemy: Enemy in active_enemies:
		enemy.process_status_effects()

	for enemy: Enemy in active_enemies:
		if not enemy.is_alive:
			continue

		EnemyAIController.perform_enemy_action(enemy, self, _camera_shake)
		await get_tree().create_timer(0.5).timeout

	if hero_system.get_alive_members().is_empty():
		return

	if _all_enemies_dead():
		_on_enemies_cleared()
		return

	_start_player_turn()

## Called when an enemy dies: pays out coins/loot and checks whether the
## wave is now cleared.
func on_enemy_died(enemy: Enemy) -> void:
	enemy_defeated.emit(enemy)

	GameStateManager.add_coins(enemy.coin_reward)
	_grant_party_xp(enemy.xp_reward)

	if enemy.is_boss or enemy.is_miniboss:
		_generate_boss_loot()
		_grant_rarity_catalysts()
	elif randf() <= 0.20:
		var item: EquipmentData = LootSystem.generate_normal_loot(current_wave)
		if item != null:
			InventoryManager.add_items([item])

	if _all_enemies_dead():
		_on_enemies_cleared.call_deferred()

## Whether every enemy currently on the field is dead.
func _all_enemies_dead() -> bool:
	for enemy: Enemy in enemies:
		if enemy.is_alive:
			return false
	return true

## Returns every enemy currently alive, typed as Character for targeting rules.
func _get_alive_enemies() -> Array[Character]:
	var result: Array[Character] = []
	for enemy: Enemy in enemies:
		if enemy.is_alive:
			result.append(enemy)
	return result

## Ends the battle in defeat once the whole party has fallen.
func _on_party_defeated() -> void:
	if _battle_ended_signaled:
		return

	_battle_ended_signaled = true
	GameStateManager.on_battle_completed()
	battle_ended.emit(false)

## Rolls and grants loot for defeating a boss or miniboss.
func _generate_boss_loot() -> void:
	var loot: Array[EquipmentData] = LootSystem.generate_boss_loot(current_wave)
	InventoryManager.add_items(loot)

## Grants both archetypes' rarity-upgrade catalysts if the current level is
## one of the campaign's designated catalyst sources (see design document,
## section 10 - RarityCatalystDatabase.level_catalyst_rarity). No-op for
## every other boss/miniboss level.
func _grant_rarity_catalysts() -> void:
	if not RarityCatalystDatabase.level_catalyst_rarity.has(_level.id):
		return

	var rarity: GameEnums.EquipmentRarity = RarityCatalystDatabase.level_catalyst_rarity[_level.id]
	for archetype: GameEnums.CharacterClass in GameEnums.CharacterClass.values():
		var catalyst: RarityCatalystData = RarityCatalystDatabase.get_catalyst(archetype, rarity)
		if catalyst != null:
			GameStateManager.add_catalyst(catalyst.id)

## Grants shared party XP and, for every level gained, applies the flat
## stat bonus to every live party member. GameStateManager only tracks the
## level/XP counters in the save — it has no reference to the active
## DualHeroSystem, so applying bonuses to the live Character instances is
## this manager's job (it already owns hero_system for the battle).
func _grant_party_xp(amount: int) -> void:
	var levels_gained: int = GameStateManager.add_party_xp(amount)
	for i: int in range(levels_gained):
		for member: PlayerCharacter in hero_system.get_party_members():
			member.increase_max_health(GameConstants.PARTY_LEVEL_HP_BONUS)
			member.increase_damage(GameConstants.PARTY_LEVEL_DAMAGE_BONUS)
			member.increase_defense(GameConstants.PARTY_LEVEL_DEFENSE_BONUS)

## Called when every enemy on the field is dead. Can fire from two
## independent places — on_enemy_died() deferred (via call_deferred) and
## _start_enemy_turn() synchronously right after the enemy turn — so it
## re-checks the current state of enemies at the start rather than relying
## solely on being called: by the time the deferred call fires, the wave
## may have already changed (see _spawn_current_wave()), in which case the
## repeat call should silently do nothing instead of firing against an
## already-different, alive wave. The separate _is_level_completed flag
## only guards the branch that completes the level as a whole, where the
## enemies lineup stays unchanged (all dead) until the end of the battle.
func _on_enemies_cleared() -> void:
	if _is_level_completed or enemies.is_empty() or not _all_enemies_dead():
		return

	if _current_wave_index + 1 < _level.waves.size():
		_current_wave_index += 1
		_spawn_current_wave()

		wave_advanced.emit(_current_wave_index, _level.waves.size())
		_start_player_turn()
		return

	_is_level_completed = true

	_change_phase(GameEnums.BattlePhase.WAVE_TRANSITION)
	level_completed.emit()

	var stats: Dictionary = hero_system.get_combined_stats()
	GameStateManager.update_hero_stats(
		stats["mage_health"], stats["mage_max_health"], stats["mage_damage"], stats["mage_defense"],
		stats["warrior_health"], stats["warrior_max_health"], stats["warrior_damage"], stats["warrior_defense"]
	)

	GameStateManager.complete_level(_level.id)
	GameStateManager.on_battle_completed()

## Clears the battle state, unsubscribes from signals and removes enemies.
## Called when transitioning to the results screen or a menu.
func end_battle() -> void:
	if hero_system != null and hero_system.party_defeated.is_connected(_on_party_defeated):
		hero_system.party_defeated.disconnect(_on_party_defeated)

	for enemy: Enemy in enemies:
		enemy.queue_free()
	enemies.clear()
