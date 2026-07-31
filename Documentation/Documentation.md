# Altarion's Journey — Documentation

This is the single reference document for the project's architecture, core systems and gameplay design. It reflects the current state of the game: a turn-based dual-hero RPG played through a campaign map, not the original Match-3 prototype.

## Architecture

The game runs on **Godot 4** with **C#** as the only scripting language. The codebase follows a few consistent patterns:

- **Singleton managers (AutoLoad).** Root-level systems (`GameStateManager`, `AudioManager`, `SaveSystem`, `SceneManager`, `SettingsManager`, `InventoryManager`, `LootSystem`, `AbilitySystem`, `UIManager`) are registered as Godot AutoLoads and exposed through a static `Instance` property.
- **Dependency inversion via interfaces.** Every manager implements a matching interface in `Scripts/Interfaces` (`IGameStateManager`, `IBattleManager`, `IInventoryManager`, `ILootSystem`, `ISaveSystem`, `ISceneManager`, `ISettingsManager`, `IUIManager`, `IAbilitySystem`, `IAudioManager`). This keeps systems loosely coupled and consumable through an abstraction rather than a concrete singleton.
- **Data-driven content.** Game content (equipment, abilities, status effects, campaign levels) lives in static database classes (`EquipmentDatabase`, `AbilityDatabase`, `CampaignDatabase`) built from plain data records, not scattered across scene files or hardcoded logic.
- **Clear separation of concerns.** Data (`Scripts/Data`), core rules/constants (`Scripts/Core`), battle logic (`Scripts/Battle`), characters (`Scripts/Characters`), managers (`Scripts/Managers`), scenes (`Scripts/Scenes`) and UI (`Scripts/UI`) are kept in separate namespaces with minimal direct coupling.

### Testability boundary

Classes derived from Godot's `Node`/`Control`/`CanvasLayer` (managers, battle scene logic, UI) require the Godot engine runtime and cannot be instantiated in a headless `dotnet test` process. Only plain C# types with no Godot-engine dependency (data records, static rule/scaling classes, e.g. `CampaignDatabase`, `ScalingSystem`, `AbilityTargetingRules`) are covered by automated unit tests in `AlJourneyTests`, using xUnit. This is an intentional, accepted boundary — extracting pure-logic rule classes out of Node-derived systems is the established pattern for keeping game logic testable.

## Core Gameplay Loop

1. **Campaign Map** (`CampaignMapScene`) — the hub between levels. Shows every location in order, which levels are unlocked/completed, and gives access to the settlement shop.
2. **Level selection** — the player picks an unlocked level; `GameStateManager.SelectLevel` records it as the current level without starting combat yet.
3. **Battle** (`BattleScene`, `BattleManager`) — a turn-based fight against a curated sequence of waves defined by the selected level. All waves of a level play out back-to-back without leaving the battle scene.
4. **Level completion** — once every wave of the level is cleared, `GameStateManager.CompleteLevel` records progress, unlocks the next main-line level (if applicable) and the game returns to the campaign map.
5. **Shop** — accessible from the campaign map hub to spend coins on equipment upgrades between attempts.

## Combat System

Combat is turn-based, not the original Match-3 grid. Key pieces:

