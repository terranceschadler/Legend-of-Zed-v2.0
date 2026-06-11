# v3.9i25 — Slot-Based Frontage Filler

## Problem

Authored spawn fallback is disabled correctly, but visible frontage gaps remain. Endpoint extension strips are being created, but too few actual buildings are placed in the empty slots.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds a final deterministic slot-based frontage filler:

- walks every valid frontage strip in fixed steps
- tests whether a road-facing slot already has a building
- uses small prefabs first
- keeps sidewalk setback
- keeps road/bounds/overlap checks
- does not use old authored tile spawn markers

## Expected Log

`V3.9I25 SLOT-BASED FRONTAGE FILLER complete...`

Watch:

- `SlotFillerPasses`
- `SlotFillerSlotsTested`
- `SlotFillerBuildings`
- `SlotFillerExistingRejects`
- `SlotFillerRoadRejects`
- `SlotFillerOverlapRejects`

## Goal

Fill obvious holes along valid frontage rows without relying on authored spawn markers.
