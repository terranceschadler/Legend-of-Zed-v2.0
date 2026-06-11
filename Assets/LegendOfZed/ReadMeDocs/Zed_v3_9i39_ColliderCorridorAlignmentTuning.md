# v3.9i39 — Collider Corridor Alignment Tuning

## Problem

v3.9i38 isolated direct collider-corridor frontage, but some corridors still drifted off the visible street.

The second merge pass was too aggressive and could group nearby parallel corridor lines together.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- Keeps direct collider corridor frontage.
- Keeps true collider-only reset.
- Keeps all supplemental frontage systems forced off.
- Reduces collider corridor line tolerance.
- Reduces second-pass line/gap tolerance.
- Uses median collider centerline/width inside the controlled direct setup.
- Slightly reduces max merged corridor short axis.

## Expected Log

`V3.9I39 COLLIDER CORRIDOR ALIGNMENT TUNING complete...`

Expected markers:

- `TrueColliderOnlyReset=True`
- `CorridorAlignmentTuning=True`
- `UsingColliderRoadRects=True`
- `DirectColliderCorridorFrontageStrips=...`

Supplemental systems should still be zero:

- `DeadEndStripsAdded=0`
- `GapFillBuildings=0`
- `IntersectionCornerStripsAdded=0`
- `FrontageEndpointExtensionsAdded=0`
- `EndpointFillerBuildings=0`
- `SlotFillerBuildings=0`

## Goal

Prevent nearby parallel street corridors from being averaged together, so frontage lines match the real street edges more closely.
