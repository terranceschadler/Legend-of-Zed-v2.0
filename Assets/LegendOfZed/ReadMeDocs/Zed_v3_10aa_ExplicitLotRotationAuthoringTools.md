# v3.10aa — Explicit Lot Rotation Authoring Tools

## What The Last Test Proved

The building name stamp showed:

`Apartment_Corner_01_AuthoredLotBuilding_Facing_East_Yaw_90`

So rotation is being applied.

The problem is the runtime auto-orient guess picked the wrong street.

## Fix

Runtime auto-orient is disabled by default.

The stable rule is now:

`authored lot marker local +Z = building front`

The right fix is to rotate the authored lot marker on the tile/prefab.

## New Menus

- `Legend of Zed / Map Integration / v3.10AA / Rotate Selected Authored Lots 90 CW`
- `Legend of Zed / Map Integration / v3.10AA / Rotate Selected Authored Lots 90 CCW`
- `Legend of Zed / Map Integration / v3.10AA / Set Selected Authored Lots Facing North`
- `Legend of Zed / Map Integration / v3.10AA / Set Selected Authored Lots Facing East`
- `Legend of Zed / Map Integration / v3.10AA / Set Selected Authored Lots Facing South`
- `Legend of Zed / Map Integration / v3.10AA / Set Selected Authored Lots Facing West`
- `Legend of Zed / Map Integration / v3.10AA / Disable Runtime Auto-Orient On Spawner`

## Expected Log

`V3.10AA EXPLICIT LOT ROTATION AUTHORING TOOLS complete...`

Look for:

`AutoOrientAuthoredLotsTowardNearestStreet=False`

## Important

Generated runtime buildings cannot be the permanent source of truth. Rotate the authored lot marker on the source tile/prefab.
