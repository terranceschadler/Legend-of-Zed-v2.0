# v3.4b — Preserve Authored Buildable Zones

## Purpose

Stops the RoomTile-4Way buildable zones from being overwritten by old one-time authoring scripts.

## What this does

This package adds a one-time cleanup script that deletes old zone-authoring scripts if they still exist:

- `ZedOneTimeRoomTile4WayBuildableZoneAuthoring.cs`
- `ZedOneTimeRoomTile4WayZoneTuningPass.cs`
- `ZedOneTimeRoomTile4WayZoneRotationAndFillTuning.cs`

Then it deletes itself.

## What this does NOT do

It does not touch:

- `RoomTile-4Way.prefab`
- `BuildableZones_Prototype`
- any `ZedBuildableZone` position
- any `ZedBuildableZone` width/depth
- runtime lot filler behavior

Your manually authored zones are now the source of truth.

## Recommended RoomTile-4Way zone layout

Use four 15x15 zones positioned like the second reference image:

- `BuildZone_NorthWest_Corner`
  - Local Position: `(-7.5, 0, 7.5)`
  - Width: `15`
  - Depth: `15`

- `BuildZone_NorthEast_Corner`
  - Local Position: `(7.5, 0, 7.5)`
  - Width: `15`
  - Depth: `15`

- `BuildZone_SouthWest_Corner`
  - Local Position: `(-7.5, 0, -7.5)`
  - Width: `15`
  - Depth: `15`

- `BuildZone_SouthEast_Corner`
  - Local Position: `(7.5, 0, -7.5)`
  - Width: `15`
  - Depth: `15`

Tune these manually in the prefab view. Do not use auto-authoring for this anymore.
