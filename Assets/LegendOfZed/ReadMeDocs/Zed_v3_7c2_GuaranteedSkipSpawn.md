# v3.7c2 — Guaranteed Skip / Dumpster Spawn

## Problem

Dumpster skips still did not spawn.

Main cause: building blockers were collected per renderer. A skip placed beside one part of a generated building could be rejected by another renderer from the same building.

## Fix

- Collects one combined bounds per generated building instance.
- Places skips against the combined building bounds.
- Allows the skip to ignore only its source building.
- Keeps rejection against roads, other buildings, and other props.
- Adds `minimumDumpsterCount`.
- Adds `maxDumpsterAttempts`.
- Adds clearer log counters:
  - `DumpsterPrefabs`
  - `DumpsterAttempts`
  - `Dumpsters`
  - `TrashPiles`
  - `DumpsterRoadRejects`
  - `DumpsterBuildingRejects`
  - `DumpsterPropRejects`

## Notes

The menu still preserves your manually tuned prefab arrays.
This patch does not touch building placement or boundary wall logic.
