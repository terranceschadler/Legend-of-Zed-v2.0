# Zed v3.0 Map Outer Bounds Segmented Boundary Wall Builder

## Correct behavior

The boundary wall should encapsulate the entire generated map and should not create walls inside the city.

This version builds segmented wall pieces only along the outer bounds of the generated playable map footprint.

## What it does

1. Waits for map generation.
2. Scans generated playable ground/road/sidewalk/floor renderers.
3. Calculates the full outer map bounds.
4. Builds segmented boundary wall pieces along:
   - North outside edge
   - South outside edge
   - East outside edge
   - West outside edge

It uses multiple segments, not one giant wall object, but all segments are only on the outside perimeter.

## What it does not do

- Does not place walls around individual tiles.
- Does not place walls inside the city.
- Does not wall off internal holes.
- Does not change tile generation.
