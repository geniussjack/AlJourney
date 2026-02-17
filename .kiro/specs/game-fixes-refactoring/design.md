# Design Document: Game Fixes and Refactoring

## Overview

This design addresses 14 critical issues in the Godot C# match-3 battle game. The fixes span architecture improvements (singleton signal management, unused state removal), core gameplay mechanics (recursive cascades, hero-specific elements, immediate swap processing), battle logic (status effects, stun checks, reflection limiting), progression systems (dynamic scaling), robustness (save error handling, null safety, board reshuffle), and code quality (FIX comment resolution, documentation).

The game features dual heroes (Mage and Warrior) fighting together against waves of enemies using a match-3 grid. Each hero owns specific element types: Mage controls Fire and Heal, Warrior controls Sword and Shield. The design ensures that only living heroes can use their elements, preventing dead heroes from contributing to combat.

## Architecture

### Singleton Signal Management Pattern

**Problem**: BattleManager subscribes to GridManager.SwapCompleted but never unsubscribes, causing potential duplicate connections and memory leaks.

**Solution**: Implement explicit lifecycle management for signal connections:

1. **Connection Tracking**: Store signal connection state in BattleManager
2. **Guarded Subscription**: Check if already connected before subscribing
3. **Explicit Cleanup**: Unsubscribe in EndBattle() method
4. **Defensive Pattern**: Use try-finally or ensure cleanup even on errors

**Implementation**:
```csharp
// BattleManager.cs
private bool _isConnectedToGridManager = false;

public void StartBattle(...)
{
    // Guard against duplicate connections
    if (!_isConnectedToGridManager)
    {
        _gridManager.SwapCompleted += OnSwapCompleted;
        _isConnectedToGridManager = true;
    }
    // ... rest of initialization
}

public void EndBattle()
{
    // Unsubscribe from GridManager signals
    if (_gridManager != null && _isConnectedToGridManager)
    {
        _gridManager.SwapCompleted -= OnSwapCompleted;
        _isConnectedToGridManager = false;
    }
    // ... rest of cleanup
}
```

### GameState Enum Cleanup

**Problem**: GameState.CharacterSelect exists but is never used (no character selection in game).

**Solution**: Remove CharacterSelect from GameState enum and all references.

**Files to modify**:
- `Scripts/Core/GameState.cs` (or wherever enum is defined)
- Any switch statements or conditionals checking for CharacterSelect

## Components and Interfaces

### GridManager Enhancements

**Recursive Cascade Processing**:

Current implementation only processes one cascade wave. New design processes recursively until no matches remain.

```csharp
// GridManager.cs - New method
public void ProcessMatchesRecursive(Action<List<ComboEffect>> onComplete)
{
    List<MatchResult> matches = FindAllMatches();
    
    if (matches.Count == 0)
    {
        // No more matches - cascade complete
        onComplete?.Invoke(new List<ComboEffect>());
        return;
    }
    
    // Process current matches
    ProcessMatches(matches);
    
    // Wait for refill animation, then check for cascades
    GetTree().CreateTimer(0.6f).Timeout += () => 
    {
        ProcessMatchesRecursive(onComplete);
    };
}
```

**Board Reshuffle**:

Detect when no valid moves exist and regenerate grid without losing swap attempts.

```csharp
// GridManager.cs
public void CheckAndReshuffleIfNeeded()
{
    if (!HasValidMoves())
    {
        GD.Print("[GridManager] No valid moves - reshuffling board");
        ReshuffleBoard();
    }
}

private void ReshuffleBoard()
{
    int attempts = 0;
    const int maxAttempts = 3;
    
    while (attempts < maxAttempts)
    {
        // Regenerate grid
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                _grid[x, y] = GenerateSafeElement(x, y);
            }
        }
        
        // Verify at least one valid move exists
        if (HasValidMoves())
        {
            EmitSignal(SignalName.GridRefillCompleted);
            GD.Print("[GridManager] Board reshuffled successfully");
            return;
        }
        
        attempts++;
    }
    
    GD.PrintErr("[GridManager] Failed to reshuffle board after 3 attempts");
}
```

**Null Safety in ProcessMatches**:

Add defensive checks to prevent NullReferenceException.

