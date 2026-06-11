# v3.9i15 — Frontage Gap Fill

## Problem

Some valid frontage rows still had holes/gaps:

- at intersections
- along straight 2-way tiles
- in the middle of otherwise good frontage rows

The center-line frontage bug is fixed, but the spawner still stops too early after the safety passes.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

Adds a conservative frontage gap-fill pass:

- re-scans existing frontage strips after the normal pass
- uses tighter same-row spacing during gap fill
- temporarily lowers building gap
- prefers smaller prefabs for tight holes
- keeps road, boundary, overlap, +Z road-facing, and sidewalk setback checks

## New Settings

- `Enable Frontage Gap Fill`
- `Extra Gap Fill Buildings Per Strip`
- `Min Frontage Gap Fill Length`
- `Prefer Small Prefabs For Gap Fill`
- `Gap Fill Same Frontage Line Tolerance`
- `Gap Fill Same Frontage Along Spacing`

## Expected Log

`V3.9I15 FRONTAGE GAP FILL complete...`

Watch:

- `GapFillPasses`
- `GapFillBuildings`

## Goal

Fill missing storefront gaps without reintroducing road-center frontage lines.
