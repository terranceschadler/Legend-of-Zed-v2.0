# v3.9k4 — Clean Merged Socket Builder

## Problem

The road-geometry socket menu from v3.9k3 created too many tiny sockets because it made sockets from every little road mesh piece.

## Fix

Adds a cleaner prefab menu:

`Legend of Zed / Map Integration / v3.9K / Rebuild CLEAN Enabled Sockets From Merged Road Geometry On Selected Prefab Assets`

This menu:

- clears old socket spam
- merges road mesh pieces into longer road corridors
- creates only a few socket lines per tile
- enables created sockets by default
- ignores line/marking/crosswalk renderers

## How To Use

1. Select one road tile prefab asset in the Project window.
2. Run the clean merged socket menu.
3. Open the prefab.
4. Review the few generated socket lines.
5. Disable or delete only the ones that are clearly wrong.

## What A Good Socket Looks Like

A good socket:

- is one long cyan line along a sidewalk/building edge
- does not cross the road
- does not sit in the middle of an intersection
- points away from road toward buildings

## Deprecated Menus

The older per-road-piece socket builder now only prints a warning.
The full-prefab placeholder builder now only prints a warning.
