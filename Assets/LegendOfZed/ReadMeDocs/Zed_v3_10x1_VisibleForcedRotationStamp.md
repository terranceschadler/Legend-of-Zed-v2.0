# v3.10x1 — Visible Forced Rotation Stamp

## Purpose

The user reported that it looks like no rotation correction is happening.

This patch makes the rotation pass visible and verifiable.

## Changes

The final hard rotation pass now:

1. Converts chosen cardinal forward to exact Y euler rotation:
   - North = 0
   - East = 90
   - South = 180
   - West = -90
2. Applies it directly to the accepted building root:
   `instance.transform.rotation = Quaternion.Euler(0, yaw, 0)`
3. Appends the forced facing/yaw to the generated object name.

Example names:

- `Apartment_Corner_01_AuthoredLotBuilding_Facing_West_Yaw_-90`
- `SM_Bld_Shop_03_AuthoredLotBuilding_Facing_North_Yaw_0`

## Expected Log

`V3.10X1 VISIBLE FORCED ROTATION STAMP complete...`

Look for:

- `FinalHardRotationApplied`
- `FinalHardRotationNameStamps`

## Interpretation

If object names show different `_Facing_...` values but meshes still face the same direction, prefab child transforms are counter-rotating.

If object names do not include `_Facing_...`, this spawner path is not the active code path.
