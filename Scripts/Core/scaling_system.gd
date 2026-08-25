class_name ScalingSystem
extends RefCounted
## Computes scaling for enemy stats, rewards and item costs based on the
## current wave. Provides a gradual increase in difficulty and reward value
## as the player progresses.

const ENEMY_STAT_COEFFICIENT: float = 0.10
const REWARD_COEFFICIENT: float = 0.1
const COST_COEFFICIENT: float = 0.05

## Computes the scaled value of an enemy stat for the given wave.
## base_stat: base stat value on the first wave.
## wave_number: current wave number.
static func scale_enemy_stat(base_stat: int, wave_number: int) -> int:
	return ceili(base_stat * (1 + (wave_number * ENEMY_STAT_COEFFICIENT)))

## Computes the increased reward the player receives on the given wave.
## base_reward: base reward amount.
static func scale_reward(base_reward: int, wave_number: int) -> int:
	return ceili(base_reward * (1 + (wave_number * REWARD_COEFFICIENT)))

## Computes the scaled cost of shop items or upgrades based on the current wave.
## base_cost: base cost of the item or upgrade.
static func scale_cost(base_cost: int, wave_number: int) -> int:
	return ceili(base_cost * (1 + (wave_number * COST_COEFFICIENT)))
