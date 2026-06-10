# v3.6f1 — Boundary Wall Builder Menu Compile Fix

## Problem

The v3.6f deprecated API cleanup pulled in a `ZedMapTileBoundaryWallBuilder.cs` version that did not expose fields expected by:

`ZedMapTileBoundaryWallBuilderMenu.cs`

Compile errors were for:

- `maxFootprintCenterY`
- `minFootprintSize`
- `coordinateTolerance`
- `includeInternalHoles`

## Fix

Adds those serialized fields back to `ZedMapTileBoundaryWallBuilder`.

## Notes

This is a compile fix only. It does not change road-frontage building placement.
