# Progress: Door-Aware Stage Generation Enhancements

Track progress on implementing the enhancements for door-aware stage generation.

## Phase 1: Preparatory Refactoring and Initial Fixes

- [X] **Split `Room` Derived Classes into Separate Files**:
  - [X] Create `StartRoom.cs` with `StartRoom` class.
  - [X] Create `NormalRoom.cs` with `NormalRoom` class.
  - [X] Create `BossRoom.cs` with `BossRoom` class.
  - [X] Create `TreasureRoom.cs` with `TreasureRoom` class.
  - [X] Create `ShopRoom.cs` with `ShopRoom` class.
  - [X] Update `Room.cs` to only contain base `Room` and `RoomType` enum.
- [X] **Refactor `PlaceRoom` in `IsaacStageGenerator.cs`**:
  - [X] Remove `room.Initialize(...)` call.
  - [X] Set `room.gridPosition` directly.
  - [X] Set `room.template` directly.
  - [X] Delegate room instantiation and door creation to `SpecialRoomFactory.CreateRoom`.
- [X] **Correct `roomType` Access in `IsaacStageGenerator.cs`**:
  - [X] Change `previousRoom.roomType` to `previousRoom.template.roomType` in `GenerateMainPath`.
- [X] **Modify `GenerateMainPath` for Boss Room Placement**:
  - [X] `GenerateMainPath` calls `GetNextRoomPosition` for boss room.
  - [X] `GenerateMainPath` calls `PlaceRoom` with required direction for boss room.
  - [X] Remove direct boss room placement from `GenerateStage`.
- [X] **Enhance `GetNextRoomPosition` for Previous Room's Outgoing Door Check**:
  - [X] Add check for `previousRoom.template.HasDoor(placementDirection)`.
  - [X] Skip placement if previous room lacks the outgoing door.
- [X] **Update `SpecialRoomFactory.CreateRoom`**
  - [X] Accept an optional `specificTemplate` parameter.
  - [X] Use `specificTemplate` if provided, otherwise select by `RoomType`.
- [ ] **Playtest Phase 1**:
  - [ ] Boss rooms connect correctly.
  - [ ] Placement respects previous room's outgoing doors.
  - [ ] Room scripts are correctly separated and functional.

## Phase 2: Refactoring for More Robust Connections

- [X] **Define `RoomPlacementResult` Class/Struct**:
  - [X] Create `RoomPlacementResult` with `PlacedRoom` and `AvailableOutgoingDoors` properties.
- [X] **Modify `SpecialRoomFactory.CreateRoom` Method**:
  - [X] Change `SpecialRoomFactory.CreateRoom` return type to `RoomPlacementResult`.
  - [X] Populate `PlacedRoom` with the created room instance.
  - [X] Populate `AvailableOutgoingDoors` with all door directions from `templateToUse.possibleDoors`.
  - [X] Ensure the `catch` block returns a `RoomPlacementResult` indicating failure.
- [X] **Modify `IsaacStageGenerator.PlaceRoom` Method (and its overloads)**:
  - [X] Change its return type to `RoomPlacementResult`.
  - [X] Update its internal calls to `_roomFactory.CreateRoom` to correctly receive and handle `RoomPlacementResult`.
  - [X] If room creation fails (e.g., `result == null` or `!result.Success`), return an appropriate `RoomPlacementResult` indicating failure.
  - [X] Upon successful room placement, `result.PlacedRoom` is added to `_stageGrid`.
- [X] **Update `PlaceRoom` Call Sites in `IsaacStageGenerator.cs`**:
  - [X] `GenerateStage`: Adapt the call for `startRoom`.
  - [X] `GenerateMainPath`: Adapt calls for `newRoom` and `bossRoom`.
  - [X] `CreateBranch`: Adapt calls for `newRoom`.
  - [X] `PlaceSpecialRooms`: Adapt calls to `PlaceRoom`.
- [X] **Refactor Path and Branch Generation Logic (`GetNextRoomPosition`)**:
  - [X] Update `GetNextRoomPosition` to utilize `AvailableOutgoingDoors` from the `RoomPlacementResult` of the `previousRoom`.
- [X] **(Optional refinement for later) Consider if `AvailableOutgoingDoors` from the factory needs immediate adjustment within `PlaceRoom` (e.g., to exclude the entry door if `requiredDoorDirection` was specified).**
- [X] **Playtest Phase 2**:
  - [X] Stage generation robustly uses `AvailableOutgoingDoors`.
  - [X] Path and branch generation is predictable and correct.

---
