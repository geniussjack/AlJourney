# Requirements Document

## Introduction

This document specifies the requirements for fixing critical issues in a Godot C# match-3 battle game. The game features dual heroes (Mage and Warrior) fighting together against waves of enemies using a match-3 grid system. The fixes address architecture problems, battle logic issues, economy scaling, save system robustness, and code quality improvements.

## Glossary

- **GridManager**: Singleton autoload managing the match-3 grid logic including swaps, matches, and cascades
- **BattleManager**: Singleton autoload managing battle flow, turn order, and combat resolution
- **ComboSystem**: System processing match-3 combos and generating combat effects
- **DualHeroSystem**: System managing both player characters (Mage and Warrior) simultaneously
- **Cascade**: Secondary matches that occur after initial matches are removed and grid refills
- **Hero_Element**: Grid element belonging to a specific hero (Fire/Heal for Mage, Sword/Shield for Warrior)
- **Status_Effect**: Temporary effect applied to characters (Weakened, Stunned, Regeneration, etc.)
- **Wave**: Battle encounter with a group of enemies
- **Dynamic_Scaling**: Automatic adjustment of game values based on wave progression

## Requirements

### Requirement 1: Singleton Signal Management

**User Story:** As a developer, I want singletons to properly manage signal connections, so that duplicate subscriptions and memory leaks are prevented.

#### Acceptance Criteria

1. WHEN BattleManager subscribes to GridManager.SwapCompleted THEN the system SHALL ensure no duplicate connections exist
2. WHEN BattleManager.EndBattle is called THEN the system SHALL unsubscribe from all GridManager signals
3. WHEN a singleton is destroyed or recreated THEN the system SHALL clean up all signal connections
4. WHEN signal connections are established THEN the system SHALL use a pattern that prevents duplicate subscriptions

### Requirement 2: Recursive Cascade Processing

**User Story:** As a player, I want cascades to process completely, so that all chain reactions from my matches are resolved.

#### Acceptance Criteria

1. WHEN matches are processed and grid refills THEN the system SHALL check for new matches recursively
2. WHEN no more matches exist after a cascade THEN the system SHALL stop cascade processing
3. WHEN multiple cascade waves occur THEN the system SHALL accumulate all combo effects before applying them
4. WHEN cascade processing completes THEN the system SHALL apply all accumulated effects in a single batch

### Requirement 3: Hero-Specific Grid Elements

**User Story:** As a player, I want grid elements to belong to specific heroes, so that only living heroes can use their elements.

#### Acceptance Criteria

1. WHEN a grid element is created THEN the system SHALL assign it to either Mage or Warrior
2. WHEN Mage dies THEN Fire and Heal combos SHALL produce no effects
3. WHEN Warrior dies THEN Sword and Shield combos SHALL produce no effects
4. WHEN a combo effect is generated THEN the system SHALL check if the owning hero is alive before applying effects
5. WHEN both heroes are alive THEN all element types SHALL function normally

### Requirement 4: Immediate Swap Processing

**User Story:** As a player, I want swaps to process immediately, so that I get classic match-3 feedback after each move.

#### Acceptance Criteria

1. WHEN a valid swap creates matches THEN the system SHALL process those matches immediately
2. WHEN matches are processed THEN the system SHALL apply cascades and effects before allowing the next swap
3. WHEN a player has remaining swaps THEN the system SHALL allow the next swap only after current processing completes
4. WHEN all swaps are consumed THEN the system SHALL transition to enemy turn

### Requirement 5: Null Safety in Match Processing

**User Story:** As a developer, I want null checks in match processing, so that NullReferenceExceptions are prevented.

#### Acceptance Criteria

1. WHEN ProcessMatches accesses grid elements THEN the system SHALL verify elements are not null
2. WHEN marking matched elements THEN the system SHALL validate position bounds and element existence
3. IF a null element is encountered THEN the system SHALL skip it and log a warning
4. WHEN removing matched elements THEN the system SHALL safely handle null references

### Requirement 6: Weakened Status Effect Implementation

**User Story:** As a player, I want the Weakened status to reduce damage and defense, so that debuffs have meaningful impact.

#### Acceptance Criteria

1. WHEN a character has Weakened status THEN damage calculations SHALL reduce output by 30%
2. WHEN a character has Weakened status THEN defense calculations SHALL reduce effectiveness by 30%
3. WHEN Weakened status expires THEN damage and defense SHALL return to normal values
4. WHEN calculating damage or defense THEN the system SHALL check for Weakened status and apply modifiers

### Requirement 7: Comprehensive Stun Checks

