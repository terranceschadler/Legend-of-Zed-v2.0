# v3.9k7.1 — Socket Facing Compile Fix

## Problem

v3.9k7 referenced old field names that do not exist in the current spawner:

- `forceZForwardFacing`
- `flipDeadEndFacing`
- `yawOffsetDegrees`

## Fix

The socket-facing helper no longer references those old fields.

Socket mode now directly rotates buildings using the socket direction:

- `awayFromRoad` = where the building sits
- building front faces `-awayFromRoad`, back toward the road

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Expected Log

`V3.9K7.1 SOCKET FACING COMPILE FIX complete...`
