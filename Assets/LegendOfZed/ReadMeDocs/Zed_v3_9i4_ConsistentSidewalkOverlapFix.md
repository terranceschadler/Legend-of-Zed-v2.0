# v3.9i4 — Consistent Sidewalk + Overlap Fix

## Problem

v3.9i3 fixed the active spawner, prefab variety, and density, but remaining visual issues were:

- inconsistent sidewalk/front setback from building to building
- some buildings intersecting after bounds anchoring
- some buildings sitting behind other frontage buildings
- some buildings not staying in a clean road-aligned row

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds strict frontage discipline:

- final visual front edge must be within `Max Front Setback Error`
- final overlap checks run after visual-bounds anchoring
- final road checks run after visual-bounds anchoring
- back-row drift rejection
- duplicate same-frontage-slot rejection
- rejected gizmos remain hidden by default

## Forced Runtime Values

When `Apply V39I3 Runtime Fix` is enabled, it now also forces:

- `Front Setback From Road = 2.25`
- `Max Front Setback Error = 0.2`
- `Reject Back Row Drift = true`
- `Back Row Drift Tolerance = 0.75`
- `Final Bounds Safety Checks = true`
- `Reject If Behind Existing Frontage Building = true`
- `Same Frontage Along Spacing = 1.0`
- `Same Frontage Line Tolerance = 2.0`

## Expected Log

`V3.9I4 CONSISTENT SIDEWALK FRONTAGE complete...`

New counters:

- `FinalOverlapRejects`
- `FinalRoadRejects`
- `BackRowRejects`
- `FrontSetbackRejects`
- `DuplicateFrontageRejects`

## Notes

If buildings become too sparse after this safety pass, raise:

`Max Buildings Total`

or reduce:

`Same Frontage Along Spacing`
