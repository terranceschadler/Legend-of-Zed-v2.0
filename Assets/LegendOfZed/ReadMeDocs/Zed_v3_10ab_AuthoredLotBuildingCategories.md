# v3.10AB — Authored Lot Building Categories

## Purpose

RoomTile owns the layout.
Authored lot marker owns position + facing.
Now the authored lot marker also owns the building category.

This prevents the spawner from placing any random building type on any lot.

## Lot Categories

`ZedAuthoredBuildingLot` now has:

`buildingCategory`

Options:

- Any
- Corner
- SmallShop
- Apartment
- Office
- Filler
- Alley

## Runtime Behavior

When `filterPrefabsByAuthoredLotCategory = true`, the spawner filters prefab candidates before dense retry/variety sorting.

If no matching prefab exists and `allowAnyPrefabFallbackForCategory = true`, it falls back to the general pool instead of leaving the lot empty.

## Editor Menu

Select authored lot markers, then use:

`Legend of Zed / Map Integration / v3.10AB / Set Selected Lots Category / ...`

## Expected Log

`V3.10AB AUTHORED LOT BUILDING CATEGORIES complete...`

Look for:

- `CategoryFilteredLots`
- `CategoryFallbackLots`
- `CategoryRejectedPrefabs`
- `FilterPrefabsByAuthoredLotCategory=True`
