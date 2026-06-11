# v3.10s2.1 — Backout Broken S2 Compile Fix

## Problem

The v3.10s2 corner side-pair patch damaged `ZedAuthoredBuildingLotSpawner.cs`.

Unity errors included missing methods such as:

- `BuildPrefabCandidateList`
- `IsCornerLot`
- `TryCalculateRendererBounds`
- `TryChooseCardinalFacingByFrontEdgeRoadScore`
- `ApplyCenteredFacing`
- `BoundsToRect`
- `ScaleRectFromCenter`
- `AxisOverlap`

## Fix

This patch restores the last compile-safe spawner from v3.10s1.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10S2.1 BACKOUT BROKEN S2 COMPILE FIX complete...`

This unblocks Unity. Corner orientation should be reworked from a clean file after this.
