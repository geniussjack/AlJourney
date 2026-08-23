extends Control
## UI for the in-game shop. Lets the player spend coins to upgrade hero
## stats between waves.

enum UpgradeType {
	MAGE_HEALTH, MAGE_DAMAGE, MAGE_DEFENSE,
	WARRIOR_HEALTH, WARRIOR_DAMAGE, WARRIOR_DEFENSE,
}

var _wave_label: Label
var _coins_label: Label
var _continue_button: Button
var _home_button: Button

var _mage_health_button: Button
var _mage_damage_button: Button
var _mage_defense_button: Button

var _warrior_health_button: Button
var _warrior_damage_button: Button
var _warrior_defense_button: Button

var _mage_health_label: Label
var _mage_damage_label: Label
var _mage_defense_label: Label
var _warrior_health_label: Label
var _warrior_damage_label: Label
var _warrior_defense_label: Label

var _mage_health_cost_label: Label
var _mage_damage_cost_label: Label
var _mage_defense_cost_label: Label
var _warrior_health_cost_label: Label
var _warrior_damage_cost_label: Label
var _warrior_defense_cost_label: Label

var _mage_health_price: int
var _mage_damage_price: int
var _mage_defense_price: int
var _warrior_health_price: int
var _warrior_damage_price: int
var _warrior_defense_price: int

var _mage_health_upgrade: int
var _mage_damage_upgrade: int
var _mage_defense_upgrade: int
var _warrior_health_upgrade: int
var _warrior_damage_upgrade: int
var _warrior_defense_upgrade: int

## Sets up all labels and buttons for each upgrade type, subscribes to
## purchase events, and initializes the shop data.
func _ready() -> void:
	_wave_label = get_node("MarginContainer/VBoxContainer/Header/WaveLabel")
	_coins_label = get_node("MarginContainer/VBoxContainer/Header/CoinsContainer/CoinsLabel")
	_continue_button = get_node("MarginContainer/VBoxContainer/BottomRow/ContinueButton")
	_home_button = get_node("MarginContainer/VBoxContainer/BottomRow/HomeButton")

	_mage_health_button = get_node("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/HealthUpgrade/BuyButton")
	_mage_damage_button = get_node("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DamageUpgrade/BuyButton")
	_mage_defense_button = get_node("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DefenseUpgrade/BuyButton")

	_warrior_health_button = get_node("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/HealthUpgrade/BuyButton")
	_warrior_damage_button = get_node("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DamageUpgrade/BuyButton")
	_warrior_defense_button = get_node("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DefenseUpgrade/BuyButton")

	_mage_health_label = get_node("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/HealthUpgrade/PriceLabel")
	_mage_damage_label = get_node("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DamageUpgrade/PriceLabel")
	_mage_defense_label = get_node("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/DefenseUpgrade/PriceLabel")
	_warrior_health_label = get_node("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/HealthUpgrade/PriceLabel")
	_warrior_damage_label = get_node("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DamageUpgrade/PriceLabel")
	_warrior_defense_label = get_node("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/DefenseUpgrade/PriceLabel")

	_mage_health_cost_label = _add_cost_ui(_mage_health_button.get_parent())
	_mage_damage_cost_label = _add_cost_ui(_mage_damage_button.get_parent())
	_mage_defense_cost_label = _add_cost_ui(_mage_defense_button.get_parent())
	_warrior_health_cost_label = _add_cost_ui(_warrior_health_button.get_parent())
	_warrior_damage_cost_label = _add_cost_ui(_warrior_damage_button.get_parent())
	_warrior_defense_cost_label = _add_cost_ui(_warrior_defense_button.get_parent())

	_mage_health_button.pressed.connect(func() -> void: _on_upgrade_purchased(UpgradeType.MAGE_HEALTH))
	_mage_damage_button.pressed.connect(func() -> void: _on_upgrade_purchased(UpgradeType.MAGE_DAMAGE))
	_mage_defense_button.pressed.connect(func() -> void: _on_upgrade_purchased(UpgradeType.MAGE_DEFENSE))
	_warrior_health_button.pressed.connect(func() -> void: _on_upgrade_purchased(UpgradeType.WARRIOR_HEALTH))
	_warrior_damage_button.pressed.connect(func() -> void: _on_upgrade_purchased(UpgradeType.WARRIOR_DAMAGE))
	_warrior_defense_button.pressed.connect(func() -> void: _on_upgrade_purchased(UpgradeType.WARRIOR_DEFENSE))
	_continue_button.pressed.connect(_on_continue_pressed)
	_home_button.pressed.connect(_on_home_pressed)

	_continue_button.text = "UI_PAUSE_RESUME"  # Can just use continue/resume.
	_home_button.text = "UI_PAUSE_MAIN_MENU"

	get_node("MarginContainer/VBoxContainer/ShopContainer/MageUpgrades/MageTitle").text = tr("UI_BATTLE_ALTARION") + " (" + tr("CHARACTER_MAGE") + ")"
	get_node("MarginContainer/VBoxContainer/ShopContainer/WarriorUpgrades/WarriorTitle").text = tr("UI_BATTLE_ALDRIC") + " (" + tr("CHARACTER_WARRIOR") + ")"

	get_node("MarginContainer/VBoxContainer/ShopTitle").text = "UI_SHOP_TITLE"

	_mage_health_button.text = "UI_SHOP_BUY"
	_mage_damage_button.text = "UI_SHOP_BUY"
	_mage_defense_button.text = "UI_SHOP_BUY"
	_warrior_health_button.text = "UI_SHOP_BUY"
	_warrior_damage_button.text = "UI_SHOP_BUY"
	_warrior_defense_button.text = "UI_SHOP_BUY"

	_initialize_shop()

	print("[ShopUI] Initialized")

