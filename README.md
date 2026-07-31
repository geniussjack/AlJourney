# Altarion's Journey

![Godot](https://img.shields.io/badge/Godot-4.7.1-478CBF?style=for-the-badge&logo=godotengine&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-tested-25A162?style=for-the-badge)

**Altarion's Journey** is a turn-based dual-hero RPG. The player leads a Mage and a Warrior through a campaign map — from the ruins of a village to a necromancer's lair — fighting curated waves of enemies, gearing up, and unlocking abilities along the way.

---

## Gameplay

* **Campaign map:** progress through five locations in order, each with a main line of levels and an optional branch level guarding a miniboss.
* **Turn-based combat:** command both heroes independently each round, target enemies or allies, and build up a shared ultimate charge.
* **Deep RPG layer:** equipment across seven slots and five rarity tiers, per-hero unlockable abilities, and difficulty that scales with level progression.
* **Data-driven content:** enemies, equipment, abilities and campaign levels are all defined as data, not hardcoded logic — easy to balance and extend.

---

## Technology & Architecture

Built with **C#** on **Godot 4.7.1**, targeting **.NET 10**.

Key characteristics of the codebase:
* **Interface-driven managers:** every core system (`GameStateManager`, `IInventoryManager`, `IBattleManager`, and more) is exposed through an interface, keeping systems loosely coupled.
* **Data-driven design:** content lives in static database classes (`EquipmentDatabase`, `AbilityDatabase`, `CampaignDatabase`) built from plain data records.
* **Automated tests:** engine-independent logic (data validation, targeting rules, scaling formulas) is covered by xUnit tests in `AlJourneyTests`, mirroring the `Scripts/` folder structure.
* **XML documentation:** public types and members are documented with XML doc comments throughout the codebase.

*Full architecture, combat and API documentation lives in [`Documentation/Documentation.md`](Documentation/Documentation.md).*

---

## Running the project

1. Install [Godot 4.x with .NET (C#) support](https://godotengine.org/download).
2. Clone or download this repository.
3. Open the project (`project.godot`) in Godot Engine.
4. Click **Build** in the top-right corner to compile the C# scripts.
5. Press **Play** (or `F5`) to run the game.

---

## Project structure

```text
AlJourney/
├── Documentation/     # Architecture, combat, campaign and API documentation
├── Scenes/            # Godot .tscn scenes (Battle, Campaign Map, Main Menu, UI)
├── Scripts/           # C# source code
│   ├── Battle/        # Turn-based combat logic, targeting rules, enemy AI
│   ├── Characters/    # Hero and enemy classes
│   ├── Core/          # Shared constants and enums
│   ├── Data/          # Content databases (EquipmentDatabase, AbilityDatabase, CampaignDatabase)
│   ├── Interfaces/    # Abstractions for every manager
│   ├── Managers/      # AutoLoad singleton systems (GameState, Inventory, Loot, Save...)
│   ├── Scenes/        # Scene-level controllers
│   └── UI/            # User interface screens and widgets
├── AlJourneyTests/    # xUnit tests, mirroring the Scripts/ folder structure
└── Resources/         # Sprites, fonts, audio
```
