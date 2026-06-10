# v3.5g — CityBlock Contour Edge Placement

## Problem

Buildings were spawning in the wrong places because placement was using rectangular bounds around detected blocks.

Bounds are not the real city block edge.

## Fix

The filler now builds contour/perimeter edges from CityBlock-tagged renderer pieces.

It places buildings along the actual detected CityBlock contour edges, not the rectangle around a merged island.

## Still true

- CityBlock only
- no BlockSurface
- no tile-footprint fallback
- no name/material fallback

## Expected log

`CityBlock contour-edge filler complete. TaggedPieces=..., placementEdges=..., Buildings=..., fallbacks=OFF.`

## Debug

Select `Zed_PostGeneration_CityBlockBuildingFiller`.

Gizmos:

- cyan = detected CityBlock renderer pieces
- yellow = contour edges used for building placement

## Tuning

If buildings are too close/far from road-facing edges:

- adjust `edgeInset`

If too many tiny edges are used:

- raise `minContourEdgeLength`
- lower `maxPlacementEdges`

If contour pieces touch across roads:

- raise `pieceInset` slightly