## Appends a small coin-icon cost label next to an upgrade row and returns it.
static func _add_cost_ui(parent: Node) -> Label:
	var hbox := HBoxContainer.new()
	hbox.alignment = BoxContainer.ALIGNMENT_CENTER

	var cost_label := Label.new()
	cost_label.add_theme_font_size_override("font_size", 22)
	hbox.add_child(cost_label)

	var coin_icon := TextureRect.new()
	coin_icon.texture = load("res://Resources/Sprites/UI/coin_icon.png")
	coin_icon.custom_minimum_size = Vector2(24, 24)
	coin_icon.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	coin_icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	hbox.add_child(coin_icon)

	parent.add_child(hbox)
	return cost_label

## Rolls this visit's upgrade prices/amounts and refreshes the display.
func _initialize_shop() -> void:
	var current_wave: int = GameStateManager.current_wave
	var completed_wave: int = maxi(1, current_wave - 1)
	var coins: int = GameStateManager.coins

	_wave_label.text = "%s: %d" % [tr("UI_SHOP_NEXT_WAVE"), current_wave]
	_coins_label.text = "%d" % coins

	_calculate_prices(current_wave)
	_update_shop_display()

	print("[ShopUI] Shop opened after Wave %d" % completed_wave)

## Rolls randomized upgrade amounts and computes their wave-scaled prices.
func _calculate_prices(wave: int) -> void:
	var scale_factor: float = GameConstants.SHOP_WAVE_SCALE_FACTOR
	var base_price: int = ceili(10 * (1 + (wave * 0.5)))

	_mage_health_price = ceili(base_price * scale_factor * 1.2)
	_warrior_health_price = ceili(base_price * scale_factor * 1.2)
	_mage_damage_price = ceili(base_price * scale_factor)
	_warrior_damage_price = ceili(base_price * scale_factor)
	_mage_defense_price = ceili(base_price * scale_factor * 0.8)
	_warrior_defense_price = ceili(base_price * scale_factor * 0.8)

	_mage_health_upgrade = randi_range(GameConstants.SHOP_UPGRADE_HP_MIN, GameConstants.SHOP_UPGRADE_HP_MAX)
	_mage_damage_upgrade = randi_range(GameConstants.SHOP_UPGRADE_DAMAGE_MIN, GameConstants.SHOP_UPGRADE_DAMAGE_MAX)
	_mage_defense_upgrade = randi_range(GameConstants.SHOP_UPGRADE_DEFENSE_MIN, GameConstants.SHOP_UPGRADE_DEFENSE_MAX)
	_warrior_health_upgrade = randi_range(GameConstants.SHOP_UPGRADE_HP_MIN, GameConstants.SHOP_UPGRADE_HP_MAX)
	_warrior_damage_upgrade = randi_range(GameConstants.SHOP_UPGRADE_DAMAGE_MIN, GameConstants.SHOP_UPGRADE_DAMAGE_MAX)
	_warrior_defense_upgrade = randi_range(GameConstants.SHOP_UPGRADE_DEFENSE_MIN, GameConstants.SHOP_UPGRADE_DEFENSE_MAX)

