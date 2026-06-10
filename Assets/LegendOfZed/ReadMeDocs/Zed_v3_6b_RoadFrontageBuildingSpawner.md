# v3.6b — Road Frontage Building Spawner

## Purpose

Uses the approved v3.6a road frontage lines to spawn buildings.

This replaces the failed CityBlock/surface-bound guessing approach.

## Placement rule

Road surface -> road edge -> frontage line -> building center behind frontage line.

Buildings face the road.

## Safety

The spawner checks:

- generated building footprints against other generated buildings
- generated building footprints against road cells

This prevents buildings from spawning into the road or lodged into each other.

## Menu

`Legend of Zed / Map Integration / Add Road Frontage Building Spawner`

The menu disables:

- old `ZedPostGenerationCityBlockBuildingFiller.runOnStart`
- diagnostic `ZedRoadFrontageDebugLines.runOnStart`

## Expected log

`Road frontage building spawner complete. RoadRects=..., RoadCells=..., FrontageSegments=..., Buildings=..., RoundRobin=True.`

## Debug Gizmos

- Green lines = frontage segments
- Magenta boxes = generated building footprint reservations
