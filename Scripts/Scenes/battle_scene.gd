extends Node
## Main controller for the battle scene. Wires together and coordinates
## every system involved in combat: the UI, the hero party system, and the
## turn-based combat manager.

var _battle_hud: BattleHUD
var _turn_action_panel: TurnActionPanel
var _battle_manager: BattleManager
var _hero_system: DualHeroSystem
var _camera: Camera2D
var _camera_shake: CameraShake

var _is_battle_transition_queued: bool = false
var _level: LevelDefinition

## Sets up the camera, loads hero data from the save, initializes the UI,
## and subscribes to events. Starts the battle for the current wave.
func _ready() -> void:
	_battle_hud = get_node("CanvasLayer/BattleHUD")
	_battle_manager = get_node("BattleManager")
	_is_battle_transition_queued = false

	_camera = Camera2D.new()
	_camera.enabled = true
	_camera.position = Vector2(960, 540)
	add_child(_camera)

	_camera_shake = CameraShake.new()
	_camera.add_child(_camera_shake)

	_hero_system = DualHeroSystem.new()
	add_child(_hero_system)

	_initialize_heroes()
	_initialize_companion()

	_battle_hud.initialize(_hero_system, _battle_manager)

	_turn_action_panel = TurnActionPanel.new()
	(get_node("CanvasLayer") as CanvasLayer).add_child(_turn_action_panel)

	_battle_manager.level_completed.connect(_on_level_completed)
	_battle_manager.wave_advanced.connect(_on_wave_advanced)
	_battle_manager.battle_ended.connect(_on_battle_ended)
	_battle_manager.enemy_defeated.connect(_on_enemy_defeated)

	_level = CampaignDatabase.get_level(GameStateManager.current_level_id)
	if _level == null:
		_level = CampaignDatabase.get_level(CampaignDatabase.FIRST_LEVEL_ID)

	GameStateManager.start_level(_level)
	_battle_manager.start_battle(_hero_system, _level, _camera_shake)

	_turn_action_panel.initialize(_battle_manager)

	_battle_hud.setup_enemies(_battle_manager.enemies)

	_start_portrait_animations()

	print("[BattleScene] Battle started - Level %s (difficulty %d)" % [_level.id, _level.difficulty_rating])

## Kicks off the idle bob animation for both hero portraits.
func _start_portrait_animations() -> void:
	var mage: TextureRect = get_node_or_null("CanvasLayer/DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/MageRow/MagePortraitContainer/MagePortrait")
	var warrior: TextureRect = get_node_or_null("CanvasLayer/DecorativeLayer/LeftPanel/MarginContainer/VBoxContainer/WarriorRow/WarriorPortraitContainer/WarriorPortrait")

	print("[BattleScene] Portraits loaded: Mage=%s, Warrior=%s" % [mage != null, warrior != null])

	_animate_portrait(mage)
	_animate_portrait(warrior)

## Starts a subtle idle bob/scale loop on a single portrait.
func _animate_portrait(portrait: TextureRect) -> void:
	if portrait == null:
		return

	portrait.pivot_offset = portrait.size / 2
	if portrait.pivot_offset == Vector2.ZERO:
		portrait.pivot_offset = Vector2(48, 48)  # Fallback.

	print("[BattleScene] Animating portrait: %s with PivotOffset=%s" % [portrait.name, portrait.pivot_offset])

	var tween: Tween = create_tween()
	tween.set_loops()
	tween.set_trans(Tween.TRANS_SINE)
	tween.set_ease(Tween.EASE_IN_OUT)

	var delay: float = randf() * 0.5
	var dur1: float = 1.0 + (randf() * 0.2)
	var dur2: float = 1.0 + (randf() * 0.2)

	tween.tween_interval(delay)
	tween.tween_property(portrait, "scale", Vector2(1.1, 1.1), dur1)
	tween.parallel().tween_property(portrait, "position", portrait.position - Vector2(0, 4), dur1)
	tween.tween_property(portrait, "scale", Vector2(1.0, 1.0), dur2)
	tween.parallel().tween_property(portrait, "position", portrait.position, dur2)