## Refreshes every upgrade row's button/price/cost display.
func _update_shop_display() -> void:
	var coins: int = GameStateManager.coins
	var save_data: SaveData = GameStateManager.current_save
	if save_data == null:
		printerr("[ShopUI] SaveData is null! Make sure the game is started via Main Menu to initialize GameStateManager.")
		return

	_update_upgrade_button(_mage_health_button, _mage_health_label, _mage_health_cost_label, _mage_health_price, coins, save_data.mage_max_health, _mage_health_upgrade, tr("UI_SHOP_HP"))
	_update_upgrade_button(_mage_damage_button, _mage_damage_label, _mage_damage_cost_label, _mage_damage_price, coins, save_data.mage_damage, _mage_damage_upgrade, tr("UI_SHOP_DMG"))
	_update_upgrade_button(_mage_defense_button, _mage_defense_label, _mage_defense_cost_label, _mage_defense_price, coins, save_data.mage_defense, _mage_defense_upgrade, tr("UI_SHOP_DEF"))
	_update_upgrade_button(_warrior_health_button, _warrior_health_label, _warrior_health_cost_label, _warrior_health_price, coins, save_data.warrior_max_health, _warrior_health_upgrade, tr("UI_SHOP_HP"))
	_update_upgrade_button(_warrior_damage_button, _warrior_damage_label, _warrior_damage_cost_label, _warrior_damage_price, coins, save_data.warrior_damage, _warrior_damage_upgrade, tr("UI_SHOP_DMG"))
	_update_upgrade_button(_warrior_defense_button, _warrior_defense_label, _warrior_defense_cost_label, _warrior_defense_price, coins, save_data.warrior_defense, _warrior_defense_upgrade, tr("UI_SHOP_DEF"))

## Updates a single upgrade row's afford-state, price preview and cost text.
static func _update_upgrade_button(button: Button, price_label: Label, cost_label: Label, price: int, current_coins: int, current_stat: int, upgrade_amount: int, stat_name: String) -> void:
	var can_afford: bool = current_coins >= price
	button.disabled = not can_afford
	button.modulate = Color.WHITE if can_afford else Color(1, 1, 1, 0.4)

	var new_stat: int = current_stat + upgrade_amount
	price_label.text = "%d -> %d %s" % [current_stat, new_stat, stat_name]
	price_label.modulate = Color.WHITE if can_afford else Color.GRAY

	cost_label.text = "%s: %d" % [button.tr("UI_SHOP_COST"), price]
	cost_label.modulate = Color.WHITE if can_afford else Color.GRAY

## Attempts to spend coins for the given upgrade and applies it on success.
func _on_upgrade_purchased(upgrade_type: UpgradeType) -> void:
	var price: int = _get_upgrade_price(upgrade_type)

	if not GameStateManager.spend_coins(price):
		print("[ShopUI] Cannot afford %s" % UpgradeType.keys()[upgrade_type])
		var btn: Button = _get_upgrade_button(upgrade_type)
		if btn != null:
			_shake_button(btn)

		return

	_apply_upgrade(upgrade_type)
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")

	var purchased: Button = _get_upgrade_button(upgrade_type)
	if purchased != null:
		_pulse_button(purchased)

	_coins_label.text = "%d" % GameStateManager.coins
	_update_shop_display()

	print("[ShopUI] Purchased %s for %d coins" % [UpgradeType.keys()[upgrade_type], price])