- **`BattleManager`** drives the fight: turn order (`BattlePhase.PlayerTurn` / `EnemyTurn` / `WaveTransition`), action resolution, wave progression within a level, and battle-end conditions.
- **`DualHeroSystem`** manages the two-hero party (Mage and Warrior). The player controls both heroes; each acts independently within a round.
- **Ability targeting** (`AbilityTargetingRules`) — pure, engine-independent rules for resolving valid targets and selecting default targets (e.g. highest-health enemy), fully unit tested.
- **Ultimate charge** — a shared party resource (0–100, `BattleManager.MaxUltimateCharge`) that fills as heroes and enemies act (`UltimateChargePerAction`) and unlocks a powerful ultimate ability once full.
- **Status effects** (`StatusEffect`): `Burning`, `Bleeding`, `Regeneration`, `ShieldReflect`, `Immunity`, `Stunned`, `Weakened`, `Freeze`, `Shock`, `Vulnerable`.
- **Enemy AI** (`EnemyAIController`) drives enemy turns, including special boss behavior (e.g. the Necromancer's summon ability).

## Campaign & Progression

`CampaignDatabase` is a static, hand-curated table of levels grouped into five locations, from the village ruins to the necromancer's lair:

`VillageRuins` → `DarkForest` → `BuriedCatacombs` → `FrozenWastes` → `NecromancerLair`

- Each location has a **main line** of levels (`LevelDefinition.IsBranch == false`) that must be cleared in order; clearing the last level of a location unlocks the first level of the next.
- Some locations also have an optional **branch level** — a tougher side fight guarding a miniboss (General of Draugr, Archskeleton), used as an extra source of loot.
- A level's **difficulty rating** (`LevelDefinition.DifficultyRating`) feeds `ScalingSystem` — the same scaling formulas used previously for the endless wave counter now scale off a level's difficulty instead of a wave number.
- Progress (`SaveData.CurrentLevelId`, `SaveData.CompletedLevelIds`) is persisted between sessions; `GameStateManager` exposes it through `CurrentLevelId` / `CompletedLevelIds`.

## Equipment & Loot

- **Slots** (`EquipmentSlot`): `Weapon`, `Head`, `Body`, `Legs`, `Necklace`, `Ring`, `Earring`.
- **Rarity** (`EquipmentRarity`): `Common`, `Uncommon`, `Rare`, `Epic`, `Legendary` — affects stats, value and drop odds.
- `LootSystem` generates loot for normal waves and guaranteed, higher-value loot for bosses, scaled by the current level's difficulty.
- `InventoryManager` handles adding, equipping and unequipping items per hero.

## Abilities

`AbilitySystem` manages per-hero ability unlocks and loadouts. Abilities (`AbilityData`) are either `Attack` or `Support` type, tied to an element (`Fire`, `Heal`, `Sword`, `Shield`), and target either `Enemy` or `AllyOrSelf`.

## Save System

`SaveSystem` serializes `SaveData` to JSON (`System.Text.Json`). `SaveData`'s parameterless constructor initializes every field, including newer ones (like campaign progress) with sensible defaults — so saves created before a given feature was added still deserialize safely, since missing JSON properties simply keep their constructor-assigned default.

## Project Structure

```text
AlJourney/
├── Documentation/     # This document
├── Scenes/            # Godot .tscn scenes (Battle, Campaign Map, Main Menu, UI)
├── Scripts/
│   ├── Battle/        # Turn-based combat logic, targeting rules, enemy AI
│   ├── Characters/    # Hero and enemy classes, party management
│   ├── Core/          # Constants and enums shared across systems
│   ├── Data/          # Static content databases and data records
│   ├── Interfaces/    # Abstractions for every manager
│   ├── Managers/      # AutoLoad singleton systems
│   ├── Scenes/         # Scene-level controllers (BattleScene, CampaignMapScene)
│   ├── UI/            # UI screens and widgets
│   └── Utils/          # Small standalone helpers (camera shake, particles, etc.)
├── AlJourneyTests/    # xUnit tests, mirroring the Scripts/ folder structure
└── Resources/         # Sprites, fonts, audio
```

## Manager API Reference

### IGameStateManager
Global game state: session lifecycle, campaign progress, coins, hero stats.
- `StartNewGame()` / `LoadGame(SaveData)` — start or resume a session.
- `CurrentState`, `ChangeState(GameState)` — high-level screen state (`MainMenu`, `Map`, `Battle`, `Shop`, `GameOver`, `Victory`).
- `CurrentLevelId`, `CompletedLevelIds`, `SelectLevel(string)`, `StartLevel(LevelDefinition)`, `CompleteLevel(string)` — campaign progress.
- `Coins`, `AddCoins(int)`, `SpendCoins(int)` — economy.
- `UpdateHeroStats(...)`, `EndGame(bool)`, `ReturnToMainMenu()`.

### IBattleManager
Turn-based combat orchestration for the current level.
- `CurrentPhase`, `CurrentWave`, `CurrentWaveIndex`, `TotalWavesInLevel`.
- `SelectActor(PlayerCharacter)`, `SelectAbility(AbilityData)`, `GetValidTargets()`, `ConfirmTarget(Character)`.
- `UltimateCharge`, `IsUltimateReady`.
- `StartBattle(DualHeroSystem, LevelDefinition, CameraShake)`, `EndBattle()`.

### IInventoryManager
- `AddItems(List<EquipmentData>)`, `EquipItem(CharacterClass, EquipmentData)`, `UnequipItem(CharacterClass, EquipmentSlot)`.

### ILootSystem
- `GenerateNormalLoot(int waveNumber)`, `GenerateBossLoot(int waveNumber)`.

### ISaveSystem
- `SaveGame()`, `LoadGame()`, `DeleteSave()`, `SaveFileExists()`, `AutoSave()`.

### ISceneManager
- `LoadScene(GameState)`, `LoadSceneByPath(string)`, `DeferredSceneChange(string)`, `ReloadCurrentScene()`.

### ISettingsManager
- Video: `Resolution`, `WindowMode`, `MaxFps`, `SetResolution(...)`, `SetWindowMode(...)`, `SetMaxFps(...)`, `ApplyVideoSettings()`.
- Audio: `MasterVolume`, `MusicVolume`, `SfxVolume`, `SetMasterVolume(...)`, `SetMusicVolume(...)`, `SetSfxVolume(...)`.
- `Language`, `SetLanguage(string)`, `SaveSettings()`, `LoadSettings()`, `ResetToDefaults()`.

### IUIManager
- `OpenMenu(Control)`, `CloseCurrentMenu()`, `CloseAllMenus()`.

### IAbilitySystem
- `GetAvailableAbilities(CharacterClass)`, `GetEquippedAbilities(CharacterClass)`, `UnlockAbility(...)`, `EquipAbility(...)`, `GetAbilityEffect(...)`, `GetTotalAbilityStats(...)`.

### IAudioManager
- `MasterVolume`, `MusicVolume`, `SfxVolume`.
- `PlayMusic(...)`, `TryPlayMusic(...)`, `StopMusic()`, `PlaySfx(...)`, `TryPlaySfx(...)`, `FadeOutMusic(...)`, `FadeInMusic(...)`, `CrossfadeMusic(...)`.
