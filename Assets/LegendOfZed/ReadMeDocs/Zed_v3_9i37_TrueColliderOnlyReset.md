# v3.9i37 — True Collider-Only Reset

## Problem

v3.9i36 was intended to be collider-only, but the log showed some supplemental systems still active:

- `DeadEndStripsAdded=17`
- `GapFillBuildings=13`

That polluted the frontage-line test.

## Fix

Adds a hard reset method called during `BuildNow`:

`ApplyV39I37TrueColliderOnlyReset()`

## Forced ON

- collider road detection
- collider corridor merge
- second-pass corridor merge
- corridor width clamp
- final raw road cleanup

## Forced OFF

- intersection corner supplemental frontage
- endpoint extension frontage
- tile-edge frontage
- dead-end supplemental frontage
- frontage gap fill
- endpoint filler pass
- slot filler pass
- authored spawn fallback
- median-width experiment

## Expected Log

`V3.9I37 TRUE COLLIDER-ONLY RESET complete...`

Expected values:

- `TrueColliderOnlyReset=True`
- `DeadEndStripsAdded=0`
- `GapFillBuildings=0`
- `IntersectionCornerStripsAdded=0`
- `FrontageEndpointExtensionsAdded=0`
- `EndpointFillerBuildings=0`
- `SlotFillerBuildings=0`
- `AuthoredSpawnBuildings=0`

## Goal

Show only the base merged collider corridor frontage lines so we can judge the corridor math without noise.
