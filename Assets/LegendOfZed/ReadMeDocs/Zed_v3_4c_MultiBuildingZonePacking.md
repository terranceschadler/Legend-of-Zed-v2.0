# v3.4c — Multi-Building Zone Packing

## Problem

15x15 buildable zones were often getting only one building because the lot filler picked the widest building that fit. One wide building consumed most of the row and left no room for a second building.

## Fix

`ZedBuildingLotFiller` now prefers multiple smaller buildings per zone:

- `preferMultipleBuildingsPerZone`
- `targetBuildingsPerZone`
- `maxPreferredWidthFraction`

The filler now reserves enough width for future buildings before picking the first one.

It also delays alley insertion until the target count is reached, so an early alley does not block the second building.

## Still safe

Strict containment is preserved:

- buildings must fit zone width
- buildings must fit zone depth
- final footprint must stay inside the zone

If only one building can fit, it still allows one. If two can fit, it should prefer two.

## What this does not do

This does not overwrite your authored `ZedBuildableZone` positions or sizes.
