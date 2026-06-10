# v3.5i — CityBlock Grid Contour Edges

## Problem

The direct rectangle contour method found CityBlock renderer pieces but produced:

`placementEdges=0`

The imported sidewalk renderers overlap/touch too much for edge-by-edge rectangle comparisons.

## Fix

The filler now rasterizes CityBlock renderer pieces into a 2D occupancy grid.

Then it builds placement edges wherever an occupied CityBlock grid cell touches an empty cell.

This should produce yellow debug edges.

## Still true

- CityBlock only
- no BlockSurface
- no tile-footprint fallback
- no name/material fallback
- no rectangular bounds placement

## Expected log

`CityBlock grid-contour filler complete. TaggedPieces=..., occupiedCells=..., placementEdges=..., Buildings=..., cellSize=..., fallbacks=OFF.`

## Tuning

If edges are too chunky:

- lower `contourCellSize`

If there are too many tiny edges:

- raise `contourCellSize`
- raise `minContourEdgeLength`
- lower `maxPlacementEdges`
