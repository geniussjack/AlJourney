class_name GameEnums
extends RefCounted
## Namespace for every shared game enum. GDScript has no free-standing global
## enums, so they are grouped as nested enums on this class and referenced as
## e.g. GameEnums.CharacterClass.MAGE.

## Possible game states, determining the current screen and behavior.
enum GameState {
	MAIN_MENU,
	MAP,
	BATTLE,
	SHOP,
	GAME_OVER,
	VICTORY,
}

## Character classes, defining their role in combat.
enum CharacterClass {
	MAGE,
	WARRIOR,
}

## The different enemy types the player can encounter while playing.
enum EnemyType {
	SKELETON_WARRIOR,
	SKELETON_ARCHER,
	ZOMBIE,
	SLIME,
	DRAUGR_WARRIOR,
	DRAUGR_DEFENDER,
	DRAUGR_CASTER,

	GENERAL_OF_DRAUGR,
	ARHISKELETON,

	NECROMANCER,
}

## Types of damage dealt.
enum AttackType {
	PHYSICAL,
	MAGICAL,
}

## Status effects that can be applied to characters or enemies during combat.
enum StatusEffect {
	NONE,
	BURNING,
	BLEEDING,
	REGENERATION,
	SHIELD_REFLECT,
	IMMUNITY,
	STUNNED,
	WEAKENED,
	FREEZE,
	SHOCK,
	VULNERABLE,
}

## Turn phases of the turn-based combat system. Determine whose turn it
## currently is and whether a wave transition is in progress.
enum BattlePhase {
	PLAYER_TURN,
	ENEMY_TURN,
	WAVE_TRANSITION,
}

## Equipment slots for characters. Determine which slot an item can be
## equipped into.
enum EquipmentSlot {
	WEAPON,
	HEAD,
	BODY,
	LEGS,
	NECKLACE,
	RING,
	EARRING,
}

## Item rarity tiers, affecting their stats, value and drop chance.
enum EquipmentRarity {
	COMMON,
	UNCOMMON,
	RARE,
	EPIC,
	LEGENDARY,
}

## Types of abilities available to characters.
enum AbilityType {
	ATTACK,
	SUPPORT,
}

## Elements associated with characters' active abilities.
enum AbilityElement {
	FIRE,
	HEAL,
	SWORD,
	SHIELD,
	ICE,
	LIGHTNING,
	BLEED,
	PIERCE,
}

## Defines who an ability can be targeted at: an enemy, or the caster/an ally.
enum AbilityTargetType {
	ENEMY,
	ALLY_OR_SELF,
}

## Campaign map locations, arranged in order of distance from the village to
## the necromancer's lair. Declaration order matches playthrough order.
enum LocationId {
	VILLAGE_RUINS,
	DARK_FOREST,
	BURIED_CATACOMBS,
	FROZEN_WASTES,
	NECROMANCER_LAIR,
}

## Strategic resources gathered by villagers assigned to work the
## settlement (see design document, section 9). Distinct from the
## "trophy" loot economy (Coins, EquipmentData) — these fuel building
## upgrades instead. Working list, adjustable.
enum StrategicResource {
	WOOD,
	STONE,
	IRON,
	SILVER,
	GOLD,
}

## Settlement buildings. Every building is tied to a concrete gameplay
## mechanic (see design document, section 9) — deliberately no purely
## decorative buildings in this list.
enum BuildingType {
	FORGE,
	HERBALIST,
	BARRACKS,
	WAREHOUSE,
	WALL,
	HOUSES,
}

## Mercenary specialization roles (see design document, section 4). Shared
## conceptually across both archetypes (Mage/Warrior) — each class maps to
## a differently-named, differently-flavored subclass per archetype (see
## MercenaryDatabase). Working list, not final — revisit once the game is
## fully playable and can actually be balance-tested.
enum MercenaryClass {
	HEALER,
	WARDEN,
	PYRO,
	CRYO,
	STORM,
}

## What a Herbalist-brewed battle potion does when used (see design
## document, section 9).
enum PotionType {
	## Heals the party member who used the potion.
	SINGLE_HEAL,
	## Heals every living party member.
	PARTY_HEAL,
	## Adds to the party's shared ultimate charge.
	ULTIMATE_FILL,
}
