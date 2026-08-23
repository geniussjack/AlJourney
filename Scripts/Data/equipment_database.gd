class_name EquipmentDatabase
extends RefCounted
## Equipment data class. Stores equipment templates and their parameters.

## The registry of equipment templates, keyed by item id.
static var templates: Dictionary[String, EquipmentData] = {
	# Mage Weapons
	"fireball": EquipmentData.new(
		"fireball", "WPN_NAME_FIREBALL", "WPN_DESC_FIREBALL", GameEnums.EquipmentSlot.WEAPON, GameEnums.EquipmentRarity.COMMON, 1, 10,
		{"damage": 5, "burn_damage": 2} as Dictionary[String, int], {} as Dictionary[String, String]),

	"iceball": EquipmentData.new(
		"iceball", "WPN_NAME_ICEBALL", "WPN_DESC_ICEBALL", GameEnums.EquipmentSlot.WEAPON, GameEnums.EquipmentRarity.UNCOMMON, 1, 10,
		{"damage": 2, "weaken_amount": 30} as Dictionary[String, int], {} as Dictionary[String, String]),

	"electroball": EquipmentData.new(
		"electroball", "WPN_NAME_ELECTROBALL", "WPN_DESC_ELECTROBALL", GameEnums.EquipmentSlot.WEAPON, GameEnums.EquipmentRarity.RARE, 1, 10,
		{"damage": 3, "shock_amount": 50} as Dictionary[String, int], {} as Dictionary[String, String]),

	# Warrior Weapons
	"sword": EquipmentData.new(
		"sword", "WPN_NAME_SWORD", "WPN_DESC_SWORD", GameEnums.EquipmentSlot.WEAPON, GameEnums.EquipmentRarity.COMMON, 1, 10,
		{"damage": 5} as Dictionary[String, int], {} as Dictionary[String, String]),

	"axe": EquipmentData.new(
		"axe", "WPN_NAME_AXE", "WPN_DESC_AXE", GameEnums.EquipmentSlot.WEAPON, GameEnums.EquipmentRarity.UNCOMMON, 1, 10,
		{"damage": 3, "bleed_damage": 2} as Dictionary[String, int], {} as Dictionary[String, String]),

	"spear": EquipmentData.new(
		"spear", "WPN_NAME_SPEAR", "WPN_DESC_SPEAR", GameEnums.EquipmentSlot.WEAPON, GameEnums.EquipmentRarity.RARE, 1, 10,
		{"damage": 2, "vulnerable_amount": 50} as Dictionary[String, int], {} as Dictionary[String, String]),

	# Armor (keeping a couple for defaults if needed)
	"leather_armor": EquipmentData.new(
		"leather_armor", "WPN_NAME_LEATHER_ARMOR", "", GameEnums.EquipmentSlot.BODY, GameEnums.EquipmentRarity.COMMON, 1, 5,
		{"defense": 3} as Dictionary[String, int], {} as Dictionary[String, String]),

	"dragon_scales": EquipmentData.new(
		"dragon_scales", "WPN_NAME_DRAGON_SCALES", "", GameEnums.EquipmentSlot.BODY, GameEnums.EquipmentRarity.LEGENDARY, 1, 25,
		{"defense": 15, "immunity_burn": 100} as Dictionary[String, int], {} as Dictionary[String, String]),

	"power_ring": EquipmentData.new(
		"power_ring", "WPN_NAME_POWER_RING", "", GameEnums.EquipmentSlot.RING, GameEnums.EquipmentRarity.RARE, 1, 15,
		{"damage": 10} as Dictionary[String, int], {} as Dictionary[String, String]),

	"life_amulet": EquipmentData.new(
		"life_amulet", "WPN_NAME_LIFE_AMULET", "", GameEnums.EquipmentSlot.NECKLACE, GameEnums.EquipmentRarity.EPIC, 1, 20,
		{"hp_percent": 20} as Dictionary[String, int], {} as Dictionary[String, String]),
}
