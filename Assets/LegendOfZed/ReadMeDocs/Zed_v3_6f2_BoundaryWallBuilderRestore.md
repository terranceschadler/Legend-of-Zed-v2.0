# v3.6f2 — Boundary Wall Builder Restore

## What went wrong
The prior compile-fix patch restored an older simplified `ZedMapTileBoundaryWallBuilder` implementation.
That older version builds the wrong boundary shape.

## Fix
This patch restores the newer footprint-union / internal-hole-aware boundary wall builder and keeps the non-deprecated object find API usage.

## Changed file
- `Assets/LegendOfZed/Scripts/MapIntegration/ZedMapTileBoundaryWallBuilder.cs`
