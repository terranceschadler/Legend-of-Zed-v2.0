# v3.9i40 — Restore v38 Coverage Baseline

## Problem

v3.9i39 fixed some alignment by making the corridor merge tighter, but it created huge frontage gaps:

- too many road corridors
- too many short chunks
- low building count

## Fix

This patch restores the stronger v38-style corridor coverage while keeping the controlled collider-only setup:

- direct collider corridor frontage
- true collider-only reset
- all supplemental frontage systems off
- authored spawn fallback disabled
- raw road cleanup enabled

## Expected Log

`V3.9I40 RESTORE V38 COVERAGE BASELINE complete...`

Expected markers:

- `TrueColliderOnlyReset=True`
- `RestoredCoverageBaseline=True`
- `UsingColliderRoadRects=True`
- `DirectColliderCorridorFrontageStrips=...`

Supplemental systems should still be zero:

- `DeadEndStripsAdded=0`
- `GapFillBuildings=0`
- `IntersectionCornerStripsAdded=0`
- `FrontageEndpointExtensionsAdded=0`
- `EndpointFillerBuildings=0`
- `SlotFillerBuildings=0`

## Goal

Return to the better coverage baseline before tuning frontage offset/width again.