## Returns the buy button for the given upgrade type.
func _get_upgrade_button(type: UpgradeType) -> Button:
	match type:
		UpgradeType.MAGE_HEALTH:
			return _mage_health_button
		UpgradeType.MAGE_DAMAGE:
			return _mage_damage_button
		UpgradeType.MAGE_DEFENSE:
			return _mage_defense_button
		UpgradeType.WARRIOR_HEALTH:
			return _warrior_health_button
		UpgradeType.WARRIOR_DAMAGE:
			return _warrior_damage_button
		UpgradeType.WARRIOR_DEFENSE:
			return _warrior_defense_button
		_:
			return null

## A small side-to-side shake to indicate an unaffordable purchase.
static func _shake_button(button: Button) -> void:
	var original_pos: Vector2 = button.position
	var tween: Tween = button.create_tween()
	tween.tween_property(button, "position:x", original_pos.x + 5, 0.05)
	tween.tween_property(button, "position:x", original_pos.x - 5, 0.05)
	tween.tween_property(button, "position:x", original_pos.x, 0.05)

## A small scale pulse to celebrate a successful purchase.
static func _pulse_button(button: Button) -> void:
	var tween: Tween = button.create_tween()
	tween.tween_property(button, "scale", Vector2(1.1, 1.1), 0.1)
	tween.tween_property(button, "scale", Vector2.ONE, 0.1)

## Returns the current price for the given upgrade type.
func _get_upgrade_price(type: UpgradeType) -> int:
	match type:
		UpgradeType.MAGE_HEALTH:
			return _mage_health_price
		UpgradeType.MAGE_DAMAGE:
			return _mage_damage_price
		UpgradeType.MAGE_DEFENSE:
			return _mage_defense_price
		UpgradeType.WARRIOR_HEALTH:
			return _warrior_health_price
		UpgradeType.WARRIOR_DAMAGE:
			return _warrior_damage_price
		UpgradeType.WARRIOR_DEFENSE:
			return _warrior_defense_price
		_:
			return 0

## Applies the purchased upgrade directly to the current save data.
func _apply_upgrade(type: UpgradeType) -> void:
	var save_data: SaveData = GameStateManager.current_save
	if save_data == null:
		return

	match type:
		UpgradeType.MAGE_HEALTH:
			save_data.mage_max_health += _mage_health_upgrade
			save_data.mage_health += _mage_health_upgrade
		UpgradeType.MAGE_DAMAGE:
			save_data.mage_damage += _mage_damage_upgrade
		UpgradeType.MAGE_DEFENSE:
			save_data.mage_defense += _mage_defense_upgrade
		UpgradeType.WARRIOR_HEALTH:
			save_data.warrior_max_health += _warrior_health_upgrade
			save_data.warrior_health += _warrior_health_upgrade
		UpgradeType.WARRIOR_DAMAGE:
			save_data.warrior_damage += _warrior_damage_upgrade
		UpgradeType.WARRIOR_DEFENSE:
			save_data.warrior_defense += _warrior_defense_upgrade

	GameStateManager.hero_stats_changed.emit()
	print("[ShopUI] Applied upgrade: %s" % UpgradeType.keys()[type])

func _on_continue_pressed() -> void:
	print("[ShopUI] Closing shop, returning to campaign map")
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")
	# No explicit save here: arriving at the campaign map autosaves once, centrally (see CampaignMapScene).
	SceneManager.go_to_map()

func _on_home_pressed() -> void:
	print("[ShopUI] Return to main menu")
	AudioManager.try_play_sfx("res://Resources/Audio/SFX/button_click.wav")
	SaveSystem.save_game()
	SceneManager.go_to_main_menu()
