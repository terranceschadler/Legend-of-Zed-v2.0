# v3.9i36 — Collider Frontage Controlled Reset

## Problem

v3.9i35 proved the stacked frontage systems are fighting each other:

- too many supplemental frontage lines
- bad extra slots
- raw road cleanup removed too many buildings
- final building count collapsed

## Fix

This patch resets frontage generation to a controlled baseline:

- use collider road corridors
- keep corridor merge
- keep corridor width clamp
- disable median-width experiment
- disable intersection corner supplemental strips
- disable endpoint extension strips
- disable dead-end supplemental strips
- disable endpoint filler
- disable slot filler
- keep authored spawn fallback disabled
- keep bad tile-edge frontage disabled
- keep raw road cleanup enabled

## Expected Log

`V3.9I36 COLLIDER FRONTAGE CONTROLLED RESET complete...`

Expected markers:

- `ControlledColliderOnly=True`
- `UsingColliderRoadRects=True`
- `TileEdgeFrontageStripsAdded=0`
- `IntersectionCornerStripsAdded=0`
- `FrontageEndpointExtensionsAdded=0`
- `EndpointFillerBuildings=0`
- `SlotFillerBuildings=0`

## Goal

First confirm that pure merged collider corridor frontage lines match the streets. Once that base is correct, we can add selective gap filling back in carefully.
