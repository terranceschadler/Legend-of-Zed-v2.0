# v3.2b — Runtime Editor Selection Destroy Safety

## Problem

Unity threw editor-only Inspector errors:

- `MissingReferenceException: The variable m_Targets of GameObjectInspector doesn't exist anymore`
- `SerializedObjectNotCreatableException: Object at index 0 is null`

These happen when Unity's Inspector is looking at an object that gets destroyed during Play. The map generator destroys `TileSpawn` markers as it consumes spawn points.

## Fix

`ZedLegacyRandomMapGenerator` now clears editor selection before destroying a selected `TileSpawn` marker or one of its children.

This prevents the Inspector from holding a stale target.

## Log cleanup

`ZedParkTilePropSpawner` now suppresses routine success logs by default, even if older prefab instances still have `logDetails` enabled.

## Not changed

- tile placement
- weighted city/park selection
- park tree spawning behavior
- boundary walls
- bad inferred building lot filler remains disabled from v3.2a

## Remaining expected warning

`Map tile bridge has no player or playerPrefab assigned yet...`

This is not a generator error. Assign the real top-down controller prefab to `Zed_MapTileGenerator_RuntimeBridge > Player Prefab`, or place a scene player tagged `Player`.
