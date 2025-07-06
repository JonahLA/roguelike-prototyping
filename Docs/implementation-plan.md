# Implementation Plan for Prototype

This document outlines a focused plan to build and test the core mechanics of **Flare - Prototype**:
1. Procedural stage generation with room prefabs
2. Enemy spawning and basic combat
3. Card system and card-based attacks
4. Card acquisition, upgrades, and shop overlay
5. Risk/Reward difficulty system

Each vertical slice follows the cycle: **design → code → playtest**.

---

## 0. Player Setup

**Goal**: Create a 2D top-down Player GameObject with responsive movement controls for scene testing.

- Player prefab:
  - Use a placeholder square sprite or simple 2D sprite asset
  - Components: `SpriteRenderer`, `Rigidbody2D` (Body Type: Dynamic, freeze Z rotation), `Collider2D` (BoxCollider2D or CircleCollider2D)
  - Optional: `Animator` for future sprite animations
- `PlayerMovementController` script:
  - Handle `Input.GetAxis("Horizontal")` and `Input.GetAxis("Vertical")`
  - Move the player by setting `Rigidbody2D.velocity` or using `Rigidbody2D.MovePosition`
  - Ensure movement is frame-rate independent (use `FixedUpdate` for physics)
- Camera setup:
  - Use an orthographic camera for top-down view
  - Attach a simple follow script or configure a Cinemachine Virtual Camera to follow the player

**Playtest**: Move the player around an empty 2D scene to verify input handling and movement.

---

## 1. Stage Generation

**Goal**: Procedurally assemble connected rooms using prefabs and generate a navigable floor plan.

- Data Model (`Stage`):
  - Room nodes (ID, prefab reference, exit positions)
  - Connection graph (which rooms connect via doors)
- `StageManager`:
  - Place `N` room prefabs on a grid or free-form layout according to a simple dungeon graph
  - Instantiate placeholder floor tiles to fill gaps between room centers
  - Spawn player at entry door and position camera
- Placeholder Assets:
  - Simple 2D room sprite prefab (`SpriteRenderer`, `BoxCollider2D`) with child door anchors (e.g., `Door_North`, `Door_South`)
  - Floor tile sprite prefab for filling gaps

**Playtest**: Verify a chain of rooms spawns, player can walk between them, and doors align.

---

## 1.5. Stage Generation via the BSP algorithm

**Goal**: Fully implement BSP-based dungeon generation from scratch in Unity.

- Design a `BSPNode` class holding a `Rect` region and references to left/right children.
- Implement a recursive split method:
  - Randomly choose vertical or horizontal splits based on region aspect ratio.
  - Enforce `minRoomSize` constraints to prevent sections from being too small or skinny.
- For each leaf node, define a room rectangle within the partitioned region.
- Carve corridors between sibling rooms:
  - Create straight or L-shaped corridors connecting room centers.
- Convert the BSP tree result into a 2D data structure or Tilemap:
  - Mark floor and wall tiles, and update Tilemap or generate meshes.
- Integrate the BSP generator into a new `BSPStageManager` or extend existing `StageManager`.
- Expose tunable parameters: `minRoomSize`, `maxRoomSize`, `splitIterations`, `corridorWidth`, `randomSeed`.

**Playtest**: Run multiple dungeon generations to validate connectivity, layout variety, and performance.

---

## 1.6. Isaac-Style Stage Generation

**Goal**: Implement a room-based dungeon generation system inspired by The Binding of Isaac, with grid-based layouts and specialized room types.

- Design core data structures:
  - `RoomTemplate` ScriptableObject with door positions, enemy spawn points, and obstacles
  - `RoomType` enum (Start, Normal, Boss, Treasure, Shop) with extendible architecture
  - `StageGrid` representing the dungeon layout on a grid
- Implement the `IsaacStageGenerator`:
  - Start from a central room
  - Generate main path from start room to boss room
  - Branch off secondary paths for exploration
  - Place special rooms (treasure, shop) according to rules
- Room connectivity system:
  - Implement `DoorController` to manage doors in cardinal directions
  - Support different door states (open, locked, none)
  - Handle room transitions when player touches doors
- Room content population:
  - Instance the appropriate room template based on position and type
  - Spawn enemies, obstacles, and rewards from the template
