# v3.10y — Authored Lot Forward Is Authority

## Problem

The visible facing stamp proved rotation is being applied:

`_Facing_North_Yaw_0`

So the issue is no longer that rotation is blocked. The issue is that the spawner is choosing the wrong direction.

## Fix

Stop inferring facing from roads.

The authored lot marker is now the authority:

`lot marker local +Z = building front direction`

The spawned building uses:

`building local +Z = lot marker local +Z`

The result is still snapped to cardinal directions.

## Disabled by Default

These inference paths are now disabled by default:

- nearest-road facing
- front-edge scoring
- right-hand corner pair orientation
- corner prefab name orientation
- corner side metadata orientation

## What This Means

If a building faces wrong, rotate the authored lot marker on the tile prefab.

The prefab model orientation remains the same:

- model `+Z` = front
- model `+X` = right

## Expected Log

`V3.10Y AUTHORED LOT FORWARD IS AUTHORITY complete...`

Look for:

- `DirectionMode=AuthoredLotForward`
- `UseAuthoredLotForwardAsFacingAuthority=True`
- `AuthoredLotForwardFacingUsed`
- `_Facing_..._Yaw_...` in generated building names
