# v3.9k7 — Socket-Owned Building Facing

## Problem

Explicit sockets were being used for placement, but some buildings still faced the wrong direction, especially on dead ends.

The old placement logic still had dead-end/frontage facing correction rules that could override the semantic socket direction.

## Fix

Socket-mode building placement now uses the socket direction for rotation:

- `awayFromRoad` = where the building sits
- building front faces `-awayFromRoad`, back toward the road

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## New Setting

`socketModeOwnsBuildingFacing = true`

## Expected Log

`V3.9K7 SOCKET-OWNED BUILDING FACING complete...`

Important marker:

`SocketModeOwnsBuildingFacing=True`

## Result

Dead-end socket buildings should face according to the socket direction instead of old dead-end flip logic.
