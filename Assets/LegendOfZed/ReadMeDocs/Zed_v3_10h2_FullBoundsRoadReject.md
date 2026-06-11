# v3.10h2 — Full Bounds Road Reject

## Problem

A building could spawn in the street after the tighter block-packing pass.

The cause was that the same reduced safety footprint was being used for both:

- building-to-building spacing
- road overlap rejection

That made the road check too forgiving.

## Fix

The authored lot spawner now uses two separate rects:

- `roadSafetyRect` = full actual prefab renderer bounds
- `buildingSpacingRect` = tighter packed/scaled rect for building-to-building spacing only

## Result

Road rejection uses the real building footprint again.

Tighter packing still applies between buildings.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10H2 FULL BOUNDS ROAD REJECT complete...`

Look for:

`RoadCheckUsesFullBounds=True`