```csharp
// GridManager.cs
public void ProcessMatches(List<MatchResult> matches)
{
    if (matches == null || matches.Count == 0)
    {
        return;
    }
    
    // Mark matched elements with null checks
    foreach (MatchResult match in matches)
    {
        if (match?.MatchedPositions == null) continue;
        
        foreach ((int x, int y) in match.MatchedPositions)
        {
            if (IsValidPosition(x, y) && _grid[x, y] != null)
            {
                _grid[x, y].IsMatched = true;
            }
        }
    }
    
    // Remove matched elements with null checks
    for (int x = 0; x < GridSize; x++)
    {
        for (int y = 0; y < GridSize; y++)
        {
            if (_grid[x, y] != null && _grid[x, y].IsMatched)
            {
                _grid[x, y] = null;
            }
        }
    }
    
    ApplyGravity();
    RefillGrid();
}
```

### BattleManager Enhancements

**Immediate Swap Processing**:

Current design waits for all swaps to complete. New design processes each swap immediately.

```csharp
// BattleManager.cs
private void OnSwapCompleted(bool wasValid)
{
    if (!wasValid)
    {
        return;
    }
    
    // Process matches immediately after each valid swap
    ProcessSingleSwapMatches();
}

private void ProcessSingleSwapMatches()
{
    CurrentPhase = BattlePhase.PlayerCombo;
    EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
    
    // Process matches and cascades for this swap
    ProcessMatchesRecursive();
}

private void ProcessMatchesRecursive(bool isCascade = false)
{
    List<MatchResult> matches = _gridManager.FindAllMatches();
    
    if (matches.Count == 0)
    {
        // No more matches - apply accumulated effects
        ApplyAccumulatedEffects();
        
        // Check if player has more swaps
        if (_gridManager.RemainingSwaps > 0)
        {
            // Return to swap phase
            CurrentPhase = BattlePhase.PlayerSwap;
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        }
        else
        {
            // All swaps consumed - start enemy turn
            StartEnemyTurn();
        }
        return;
    }
    
    // Process combo effects and accumulate
    List<ComboEffect> comboEffects = _comboSystem.ProcessMatches(matches, isCascade);
    _accumulatedEffects.AddRange(comboEffects);
    
    // Visualize and process grid
    _gridUI?.VisualizeMatchesAndEffects(matches, comboEffects);
    _gridManager.ProcessMatches(matches);
    
    // Wait for animation, then check for cascades
    GetTree().CreateTimer(0.6f).Timeout += () => ProcessMatchesRecursive(isCascade: true);
}
```

**Hero-Specific Element Filtering**:

Modify ApplyComboEffect to check if owning hero is alive before applying effects.

```csharp
// BattleManager.cs
private void ApplyComboEffect(ComboEffect effect)
{
    // Get the hero responsible for this element
    PlayerCharacter activeHero = HeroSystem.GetHeroForElement(effect.ElementType);
    
    if (activeHero == null)
    {
        GD.PrintErr($"[BattleManager] No hero found for element type: {effect.ElementType}");
        return;
    }
    
    // CHECK: Is hero alive?
    if (!activeHero.IsAlive)
    {
        GD.Print($"[BattleManager] {activeHero.CharacterName} is dead, cannot use {effect.ElementType} combo");
        return; // Dead hero's elements do nothing
    }
    
    // Hero is alive - apply effect normally
    switch (effect.ElementType)
    {
        case ElementType.Fire:
        case ElementType.Sword:
            ApplyDamageEffect(effect, activeHero);
            break;
        case ElementType.Heal:
            ApplyHealEffect(effect, activeHero);
            break;
        case ElementType.Shield:
            ApplyShieldEffect(effect, activeHero);
            break;
    }
}
```

**Comprehensive Stun Checks**:

Add IsStunned check to all enemy actions including boss abilities.

```csharp
// BattleManager.cs
private void PerformEnemyAction(Enemy enemy)
{
    // CHECK: Is enemy stunned?
    if (enemy.IsStunned)
    {
        GD.Print($"[BattleManager] {enemy.CharacterName} is stunned and cannot act");
        return; // Skip all actions
    }
    
    // ... rest of action logic
}

private void PerformNecromancerAction(Enemy necromancer, PlayerCharacter target)
{
    // CHECK: Is boss stunned?
    if (necromancer.IsStunned)
    {
        GD.Print($"[BattleManager] {necromancer.CharacterName} is stunned and cannot use abilities");
        return; // Skip all abilities including summons
    }
    
    // ... rest of boss ability logic
}
```

