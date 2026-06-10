# v3.4a — RoomTile-4Way Zone Rotation + Fill Tuning

## Problem

`RoomTile-4Way` explicit zones worked partially, but:

- some zones did not spawn buildings
- some buildings faced the wrong way

## Fix

### Rotation

`ZedBuildableZone` now has:

- `buildingYawOffset`
- `GetBuildingRotation()`

`ZedBuildingLotFiller` now spawns buildings with:

`zone rotation + buildingYawOffset`

The one-time tuning pass sets the current `RoomTile-4Way` zones to `buildingYawOffset = 180`.

### Fill reliability

`ZedBuildingLotFiller` now has:

- `trySingleBestFitIfRowEmpty`

If a zone cannot place a row, it tries to place one largest fitting building at the zone center/back edge.

### Zone retune

The one-time tuning pass recreates the four `RoomTile-4Way` zones slightly larger:

- wider than v3.4
- shallow enough to stay off roads
- still strict containment checked at runtime

## Still limited to one tile

Only `RoomTile-4Way` is touched.

Other room tiles still use the old fixed building spawns.
