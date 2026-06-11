# v3.9i21 — Frontage Endpoint Extension

## Problem

Intersection/corner supplemental strips still did not appear:

`IntersectionCornerRoadRectsConsidered > 0`
`IntersectionCornerRoadRectsEligible = 0`

The raw road renderer rects are too fragmented/tiny to use directly. The visible gaps are at the start/end of otherwise valid frontage rows.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

Adds endpoint extension strips:

- copies each valid existing frontage strip
- adds a short strip before its start
- adds a short strip after its end
- keeps the same road-facing side and sidewalk setback
- final road/overlap/boundary checks still decide whether buildings can place

## Expected Log

`V3.9I21 FRONTAGE ENDPOINT EXTENSION complete...`

Watch:

- `FrontageEndpointExtensionsAdded`
- `FrontageEndpointExtensionRejects`
- `GapFillBuildings`
- `Buildings`

## Goal

Fill gaps that consistently appear beside 3-way intersections and 2-way corners.