**Reflection Chain Limiting**:

Track reflection state to prevent infinite loops.

```csharp
// Character.cs - Add parameter to TakeDamage
public virtual int TakeDamage(int damage, AttackType attackType, bool canReflect = true)
{
    if (!IsAlive)
    {
        return 0;
    }
    
    // ... existing damage calculation ...
    
    // Check for reflect damage ONLY if reflection is allowed
    if (canReflect)
    {
        StatusEffectData reflectEffect = _activeEffects.FirstOrDefault(e => e.Type == StatusEffect.ShieldReflect);
        if (reflectEffect != null && finalDamage > 0)
        {
            int reflectedDamage = Mathf.CeilToInt(damage * reflectEffect.ExtraData);
            GD.Print($"[{_name}] Reflected {reflectedDamage} damage!");
            return reflectedDamage;
        }
    }
    
    return 0;
}

// BattleManager.cs - Apply reflected damage with canReflect=false
int reflected = target.TakeDamage(damage, enemy.AttackType, canReflect: true);
if (reflected > 0)
{
    // Apply reflected damage WITHOUT allowing another reflection
    enemy.TakeDamage(reflected, target.AttackType, canReflect: false);
}
```

### Character System Enhancements

**Weakened Status Effect Implementation**:

Modify damage and defense calculations to check for Weakened status.

```csharp
// PlayerCharacter.cs
public int CalculateDamage(int baseDamage, ElementType elementType)
{
    int damage = baseDamage + _baseDamage;
    
    // Apply Weakened debuff
    if (HasStatusEffect(StatusEffect.Weakened))
    {
        damage = Mathf.CeilToInt(damage * 0.7f); // 30% reduction
        GD.Print($"[{_name}] Damage reduced by Weakened status");
    }
    
    return damage;
}

// Character.cs
public virtual int TakeDamage(int damage, AttackType attackType, bool canReflect = true)
{
    // ... existing code ...
    
    int finalDamage = damage;
    
    // Apply defense reduction
    int effectiveDefense = _baseDefense;
    
    // Apply Weakened debuff to defense
    if (HasStatusEffect(StatusEffect.Weakened))
    {
        effectiveDefense = Mathf.CeilToInt(effectiveDefense * 0.7f); // 30% reduction
        GD.Print($"[{_name}] Defense reduced by Weakened status");
    }
    
    finalDamage = Mathf.Max(1, finalDamage - effectiveDefense);
    
    // ... rest of damage calculation ...
}
```

**GetCombinedStats Documentation**:

Add clear documentation and use named tuple parameters.

```csharp
// DualHeroSystem.cs
/// <summary>
/// Gets combined stats for both heroes for saving.
/// RETURN ORDER: (mageHealth, mageMaxHealth, mageDamage, mageDefense,
///                warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense)
/// </summary>
public (int mageHealth, int mageMaxHealth, int mageDamage, int mageDefense,
        int warriorHealth, int warriorMaxHealth, int warriorDamage, int warriorDefense) GetCombinedStats()
{
    // Get Mage stats
    (int mageMaxHealth, int mageHealth, int mageDamage, int mageDefense) = Mage.GetStats();
    
    // Get Warrior stats
    (int warriorMaxHealth, int warriorHealth, int warriorDamage, int warriorDefense) = Warrior.GetStats();
    
    // Return in documented order
    return (
        mageHealth, mageMaxHealth, mageDamage, mageDefense,
        warriorHealth, warriorMaxHealth, warriorDamage, warriorDefense
    );
}
```

## Data Models

### ElementData Enhancement

Add hero ownership to grid elements.

```csharp
// ElementData.cs
public class ElementData
{
    public ElementType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsMatched { get; set; }
    public CharacterClass OwningHero { get; set; } // NEW: Which hero owns this element
    
    public ElementData(ElementType type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
        IsMatched = false;
        
        // Assign owner based on element type
        OwningHero = type switch
        {
            ElementType.Fire => CharacterClass.Mage,
            ElementType.Heal => CharacterClass.Mage,
            ElementType.Sword => CharacterClass.Warrior,
            ElementType.Shield => CharacterClass.Warrior,
            _ => CharacterClass.Mage // Default
        };
    }
    
    public static ElementData CreateRandom(int x, int y)
    {
        ElementType[] types = [ElementType.Fire, ElementType.Heal, ElementType.Sword, ElementType.Shield];
        ElementType randomType = types[GD.RandRange(0, types.Length - 1)];
        return new ElementData(randomType, x, y);
    }
}
```

