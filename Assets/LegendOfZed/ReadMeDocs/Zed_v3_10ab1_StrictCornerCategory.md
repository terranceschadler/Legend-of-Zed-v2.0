# v3.10AB1 — Strict Corner Category

## Problem

v3.10AB added lot categories, but the fallback behavior still allowed a Corner lot to end up with a non-corner building if no matching corner prefab was found.

That is wrong for this project.

## Fix

Corner lots are now strict:

- Corner category only accepts prefabs matching the corner keyword list.
- Corner category does not fall back to Any/general prefabs.
- If a marker is still `Any` but the lot is detected as a corner, it is treated as `Corner`.

## Default Settings

- `allowAnyPrefabFallbackForCategory = false`
- `strictCornerCategoryRequiresCornerPrefab = true`
- `autoTreatDetectedCornerLotsAsCornerCategory = true`

## Expected Log

`V3.10AB1 STRICT CORNER CATEGORY complete...`

Look for:

- `StrictCornerCategoryRequiresCornerPrefab=True`
- `AutoTreatDetectedCornerLotsAsCornerCategory=True`
- `AutoCornerCategoryLots`
- `StrictCornerCategoryRejects`

## Usage

Set real corner lot markers to:

`buildingCategory = Corner`

Then only prefabs whose names match the Corner keyword list can spawn there.
