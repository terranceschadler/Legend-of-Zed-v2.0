# v3.6d — Road Frontage Placement Polish

## Purpose

Polishes the working road-frontage building spawner.

This keeps the v3.6c actual-renderer-footprint fix and improves street coverage quality.

## Changes

- increases default building setback from the frontage line
- adds `segmentEndMargin` so buildings do not jam into intersections/corners
- adds `buildingGapJitter` for less repetitive spacing
- adds `minRemainingSegmentSpace` to avoid tiny leftover placements
- prefers larger buildings that fit the current frontage slot
- reduces random alley frequency slightly
- widens guaranteed alley gaps
- keeps real renderer bounds alignment and real footprint reservations

## New / updated fields

- `buildingSetback = 0.75`
- `segmentEndMargin = 2.5`
- `buildingGap = 0.45`
- `buildingGapJitter = 0.25`
- `minRemainingSegmentSpace = 3`
- `preferLargerBuildings = true`
- `buildingSelectionTopPercent = 0.35`
- `guaranteedAlleyEveryNBuildings = 7`
- `randomAlleyChance = 0.04`
- `alleyWidthMin = 4`
- `alleyWidthMax = 7`

## Still true

- road-frontage source of truth
- buildings face roads
- actual renderer bounds align to placement center
- magenta boxes use actual renderer bounds
- generated buildings reject road overlap
- generated buildings reject generated-building overlap
