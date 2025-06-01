# Implementation Plan for Door-Aware Stage Generation Enhancements

This document outlines a two-phased plan to improve the "door direction" awareness in the `IsaacStageGenerator` for **Flare - Prototype**. The goal is to ensure robust and logical connections between rooms, particularly for critical rooms like the Boss room, and to make the generation logic more resilient.

---

## Phase 1: Immediate Fixes and Enhancements

**Goal**: Address critical bugs in the current door-aware logic and strengthen existing checks with minimal structural changes.

1.  **Modify `GenerateMainPath` for Boss Room Placement**:
    *   The `GenerateMainPath` method will take responsibility for placing the Boss room, ensuring it connects correctly to the end of the main path.
    *   It will call `GetNextRoomPosition` to determine the `bossRoomPos` and the `bossRoomRequiredDoorDir` based on the last normal room placed.
    *   It will then call `PlaceRoom(bossRoomPos, RoomType.Boss, bossRoomRequiredDoorDir)`.
    *   The direct `PlaceRoom` call for the Boss room in `GenerateStage` will be removed.

2.  **Enhance `GetNextRoomPosition` for Previous Room's Outgoing Door Check**:
    *   Modify `GetNextRoomPosition` to check if the `previousRoom` (the room from which a new room is being placed) has an actual door facing the intended `placementDirection`.
    *   This check (`previousRoom.template.HasDoor(placementDirection)`) will occur before considering a `nextPos` valid, preventing attempts to place rooms where the originating room cannot connect outwards.

**Playtest**: Verify that Boss rooms are consistently placed with correct door alignment to the main path. Confirm that room placement attempts are skipped if the source room lacks an appropriate outgoing door.

---

## Phase 2: Refactoring for More Robust Connections

**Goal**: Architecturally improve the connection logic by making rooms explicitly report their available outgoing doors, leading to a more robust and extensible generation system.

1.  **Define `RoomPlacementResult` Class/Struct**:
    *   Create a new data structure (`RoomPlacementResult`) to encapsulate the result of a room placement operation.
    *   It will contain the `Room PlacedRoom` and a `List<Direction> AvailableOutgoingDoors`.

2.  **Modify `PlaceRoom` Method**:
    *   Change the return type of `PlaceRoom` from `Room` to `RoomPlacementResult`.
    *   If placement fails, return `null`.
    *   If successful, populate `PlacedRoom` and `AvailableOutgoingDoors`. The `AvailableOutgoingDoors` list should exclude the direction used for the incoming connection to the current room.

3.  **Refactor Path and Branch Generation Logic**:
    *   Update methods like `GenerateMainPath` and `CreateBranch` to use `RoomPlacementResult`.
    *   When determining the next room's position, the `possibleDirections` will be sourced from the `previousRoomResult.AvailableOutgoingDoors`.
    *   Modify `GetNextRoomPosition` (or create a helper) to work with this list of available outgoing directions from the previous room, rather than iterating all cardinal directions.

**Playtest**: Thoroughly test stage generation to ensure rooms connect logically based on the new `AvailableOutgoingDoors` mechanism. Verify that path and branch generation is more constrained and predictable.

---
