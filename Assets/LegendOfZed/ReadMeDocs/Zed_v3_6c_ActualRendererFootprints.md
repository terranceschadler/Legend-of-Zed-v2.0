# v3.6c — Actual Renderer Footprints

## Problem

The magenta boxes did not line up with the visible buildings.

Reason: the magenta boxes were estimated reservation footprints centered on the intended spawn point, but many building prefab pivots are not centered on the visible mesh.

## Fix

The road-frontage spawner now:

1. spawns the prefab at the intended frontage placement
2. reads the actual renderer bounds after rotation
3. shifts the instance so the visible renderer bounds center aligns to the intended placement center
4. creates the magenta reservation footprint from the actual renderer bounds
5. destroys the instance if the real footprint overlaps road cells or another generated building

## New fields

- `alignVisualBoundsToPlacementCenter`
- `useActualRendererBoundsForFootprints`

Both default to true.

## Expected result

The magenta boxes should line up with the visible building meshes much more closely.

Buildings should also stop lodging into each other due to bad estimated footprints.
