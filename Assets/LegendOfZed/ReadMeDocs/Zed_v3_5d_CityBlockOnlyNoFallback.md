# v3.5d — CityBlock Only, No Fallback

## Problem

The scene still had old serialized fallback values enabled, so even after CityBlock detection failed to produce valid merged blocks, the filler fell back to tile footprints.

That produced tile-edge buildings again.

## Fix

`ZedPostGenerationCityBlockBuildingFiller` now hard-forces:

- `trySurfaceRendererDetection = false`
- `fallbackToGeneratedTileFootprints = false`

in `OnValidate` and `Awake`.

Existing scene values cannot sneak the old fallback back on.

## Detection change

The filler now detects unique CityBlock-tagged root objects, builds bounds from each tagged root's renderers, then merges those root bounds.

This avoids counting thousands of child renderers as separate surfaces.

## Correct tagging rule

Tag the actual sidewalk/block surface object or a parent that contains only the block surface:

`CityBlock`

Do not tag:

- the whole tile root
- the whole generated map root
- roads
- curbs
- buildings
- props
- spawn markers

## Expected logs

Good case:

`CityBlock-only detection complete. TaggedRoots=..., mergedBlocks=..., fallbacks=OFF.`

Bad tag case:

`Rejected oversized CityBlock bounds...`

That means the tag is too high in the hierarchy, usually on the whole tile instead of the sidewalk/block surface.
