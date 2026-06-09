# Zed v3.0 Tile Union Perimeter Boundary Wall Builder

## Target behavior

The wall should match the red outline example:

- follow the outer perimeter of the generated map tile footprint
- ignore internal city streets
- ignore internal holes/gaps
- not use renderer bounds
- not use per-building geometry

## Algorithm

1. Read the generated tile footprint from `ZedLegacyRandomMapGenerator.tilePositions`.
2. Snap those positions to tile cells.
3. Flood-fill empty cells from outside the occupied bounds.
4. Generate edge segments only where an occupied cell touches outside space.
5. Merge contiguous edge runs into longer wall segments.
6. Optionally split long merged runs by `segmentLength`.

This creates the perimeter of the union of the generated map tiles, which is the correct source for the boundary wall.
