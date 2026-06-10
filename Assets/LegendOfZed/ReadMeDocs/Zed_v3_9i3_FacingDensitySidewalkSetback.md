# v3.9i3 — Facing + Density + Sidewalk Setback Runtime Fix

## Purpose

Fix the remaining frontage-strip issues from v3.9i2:

- buildings not facing the road
- not enough buildings
- buildings need to be set back from the road to leave sidewalk/curb space

## Patched

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Runtime Fix

Adds:

`Apply V39I3 Runtime Fix`

Default: enabled.

This forces known-good runtime values so older serialized Inspector values do not keep overriding the package.

## Forced Runtime Values

- `Building Yaw Offset = 0`
- `Max Buildings Total = 210`
- `Max Buildings Per Strip = 5`
- `Strip End Inset = 2.5`
- `Front Setback From Road = 2.0`
- `Building Gap = 0.8`
- `Draw Rejected Gizmos = false`

## Expected Log

`V3.9I3 FACING DENSITY SIDEWALK SETBACK complete... RuntimeFix=True, YawOffset=0, FrontSetback=2`

## Tuning

If buildings still face backward, disable `Apply V39I3 Runtime Fix` and set:

`Building Yaw Offset = 180`

If buildings are too far from the road:

`Front Setback From Road = 1.5`

If buildings are too sparse:

`Max Buildings Total = 240`
