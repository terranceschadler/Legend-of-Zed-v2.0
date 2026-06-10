# v3.5h — Sampled CityBlock Contour Edges

## Problem

The previous contour pass found CityBlock renderer pieces but produced no placement edges:

`TaggedPieces=2423, placementEdges=0`

The edge test was too strict. It rejected an edge if any neighboring CityBlock piece touched or overlapped that side. The imported sidewalk meshes touch/overlap slightly, so every candidate edge was classified as internal.

## Fix

The contour test now samples just outside each candidate edge.

If at least one outside sample is not inside another CityBlock piece, the edge is kept as a real perimeter edge.

## Still true

- CityBlock only
- no BlockSurface
- no tile-footprint fallback
- no name/material fallback
- no rectangular bounds placement

## New tuning fields

- `outsideSampleDistance`
- `samplesPerCandidateEdge`

## Expected log

`CityBlock contour-edge filler complete. TaggedPieces=..., placementEdges=..., Buildings=..., fallbacks=OFF.`

You should now see yellow gizmo lines when selecting `Zed_PostGeneration_CityBlockBuildingFiller`.
