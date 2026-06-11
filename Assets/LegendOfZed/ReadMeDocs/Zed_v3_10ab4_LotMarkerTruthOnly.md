# v3.10AB4 — Lot Marker Truth Only

## Problem

The category system had too much runtime detection:

- road corner detection
- footprint corner detection
- auto corner promotion
- visual inference

That made the result unpredictable and hard to author.

## Fix

The authored lot marker is now the source of truth.

The spawner uses only:

`ZedAuthoredBuildingLot.buildingCategory`

No road detection.
No footprint detection.
No automatic corner guessing.

## Rule

If the lot marker says:

`buildingCategory = Corner`

then only corner prefabs may spawn there.

If the lot marker says:

`buildingCategory = SmallShop`

then only SmallShop prefabs may spawn there.

Same for Apartment, Office, Filler, Alley, etc.

## Important Defaults

- `lotMarkerCategoryIsOnlyTruth = true`
- `filterPrefabsByAuthoredLotCategory = true`
- `allowAnyPrefabFallbackForCategory = false`
- `autoTreatDetectedCornerLotsAsCornerCategory = false`

## Corner Enforcement

For marker category Corner:

- non-corner overridePrefab is rejected
- main pool is filtered to corner prefabs
- final selected prefab is checked again before spawn
- no fallback to Any/general pool

## Expected Log

`V3.10AB4 LOT MARKER TRUTH ONLY complete...`

Look for:

- `LotMarkerCategoryIsOnlyTruth=True`
- `FilterPrefabsByAuthoredLotCategory=True`
- `AllowAnyPrefabFallbackForCategory=False`

## Usage

Set each lot marker category intentionally on the RoomTile prefab.

Do not rely on Any for finished authored tiles.