- Special room extensibility:
  - Create a `SpecialRoomFactory` class using the Factory pattern
  - Design an interface/abstract base class for special rooms
  - Make room types easily extendible for future additions
- Implement a simple mini-map UI:
  - Show discovered rooms and connections
  - Indicate current player position and special rooms

**Playtest**: Verify that multi-room stages generate correctly, doors connect rooms properly, and the player can navigate between them. Test different generation seeds for layout variety.

---

## 2. Enemy Spawning & Basic Combat

**Goal**: Populate rooms with enemies and allow the player to deal damage via a default attack.

- `Enemy` prefab:
  - Components: `Health`, `Damage`, basic AI (wander, chase player)
- `EnemySpawner`:
  - Takes spawn points from `StageManager` (e.g. designated transforms in each room)
  - Instantiates a configurable number of enemies per room
- Basic combat:
  - Player’s default attack (e.g. melee swing) deals damage on collision
  - Enemy death removes object and awards simple feedback (particle, sound)

**Playtest**: Confirm enemies spawn, chase, and can be killed by the player’s default attack.

---

## 3. Card System & Card-Based Attacks

**Goal**: Implement a hand of cards, flare point meter, and card-driven attacks.

- `Card` class:
  - Properties: `string Name`, `int FlareCost`, `CardType {Damage, Buff, Debuff}`, VFX placeholder
- Six base cards (placeholders):
  - 2× Damage cards
  - 2× Buff cards (e.g. heal, speed boost)
  - 2× Debuff cards (e.g. slow enemy, confusion)
- `DeckManager`:
  - Draw pile, discard pile
  - Shuffle on start and when deck exhausted
  - Draw a hand of up to 10 cards
  - Play card: subtract flare cost, move to discard pile, trigger effect
- UI:
  - Hand panel with card placeholders
  - Flare meter (fills over time)
- Card effects:
  - Spawn a basic projectile or apply buff/debuff placeholder

**Playtest**: Draw from the deck, spend flare points, play cards, and see effects on enemies.

---

## 4. Card Acquisition & Upgrades with Shop Overlay

**Goal**: Let players add new cards and upgrade existing ones via an in-game overlay shop.

- End-of-stage overlay:
  - Present 3–4 placeholder card options
  - Player picks one to add to draw pile
- Upgrade paths:
  - Radiant (good) vs. Cursed (evil) variants
  - Data object `CardUpgrade` adjusting stats (e.g. cost, effect magnitude)
- Shopkeeper interaction:
  - Trigger overlay when player uses an interact key near a shop NPC transform
- Placeholder UI panel for shop overlay

**Playtest**: Run multiple stages, open shop overlay, add/upgrade cards, and confirm deck updates.

---

## 5. Difficulty Scaling: Risk/Reward System

**Goal**: Empower the player to optionally increase challenge in exchange for greater rewards, creating a dynamic difficulty curve based on their choices.

- **Cursed Chests & Altars**:
  - Introduce interactable objects that offer powerful loot (e.g., rare cards, permanent stat boosts).
  - Activating them applies a "curse" or challenge, such as making all enemies on the floor stronger or spawning an elite enemy immediately.
- **Branching Paths**:
  - At the end of a floor, present the player with a choice of two or more doors leading to the next floor.
  - Paths can be labeled with their inherent risk and reward (e.g., "The Perilous Crypt: More enemies, guaranteed rare card" vs. "The Quiet Catacombs: Standard difficulty").
- **Challenge Rooms**:
  - Design optional rooms that are clearly marked as high-difficulty.
  - Completing the challenge (e.g., defeating waves of enemies, surviving for a set time) unlocks high-tier rewards.

**Playtest**: Verify that players can opt into challenges, that the difficulty increases as described, and that the rewards are correctly granted.

---

## Milestones & Next Steps

1. **Slice Completion**: Build and playtest each vertical slice end-to-end.
2. **Balance Iteration**: Adjust flare regen rate, card costs, enemy stats.
3. **UI/Feedback**: Add basic VFX, sound cues, and polish animations.
4. **Meta Progression**: Record Radiant vs. Cursed choices, track ratio for ending logic.

---

Keep this document updated as new prototype features are planned. Good luck and happy prototyping!
