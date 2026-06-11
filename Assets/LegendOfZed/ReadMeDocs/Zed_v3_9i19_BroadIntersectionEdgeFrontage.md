# v3.9i19 — Broad Intersection Edge Frontage

## Problem

v3.9i18 ran, but:

`IntersectionCornerStripsAdded=0`
`IntersectionCornerProbeRejects=0`
`IntersectionCornerExistingRejects=0`

That means supplemental strips were not being rejected. They were never being created because the road-rect eligibility filter was too narrow.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- Broadens intersection/corner road-rect eligibility.
- Removes the overly strict short-axis requirement from deciding whether to create supplemental frontage strips.
- Keeps final placement safety:
  - road checks
  - overlap checks
  - boundary checks
  - +Z road-facing
  - sidewalk setback

## Expected Log

`V3.9I19 BROAD INTERSECTION EDGE FRONTAGE complete...`

Watch:

- `IntersectionCornerRoadRectsConsidered`
- `IntersectionCornerRoadRectsEligible`
- `IntersectionCornerStripsAdded`
- `GapFillBuildings`

## Goal

Make the supplemental frontage strip pass actually produce strips around 3-way and 2-way corner gaps.
