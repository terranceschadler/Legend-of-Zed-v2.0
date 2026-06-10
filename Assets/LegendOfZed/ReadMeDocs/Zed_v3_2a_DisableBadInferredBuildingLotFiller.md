# v3.2a — Disable Bad Inferred Building Lot Filler

## Reason

The inferred-row prototype placed buildings in streets and rotated some buildings incorrectly.

That happened because the old building spawn markers are not reliable enough to infer full lot rows from.

## Fix

- `ZedLegacyRoomTile.useBuildingLotFiller` now defaults to `false`.
- The lot filler no longer auto-adds itself.
- `ZedBuildingLotFiller` is changed to explicit-zone-only.
- Old fixed building spawn behavior is restored by default.

## Next correct direction

Use authored `ZedBuildableZone` children on each city tile.

Do not infer lots from old markers.

The correct next pass should be:

1. Add one or two explicit `ZedBuildableZone` strips to `RoomTile-4Way`.
2. Enable `useBuildingLotFiller` only on that prefab.
3. Tune zone position, rotation, width, and depth.
4. Repeat on other city tiles after one tile works.

## Preserved

- weighted city/park tile selection
- park tile support
- boundary walls
- tree spawning
- old fixed building spawns as fallback/default
