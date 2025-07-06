# Entity & Player Death Handling Progress

- [x] `EntityDeathHandler.cs` created (modular, Inspector-driven, supports VFX, destroy/deactivate, UnityEvents)
- [x] `PlayerDeathHandler.cs` created (extends EntityDeathHandler, disables input, loads death screen scene)
- [x] Health.cs refactored to remove direct VFX spawning; VFX now handled by DeathHandler scripts
- [x] VFX integration with `VFXSpawner` (via Inspector field in handlers)
- [ ] Animation/fade-out support
- [ ] Loot drop hook implemented and tested
- [ ] Death screen scene created and loaded additively on player death
- [ ] Editor setup instructions written
- [ ] Documentation for extending/customizing handlers