## Loads hero stats from the current save into the party system.
func _initialize_heroes() -> void:
	var save_data: SaveData = GameStateManager.current_save

	if save_data != null:
		_hero_system.load_from_save(
			save_data.mage_health, save_data.mage_max_health, save_data.mage_damage, save_data.mage_defense,
			save_data.warrior_health, save_data.warrior_max_health, save_data.warrior_damage, save_data.warrior_defense
		)

		print("[BattleScene] Heroes loaded from save")
	else:
		print("[BattleScene] New heroes created with base stats")

## Fills the party's third slot with the currently active mercenary, if
## one is set (see design document, section 9). No-op if the slot is
## empty, or if the active mercenary somehow isn't available anymore
## (defensive - GameStateManager only lets an available one become active
## in the first place, but a save could theoretically be edited by hand).
func _initialize_companion() -> void:
	var subclass: MercenarySubclassData = GameStateManager.get_active_mercenary()
	if subclass == null or not GameStateManager.is_mercenary_unlocked(subclass):
		return

	_hero_system.set_companion(PlayerCharacter.create_mercenary(subclass))
	print("[BattleScene] Companion joined the battle: %s" % subclass.get_key())

func _on_enemy_defeated(enemy: Enemy) -> void:
	print("[BattleScene] Enemy defeated: %s" % enemy.get_character_name())

## Called once every wave of the current level has been cleared. The level
## (not the wave) is the unit of exiting combat: the shop no longer opens
## here, the player returns to the campaign map. If the cleared level was
## the Necromancer's, the defeat cutscene plays first, then the run ends
## with the victory screen instead of returning to the map.
func _on_level_completed() -> void:
	if _is_battle_transition_queued:
		return

	_is_battle_transition_queued = true
	print("[BattleScene] Level completed! Transitioning to campaign map...")

	_save_hero_stats()

	var defeated_necromancer: bool = false
	for wave: WaveDefinition in _level.waves:
		for spawn: EnemySpawnDefinition in wave.enemies:
			if spawn.type == GameEnums.EnemyType.NECROMANCER:
				defeated_necromancer = true

	get_tree().create_timer(1.0).timeout.connect(func() -> void:
		if defeated_necromancer:
			CutscenePlayer.play(get_node("CanvasLayer"), CutsceneDatabase.necromancer_defeat, SceneManager.go_to_victory)
		else:
			SceneManager.go_to_map()
	)

## Called when advancing to the next wave within the same level (combat
## continues without leaving the scene) — the enemy health bars need to
## refresh for the new lineup.
func _on_wave_advanced(wave_index: int, total_waves: int) -> void:
	_battle_hud.setup_enemies(_battle_manager.enemies)
	print("[BattleScene] Advanced to wave %d/%d within the level" % [wave_index + 1, total_waves])

func _on_battle_ended(player_won: bool) -> void:
	if not player_won:
		if _is_battle_transition_queued:
			return

		_is_battle_transition_queued = true
		print("[BattleScene] Battle lost - transitioning to Game Over...")

		get_tree().create_timer(1.5).timeout.connect(SceneManager.game_over)

## Persists the party's current stats back into the save.
func _save_hero_stats() -> void:
	var stats: Dictionary = _hero_system.get_combined_stats()
	GameStateManager.update_hero_stats(
		stats["mage_health"], stats["mage_max_health"], stats["mage_damage"], stats["mage_defense"],
		stats["warrior_health"], stats["warrior_max_health"], stats["warrior_damage"], stats["warrior_defense"]
	)

	print("[BattleScene] Hero stats saved")

## Unsubscribes from every manager event to avoid memory leaks and calls
## into destroyed objects, and properly wraps up the battle.
func _exit_tree() -> void:
	if _battle_manager != null:
		_battle_manager.level_completed.disconnect(_on_level_completed)
		_battle_manager.wave_advanced.disconnect(_on_wave_advanced)
		_battle_manager.battle_ended.disconnect(_on_battle_ended)
		_battle_manager.enemy_defeated.disconnect(_on_enemy_defeated)
		_battle_manager.end_battle()
