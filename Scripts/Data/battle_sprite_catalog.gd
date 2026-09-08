class_name BattleSpriteCatalog
extends RefCounted
## Central registry of runtime battle atlases and temporary static fallbacks.
## Presentation code asks this catalog for a path instead of duplicating enemy
## type checks whenever another combatant receives final artwork.

## Animated 4 by 4 atlases for enemy types that have production battle art.
const ENEMY_ATLASES: Dictionary[GameEnums.EnemyType, String] = {
	GameEnums.EnemyType.SKELETON_WARRIOR: "res://Resources/Sprites/Characters/Animated/skeleton_battle_atlas.png",
	GameEnums.EnemyType.SKELETON_ARCHER: "res://Resources/Sprites/Characters/Animated/skeleton_archer_battle_atlas.png",
	GameEnums.EnemyType.ZOMBIE: "res://Resources/Sprites/Characters/Animated/zombie_battle_atlas.png",
	GameEnums.EnemyType.SLIME: "res://Resources/Sprites/Characters/Animated/slime_battle_atlas.png",
	GameEnums.EnemyType.DRAUGR_WARRIOR: "res://Resources/Sprites/Characters/Animated/draugr_warrior_battle_atlas.png",
	GameEnums.EnemyType.DRAUGR_DEFENDER: "res://Resources/Sprites/Characters/Animated/draugr_defender_battle_atlas.png",
	GameEnums.EnemyType.DRAUGR_CASTER: "res://Resources/Sprites/Characters/Animated/draugr_caster_battle_atlas.png",
}

## Returns a main hero's unique atlas, or the archetype fallback still used by
## mercenaries that do not have their own artwork yet.
static func get_player_texture_path(unit: PlayerCharacter, party: DualHeroSystem) -> String:
	if not unit.is_mercenary and unit == party.mage:
		return "res://Resources/Sprites/Characters/Animated/altarion_battle_atlas.png"
	if not unit.is_mercenary and unit == party.warrior:
		return "res://Resources/Sprites/Characters/Animated/aldric_battle_atlas.png"
	return "res://Resources/Sprites/Characters/mage_sprite.png" if unit.character_class == GameEnums.CharacterClass.MAGE else "res://Resources/Sprites/Characters/warrior_sprite.png"

## Returns an enemy's animated atlas or the legacy skeleton fallback while its
## unique artwork is still pending.
static func get_enemy_texture_path(enemy: Enemy) -> String:
	return ENEMY_ATLASES.get(enemy.enemy_type, "res://Resources/Sprites/Characters/skeleton_sprite.png")

## Reports whether a texture follows the runtime 4 by 4 animation layout.
static func is_animated_texture(path: String) -> bool:
	return path.contains("/Animated/")
