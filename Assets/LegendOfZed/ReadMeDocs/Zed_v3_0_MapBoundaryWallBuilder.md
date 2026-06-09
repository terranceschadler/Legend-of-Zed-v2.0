# Zed v3.0 Map Boundary Wall Builder

## Purpose

Adds an automatic outside boundary wall around the generated map so the player and AI cannot walk off the edge.

## What it does

Adds:

`ZedMapTileBoundaryWallBuilder`

The builder waits for the legacy map tile generator to complete, calculates bounds from generated room tile renderers, then creates four solid cube walls:

- North
- South
- East
- West

Generated wall root:

`Generated_Map_Boundary_Walls`

## Menu

Run:

`Legend of Zed/Map Integration/Add Boundary Wall Builder To Scene`

Optional cleanup:

`Legend of Zed/Map Integration/Remove Boundary Walls From Scene`

## Defaults

- Wall height: `5`
- Wall thickness: `2`
- Padding around generated map: `10`
- Visible wall mesh: enabled

## Notes

This does not change tile generation, dead-end spawning, duplicate tile prevention, or map layout logic.
