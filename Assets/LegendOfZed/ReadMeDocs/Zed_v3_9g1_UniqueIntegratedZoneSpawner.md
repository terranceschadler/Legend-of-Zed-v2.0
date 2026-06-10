# v3.9g1 — Unique Integrated Zone Spawner

## Why

The previous log still showed the old spawner output and did not include:

`AutoZonesCreated=...`

That means Unity was still running the old `ZedZoneBasedBuildingSpawner`.

## Fix

Adds a new uniquely named component:

`ZedZoneBasedBuildingSpawnerV39G`

This avoids stale/serialized old component confusion.

## New Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedZoneBasedBuildingSpawnerV39G.cs`
- `Assets/LegendOfZed/Editor/ZedZoneBasedBuildingSpawnerV39GMenu.cs`

## Menus

Run these in order:

1. `Legend of Zed / Map Integration / Disable Old Building Spawners For V3.9G1`
2. `Legend of Zed / Map Integration / Add V3.9G1 Integrated Zone Building Spawner`

Then regenerate.

## Expected Log

The log must start with:

`V3.9G1 integrated zone building spawner complete...`

And include:

`AutoZonesCreated=...`

If it still says:

`Zone-based building spawner complete...`

then the old component is still active.

## Notes

Generated buildings still go under:

`Generated_RoadFrontage_Buildings`

so the street prop blocker system still sees them.
