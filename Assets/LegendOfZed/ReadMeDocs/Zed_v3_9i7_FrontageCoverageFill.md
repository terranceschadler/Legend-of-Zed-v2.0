# v3.9i7 — Frontage Coverage Fill

## Problem

v3.9i6 fixed the +Z facing rule, but bald spots remained because the duplicate-frontage blocker was too aggressive.

The worst setting was:

`Same Frontage Line Tolerance = 9`

That blocked nearby valid frontage rows and left open areas with no buildings.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- `Max Buildings Total = 280`
- `Target Buildings Total = 230`
- `Max Buildings Per Strip = 7`
- `Same Frontage Line Tolerance = 3`
- `Same Frontage Along Spacing = 0.75`
- `Strip End Inset = 2`
- `Building Gap = 0.55`

Adds a second coverage fill pass that fills remaining valid frontage gaps if the first pass does not reach target density.

## Expected Log

`V3.9I7 FRONTAGE COVERAGE FILL complete... Buildings=..., TargetBuildings=230, ForceZForward=True`

## Notes

This keeps:

- building local +Z faces the road
- consistent sidewalk setback
- final road/overlap checks
