# Entity & Player Death Handling Progress

- [x] `EntityDeathHandler.cs` created (modular, Inspector-driven, supports VFX, destroy/deactivate, UnityEvents)
- [x] `PlayerDeathHandler.cs` created (extends EntityDeathHandler, disables input, loads death screen scene)
- [x] Health.cs refactored to remove direct VFX spawning; VFX now handled by DeathHandler scripts
- [x] VFX integration with `VFXSpawner` (via Inspector field in handlers)
- [x] Animation/fade-out support (Animator trigger, fade-out, and unified cleanup logic via WaitAndCleanup)
- [x] Loot drop hook implemented and tested
- [X] Death screen scene created and loaded additively on player death
- [x] Editor setup instructions written
- [x] Documentation for extending/customizing handlers

For future:

- [ ] Loot drop event (UnityEvent and C# event) implemented and tested in `EntityDeathHandler`
- [ ] Plan for future `LootManager` integration added

## Editor Setup Instructions

### Adding EntityDeathHandler to an Enemy Prefab
1. Select your enemy prefab in the Project window.
2. In the Inspector, click **Add Component** and search for `EntityDeathHandler`.
3. Assign the required references:
   - **Health**: Drag the Health component here (or leave blank to auto-find).
   - **Scripts to Disable**: Add any AI, movement, or combat scripts to disable on death.
   - **Sprites to Hide**: Add SpriteRenderers to hide on death.
   - **Death VFX Prefab**: (Optional) Assign a VFX prefab to spawn on death.
   - **OnDeathEvents**: (Optional) Add UnityEvents for custom logic (e.g., loot drop).
   - **OnLootDrop**: (Optional) Add UnityEvents for loot drop logic.
   - **Cleanup Options**: Set delay and destroy/deactivate as needed.
   - **Animation & Fade-Out**: Assign Animator and trigger if using death animation.
4. Apply changes to the prefab.

### Adding PlayerDeathHandler to the Player Prefab
1. Select your player prefab in the Project window.
2. In the Inspector, click **Add Component** and search for `PlayerDeathHandler`.
3. Assign the required references:
   - **Input Scripts to Disable**: Add PlayerInput or custom input scripts.
   - **Death Screen Scene Name**: Set to `DeathScreen` (or your custom scene name).
   - All other fields as in `EntityDeathHandler`.
4. Apply changes to the prefab.

## Extending/Customizing Handlers
- Use UnityEvents in the Inspector to hook up custom logic (e.g., loot drops, UI transitions) without code changes.
- Subscribe to C# events in code for more advanced or centralized logic (e.g., a future LootManager).
- To add new death effects, extend `EntityDeathHandler` or `PlayerDeathHandler` and override `HandleDeath()`.
- For UI/scene changes, update the DeathScreen scene or load additional scenes as needed.
