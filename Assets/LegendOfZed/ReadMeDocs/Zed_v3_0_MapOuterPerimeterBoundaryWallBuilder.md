# Zed v3.0 Map Outer Perimeter Boundary Wall Builder

## Purpose

Corrects the boundary wall behavior.

The wall should wrap the outer edge of the entire generated map, not create walls around every individual tile and not wall off internal holes/gaps.

## Behavior

The builder:

1. Finds generated room tile cells.
2. Flood-fills empty cells from outside the generated map bounds.
3. Creates wall segments only where a room tile touches outside-connected empty space.

This creates a boundary along the true outer perimeter of the generated map.

## Important distinction

Previous contour behavior placed walls on every exposed tile edge, including internal holes.

This version only places walls on exposed edges connected to the outside of the map footprint.

## Menu

Run:

`Legend of Zed/Map Integration/Add Boundary Wall Builder To Scene`

Optional cleanup:

`Legend of Zed/Map Integration/Remove Boundary Walls From Scene`

## Notes

This does not change tile generation, dead-end spawning, duplicate tile prevention, player spawning, or AI.
