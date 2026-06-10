# v3.5a — City Block Filler Tile Footprint Fallback

## Problem

The post-generation city block filler reported:

`Blocks=0, Buildings=0`

The first detector was too dependent on renderer names/materials/tags. The imported map tiles do not expose sidewalk/block surfaces reliably through those names.

## Fix

`ZedPostGenerationCityBlockBuildingFiller` now has a fallback detection mode:

1. Try surface renderer detection.
2. If that finds zero blocks, scan generated `ZedLegacyRoomTile` objects.
3. For each tile, find its best low/flat footprint renderer using the same style of logic as the boundary wall builder.
4. Shrink the footprint inward with `fallbackFootprintInset`.
5. Use those safe inset rectangles as block candidates.

## Expected log

You should now see something like:

`City block tile footprint fallback: roomTiles=36, candidateBlocks=...`

and then:

`Post-generation city block filler complete. Blocks=..., Buildings=...`

## Tuning fields

If buildings are too close to roads:

- increase `fallbackFootprintInset`
- increase `edgeInset`

If too few blocks are detected:

- lower `minBlockSize`
- lower `minFootprintRendererSize`

## Note

This is still a stepping stone toward true connected sidewalk island detection. It should get the system spawning again without returning zero.
