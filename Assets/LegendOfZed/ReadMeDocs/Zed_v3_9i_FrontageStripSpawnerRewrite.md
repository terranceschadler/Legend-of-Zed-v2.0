# v3.9i — Frontage-Strip Spawner Rewrite

## Purpose

Replace zone-center building placement with road-edge frontage placement.

Previous failures:

- buildings did not face the street
- buildings were too far from the street
- too many back-row/grouped buildings
- prefab variety collapsed

## New Rule

Buildings spawn from detected road edges:

`road edge -> frontage strip -> building front edge near road -> building faces road`

## New Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`
- `Assets/LegendOfZed/Editor/ZedFrontageStripBuildingSpawnerV39IMenu.cs`

## Menu

Run:

`Legend of Zed / Map Integration / CLEAN INSTALL V3.9I Frontage Strip Building System`

This disables older building spawners and installs:

`Zed_V39I_FrontageStripBuildingSpawner`

## Expected Log

`V3.9I FRONTAGE STRIP spawner complete. RoadRenderers=..., RoadRects=..., IntersectionsSkipped=..., StripsBuilt=..., StripsUsed=..., PrefabsAvailable=..., UniquePrefabsUsed=..., Buildings=..., FitRejects=..., RoadRejects=..., OverlapRejects=..., BoundsRejects=..., EmptyPrefabRejects=...`

## Inspector Tuning

If buildings face backward:

- toggle `Building Yaw Offset` between `180` and `0`

If buildings are too far from the road:

- lower `Front Setback From Road`

If too many buildings spawn:

- lower `Max Buildings Total`
- lower `Max Buildings Per Strip`

If too few strips build:

- lower `Min Road Long Axis`
- increase `Max Road Short Axis`

## Notes

Buildings still spawn under:

`Generated_RoadFrontage_Buildings`

so the existing street prop blocker system can still see them.
