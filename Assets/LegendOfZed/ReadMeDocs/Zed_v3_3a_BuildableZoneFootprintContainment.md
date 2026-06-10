# v3.3a — Buildable Zone Footprint Containment

## Problem

Buildings spawned from explicit zones could still extend outside the zone because the filler only used the zone as a placement guide. Large buildings could cover streets or too much sidewalk.

## Fix

`ZedBuildingLotFiller` now has strict containment:

- rejects buildings wider than the remaining zone width
- rejects buildings deeper than the usable zone depth
- checks the final footprint against the zone rectangle before spawning
- uses a safety `zoneEdgeInset`
- if no building fits, it spawns nothing instead of forcing a bad placement

## Important

This may make some zones spawn no buildings if the zone is too small for the available building prefabs. That is correct.

To fill those zones, either:

- enlarge the zone
- assign smaller building prefabs
- add `ZedBuildingFootprint` to building prefabs and set accurate manual sizes

## Not changed

- weighted tile generation
- park spawning
- boundary walls
- fixed building spawning on non-lot-filler tiles
