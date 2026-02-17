# Implementation Plan: Game Fixes and Refactoring

## Overview

This implementation plan addresses 14 critical issues in the Godot C# match-3 battle game through systematic fixes to architecture, gameplay mechanics, battle logic, progression systems, and code quality. The fixes are organized into logical groups that build incrementally, with testing integrated throughout.

## Tasks

- [x] 1. Architecture and State Cleanup
  - [x] 1.1 Remove GameState.CharacterSelect enum value
    - Remove CharacterSelect from GameState enum definition
    - Remove any switch cases or conditionals referencing CharacterSelect
    - Verify game flow proceeds directly from MainMenu to Battle
    - _Requirements: 14.1, 14.2, 14.4_
  
  - [ ]* 1.2 Write unit tests for GameState enum
    - Test that CharacterSelect value does not exist
    - Test game flow transitions from MainMenu to Battle
    - _Requirements: 14.1, 14.2, 14.4_
  
  - [x] 1.3 Implement singleton signal management pattern in BattleManager
    - Add _isConnectedToGridManager boolean field
    - Add connection guard in StartBattle() to prevent duplicates
    - Implement signal unsubscription in EndBattle()
    - Add null checks before unsubscribing
    - _Requirements: 1.1, 1.2_
  
  - [ ]* 1.4 Write property test for signal connection idempotence
    - **Property 1: Signal Connection Idempotence**
    - **Validates: Requirements 1.1**
  
  - [ ]* 1.5 Write property test for signal cleanup
    - **Property 2: Signal Cleanup on EndBattle**
    - **Validates: Requirements 1.2**

- [x] 2. GridManager Core Fixes
  - [x] 2.1 Add null safety to ProcessMatches method
    - Add null check for matches parameter
    - Add null checks when accessing grid elements
    - Add bounds validation for positions
    - Add defensive logging for skipped null elements
    - _Requirements: 5.1, 5.2, 5.3_
  
  - [ ]* 2.2 Write property test for null-safe match processing
    - **Property 10: Null-Safe Match Processing**
    - **Validates: Requirements 5.1, 5.2, 5.3**
  
  - [x] 2.3 Implement HasValidMoves detection
    - Verify existing HasValidMoves() method works correctly
    - Test with grids that have no valid moves
    - _Requirements: 12.1_
  
  - [x] 2.4 Implement automatic board reshuffle
    - Create CheckAndReshuffleIfNeeded() method
    - Implement ReshuffleBoard() with retry logic (max 3 attempts)
    - Ensure reshuffle preserves swap count
    - Ensure reshuffled grid has no initial matches
    - Verify at least one valid move exists after reshuffle
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5_
  
  - [ ]* 2.5 Write property tests for board reshuffle
    - **Property 20: No Valid Moves Detection**
    - **Property 21: Reshuffle Generates Valid Grid**
    - **Property 22: Reshuffle Preserves Swap Count**
    - **Property 23: Reshuffle Retry Logic**
    - **Validates: Requirements 12.1, 12.2, 12.3, 12.4, 12.5**
  
  - [x] 2.6 Fix FIX comments in GridManager
    - Fix line 217-219: Emit match count as integer (not List)
    - Verify signal emission matches signal definition
    - _Requirements: 13.1, 13.5_
  
  - [ ]* 2.7 Write unit test for MatchesFound signal
    - Test that signal emits integer count, not List
    - _Requirements: 13.1_

- [x] 3. Checkpoint - Verify GridManager fixes
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Recursive Cascade Processing
  - [x] 4.1 Refactor BattleManager to process cascades recursively
    - Modify ProcessMatchesRecursive to loop until no matches
    - Accumulate all combo effects during cascade processing
    - Apply all effects in single batch at end
    - Add termination condition (no matches found)
    - _Requirements: 2.1, 2.2, 2.3, 2.4_
  
  - [ ]* 4.2 Write property tests for cascade processing
    - **Property 3: Recursive Cascade Termination**
    - **Property 4: Cascade Effect Accumulation and Batch Application**
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4**
  
  - [x] 4.3 Implement immediate swap processing
    - Modify OnSwapCompleted to process matches immediately
    - Process cascades before allowing next swap
    - Return to swap phase if swaps remain, else start enemy turn
    - _Requirements: 4.1, 4.2, 4.3, 4.4_
  
  - [ ]* 4.4 Write property tests for immediate swap processing
    - **Property 8: Immediate Match Processing After Swap**
    - **Property 9: Phase Transition After Swap Exhaustion**
    - **Validates: Requirements 4.1, 4.2, 4.3, 4.4**

