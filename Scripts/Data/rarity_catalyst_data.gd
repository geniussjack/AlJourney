class_name RarityCatalystData
extends RefCounted
## Data structure representing a unique material used to upgrade equipment
## to a specific rarity tier (see design document, section 10). Catalysts
## are archetype-specific — a Mage-archetype item needs a Mage catalyst,
## a Warrior-archetype item needs a Warrior catalyst.

## Unique id used to key owned counts (SaveData.catalyst_counts).
var id: String
## Localization key for the catalyst's display name.
var name_key: String
## The rarity tier this catalyst upgrades equipment TO (e.g. a RARE
## catalyst is spent to upgrade an UNCOMMON item into a RARE one).
var rarity: GameEnums.EquipmentRarity
## Which archetype's equipment this catalyst applies to.
var archetype: GameEnums.CharacterClass

## Builds a catalyst definition.
func _init(
	id: String,
	name_key: String,
	rarity: GameEnums.EquipmentRarity,
	archetype: GameEnums.CharacterClass,
) -> void:
	self.id = id
	self.name_key = name_key
	self.rarity = rarity
	self.archetype = archetype
