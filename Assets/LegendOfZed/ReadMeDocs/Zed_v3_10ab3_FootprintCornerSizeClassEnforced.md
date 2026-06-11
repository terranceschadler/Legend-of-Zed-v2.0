# v3.10AB3 — Footprint Corner Size Class Enforced

## Problem

Some visual corner lots still spawned non-corner buildings.

The road-based corner detector was not enough. The screenshot showed the selected generated building has:

`ZedBuildingFootprint -> Size Class = Corner`

That authored footprint data must be treated as authoritative.

## Fix

A lot is now treated as a strict Corner lot if any of these are true:

- `ZedAuthoredBuildingLot.buildingCategory == Corner`
- attached `ZedBuildingFootprint` serialized data says `Corner`
- road detection says it is a corner

For an effective Corner lot:

- only corner prefabs can spawn
- non-corner override prefabs are rejected
- no Any/general fallback is allowed

## Expected Log

`V3.10AB3 FOOTPRINT CORNER SIZE CLASS ENFORCED complete...`

Look for:

- `FootprintCornerCategoryLots`
- `StrictCornerCategoryRequiresCornerPrefab=True`
- `CornerFinalPrefabRejected`
- `CornerOverridePrefabRejected`

## Result

Lots marked as Size Class Corner cannot spawn non-corner buildings.
