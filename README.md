# Altarion's Journey

![Godot](https://img.shields.io/badge/Godot-4.7-478CBF?style=for-the-badge&logo=godotengine&logoColor=white)
![GDScript](https://img.shields.io/badge/GDScript-355570?style=for-the-badge&logo=godotengine&logoColor=white)

**Altarion's Journey** is a turn-based dual-hero RPG. The player leads a Mage and a Warrior through a campaign map — from the ruins of a village to a necromancer's lair — fighting curated waves of enemies, gearing up, and unlocking abilities along the way.

---

## Gameplay

* **Campaign map:** progress through five locations in order, each with a main line of levels and an optional branch level guarding a miniboss.
* **Turn-based combat:** command both heroes independently each round, target enemies or allies, and build up a shared ultimate charge.
* **Deep RPG layer:** equipment across seven slots and five rarity tiers, per-hero unlockable abilities, and difficulty that scales with level progression.
* **Data-driven content:** enemies, equipment, abilities and campaign levels are all defined as data, not hardcoded logic — easy to balance and extend.

---

## Technology & Architecture

Built with **GDScript** on **Godot 4.7**.

Key characteristics of the codebase:
* **Autoload-based managers:** every core system (`GameStateManager`, `InventoryManager`, `SaveSystem`, and more) is a Godot autoload singleton, referenced directly by its global name — no separate interface layer.
* **Data-driven design:** content lives in static database scripts (`EquipmentDatabase`, `AbilityDatabase`, `CampaignDatabase`) built from plain data classes.
* **GDScript documentation:** classes, methods and signals are documented with `##` doc comments throughout the codebase, per the Godot docstring convention.

---

## Running the project

1. Install [Godot 4.7](https://godotengine.org/download) (the Steam release works — no .NET/Mono support required).
2. Clone or download this repository.
3. Open the project (`project.godot`) in Godot Engine.
4. Press **Play** (or `F5`) to run the game.

---

## Project structure

```text
AlJourney/
├── Scenes/            # Godot .tscn scenes (Battle, Campaign Map, Main Menu, UI)
├── Scripts/           # GDScript source code
│   ├── Battle/        # Turn-based combat logic, targeting rules, enemy AI
│   ├── Characters/    # Hero and enemy classes
│   ├── Core/          # Shared constants and enums
│   ├── Data/          # Content databases (EquipmentDatabase, AbilityDatabase, CampaignDatabase)
│   ├── Managers/      # Autoload singleton systems (GameState, Inventory, Loot, Save...)
│   ├── Scenes/        # Scene-level controllers
│   └── UI/            # User interface screens and widgets
└── Resources/         # Sprites, fonts, audio
```
