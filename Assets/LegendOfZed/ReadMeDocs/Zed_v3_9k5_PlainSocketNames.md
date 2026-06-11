# v3.9k5 — Plain Socket Names

## Problem

Generated socket names like `MergedRoad_00_North` were still confusing.

## Fix

The clean merged socket builder now creates sockets named by where buildings spawn:

- `Buildings_North_00`
- `Buildings_South_00`
- `Buildings_East_00`
- `Buildings_West_00`

## Meaning

- `Buildings_North` = buildings spawn north/above the cyan line
- `Buildings_South` = buildings spawn south/below the cyan line
- `Buildings_East` = buildings spawn east/right of the cyan line
- `Buildings_West` = buildings spawn west/left of the cyan line

## How To Use

1. Select the tile prefab asset.
2. Run:
   `Legend of Zed / Map Integration / v3.9K / Rebuild CLEAN Enabled Sockets From Merged Road Geometry On Selected Prefab Assets`
3. Open the prefab.
4. Review the generated `Buildings_*` sockets.
5. Disable/delete only the ones that are obviously wrong.

## Enable Rule

Keep a socket enabled only if its cyan line sits on a valid sidewalk/building edge.
