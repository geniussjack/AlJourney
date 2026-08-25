extends Control
## Ability browser UI for the mage and warrior (legacy ability system — see
## design document, section 4).

var _close_button: Button
var _mage_abilities_container: VBoxContainer
var _warrior_abilities_container: VBoxContainer

func _ready() -> void:
	_close_button = get_node("MarginContainer/VBoxContainer/Header/CloseButton")
	_mage_abilities_container = get_node("MarginContainer/VBoxContainer/ContentHBox/MageSection/ScrollContainer/MageAbilitiesContainer")
	_warrior_abilities_container = get_node("MarginContainer/VBoxContainer/ContentHBox/WarriorSection/ScrollContainer/WarriorAbilitiesContainer")

	_close_button.pressed.connect(_on_close_pressed)

	_refresh_ui()

	print("[AbilitiesUI] Initialized")

## Rebuilds both heroes' ability lists from AbilitySystem.
func _refresh_ui() -> void:
	_populate_abilities_container(_mage_abilities_container, GameEnums.CharacterClass.MAGE)
	_populate_abilities_container(_warrior_abilities_container, GameEnums.CharacterClass.WARRIOR)

## Rebuilds one hero's ability button list.
static func _populate_abilities_container(container: VBoxContainer, hero_class: GameEnums.CharacterClass) -> void:
	for child: Node in container.get_children():
		child.queue_free()

	var abilities: Array[AbilityData] = AbilitySystem.get_available_abilities(hero_class)
	for ability: AbilityData in abilities:
		var btn := Button.new()
		btn.text = "%s (Cost: %d)" % [ability.name, ability.unlock_cost]
		btn.custom_minimum_size = Vector2(0, 50)

		btn.pressed.connect(func() -> void: _on_ability_pressed(ability, hero_class))
		container.add_child(btn)

## Placeholder for unlock/equip logic.
static func _on_ability_pressed(ability: AbilityData, _hero_class: GameEnums.CharacterClass) -> void:
	print("[AbilitiesUI] Ability pressed: %s" % ability.name)

func _on_close_pressed() -> void:
	print("[AbilitiesUI] Closing abilities menu")
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")
	queue_free()
