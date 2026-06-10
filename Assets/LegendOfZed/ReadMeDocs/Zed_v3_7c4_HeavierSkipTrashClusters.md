# v3.7c4 — Heavier Skip Trash Clusters

## Purpose

Increases trash and debris density around spawned skips/dumpsters.

## Changes

- `trashPilesPerDumpster` default increased from 4 to 10.
- `trashPileScatterRadius` default increased from 2.2 to 3.6.
- Adds `paperDebrisPerDumpster`.
- Adds `outerDebrisPerDumpster`.
- Adds `outerDebrisScatterRadius`.
- Spawns both trash-pile prefabs and paper-debris prefabs around each accepted skip.
- Adds `SkipAreaPaperDebris` to the log.

## Notes

This does not change skip placement rules.
This does not touch road-frontage buildings or boundary walls.
The menu still preserves your tuned prefab arrays.