- [x] 5. Hero-Specific Grid Elements
  - [x] 5.1 Add OwningHero field to ElementData
    - Add CharacterClass OwningHero property
    - Assign owner in constructor based on element type
    - Fire/Heal → Mage, Sword/Shield → Warrior
    - Update CreateRandom to assign owners
    - _Requirements: 3.1_
  
  - [ ]* 5.2 Write property test for element ownership
    - **Property 5: Element Hero Ownership**
    - **Validates: Requirements 3.1**
  
  - [x] 5.3 Implement dead hero element filtering in BattleManager
    - Modify ApplyComboEffect to check if owning hero is alive
    - Return early if hero is dead (no effects applied)
    - Add logging for dead hero element attempts
    - _Requirements: 3.2, 3.3, 3.4, 3.5_
  
  - [ ]* 5.4 Write property tests for hero element filtering
    - **Property 6: Dead Hero Element Filtering**
    - **Property 7: Living Heroes Enable All Elements**
    - **Validates: Requirements 3.2, 3.3, 3.5**

- [x] 6. Checkpoint - Verify cascade and hero element fixes
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Status Effect Implementations
  - [x] 7.1 Implement Weakened status in damage calculations
    - Modify PlayerCharacter.CalculateDamage to check for Weakened
    - Reduce damage by 30% if Weakened status active
    - Add logging for Weakened damage reduction
    - _Requirements: 6.1_
  
  - [x] 7.2 Implement Weakened status in defense calculations
    - Modify Character.TakeDamage to check for Weakened
    - Reduce defense by 30% if Weakened status active
    - Add logging for Weakened defense reduction
    - _Requirements: 6.2_
  
  - [ ]* 7.3 Write property tests for Weakened status
    - **Property 11: Weakened Status Reduces Combat Stats**
    - **Property 12: Weakened Status Expiration Restores Stats**
    - **Validates: Requirements 6.1, 6.2, 6.3**
  
  - [x] 7.4 Add comprehensive stun checks to enemy actions
    - Add IsStunned check at start of PerformEnemyAction
    - Add IsStunned check at start of PerformNecromancerAction
    - Ensure stunned enemies skip all actions but process status effects
    - _Requirements: 7.1, 7.2, 7.3, 7.4_
  
  - [ ]* 7.5 Write property tests for stun behavior
    - **Property 13: Stunned Enemies Skip All Actions**
    - **Validates: Requirements 7.1, 7.2, 7.4**

- [x] 8. Reflection Chain Limiting
  - [x] 8.1 Add canReflect parameter to Character.TakeDamage
    - Add bool canReflect = true parameter
    - Only check for reflection if canReflect is true
    - Return reflected damage as before
    - _Requirements: 8.1, 8.2, 8.3, 8.4_
  
  - [x] 8.2 Update all TakeDamage calls to use canReflect
    - Initial attacks: canReflect = true
    - Reflected damage: canReflect = false
    - Update BattleManager.ApplyDamageEffect
    - Update BattleManager.PerformEnemyAction
    - _Requirements: 8.1_
  
  - [ ]* 8.3 Write property test for reflection limiting
    - **Property 14: Reflection Chain Limiting**
    - **Validates: Requirements 8.1**

- [x] 9. Checkpoint - Verify status effects and reflection fixes
  - Ensure all tests pass, ask the user if questions arise.

