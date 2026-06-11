# v3.10s2 — Corner Side-Pair Axis Mapping

## Problem

The previous corner pass tested all four cardinal rotations with loose road-edge scoring.

That can still choose an improper orientation because broad road/sidewalk blocker rectangles can score a nearby side/back road as valid.

## Fix

Corner lots now use deterministic side-pair mapping.

Model convention remains:

- local `+Z` = front / door side
- local `+X` = right-side window wall

Detected corner pairs map directly:

- north + east roads => forward +Z / right +X
- east + south roads => forward +X / right -Z
- south + west roads => forward -Z / right -X
- west + north roads => forward -X / right +Z

The candidate is still checked against road blockers before it is accepted.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10S2 CORNER SIDE-PAIR AXIS MAPPING complete...`

Look for:

`CornerCandidateMode=DetectedSidePairAxisMapping`
