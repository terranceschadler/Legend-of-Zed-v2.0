# v3.5 — Post-Generation City Block Building Filler

## Purpose

Stops trying to place buildings inside individual tile spaces.

Buildings should be generated after the full map exists, because the actual city block is created by adjacent tiles together.

## New system

`ZedPostGenerationCityBlockBuildingFiller`

This component:

1. waits for the legacy map generator to finish
2. scans the completed map for sidewalk/block surfaces
3. merges connected block surfaces
4. treats each merged block as one full city block
5. spawns buildings along the block perimeter
6. keeps old per-tile building spawning suppressed while active

## Added menu

`Legend of Zed/Map Integration/Add Post-Generation City Block Building Filler`

Run this once in the scene.

It creates:

`Zed_PostGeneration_CityBlockBuildingFiller`

## Important behavior

When the post-generation filler exists with `suppressLegacyTileBuildingSpawns = true`, old `ZedLegacyRoomTile` building spawning is skipped.

This prevents double-building placement.

## Current prototype limits

This first pass detects city blocks by scanning large flat sidewalk/floor renderers.

If it detects too many or too few blocks, tune these fields:

- `minBlockSize`
- `maxMergedBlockSize`
- `rejectRendererNameContains`
- `mergePadding`

If building placement is too close to roads, increase:

- `edgeInset`

## Not touched

This does not modify:

- road generation
- park tile generation
- boundary walls
- authored park prefabs
- your manual `ZedBuildableZone` objects

The old tile-local zone experiment is no longer the path forward.
