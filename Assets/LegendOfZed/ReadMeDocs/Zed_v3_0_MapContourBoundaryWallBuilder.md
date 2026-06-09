# Zed v3.0 Map Contour Boundary Wall Builder

## Purpose

Replaces the large rectangular map boundary with a tight contour boundary.

## Behavior

The builder waits for map generation to complete, finds generated room tile cells, then creates wall segments only along exposed tile edges.

This means the boundary follows the actual generated map shape instead of creating one large rectangle around the whole map.

## Protected edges

A wall segment is created when a generated tile has no neighboring tile on that side:

- North exposed edge
- South exposed edge
- East exposed edge
- West exposed edge

This also protects internal holes/gaps because those are still exposed tile edges.

## Menu

Run:

`Legend of Zed/Map Integration/Add Boundary Wall Builder To Scene`

Optional cleanup:

`Legend of Zed/Map Integration/Remove Boundary Walls From Scene`

## Defaults

- Tile grid size: `20`
- Wall height: `5`
- Wall thickness: `1.5`
- Visible wall mesh: enabled

## Notes

This does not change tile generation, dead-end spawning, duplicate tile prevention, player spawning, or AI.
