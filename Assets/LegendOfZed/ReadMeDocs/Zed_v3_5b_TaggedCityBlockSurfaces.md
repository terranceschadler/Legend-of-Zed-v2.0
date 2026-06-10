# v3.5b — Tagged City Block Surfaces

## Why

The tile-footprint fallback worked, but it treated each tile as a block and placed buildings along tile edges.

That is not the desired result.

## New preferred workflow

Tag the actual sidewalk/block surface prefabs as:

- `CityBlock`

or:

- `BlockSurface`

Then the post-generation city block filler will:

1. find renderers with that tag or a tagged parent
2. merge adjacent tagged surfaces into finished blocks
3. spawn buildings on the outside perimeter of the merged block

## Important defaults

`fallbackToGeneratedTileFootprints` is now disabled by default.

That means if no tagged block surfaces are found, the filler should spawn no buildings instead of tile-edge garbage.

## Menu update

Running:

`Legend of Zed/Map Integration/Add Post-Generation City Block Building Filler`

now ensures these tags exist:

- `CityBlock`
- `BlockSurface`

It also configures the filler to prefer tagged surfaces and disables tile fallback.

## What to tag

Tag the actual sidewalk/block floor prefab pieces, not:

- roads
- curbs
- lane markings
- trees
- building props
- TileSpawn markers

If the renderer is under a parent, tagging the parent is fine because `allowTaggedParents` is enabled by default.
