# v3.6a — Road Frontage Debug Lines Only

## Purpose

This is a diagnostic reset for building placement.

It does not spawn buildings.

It detects roads, builds road-edge frontage lines, and draws gizmos so the road-frontage source of truth can be verified before any building placement work continues.

## Gizmos

- Green lines = proposed building frontage lines
- Yellow arrows = direction buildings should face, toward the road

## Menu

`Legend of Zed / Map Integration / Add Road Frontage Debug Lines`

The menu also disables the old CityBlock building filler `runOnStart` so it will not spawn buildings during this diagnostic pass.

## Expected log

`Road frontage debug complete. RoadRects=..., RoadCells=..., FrontageSegments=..., SpawnedBuildings=0.`

## What to check

Only judge the green/yellow debug lines.

If the lines are wrong, we fix road frontage detection.

If the lines are right, the next step is a separate spawner that places buildings on those approved frontage lines.