### SaveData Validation Enhancement

Add schema version and migration support.

```csharp
// SaveData.cs
public class SaveData
{
    public int SchemaVersion { get; set; } = 1; // NEW: Track save format version
    
    // ... existing fields ...
    
    /// <summary>
    /// Migrates old save data to current schema version.
    /// </summary>
    public static SaveData Migrate(SaveData oldData)
    {
        if (oldData.SchemaVersion == 1)
        {
            // Current version - no migration needed
            return oldData;
        }
        
        // Future: Add migration logic for older versions
        GD.Print($"[SaveData] Migrating from schema version {oldData.SchemaVersion} to 1");
        
        // For now, return null to indicate migration failure
        return null;
    }
}
```

### Dynamic Scaling System

Create a centralized scaling calculator.

```csharp
// ScalingSystem.cs - NEW FILE
public static class ScalingSystem
{
    // Scaling coefficients
    private const float ENEMY_STAT_COEFFICIENT = 0.15f;
    private const float REWARD_COEFFICIENT = 0.1f;
    private const float COST_COEFFICIENT = 0.05f;
    
    /// <summary>
    /// Calculates scaled enemy stat based on wave number.
    /// Formula: baseStat * (1 + wave * 0.15)
    /// </summary>
    public static int ScaleEnemyStat(int baseStat, int waveNumber)
    {
        return Mathf.CeilToInt(baseStat * (1 + waveNumber * ENEMY_STAT_COEFFICIENT));
    }
    
    /// <summary>
    /// Calculates scaled coin reward based on wave number.
    /// Formula: baseReward * (1 + wave * 0.1)
    /// </summary>
    public static int ScaleReward(int baseReward, int waveNumber)
    {
        return Mathf.CeilToInt(baseReward * (1 + waveNumber * REWARD_COEFFICIENT));
    }
    
    /// <summary>
    /// Calculates scaled upgrade cost based on wave number.
    /// Formula: baseCost * (1 + wave * 0.05)
    /// </summary>
    public static int ScaleCost(int baseCost, int waveNumber)
    {
        return Mathf.CeilToInt(baseCost * (1 + waveNumber * COST_COEFFICIENT));
    }
}

// Enemy.cs - Update Create method
public static Enemy Create(EnemyType enemyType, int waveNumber)
{
    Enemy enemy = new()
    {
        EnemyType = enemyType,
        _waveNumber = waveNumber
    };
    
    (string name, int baseHp, int baseDmg, int baseDef, AttackType attackType, int coinReward) = GetEnemyBaseStats(enemyType);
    
    // Use ScalingSystem for consistent scaling
    int scaledHp = ScalingSystem.ScaleEnemyStat(baseHp, waveNumber);
    int scaledDmg = ScalingSystem.ScaleEnemyStat(baseDmg, waveNumber);
    int scaledDefense = ScalingSystem.ScaleEnemyStat(baseDef, waveNumber);
    int scaledReward = ScalingSystem.ScaleReward(coinReward, waveNumber);
    
    enemy.Initialize(name, scaledHp, scaledDmg, scaledDefense, attackType);
    enemy.CoinReward = scaledReward;
    
    return enemy;
}
```

### SaveSystem Error Handling

Enhance LoadGame with try-catch and validation.

