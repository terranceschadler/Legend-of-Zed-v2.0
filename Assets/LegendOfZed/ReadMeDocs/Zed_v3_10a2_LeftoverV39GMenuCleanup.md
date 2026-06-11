# v3.10a2 — Leftover V39G Menu Cleanup

## Problem

The v3.10A cleanup removed the failed zone/frontage building spawner scripts, but missed one old editor menu:

`Assets/LegendOfZed/Editor/ZedZoneBasedBuildingSpawnerV39GMenu.cs`

That menu still referenced deleted types:

- `ZedRoadFrontageBuildingSpawner`
- `ZedZoneBasedBuildingSpawner`

So Unity compiled with missing type errors.

## Fix

This patch replaces the old menu with a compile-safe empty placeholder so Unity can compile again.

It also adds a one-time cleanup menu:

`Legend of Zed / Map Integration / v3.10A / REMOVE Leftover V39G Failed Menu`

Run that after import to delete the leftover V39G menu file completely.

## Expected Log

`V3.10A2 LEFTOVER V39G FAILED MENU cleanup complete...`