- [x] 10. Dynamic Scaling System
  - [x] 10.1 Create ScalingSystem static class
    - Create Scripts/Core/ScalingSystem.cs
    - Implement ScaleEnemyStat(baseStat, wave) with formula: baseStat * (1 + wave * 0.15)
    - Implement ScaleReward(baseReward, wave) with formula: baseReward * (1 + wave * 0.1)
    - Implement ScaleCost(baseCost, wave) with formula: baseCost * (1 + wave * 0.05)
    - Use Mathf.CeilToInt for all calculations
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_
  
  - [x] 10.2 Update Enemy.Create to use ScalingSystem
    - Replace manual scaling with ScalingSystem.ScaleEnemyStat
    - Scale HP, damage, and defense
    - Scale coin reward with ScalingSystem.ScaleReward
    - _Requirements: 9.1, 9.2, 9.5_
  
  - [x] 10.3 Update shop system to use ScalingSystem for costs
    - Find shop upgrade cost calculations
    - Replace with ScalingSystem.ScaleCost
    - _Requirements: 9.3_
  
  - [ ]* 10.4 Write property tests for scaling formulas
    - **Property 15: Enemy Stat Scaling Formula**
    - **Property 16: Reward Scaling Formula**
    - **Property 17: Cost Scaling Formula**
    - **Validates: Requirements 9.1, 9.2, 9.3**

- [x] 11. Save System Error Handling
  - [x] 11.1 Add SchemaVersion field to SaveData
    - Add public int SchemaVersion { get; set; } = 1
    - Implement static SaveData Migrate(SaveData oldData) method
    - Return null for unsupported versions (for now)
    - _Requirements: 10.3_
  
  - [x] 11.2 Enhance SaveSystem.LoadGame with error handling
    - Add try-catch around entire method
    - Add null/empty check for JSON string
    - Add try-catch specifically for JsonException
    - Check schema version and attempt migration
    - Add comprehensive error logging
    - Return null on all error paths (no exceptions thrown)
    - _Requirements: 10.1, 10.2, 10.3_
  
  - [ ]* 11.3 Write property tests for save error handling
    - **Property 18: Save File JSON Validation**
    - **Property 19: Save Schema Migration**
    - **Validates: Requirements 10.1, 10.2, 10.3**
  
  - [ ]* 11.4 Write unit tests for save error scenarios
    - Test corrupted JSON
    - Test empty file
    - Test missing file
    - Test invalid data values
    - Test schema version mismatch
    - _Requirements: 10.1, 10.2, 10.3_

- [x] 12. Code Quality and Documentation
  - [x] 12.1 Fix FIX comments in SceneManager
    - Line 82: Already uses nameof() - verify correctness
    - Line 86: Verify DeferredSceneChange is public
    - Lines 136, 156, 170, 179, 188: Verify all use Instance consistently
    - _Requirements: 13.2, 13.3, 13.4_
  
  - [ ]* 12.2 Write unit test for DeferredSceneChange visibility
    - Use reflection to verify method is public
    - _Requirements: 13.3_
  
  - [x] 12.3 Add comprehensive documentation to DualHeroSystem.GetCombinedStats
    - Add XML doc comment explaining return order
    - Use named tuple parameters in return type
    - Add inline comments for clarity
    - Update all call sites to use named deconstruction
    - _Requirements: 11.1, 11.2, 11.3, 11.4_

- [x] 13. Final Integration and Testing
  - [x] 13.1 Integration test: Full battle with all fixes
    - Start battle with both heroes
    - Make swaps that trigger cascades
    - Kill one hero and verify their elements don't work
    - Apply Weakened and Stunned status effects
    - Trigger reflection damage
    - Verify board reshuffle when no moves
    - Complete wave and verify scaling
    - _Requirements: All_
  
  - [ ]* 13.2 Integration test: Save/load with error scenarios
    - Save game state
    - Load successfully
    - Test with corrupted save
    - Test with missing save
    - _Requirements: 10.1, 10.2, 10.3_
  
  - [x] 13.3 Manual testing checklist
    - Play through multiple waves
    - Verify cascades process completely
    - Verify hero death disables their elements
    - Verify status effects work correctly
    - Verify scaling increases with waves
    - Verify save/load works
    - Verify board reshuffles when stuck

- [x] 14. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster implementation
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation at logical breakpoints
- Property tests validate universal correctness properties across all inputs
- Unit tests validate specific examples, edge cases, and error conditions
- Integration tests verify all fixes work together correctly
- The implementation order ensures each fix builds on previous work
- Signal management and null safety fixes come first to prevent crashes
- Cascade and hero element fixes build on stable grid processing
- Status effects and reflection fixes enhance battle logic
- Scaling and save system fixes improve progression and robustness
- Code quality fixes ensure maintainability