**User Story:** As a player, I want stunned enemies to skip all actions, so that stun effects work consistently.

#### Acceptance Criteria

1. WHEN an enemy is stunned THEN the system SHALL skip all enemy actions including attacks
2. WHEN a boss is stunned THEN the system SHALL skip special abilities including summons and spells
3. WHEN checking if an enemy can act THEN the system SHALL verify IsStunned status first
4. WHEN processing enemy turn THEN stunned enemies SHALL only process status effects and skip actions

### Requirement 8: Reflection Chain Limiting

**User Story:** As a player, I want reflection damage to occur once per attack, so that infinite reflection loops are prevented.

#### Acceptance Criteria

1. WHEN damage is reflected back to attacker THEN the system SHALL not trigger another reflection
2. WHEN an attack deals damage THEN the system SHALL track if reflection has already occurred
3. WHEN reflection damage is applied THEN the system SHALL mark the damage as non-reflectable
4. WHEN processing damage THEN the system SHALL limit reflection chains to maximum one reflection per attack

### Requirement 9: Dynamic Wave Scaling

**User Story:** As a player, I want game difficulty to scale with wave number, so that progression feels balanced and challenging.

#### Acceptance Criteria

1. WHEN calculating enemy stats THEN the system SHALL apply formula: baseStat * (1 + wave * 0.15)
2. WHEN calculating coin rewards THEN the system SHALL apply formula: baseReward * (1 + wave * 0.1)
3. WHEN calculating upgrade costs THEN the system SHALL apply formula: baseCost * (1 + wave * 0.05)
4. WHEN wave number increases THEN all scaling formulas SHALL automatically adjust values
5. WHEN generating enemies THEN health, damage, and defense SHALL scale based on wave number

### Requirement 10: Save System Error Handling

**User Story:** As a player, I want corrupted saves to be handled gracefully, so that I don't lose all progress from file errors.

#### Acceptance Criteria

1. WHEN loading a save file THEN the system SHALL validate JSON structure before deserialization
2. IF save file is corrupted THEN the system SHALL log error and return null without crashing
3. IF save file has outdated schema THEN the system SHALL attempt migration or use default values
4. WHEN save loading fails THEN the system SHALL display user-friendly error message
5. WHEN save validation fails THEN the system SHALL offer option to start new game

### Requirement 11: GetCombinedStats Documentation

**User Story:** As a developer, I want clear documentation for GetCombinedStats return order, so that stat assignment errors are prevented.

#### Acceptance Criteria

1. WHEN GetCombinedStats is called THEN the return tuple SHALL have clear parameter names
2. WHEN reading GetCombinedStats code THEN comments SHALL explain the exact order of returned values
3. WHEN using GetCombinedStats THEN tuple deconstruction SHALL use named variables matching documentation
4. THE DualHeroSystem.GetCombinedStats method SHALL document return format as: (mageHealth, mageMaxHealth, mageDamage, mageDefense, warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense)

### Requirement 12: Automatic Board Reshuffle

**User Story:** As a player, I want the board to reshuffle when no moves exist, so that I don't get stuck without losing swap attempts.

#### Acceptance Criteria

1. WHEN no valid moves exist on the grid THEN the system SHALL detect this condition
2. WHEN board reshuffle is triggered THEN the system SHALL regenerate the grid without initial matches
3. WHEN board reshuffles THEN remaining swap attempts SHALL not be reduced
4. WHEN reshuffle completes THEN the system SHALL verify at least one valid move exists
5. IF reshuffle fails to create valid moves THEN the system SHALL retry up to 3 times

### Requirement 13: FIX Comment Resolution

**User Story:** As a developer, I want all FIX comments resolved, so that temporary workarounds are replaced with proper solutions.

#### Acceptance Criteria

1. WHEN emitting MatchesFound signal THEN the system SHALL pass match count as integer (not List)
2. WHEN calling deferred scene changes THEN the system SHALL use nameof() for method names
3. WHEN DeferredSceneChange is defined THEN the method SHALL be public for CallDeferred access
4. WHEN loading scenes THEN the system SHALL use SceneManager.Instance consistently
5. THE GridManager SHALL emit signals with correct parameter types matching signal definitions

### Requirement 14: Remove Unused GameState

**User Story:** As a developer, I want unused game states removed, so that the codebase reflects actual game flow.

#### Acceptance Criteria

1. THE GameState enum SHALL not include CharacterSelect value
2. WHEN transitioning game states THEN the system SHALL never reference GameState.CharacterSelect
3. WHEN checking game states THEN no code SHALL handle CharacterSelect case
4. THE game flow SHALL proceed directly from MainMenu to Battle without character selection
