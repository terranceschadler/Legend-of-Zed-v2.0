# Zed v3.0 Map Outer Contour Boundary Wall Builder

## Correct boundary behavior

The boundary wall should follow the outside contour/perimeter of the full generated map.

It should not:

- be a single four-wall rectangle
- place walls around every individual tile
- wall off internal holes or internal tile gaps
- cross roads inside the city

## Algorithm

1. Collect generated room tile cells.
2. Flood-fill empty cells from outside the generated map bounds.
3. For each generated tile, add a wall segment only on sides touching outside-connected empty space.

That produces many wall segments, but only along the true outside perimeter of the whole map.

## Output

Generated root:

`Generated_Map_Boundary_Walls`

Expected log:

`Generated outer-contour map boundary walls built. Segments=X.`

## Notes

This does not change tile generation, dead-end spawning, duplicate tile prevention, player spawning, or AI.
