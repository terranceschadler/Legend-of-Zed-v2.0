# v3.5k — Street-Safe Building Footprints + Coverage

## Problem

Buildings were spawning into the street and lodged into other generated buildings.

The old placement pushed every building inward by a fixed edge inset. It did not account for building depth, so deeper buildings could extend backward into the road.

Generated buildings also had no reliable 2D footprint overlap registry.

## Fix

The filler now:

- offsets each building by `buildingDepth * 0.5 + edgeInset`
- tracks generated building footprints in 2D
- rejects new generated buildings that overlap existing generated buildings
- keeps round-robin street coverage active
- keeps trying along an edge after failed placement instead of abandoning coverage immediately

## Still true

- CityBlock only
- no BlockSurface
- no tile-footprint fallback
- no name/material fallback
- grid contour edge placement

## New / changed fields

- `generatedBuildingPadding`
- `preventGeneratedBuildingOverlap`
- `requireBuildingCenterOnCityBlock`

## Expected log

`CityBlock street-coverage filler complete. TaggedPieces=..., occupiedCells=..., placementEdges=..., Buildings=..., roundRobin=True, fallbacks=OFF.`
