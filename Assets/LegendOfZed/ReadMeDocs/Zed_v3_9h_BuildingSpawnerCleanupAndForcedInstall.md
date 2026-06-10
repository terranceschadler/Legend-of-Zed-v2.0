# v3.9h — Building Spawner Cleanup + Forced Install

## Purpose

Clean up the building spawner mess and install one unmistakable active building system.

## New Menu

Run:

`Legend of Zed / Map Integration / CLEAN INSTALL V3.9H Building System`

This menu:

- disables obsolete building spawner components
- clears generated building/auto-zone children
- installs `ZedForcedZoneBuildingSpawnerV39H`
- copies building prefabs from any old spawner that has them
- leaves one active building spawner

## Expected Runtime Log

After regenerating, you must see:

`V3.9H CLEAN forced zone building spawner complete...`

If you do not see that line, the new spawner is not running.

## Generated Root

Buildings are still created under:

`Generated_RoadFrontage_Buildings`

so the street prop blocker system sees them.

## Notes

This does not touch player, bridge, street props, or boundary walls.
