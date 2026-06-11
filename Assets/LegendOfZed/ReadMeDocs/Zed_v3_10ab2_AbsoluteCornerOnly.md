# v3.10AB2 — Absolute Corner-Only Building Pool

## Problem

Some corner lots still spawned non-corner buildings.

The bypasses were:

1. `overridePrefab` could bypass category filtering.
2. Detected corners were not always promoted to Corner category.
3. There was no final prefab validation right before spawn.

## Fix

Corner-only is now enforced in every path.

For an effective Corner lot:

- non-corner `overridePrefab` is rejected and ignored
- main prefab pool is filtered to Corner prefabs only
- no Any/general fallback is allowed
- selected prefab is checked again before spawn
- detected corner lots are promoted to Corner category even if the marker was set wrong

## Definition of Corner Prefab

A corner prefab is any building prefab whose name matches the Corner category keyword list.

Default:

`cornerCategoryKeywords = ["Corner"]`

## Expected Log

`V3.10AB2 ABSOLUTE CORNER-ONLY BUILDING POOL complete...`

Look for:

- `StrictCornerCategoryRequiresCornerPrefab=True`
- `AutoTreatDetectedCornerLotsAsCornerCategory=True`
- `CornerOverridePrefabRejected`
- `CornerFinalPrefabRejected`
- `StrictCornerCategoryRejects`

## Result

A corner lot cannot spawn a non-corner building.

If no corner prefab is available, that lot stays empty rather than spawning the wrong building.
