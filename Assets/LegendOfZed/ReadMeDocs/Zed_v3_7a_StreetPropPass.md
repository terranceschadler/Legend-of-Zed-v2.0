# v3.7a — Street Prop Pass

## Purpose

Adds street-side prop generation around the working v3.6 road-frontage city baseline.

This does not change building placement.

## Menu

`Legend of Zed / Map Integration / Add Road Frontage Street Prop Spawner`

## Props supported

- street lights
- benches
- trash cans
- hydrants
- mailboxes
- paper/debris scatter

The menu tries to auto-fill prefab arrays by searching project prefab names.

## Placement rules

- road-frontage detection is reused
- props are placed outside road edges
- props avoid road cells
- props avoid generated building bounds
- props avoid other generated props
- props stay away from road segment ends/intersections
- generated props go under `Generated_RoadFrontage_StreetProps`

## Expected log

`Road frontage street prop pass complete. RoadRects=..., RoadCells=..., FrontageSegments=..., BuildingBlockers=..., Props=...`
