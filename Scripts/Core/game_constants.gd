class_name GameConstants
extends RefCounted
## Storage for all game constants: base hero stats, enemy stats, wave
## scaling, shop pricing and save data settings.

## Coefficient by which enemy health increases with each new wave.
const ENEMY_HP_SCALE_PER_WAVE: float = 0.10
## Coefficient by which enemy damage increases with each new wave.
const ENEMY_DAMAGE_SCALE_PER_WAVE: float = 0.06
## Maximum number of enemies allowed in a single wave (used, for example, as
## the cap on the number of creatures the necromancer can summon — see
## EnemyAIController).
const MAX_ENEMIES_PER_WAVE: int = 5

## Base maximum health of the Mage.
const MAGE_BASE_HP: int = 80
## Base damage of the Mage, before equipment.
const MAGE_BASE_DAMAGE: int = 8
## Base defense of the Mage, before equipment.
const MAGE_BASE_DEFENSE: int = 2

## Base maximum health of the Warrior.
const WARRIOR_BASE_HP: int = 120
## Base damage of the Warrior, before equipment.
const WARRIOR_BASE_DAMAGE: int = 12
## Base defense of the Warrior, before equipment.
const WARRIOR_BASE_DEFENSE: int = 4

## Health of the Skeleton Warrior.
const SKELETON_WARRIOR_HP: int = 30
## Damage of the Skeleton Warrior.
const SKELETON_WARRIOR_DAMAGE: int = 25
## Defense of the Skeleton Warrior.
const SKELETON_WARRIOR_DEFENSE: int = 2

## Health of the Skeleton Archer.
const SKELETON_ARCHER_HP: int = 25
## Damage of the Skeleton Archer.
const SKELETON_ARCHER_DAMAGE: int = 18
## Defense of the Skeleton Archer.
const SKELETON_ARCHER_DEFENSE: int = 1

## Health of the Zombie.
const ZOMBIE_HP: int = 60
## Damage of the Zombie.
const ZOMBIE_DAMAGE: int = 20
## Defense of the Zombie.
const ZOMBIE_DEFENSE: int = 3

## Health of the Slime.
const SLIME_HP: int = 20
## Damage of the Slime.
const SLIME_DAMAGE: int = 12
## Defense of the Slime.
const SLIME_DEFENSE: int = 0

## Health of the Draugr Warrior.
const DRAUGR_WARRIOR_HP: int = 45
## Damage of the Draugr Warrior.
const DRAUGR_WARRIOR_DAMAGE: int = 22
## Defense of the Draugr Warrior.
const DRAUGR_WARRIOR_DEFENSE: int = 4

## Health of the Draugr Defender.
const DRAUGR_DEFENDER_HP: int = 70
## Damage of the Draugr Defender.
const DRAUGR_DEFENDER_DAMAGE: int = 15
## Defense of the Draugr Defender.
const DRAUGR_DEFENDER_DEFENSE: int = 8

## Health of the Draugr Caster.
const DRAUGR_CASTER_HP: int = 40
## Damage of the Draugr Caster.
const DRAUGR_CASTER_DAMAGE: int = 25
## Defense of the Draugr Caster.
const DRAUGR_CASTER_DEFENSE: int = 3

## Health of the General of Draugr.
const GENERAL_DRAUGR_HP: int = 150
## Damage of the General of Draugr.
const GENERAL_DRAUGR_DAMAGE: int = 35
## Defense of the General of Draugr.
const GENERAL_DRAUGR_DEFENSE: int = 10

## Health of the Archskeleton.
const ARHISKELETON_HP: int = 120
## Damage of the Archskeleton.
const ARHISKELETON_DAMAGE: int = 25
## Defense of the Archskeleton.
const ARHISKELETON_DEFENSE: int = 5
## Number of arrow shots the Archskeleton fires per turn.
const ARHISKELETON_ARROWS_PER_TURN: int = 3

## Health of the Necromancer.
const NECROMANCER_HP: int = 300
## Damage of the Necromancer.
const NECROMANCER_DAMAGE: int = 45
## Defense of the Necromancer.
const NECROMANCER_DEFENSE: int = 12

## Minimum health increase granted by a shop upgrade purchase.
const SHOP_UPGRADE_HP_MIN: int = 25
## Maximum health increase granted by a shop upgrade purchase.
const SHOP_UPGRADE_HP_MAX: int = 60
## Minimum damage increase granted by a shop upgrade purchase.
const SHOP_UPGRADE_DAMAGE_MIN: int = 2
## Maximum damage increase granted by a shop upgrade purchase.
const SHOP_UPGRADE_DAMAGE_MAX: int = 5
## Minimum defense increase granted by a shop upgrade purchase.
const SHOP_UPGRADE_DEFENSE_MIN: int = 1
## Maximum defense increase granted by a shop upgrade purchase.
const SHOP_UPGRADE_DEFENSE_MAX: int = 4
## Coefficient scaling shop prices as the wave number increases.
const SHOP_WAVE_SCALE_FACTOR: float = 0.7

## Coins dropped for defeating a basic enemy.
const COINS_PER_BASIC_ENEMY: int = 8
## Coins dropped for defeating a miniboss.
const COINS_PER_MINIBOSS: int = 40
## Coins dropped for defeating a main boss.
const COINS_PER_BOSS: int = 150

## File name used to store the player's save data.
const SAVE_FILE_NAME: String = "save_data.json"
## Directory in which the save file is stored.
const SAVE_DIRECTORY: String = "user://SaveData/"
