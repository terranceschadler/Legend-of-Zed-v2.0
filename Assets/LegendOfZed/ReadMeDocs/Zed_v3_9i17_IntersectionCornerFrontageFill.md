# v3.9i17 — Intersection / Corner Frontage Fill

## Problem

The remaining frontage gaps are clustered around:

- 3-way intersections
- 2-way corners
- short straight segments next to intersections

The normal merged road-run strips trim these areas too aggressively, leaving visible gaps.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

Adds short supplemental frontage strips near road rectangles that are too short/fragmented for the main merged strip pass.

This keeps:

- tile repair prebuild call
- road center/lane marking filter
- +Z road-facing rule
- sidewalk setback
- final road/overlap checks
- boundary checks

## Expected Log

`V3.9I17 INTERSECTION/CORNER FRONTAGE FILL complete...`

Watch:

`IntersectionCornerStripsAdded=...`

## Goal

Fill circled frontage gaps at 3-way and corner tiles without bringing back frontage lines down road centers.
