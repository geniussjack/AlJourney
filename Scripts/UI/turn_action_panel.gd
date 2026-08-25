class_name TurnActionPanel
extends Control
## Player action selection panel for turn-based combat. Implements the
## "choose fighter → choose ability" steps (target hover and confirmation
## is done by clicking an ally portrait/enemy health bar, see BattleHUD).
## Only appears during the BattlePhase.PLAYER_TURN phase.

var _battle_manager: BattleManager

var _prompt_label: Label
var _actor_row: HBoxContainer
var _ability_row: HBoxContainer

## Builds the panel's visual structure. The panel itself is added to the
## scene by the calling code (BattleScene).
func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	set_anchors_preset(Control.PRESET_FULL_RECT)

	var root := VBoxContainer.new()
	root.alignment = BoxContainer.ALIGNMENT_CENTER
	root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.set_anchors_preset(Control.PRESET_CENTER_BOTTOM)
	root.grow_vertical = Control.GROW_DIRECTION_BEGIN
	root.grow_horizontal = Control.GROW_DIRECTION_BOTH
	root.position -= Vector2(0, 130)
	root.add_theme_constant_override("separation", 8)
	add_child(root)

	_prompt_label = Label.new()
	_prompt_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	root.add_child(_prompt_label)

	_actor_row = HBoxContainer.new()
	_actor_row.alignment = BoxContainer.ALIGNMENT_CENTER
	_actor_row.add_theme_constant_override("separation", 12)
	root.add_child(_actor_row)

	_ability_row = HBoxContainer.new()
	_ability_row.alignment = BoxContainer.ALIGNMENT_CENTER
	_ability_row.add_theme_constant_override("separation", 12)
	root.add_child(_ability_row)

## Links the panel to the battle manager and subscribes to turn state changes.
func initialize(battle_manager: BattleManager) -> void:
	_battle_manager = battle_manager

	_battle_manager.turn_state_changed.connect(_refresh)
	_battle_manager.phase_changed.connect(_on_phase_changed)
	_battle_manager.ultimate_charge_changed.connect(_on_ultimate_charge_changed)

	_refresh()

func _on_ultimate_charge_changed(_charge: int, _max_charge: int) -> void:
	_refresh()

func _on_phase_changed(_new_phase: GameEnums.BattlePhase) -> void:
	_refresh()

## Rebuilds the actor/ability rows to match the battle manager's current
## turn-selection state.
func _refresh() -> void:
	_clear_children(_actor_row)
	_clear_children(_ability_row)

	if _battle_manager.current_phase != GameEnums.BattlePhase.PLAYER_TURN:
		_prompt_label.text = ""
		visible = false
		return

	visible = true

	if _battle_manager.selected_actor == null:
		_prompt_label.text = tr("UI_BATTLE_CHOOSE_ACTOR")
		for actor: PlayerCharacter in _battle_manager.pending_actors:
			var actor_button := Button.new()
			actor_button.text = actor.get_character_name()
			actor_button.pressed.connect(func() -> void: _battle_manager.select_actor(actor))
			_actor_row.add_child(actor_button)
		return

	if _battle_manager.selected_ability == null:
		_prompt_label.text = tr("UI_BATTLE_CHOOSE_ABILITY")

		# Named attack/support for the heroes, who always have one of each -
		# a mercenary's two abilities can both be the same type instead (see
		# design document, section 4), but the button wiring below doesn't
		# care which is which, just which AbilityData each button submits.
		var abilities: Array[AbilityData] = _get_actor_abilities(_battle_manager.selected_actor)
		var attack: AbilityData = abilities[0]
		var support: AbilityData = abilities[1]

		var attack_button := Button.new()
		attack_button.text = tr(attack.name)
		attack_button.pressed.connect(func() -> void: _battle_manager.select_ability(attack))
		_ability_row.add_child(attack_button)

		var support_button := Button.new()
		support_button.text = tr(support.name)
		support_button.pressed.connect(func() -> void: _battle_manager.select_ability(support))
		_ability_row.add_child(support_button)

		# Mercenaries don't have a unique ultimate - see design document,
		# section 4/9 - only Altarion and Aldric do.
		if _battle_manager.is_ultimate_ready and not _battle_manager.selected_actor.is_mercenary:
			var ultimate: AbilityData = AbilityDatabase.get_hero_ultimate(_battle_manager.selected_actor.character_class)
			var ultimate_button := Button.new()
			ultimate_button.text = "%s: %s" % [tr("UI_BATTLE_ULTIMATE_READY"), tr(ultimate.name)]
			ultimate_button.pressed.connect(func() -> void: _battle_manager.select_ability(ultimate))
			_ability_row.add_child(ultimate_button)

		return

	_prompt_label.text = tr("UI_BATTLE_CHOOSE_TARGET")

## Returns the given actor's attack/support ability pair — from
## AbilityDatabase for the two fixed heroes, or from the mercenary's own
## subclass definition for a companion (see MercenaryDatabase; mercenaries
## don't share the heroes' AbilityDatabase-driven ability pool).
static func _get_actor_abilities(actor: PlayerCharacter) -> Array[AbilityData]:
	if actor.is_mercenary:
		return [actor.mercenary_subclass.ability_one, actor.mercenary_subclass.ability_two]
	return AbilityDatabase.get_hero_abilities(actor.character_class)

## Frees every child of the given container.
static func _clear_children(container: Node) -> void:
	for child: Node in container.get_children():
		child.queue_free()

## Unsubscribes from the battle manager's events when the node is removed
## from the tree.
func _exit_tree() -> void:
	if _battle_manager != null:
		_battle_manager.turn_state_changed.disconnect(_refresh)
		_battle_manager.phase_changed.disconnect(_on_phase_changed)
		_battle_manager.ultimate_charge_changed.disconnect(_on_ultimate_charge_changed)
