# Progress: BSP Stage Generation

Track progress on implementing the BSP-based dungeon generation algorithm.

- [X] Define `BSPNode` class to hold region `Rect` and child references
- [X] Implement recursive split method with vertical/horizontal choice
- [X] Enforce `minRoomSize` and `maxRoomSize` constraints during splits
- [X] Generate room rectangles within each leaf partition
- [X] Carve corridors (straight or L-shaped) between sibling rooms
- [X] Convert BSP tree into a 2D Tilemap or procedural mesh (floor vs. wall)
- [X] Integrate into `BSPStageManager` or extend `StageManager`
- [X] Expose parameters: `minRoomSize`, `maxRoomSize`, `splitIterations`, `corridorWidth`, `randomSeed`
- [X] Playtest multiple generations to verify connectivity, variation, and performance
