# The Binding of Isaac-Inspired Stage Generation Specification

## Overview
This document outlines a stage generation system inspired by The Binding of Isaac's approach to procedural level creation. The system will generate grid-based interconnected rooms with varying types, specialized content, and progressive difficulty.

## Room Structure

### Grid-Based Layout
- Stages are organized on a rectangular grid (typical size: 8×8 or larger)
- Rooms occupy single grid cells and connect via doors on cardinal directions (North, East, South, West)
- Not all grid cells will contain rooms - the layout forms a branching path

### Room Types
- **Starting Room**: Where the player begins each stage
- **Normal Rooms**: Standard combat encounters with varying enemy compositions
- **Boss Room**: End-of-stage room containing a boss encounter and stage exit
- **Special Rooms**: Extendible system of specialized room types
  - **Treasure Room**: Contains a card reward
  - **Shop**: Allows purchasing cards or upgrades
  - **[Future Special Room Types]**: Design system to easily add more special room types

## Generation Algorithm

### Stage Layout Creation
1. Place a starting room near the center of the grid
2. Generate a main path from the starting room to the boss room
   - Apply path length constraints (min/max number of rooms between start and boss)
3. Branch off the main path to create optional exploration paths
4. Place special rooms (treasure, shop) according to placement rules
   - Treasure rooms typically branch directly from the main path
   - Shops appear at medium distance from the starting room

### Connectivity Rules
- Rooms connect only via matching doors
- Doors only appear where rooms connect (no doors to nowhere)
- Some doors may be locked, requiring keys or special conditions to open

### Visualization
- Each room has 4 potential door positions (N, E, S, W)
- Door states: Open, Locked, None
- Mini-map reveals room layout as the player explores

## Room Content Generation

### Room Templates
- Each room type has multiple possible templates defining:
  - Door positions
  - Enemy spawn points
  - Obstacle/decoration placement
  - Special object locations

### Difficulty Scaling
- Room difficulty increases based on:
  - Distance from starting room
  - Stage/floor number
  - Number of special items collected

#### Future Enhancements for Difficulty Scaling:

##### 1. Tiered Enemy Selection
- **Concept:** Instead of or in addition to scaling enemy count, select different or more powerful enemy types based on the calculated room difficulty.
- **Implementation Idea:**
    - Modify the `EnemySpawnPoint` class within `RoomTemplate.cs`.
    - Add a structure (e.g., a list of objects or a dictionary) to `EnemySpawnPoint` that maps difficulty thresholds or tiers to specific enemy prefabs. For example:
        ```csharp
        // Inside EnemySpawnPoint class
        public List<EnemyTierMapping> difficultyTiers;
        
        [System.Serializable]
        public class EnemyTierMapping {
            public float minDifficultyThreshold; // Minimum difficulty for this tier
            public GameObject enemyPrefabForTier;  // Enemy prefab for this tier
        }
        ```
    - In `RoomContentPopulator.PopulateRoom`, iterate through these tiers (e.g., from highest threshold downwards).
    - Select the `enemyPrefabForTier` from the first tier whose `minDifficultyThreshold` is less than or equal to the current room `difficulty`.
    - This allows, for instance, basic enemies at low difficulty, and stronger variants or entirely different enemy types at higher difficulties, spawned from the same logical `EnemySpawnPoint` in the template.

##### 2. Direct Enemy Stat Scaling
- **Concept:** Modify the base statistics (health, damage, speed, etc.) of individual enemies based on room difficulty.
- **Implementation Idea:**
    - Ensure enemy prefabs have a common component (e.g., `EnemyStats.cs` or an interface like `IDifficultyScalable`) that exposes methods or properties for their core combat statistics.
    - In `RoomContentPopulator.PopulateRoom`, after an enemy is instantiated:
        - Get this stats component from the enemy GameObject.
        - Apply scaling to its stats based on the `difficulty` parameter. For example:
            ```csharp
            // Inside PopulateRoom, after enemy instantiation
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            if (stats != null) {
                float healthMultiplier = 1f + (difficulty * 0.75f); // Example: up to 75% more health at max difficulty
                float damageMultiplier = 1f + (difficulty * 0.5f);  // Example: up to 50% more damage
                stats.ScaleHealth(healthMultiplier);
                stats.ScaleDamage(damageMultiplier);
            }
            ```
    - This makes even the same enemy types more resilient and dangerous as the overall room difficulty increases.

### Special Features
- **Room Clearing**: Doors may lock until all enemies are defeated
- **Mini-map**: Progressively reveals the floor layout as the player explores

## Technical Implementation

### Core Classes
- **IsaacStageGenerator**: Main class handling the generation algorithm
- **RoomTemplate**: ScriptableObject defining room layouts and content
- **RoomInstance**: Runtime instance of a room with current state
- **DoorController**: Manages door states and connections between rooms
- **SpecialRoomFactory**: Factory pattern implementation for creating different special room types

### Generation Parameters
- **GridSize**: Width and height of the grid (default: 8×8)
- **MinRooms**: Minimum total number of rooms to generate
- **MaxRooms**: Maximum total number of rooms to generate
- **MainPathLength**: Min/max number of rooms between start and boss
- **SpecialRoomCounts**: How many of each special room type to include
- **TemplateWeights**: Probability distribution for room template selection

## Player Interaction
- Only one room is active at a time
- Camera is fixed to the current room
- Transitioning between rooms occurs when player touches a door
- Room state persists (defeated enemies stay defeated, collected items remain collected)

## Next Steps
- Create a small set of room templates for each room type
- Implement the IsaacStageGenerator class
- Create door transition mechanics
- Implement room content population
- Design a mini-map system
- Develop a special room extension framework
