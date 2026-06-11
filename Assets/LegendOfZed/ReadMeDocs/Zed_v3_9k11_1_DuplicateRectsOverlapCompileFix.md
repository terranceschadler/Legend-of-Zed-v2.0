# v3.9k11.1 — Duplicate RectsOverlap Compile Fix

## Problem

v3.9k11 added a second `RectsOverlap(Rect2, Rect2)` method to `ZedFrontageStripBuildingSpawnerV39I`, causing:

`CS0111: Type 'ZedFrontageStripBuildingSpawnerV39I' already defines a member called 'RectsOverlap' with the same parameter types`

## Fix

Removed the duplicate method. The new final building overlap cleanup now uses the existing `RectsOverlap` helper already in the spawner.

## Kept From v3.9k11

- restored stable `ZedRoadFrontageSocket` script GUID
- final building-vs-building overlap cleanup
- `FinalBuildingOverlapCleanupRemoved` log counter

## Expected Log

`V3.9K11.1 DUPLICATE RECTSOVERLAP COMPILE FIX complete...`
