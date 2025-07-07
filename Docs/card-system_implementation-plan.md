# Card System & Card-Based Attacks Implementation Plan

## Overview
This plan details the modular, extensible implementation of the card system, including hand/deck management, flare meter, card effects, and UI. The system is designed for future upgrades and designer-friendly extensibility.

---

## Steps

### 1. Card Data & Effect Architecture

- [x] **Card ScriptableObject** _(Complete)_
  - Create a `Card` ScriptableObject with: `Name`, `FlareCost`, `CardType` (Damage, Buff, Debuff), description, and references to effect and targeting ScriptableObjects.
- [x] **CardEffect ScriptableObject** _(Complete)_
  - Define a base `CardEffect` ScriptableObject (e.g., `ICardEffect`).
  - Implement basic effects: damage, heal, buff, debuff.
- [x] **TargetingStrategy ScriptableObject** _(Complete)_
  - Define a base `CardTargetingStrategySO` ScriptableObject.
  - Implement strategies: closest enemy, healthiest enemy, self, all enemies, etc.
- [x] **Extensibility for Upgrades** _(Complete)_
  - Ensure Card and Effect data structures can be extended for upgrades (e.g., stat modifiers, effect overrides).

---

### 2. Deck & Hand Management

- [ ] **DeckManager**
  - Manages draw pile, discard pile, and shuffling.
  - Handles drawing a hand of 5 cards, discarding, and reshuffling.
  - Exposes events for hand updates.
- [ ] **HandController**
  - Manages the player’s current hand, card selection, and play/discard logic.
  - Supports both “discard manually” and “discard on play” modes (configurable).
  - Integrates with Unity’s new input system for keyboard controls.

---

### 3. Flare Meter System

- [ ] **FlareMeter**
  - Tracks current/max flare points.
  - Increases over time (tick-based or per-second).
  - Exposes methods for other systems (e.g., cards, pickups) to add flare.
  - Notifies UI and DeckManager on change.

---

### 4. Card Play & Effect Resolution

- [ ] **CardPlayController**
  - Handles playing a card: checks flare cost, triggers effect, moves card to discard pile.
  - Requests targets from the card’s TargetingStrategy.
  - Integrates with player combat system for attacks/buffs/debuffs.
  - Handles error feedback (e.g., insufficient flare).

---

### 5. UI Integration

- [ ] **Hand UI**
  - Displays the player’s hand (5 cards), supports selection and play/discard feedback.
- [ ] **Flare Meter UI**
  - Visualizes current/max flare, animates changes, and provides feedback.

---

### 5.5. Buffs & Debuffs System

- [ ] **Buff/Debuff System**
  - Implement a modular Buff/Debuff system for applying temporary or permanent status effects to entities.
  - Integrate with card effects and enemy abilities.
  - Ensure effects can be stacked, timed, and removed/expired cleanly.
  - Provide hooks for UI feedback (icons, timers, etc.).

---

### 6. Card Effects Implementation

- [ ] **Damage Card Effect**
  - Spawns a basic projectile or triggers a melee attack on target(s).
- [ ] **Buff Card Effect**
  - Applies a temporary buff to the player (e.g., heal, speed boost).
- [ ] **Debuff Card Effect**
  - Applies a debuff to enemy target(s) (e.g., slow, confusion).

---

### 7. Testing & Documentation

- [ ] **Editor Tools**
  - Utilities for creating/editing cards, effects, and targeting strategies.
- [ ] **Unit Tests**
  - Tests for deck shuffling, card play, effect resolution, and targeting.
- [ ] **Documentation**
  - Guide for adding new cards/effects and integrating with other systems.

---

### 8. Polish & Playtest

- [ ] **Polish Pass**
  - Add placeholder VFX/SFX hooks for card play and effects.
  - Refine UI/UX for clarity and responsiveness.
- [ ] **Playtest**
  - Draw from the deck, spend flare, play/discard cards, and see effects on enemies.

---

## Notes

- All systems should be modular, event-driven, and extensible for future upgrades and new card types.
- Targeting and effect logic should be decoupled from enemy spawning and room population.
- No classes should be wrapped in namespaces, per project standards.
