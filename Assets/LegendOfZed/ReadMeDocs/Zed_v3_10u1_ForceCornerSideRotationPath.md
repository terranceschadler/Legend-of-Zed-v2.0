# v3.10u1 — Force Corner Side Rotation Path

## Problem

v3.10U added corner-side metadata, but it had no visible effect because the old corner/front-edge scoring path could still control the final rotation.

## Fix

For corner lots, the spawner now directly forces the rotation from:

- detected road side pair
- prefab `ZedBuildingCornerSide`
- model contract: local `+Z` is front

No all-cardinal scorer override is allowed for corner lots once a valid side-pair is detected.

## Rules

RightPositiveX:

- north + east => forward +Z
- east + south => forward +X
- south + west => forward -Z
- west + north => forward -X

LeftNegativeX:

- north + west => forward +Z
- west + south => forward -X
- south + east => forward -Z
- east + north => forward +X

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10U1 FORCE CORNER SIDE ROTATION PATH complete...`

Look for:

`CornerRotationPath=ForcedSideMetadata`
