# v3.10z — Auto-Orient Authored Lots To Streets

## Problem

The visible facing stamp proved that building rotation is being applied.

The current log showed:

- `DirectionMode=AuthoredLotForward`
- `AuthoredLotForwardFacingUsed`
- selected building name `_Facing_North_Yaw_0`

So the building is obeying the lot marker. The bad direction means the lot marker itself is facing north.

Old blgSpawn-derived authored lots were not reliably rotated to face the street.

## Fix

Before spawning, the spawner can now auto-rotate authored lot markers so their local `+Z` points toward the nearest valid street.

Then the existing clean rule works:

- lot marker `+Z` = building front
- building `+Z` = lot marker `+Z`

## New Settings

- `autoOrientAuthoredLotsTowardNearestStreet = true`
- `autoOrientLotStreetSearchDistance = 32`

## Expected Log

`V3.10Z AUTO-ORIENT AUTHORED LOTS TO STREETS complete...`

Look for:

- `AuthoredLotsAutoOrientedToStreet`
- `AuthoredLotsAutoOrientFallbacks`
- `AutoOrientAuthoredLotsTowardNearestStreet=True`

Generated names should still include:

`_Facing_..._Yaw_...`
