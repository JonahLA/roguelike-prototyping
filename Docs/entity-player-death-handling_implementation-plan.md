# Entity & Player Death Handling Implementation Plan

## Overview
This plan describes the modular, event-driven system for handling player and enemy deaths in a unified, extensible way. The system supports VFX, animation, input disabling, loot drops, and UI transitions (e.g., death screen via additive scene).

---

## Phase 1: Core Death Handler System
- Create `EntityDeathHandler`:
  - Subscribes to `Health` death events.
  - Disables gameplay scripts (AI, movement, combat, etc.) in a consistent, Inspector-driven way.
  - Disables or hides the sprite/renderer (optionally after VFX/animation).
  - Triggers VFX using `VFXSpawner`.
  - Invokes UnityEvents for custom hooks (loot drops, UI, etc.).
  - Inspector-driven: assign objects/components to disable, VFX prefab, etc.
  - Optionally destroys or deactivates the GameObject after a delay.
- Create `PlayerDeathHandler` (inherits from `EntityDeathHandler`):
  - Adds logic for player-specific consequences (e.g., disables input, triggers death screen).
  - Loads a "DeathScreen" scene additively (placeholder for now).
  - Inspector-driven: assign input components to disable, etc.

---

## Phase 2: Animation & VFX Integration
- Add support for triggering death animation (if Animator present).
- Delay hiding/disabling GameObject until animation/VFX complete.
- Optionally fade out sprite if no animation.

---

## Phase 3: Loot Drop System (for Enemies)
- Add UnityEvent or delegate for loot drop logic.
- Example: assign a prefab or ScriptableObject for drop table.

---

## Phase 4: UI/Scene Integration (Player Death)
- Create a placeholder "DeathScreen" scene.
- Implement logic in `PlayerDeathHandler` to load this scene additively on death.
- Document how to extend for future UI/scene management.
  - Note: The placeholder scene can be replaced or extended with a full-featured UI (e.g., stats, restart/quit buttons, meta progression summary) as the project evolves. The handler is already set up to load any scene by name.

---

## Phase 5: Editor Setup & Documentation
- Write step-by-step instructions for adding and configuring handlers on player/enemy prefabs.
- Document Inspector fields and recommended usage patterns.

---

## Phase 6: Loot Manager Integration (Future)
- Implement a centralized `LootManager` that listens for loot drop events and handles actual loot spawning, drop tables, and randomization.
- Refactor enemy death logic to delegate loot spawning to the `LootManager`.
