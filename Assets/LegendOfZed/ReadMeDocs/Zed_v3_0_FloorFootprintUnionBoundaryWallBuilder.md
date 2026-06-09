# Zed v3.0 Floor Footprint Union Boundary Wall Builder

## Correct target

The wall should trace the red-outline style perimeter: the outer edge of the generated floor footprint.

It should not use:

- building bounds
- road seams
- individual tile-cell assumptions
- a four-wall rectangle

## Algorithm

1. For each generated room tile, find the largest low, broad footprint renderer.
2. Treat those rectangles as the generated map floor footprint.
3. Build the union of those rectangles.
4. Flood fill from outside to ignore internal holes.
5. Emit wall segments only on the outside perimeter.
6. Offset walls outward by half wall thickness so they sit tight to the edge.

## Menu

`Legend of Zed/Map Integration/Add Boundary Wall Builder To Scene`

`Legend of Zed/Map Integration/Remove Boundary Walls From Scene`

## Notes

No tile generation logic changed.