```csharp
// SaveSystem.cs
public SaveData LoadGame()
{
    try
    {
        if (!FileAccess.FileExists(_savePath))
        {
            GD.Print("[SaveSystem] No save file found");
            EmitSignal(SignalName.LoadCompleted, false);
            return null;
        }
        
        // Read file
        using FileAccess file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"[SaveSystem] Failed to open save file: {FileAccess.GetOpenError()}");
            EmitSignal(SignalName.LoadCompleted, false);
            return null;
        }
        
        string jsonData = file.GetAsText();
        file.Close();
        
        // Validate JSON structure before deserialization
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            GD.PrintErr("[SaveSystem] Save file is empty");
            EmitSignal(SignalName.LoadCompleted, false);
            return null;
        }
        
        // Attempt deserialization
        SaveData saveData = null;
        try
        {
            saveData = JsonSerializer.Deserialize<SaveData>(jsonData, JsonOptions);
        }
        catch (JsonException jsonEx)
        {
            GD.PrintErr($"[SaveSystem] JSON deserialization failed: {jsonEx.Message}");
            GD.PrintErr("[SaveSystem] Save file may be corrupted");
            EmitSignal(SignalName.LoadCompleted, false);
            return null;
        }
        
        if (saveData == null)
        {
            GD.PrintErr("[SaveSystem] Deserialized save data is null");
            EmitSignal(SignalName.LoadCompleted, false);
            return null;
        }
        
        // Check schema version and migrate if needed
        if (saveData.SchemaVersion != 1)
        {
            GD.Print($"[SaveSystem] Outdated save schema (v{saveData.SchemaVersion}), attempting migration");
            saveData = SaveData.Migrate(saveData);
            
            if (saveData == null)
            {
                GD.PrintErr("[SaveSystem] Save migration failed");
                EmitSignal(SignalName.LoadCompleted, false);
                return null;
            }
        }
        
        // Validate save data integrity
        if (!ValidateSaveData(saveData))
        {
            GD.PrintErr("[SaveSystem] Save data validation failed - corrupted save");
            EmitSignal(SignalName.LoadCompleted, false);
            return null;
        }
        
        GD.Print($"[SaveSystem] Game loaded successfully - Wave {saveData.CurrentWave}");
        EmitSignal(SignalName.LoadCompleted, true);
        return saveData;
    }
    catch (Exception e)
    {
        GD.PrintErr($"[SaveSystem] Load failed with exception: {e.Message}");
        GD.PrintErr($"[SaveSystem] Stack trace: {e.StackTrace}");
        EmitSignal(SignalName.LoadCompleted, false);
        return null;
    }
}
```

### FIX Comment Resolutions

**GridManager Signal Fix**:
```csharp
// GridManager.cs - Line 217-219
if (allMatches.Count > 0)
{
    // Emit match count as integer (not List)
    EmitSignal(SignalName.MatchesFound, allMatches.Count);
    GD.Print($"[GridManager] Found {allMatches.Count} matches");
}
```

**SceneManager Fixes**:
```csharp
// SceneManager.cs
public void LoadScene(GameState targetState)
{
    // ... existing code ...
    
    // Use nameof for type safety
    CallDeferred(nameof(DeferredSceneChange), scenePath);
}

// Method must be public for CallDeferred
public void DeferredSceneChange(string scenePath)
{
    // ... existing code ...
}

// All scene loading methods use Instance
public static void ReturnToMainMenu()
{
    GameStateManager.Instance.ReturnToMainMenu();
    Instance.LoadScene(GameState.MainMenu);
}

public static void LoadGameFromSave()
{
    SaveData saveData = SaveSystem.Instance.LoadGame();
    if (saveData != null)
    {
        GameStateManager.Instance.LoadGame(saveData);
        Instance.LoadScene(GameState.Battle);
    }
}

public static void GoToShop()
{
    GameStateManager.Instance.ChangeState(GameState.Shop);
    Instance.LoadScene(GameState.Shop);
}

public static void StartBattle()
{
    GameStateManager.Instance.ChangeState(GameState.Battle);
    Instance.LoadScene(GameState.Battle);
}

public static void GameOver()
{
    GameStateManager.Instance.EndGame(false);
    Instance.LoadScene(GameState.GameOver);
}
```



## Correctness Properties

A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.

### Property 1: Signal Connection Idempotence

*For any* BattleManager instance, subscribing to GridManager.SwapCompleted multiple times should result in the handler being called exactly once per signal emission, not multiple times.

**Validates: Requirements 1.1**

### Property 2: Signal Cleanup on EndBattle

*For any* BattleManager instance that has subscribed to GridManager signals, calling EndBattle() should unsubscribe all handlers such that subsequent signal emissions do not trigger the handlers.

**Validates: Requirements 1.2**

### Property 3: Recursive Cascade Termination

*For any* grid state, processing matches recursively should eventually terminate when no more matches exist, not loop infinitely.

**Validates: Requirements 2.1, 2.2**

### Property 4: Cascade Effect Accumulation and Batch Application

*For any* sequence of cascading matches, all combo effects should be accumulated during cascade processing and applied in a single batch at the end, not incrementally during cascades.

**Validates: Requirements 2.3, 2.4**

### Property 5: Element Hero Ownership

*For any* grid element created, it should be assigned to either Mage or Warrior based on its element type (Fire/Heal → Mage, Sword/Shield → Warrior).

