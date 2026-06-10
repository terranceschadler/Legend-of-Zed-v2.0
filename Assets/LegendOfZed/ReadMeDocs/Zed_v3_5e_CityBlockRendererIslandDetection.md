# v3.5e — CityBlock Renderer Island Detection

## Problem

The CityBlock tag was placed on a parent named `sidewalk section`.

That is a reasonable workflow, but v3.5d treated that tagged parent as one full bounds.

Result:

`Rejected oversized CityBlock bounds. Size=(400.00, 1.17, 350.00)`

## Fix

A CityBlock-tagged parent is now valid.

The filler now:

1. finds unique CityBlock-tagged roots
2. scans child renderers under those roots
3. treats each child renderer as an individual sidewalk/block piece
4. merges nearby renderer pieces into CityBlock islands
5. places buildings around those merged islands

## Still disabled

The following remain forced OFF:

- tile-footprint fallback
- name/material fallback

No bad tile-edge placement should happen from fallback.

## Expected log

`CityBlock renderer-island detection complete. TaggedRoots=..., rendererPieces=..., mergedBlocks=..., fallbacks=OFF.`

If `mergedBlocks=0`, check the size filters:

- `minRendererPieceSize`
- `maxRendererPieceHeight`
- `maxSurfaceCenterY`
- `minBlockSize`
- `maxMergedBlockSize`

If you see oversized island warnings, tagged sidewalk pieces are merging across roads or the tag is too high in the hierarchy.
