# v3.9b — Zone-Based Density + Variety Pass

## Purpose

Increase density and variety for the new zone-based building spawner without returning to road-frontage guessing.

## Patched

`Assets/LegendOfZed/Scripts/MapIntegration/ZedZoneBasedBuildingSpawner.cs`

## What Changed

Adds:

- optional multi-row packing for deeper buildable zones
- smaller/tighter dense candidate picking
- immediate prefab-repeat avoidance
- controlled random variety picks
- row usage logging

## New Settings

On `Zed_ZoneBasedBuildingSpawner`:

- `Allow Multi Row Packing`
- `Max Rows Per Zone`
- `Row Gap`
- `Min Row Depth`
- `Prefer Dense Packing`
- `Variety Pick Chance`
- `Avoid Immediate Prefab Repeat`

## Expected Log

`Zone-based building spawner complete. Zones=..., ZonesUsed=..., Buildings=..., FitRejects=..., RoadRejects=..., OverlapRejects=..., EmptyZoneRejects=..., RowsUsed=..., VarietySelections=..., RoadRects=...`

## Target

Compared to v3.9a:

- more buildings than 56 when zones have space
- still no road-lane building placement
- old road-frontage spawner remains disabled

## Notes

This does not touch:

- old road-frontage building spawner
- player
- bridge
- weapon pickup
- street props
- boundary walls