**Validates: Requirements 3.1**

### Property 6: Dead Hero Element Filtering

*For any* combo effect generated from a dead hero's elements (Fire/Heal for dead Mage, Sword/Shield for dead Warrior), the effect should produce no combat results (no damage, healing, or shield).

**Validates: Requirements 3.2, 3.3**

### Property 7: Living Heroes Enable All Elements

*For any* combo effect when both heroes are alive, all element types (Fire, Heal, Sword, Shield) should produce their normal effects.

**Validates: Requirements 3.5**

### Property 8: Immediate Match Processing After Swap

*For any* valid swap that creates matches, the matches should be processed (including cascades) before the next swap is allowed or before transitioning to enemy turn.

**Validates: Requirements 4.1, 4.2, 4.3**

### Property 9: Phase Transition After Swap Exhaustion

*For any* battle turn, when all player swaps are consumed and match processing completes, the system should transition to enemy turn phase.

**Validates: Requirements 4.4**

### Property 10: Null-Safe Match Processing

*For any* match result list containing null elements or invalid positions, ProcessMatches should skip invalid entries without throwing NullReferenceException.

**Validates: Requirements 5.1, 5.2, 5.3**

### Property 11: Weakened Status Reduces Combat Stats

*For any* character with Weakened status, both damage output and defense effectiveness should be reduced by 30% compared to normal values.

**Validates: Requirements 6.1, 6.2**

### Property 12: Weakened Status Expiration Restores Stats

*For any* character with Weakened status, when the status expires, damage and defense should return to their base values (without the 30% reduction).

**Validates: Requirements 6.3**

### Property 13: Stunned Enemies Skip All Actions

*For any* stunned enemy (including bosses), all actions (attacks, special abilities, summons, spells) should be skipped, but status effects should still be processed.

**Validates: Requirements 7.1, 7.2, 7.4**

### Property 14: Reflection Chain Limiting

*For any* attack that triggers reflection damage, the reflected damage should not trigger another reflection, limiting the chain to exactly one reflection per attack.

**Validates: Requirements 8.1**

### Property 15: Enemy Stat Scaling Formula

*For any* enemy created at wave N, its stats (HP, damage, defense) should equal baseStat * (1 + N * 0.15), rounded up.

**Validates: Requirements 9.1**

### Property 16: Reward Scaling Formula

*For any* enemy defeated at wave N, its coin reward should equal baseReward * (1 + N * 0.1), rounded up.

**Validates: Requirements 9.2**

### Property 17: Cost Scaling Formula

*For any* upgrade offered at wave N, its cost should equal baseCost * (1 + N * 0.05), rounded up.

**Validates: Requirements 9.3**

### Property 18: Save File JSON Validation

*For any* save file with invalid JSON structure (malformed, empty, or non-JSON content), LoadGame should return null without throwing exceptions.

**Validates: Requirements 10.1, 10.2**

### Property 19: Save Schema Migration

*For any* save file with outdated schema version, LoadGame should attempt migration to current version or return null if migration fails.

**Validates: Requirements 10.3**

### Property 20: No Valid Moves Detection

*For any* grid state where no adjacent swap would create a match, HasValidMoves() should return false.

**Validates: Requirements 12.1**

### Property 21: Reshuffle Generates Valid Grid

*For any* board reshuffle, the regenerated grid should have no initial matches and at least one valid move.

**Validates: Requirements 12.2, 12.4**

### Property 22: Reshuffle Preserves Swap Count

*For any* board reshuffle triggered during a turn, the remaining swap count should not decrease.

**Validates: Requirements 12.3**

### Property 23: Reshuffle Retry Logic

*For any* board reshuffle that fails to generate valid moves, the system should retry up to 3 times before giving up.

**Validates: Requirements 12.5**

## Error Handling

### Signal Connection Errors

- **Duplicate Connections**: Prevented by checking connection state before subscribing
- **Orphaned Connections**: Cleaned up in EndBattle() to prevent memory leaks
- **Null Manager References**: Checked before unsubscribing

### Grid Processing Errors

- **Null Elements**: Skipped with warning log, processing continues
- **Invalid Positions**: Bounds checked before access
- **Infinite Cascades**: Prevented by termination condition (no matches found)
- **No Valid Moves**: Detected and triggers automatic reshuffle

### Battle Logic Errors

