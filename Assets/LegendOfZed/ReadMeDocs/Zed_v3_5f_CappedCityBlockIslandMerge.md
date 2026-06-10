# v3.5f — Capped CityBlock Island Merge

## Problem

CityBlock renderer pieces were found, but raw merging joined sidewalk pieces across the entire map:

`Rejected oversized CityBlock island. Size=(450.00, 1.17, 350.00)`

## Fix

The merge step is now capped.

A candidate piece is only merged into an island if the combined island remains under:

- `maxBlockWidth`
- `maxBlockDepth`

This prevents one map-sized island and should produce multiple block-sized islands.

## Still true

- CityBlock only
- no BlockSurface
- no tile-footprint fallback
- no name/material fallback

## Expected log

`CityBlock capped-island detection complete. TaggedRoots=..., rendererPieces=..., mergedBlocks=..., maxBlock=(... x ...), fallbacks=OFF.`

## Tuning

If blocks are still too large:

- lower `maxBlockWidth`
- lower `maxBlockDepth`
- lower `mergePadding`

If blocks fragment too much:

- raise `mergePadding`
- raise max block sizes slightly
