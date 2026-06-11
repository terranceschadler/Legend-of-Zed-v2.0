# v3.9i24 — Disable Authored Spawn Fallback

## Decision

The old RoomTile-authored Building Spawn markers are not reliable for the procedural frontage system.

They were authored before the final road/frontage layout exists, so using them as a fallback can place buildings in visually wrong spots.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- `Enable Authored Spawn Fallback` now defaults to `false`
- the runtime auto-fix no longer turns authored spawn fallback back on
- frontage placement no longer depends on old tile spawn markers

## Expected Log

`V3.9I24 AUTHORED SPAWN FALLBACK DISABLED complete...`

Expected authored spawn fields should now stay zero:

- `AuthoredSpawnBuildings=0`
- `AuthoredSpawnPointsFound=0` unless manually re-enabled

## Direction Going Forward

Building placement should be controlled by the road frontage system, not by fixed authored spawn markers.
