# v3.9f — Auto Buildable Zone Coverage

## Purpose

The zone-based spawner was working, but the map only had a small number of `ZedBuildableZone` components:

Example:

`Zones=12, Buildings=24`

This pass creates more legal buildable zones on generated room tiles before the zone-based building spawner runs.

## New Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAutoBuildableZoneCoverage.cs`
- `Assets/LegendOfZed/Editor/ZedAutoBuildableZoneCoverageMenu.cs`

## Menus

- `Legend of Zed / Map Integration / Add Auto Buildable Zone Coverage`
- `Legend of Zed / Map Integration / Run Auto Buildable Zone Coverage Now`

## Required Setup

1. Import package.
2. Run:
   `Legend of Zed / Map Integration / Add Auto Buildable Zone Coverage`
3. Keep old `ZedRoadFrontageBuildingSpawner` disabled.
4. Keep `ZedZoneBasedBuildingSpawner` enabled.
5. Regenerate the map.

## Timing

Default start delay is `0.2`.

The zone-based building spawner should run after this. If your building spawner is still too early, set:

- Auto Buildable Zone Coverage `Start Delay = 0.2`
- Zone Based Building Spawner `Start Delay = 0.6`

## Expected Logs

First:

`Auto buildable zone coverage complete. Tiles=..., TilesUsed=..., ZonesCreated=..., RoadRejects=..., SmallRejects=..., SkippedTiles=..., RoadRects=...`

Then:

`Zone-based building spawner complete. Zones=..., ZonesUsed=..., Buildings=..., AuthoredFrontRows=True, RoadRects=...`

## Target

- Zones should increase significantly.
- Buildings should increase significantly.
- Buildings still come only from `ZedBuildableZone`.
- Old road-frontage building spawner remains disabled.

## Notes

This does not spawn buildings directly.
It only creates legal `ZedBuildableZone` strips for the zone-based spawner to fill.
