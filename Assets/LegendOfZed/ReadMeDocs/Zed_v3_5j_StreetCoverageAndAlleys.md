# v3.5j — Street Coverage + Alleys

## Problem

CityBlock grid contour detection finally produced usable yellow placement edges, but buildings only appeared on some streets.

The reason was placement order: the filler filled long/early edges first and hit the building cap before every street edge got coverage.

## Fix

Building placement now supports round-robin street coverage.

It tries to place one building on each valid contour edge before adding extra buildings to already-filled edges.

## Still true

- CityBlock only
- no BlockSurface
- no tile-footprint fallback
- no name/material fallback
- grid contour placement remains active

## New / changed defaults

- `maxPlacementEdges = 512`
- `maxBuildingsPerEdge = 64`
- `maxTotalBuildings = 600`
- `useRoundRobinStreetCoverage = true`
- `guaranteedAlleyEveryNBuildings = 5`

## Alleys

An alley gap is inserted:

- every `guaranteedAlleyEveryNBuildings` buildings on an edge
- or randomly using the existing `alleyChance`

Set `guaranteedAlleyEveryNBuildings` to 0 to disable guaranteed alleys.

## Expected log

`CityBlock street-coverage filler complete. TaggedPieces=..., occupiedCells=..., placementEdges=..., Buildings=..., roundRobin=True, fallbacks=OFF.`
