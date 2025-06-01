# Progress: Isaac-Style Stage Generation

Track progress on implementing the Isaac-Style room-based dungeon generation system.

- [x] Design `RoomTemplate` ScriptableObject with door positions and content definitions
- [x] Create `RoomType` enum with extensible architecture
- [x] Implement `StageGrid` to represent the room layout
- [x] Develop the `IsaacStageGenerator` for procedural layout creation
- [x] Implement room placement starting from a central room
- [x] Generate main path from start room to boss room
- [x] Add secondary branching paths for exploration
- [x] Place special rooms (treasure, shop) according to rules
- [x] Create the `DoorController` to manage door states and connections
- [x] Implement room transitions when player touches doors
- [x] Build the `SpecialRoomFactory` class using Factory pattern
- [x] Refactor and remove code duplication between generators and factories
- [x] Create room content population system
  - [x] Create `RoomContentPopulator` script
  - [x] Implement enemy spawning logic in `RoomContentPopulator` (iterate `enemySpawnPoints`, instantiate prefabs, handle `count` and `spawnRadius`)
  - [x] Integrate `RoomContentPopulator` with `IsaacStageGenerator`
  - [x] Basic difficulty influence on content population (pass difficulty parameter)
  - [x] Implement difficulty-based enemy count scaling in `RoomContentPopulator`
  - [x] Test enemy spawning with various `RoomTemplate` configurations and difficulty values
- [ ] Design enemy spawn points and obstacle placement
- [ ] Implement mini-map UI showing discovered rooms and connections
- [ ] Add player position and special room indicators to mini-map
- [ ] Test multiple seeds to verify layout variety and connectivity
- [x] Add difficulty scaling based on room distance from start (and branch depth)