- **Dead Hero Elements**: Filtered out before applying effects
- **Infinite Reflection**: Prevented by canReflect parameter
- **Missing Status Checks**: Comprehensive checks added for Weakened and Stunned

### Save System Errors

- **Corrupted JSON**: Caught with JsonException, returns null
- **Missing File**: Checked with FileExists before loading
- **Invalid Data**: Validated after deserialization
- **Schema Mismatch**: Migration attempted, fallback to null
- **User Feedback**: Error messages logged for debugging

### Scaling Errors

- **Negative Wave Numbers**: Formulas handle gracefully (result equals base value)
- **Integer Overflow**: Unlikely with reasonable wave numbers, but CeilToInt prevents float issues

## Testing Strategy

### Dual Testing Approach

This feature requires both unit tests and property-based tests for comprehensive coverage:

**Unit Tests** focus on:
- Specific examples of signal connection/disconnection
- Edge cases like empty grids, single-element grids
- Error conditions like corrupted save files
- Integration points between managers
- Specific game state transitions

**Property Tests** focus on:
- Universal properties across all grid states
- Scaling formulas across all wave numbers
- Status effect behavior across all character states
- Cascade processing across all match patterns
- Save/load round-tripping across all valid save data

### Property-Based Testing Configuration

- **Library**: Use NUnit with FsCheck for C# property-based testing
- **Iterations**: Minimum 100 iterations per property test
- **Generators**: Custom generators for GridState, SaveData, Character states
- **Shrinking**: Enable to find minimal failing cases

### Test Organization

```
Tests/
├── Unit/
│   ├── SignalManagementTests.cs
│   ├── GridProcessingTests.cs
│   ├── BattleLogicTests.cs
│   ├── SaveSystemTests.cs
│   └── ScalingTests.cs
└── Properties/
    ├── CascadeProperties.cs
    ├── HeroElementProperties.cs
    ├── StatusEffectProperties.cs
    ├── ScalingProperties.cs
    └── SaveLoadProperties.cs
```

### Property Test Examples

**Property 3: Recursive Cascade Termination**
```csharp
[Property(Iterations = 100)]
public Property CascadeProcessingTerminates(GridState initialGrid)
{
    // Tag: Feature: game-fixes-refactoring, Property 3: Recursive Cascade Termination
    var gridManager = new GridManager();
    gridManager.SetGrid(initialGrid);
    
    int maxIterations = 100;
    int iterations = 0;
    
    gridManager.ProcessMatchesRecursive(() => iterations++);
    
    return (iterations < maxIterations).ToProperty()
        .Label("Cascade processing should terminate within reasonable iterations");
}
```

**Property 15: Enemy Stat Scaling Formula**
```csharp
[Property(Iterations = 100)]
public Property EnemyStatsScaleCorrectly(EnemyType enemyType, PositiveInt waveNumber)
{
    // Tag: Feature: game-fixes-refactoring, Property 15: Enemy Stat Scaling Formula
    int wave = waveNumber.Get;
    var enemy = Enemy.Create(enemyType, wave);
    var baseStats = Enemy.GetEnemyBaseStats(enemyType);
    
    int expectedHp = Mathf.CeilToInt(baseStats.hp * (1 + wave * 0.15f));
    int expectedDmg = Mathf.CeilToInt(baseStats.damage * (1 + wave * 0.15f));
    int expectedDef = Mathf.CeilToInt(baseStats.defense * (1 + wave * 0.15f));
    
    return (enemy.MaxHealth == expectedHp &&
            enemy.BaseDamage == expectedDmg &&
            enemy.BaseDefense == expectedDef).ToProperty()
        .Label($"Enemy stats should scale with formula for wave {wave}");
}
```

### Unit Test Balance

- Focus unit tests on specific examples and edge cases
- Avoid writing too many unit tests for scenarios covered by properties
- Unit tests should complement property tests, not duplicate them
- Property tests handle comprehensive input coverage through randomization

### Test Coverage Goals

- **Signal Management**: 100% coverage of connection/disconnection paths
- **Cascade Processing**: All termination conditions tested
- **Hero Elements**: All combinations of hero alive/dead states
- **Status Effects**: All status types and expiration scenarios
- **Scaling**: All formulas verified across wave range 1-100
- **Save System**: All error conditions and validation rules
- **Board Reshuffle**: All retry scenarios and failure modes
