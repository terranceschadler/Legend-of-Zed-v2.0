# v3.2 — Building Lot Filler Prototype

## Goal

Replace the old "one building per spawn marker" behavior with a denser city-block filler.

The old fixed spawn system does not understand different building widths, so it creates inconsistent gaps. This prototype uses the old markers as row hints, then fills the row with buildings based on estimated prefab footprint width.

## Changed behavior

`ZedLegacyRoomTile` now tries the new `ZedBuildingLotFiller` first.

If the lot filler places buildings successfully, the old fixed marker spawns are skipped.

If the lot filler cannot place anything, the old fixed spawn behavior remains as fallback.

## New scripts

- `ZedBuildingFootprint.cs`
- `ZedBuildableZone.cs`
- `ZedBuildingLotFiller.cs`

## How it works now

Without editing tile prefabs, the filler:

1. Reads the existing `buildingSpawns`.
2. Groups markers with similar facing direction into rows.
3. Estimates each building prefab footprint from renderers.
4. Packs buildings side-by-side along the row.
5. Adds occasional alley gaps.
6. Keeps park tiles safe by ignoring `TreeSpawn` and `ZedParkTilePropSpawner` tiles.

## Optional next step

For better control, add `ZedBuildableZone` children to a tile prefab.

If explicit `ZedBuildableZone` components exist, the filler uses those instead of inferring rows from old markers.

## Building footprint control

For buildings with bad renderer bounds, add `ZedBuildingFootprint` to the building prefab and enable `useManualFootprint`.

## Not changed

- road generation
- sidewalk generation
- tile placement
- park tile weighting
- boundary walls
