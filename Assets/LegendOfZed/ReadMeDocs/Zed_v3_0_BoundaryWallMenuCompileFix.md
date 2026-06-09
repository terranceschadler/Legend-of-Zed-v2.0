# Zed v3.0 Boundary Wall Menu Compile Fix

## Problem

Unity reported:

`ZedMapTileBoundaryWallBuilderMenu.cs(25,21): error CS1061: 'ZedMapTileBoundaryWallBuilder' does not contain a definition for 'tileGridSize'`

## Cause

The menu script still referenced an older boundary builder field:

`tileGridSize`

The current outer-bounds segmented boundary builder uses:

`segmentLength`

## Fix

The menu now assigns the current builder fields:

- `segmentLength`
- `wallHeight`
- `wallThickness`
- `yCenter`
- `outsidePadding`
- `maxGroundCenterY`
- `minGroundRendererSize`

No boundary generation logic changed.
