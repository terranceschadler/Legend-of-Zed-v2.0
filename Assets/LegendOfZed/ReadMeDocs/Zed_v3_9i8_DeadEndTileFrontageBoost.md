# v3.9i8 — Dead-End Tile Frontage Boost

## Problem

Dead-end tiles were not getting enough buildings.

The merged road-run frontage system works better on through-streets than on dead-end/cap tiles because dead-end road chunks often do not produce enough long frontage strips.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds a dead-end tile booster:

- scans generated `ZedLegacyRoomTile` instances
- detects tile names containing:
  - `deadend`
  - `dead-end`
  - `dead end`
- creates supplemental tile-edge frontage strips
- rejects strips that are too close to road, map boundary, or existing frontage
- allows extra buildings on those dead-end supplemental strips

## New Log Fields

- `DeadEndTilesScanned`
- `DeadEndStripsAdded`

## Expected Log

`V3.9I8 DEAD-END FRONTAGE BOOST complete... DeadEndTilesScanned=..., DeadEndStripsAdded=...`

## Tuning

If dead-end tiles are still sparse:

- increase `Max Dead End Extra Strips Per Tile`
- lower `Dead End Existing Strip Reject Distance`
- lower `Dead End Road Clearance`

If dead-end buildings get too close to boundary walls:

- increase `Dead End Boundary Clearance`
