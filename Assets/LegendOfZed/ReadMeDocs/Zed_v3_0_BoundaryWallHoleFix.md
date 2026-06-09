# Zed v3.0 Boundary Wall Hole Fix

## Problem

The perimeter wall correctly wrapped the main outer footprint, but enclosed holes/voids inside the generated map were left open.

That allowed the player to walk off the playable footprint into those hole areas.

## Fix

The boundary builder now supports:

`includeInternalHoles = true`

When enabled, wall segments are generated on **all exposed footprint boundaries**, including:

- the main outer perimeter
- enclosed internal holes/voids

Because the source is still the **union of tile floor footprints**, this does **not** place walls down city roads or along interior road seams.

## Expected result

If a blank hole exists inside the generated map footprint, walls should be generated around that hole exactly like the red example image.
