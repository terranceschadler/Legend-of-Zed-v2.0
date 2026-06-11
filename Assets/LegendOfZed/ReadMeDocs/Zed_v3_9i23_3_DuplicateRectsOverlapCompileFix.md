# v3.9i23.3 — Duplicate RectsOverlap Compile Fix

## Problem

v3.9i23.2 still failed compile because the spawner already has a `RectsOverlap(Rect2, Rect2)` method.

Compiler error:

`CS0111: Type already defines a member called RectsOverlap with the same parameter types`

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Change

Renamed the authored spawn fallback's local overlap helper to:

`AuthoredFallbackRectsOverlap`

## Expected Log

`V3.9I23.3 DUPLICATE RECTS OVERLAP COMPILE FIX complete...`
