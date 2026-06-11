# Zed v3.10AF8 — Runtime NavMesh Before Offscreen Spawns

Fixes the offscreen enemy spawn director waiting forever for a NavMesh near the player.

## What changed

- `ZedMapTileGeneratorRuntimeBridge` now queues a runtime NavMesh rebuild after map integration and a short settle delay.
- `ZedOverworldRuntimeNavMeshBuilder` now exposes whether a valid runtime NavMesh has been built.
- `ZedOffscreenEnemySpawnDirector` waits quietly for that runtime NavMesh instead of warning immediately.
- Added editor menu helpers under `Legend Of Zed > Navigation`.

## Default timing

The bridge waits 5 frames and 2 seconds after map integration, allowing the other generated-map passes to finish building walls, roads, lots, and props before the runtime NavMesh is rebuilt.

## Menu

- `Legend Of Zed > Navigation > Rebuild Runtime NavMesh Now`
- `Legend Of Zed > Navigation > Select Runtime NavMesh Builder`

## Notes

Warnings and errors still show. Normal successful build logs stay quiet unless the builder's `LogBuild` toggle is enabled.
