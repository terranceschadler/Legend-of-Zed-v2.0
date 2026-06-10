# v3.9i9 — Dead-End Facing Flip

## Problem

Dead-end supplemental buildings were all facing the wrong way.

The main frontage strips were correct, but the supplemental dead-end tile-edge strips had their `awayFromRoad` normal flipped. Since buildings use local `+Z` to face the road, those dead-end buildings faced away from the dead-end street.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds:

`Flip Dead End Supplement Facing = true`

This flips only supplemental dead-end strip facing:

- north tile edge
- south tile edge
- east tile edge
- west tile edge

Normal road frontage strips are unchanged.

## Expected Log

`V3.9I9 DEAD-END FACING FLIP complete... FlipDeadEndFacing=True`

## Notes

If a specific dead-end tile variant is still reversed, toggle:

`Flip Dead End Supplement Facing`

on `Zed_V39I_FrontageStripBuildingSpawner`.
