# v3.9g — Integrated Auto Zone Coverage

## Problem

The separate `ZedAutoBuildableZoneCoverage` component did not run before the zone-based building spawner, so the log never showed:

`Auto buildable zone coverage complete...`

The zone spawner still only saw a small number of zones:

`Zones=16, Buildings=32`

## Fix

Patched only:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedZoneBasedBuildingSpawner.cs`

The zone-based building spawner now generates extra auto buildable zones itself before collecting zones.

This removes the setup/timing failure point.

## Expected Log

`Zone-based building spawner complete. Zones=..., ZonesUsed=..., Buildings=..., ... AuthoredFrontRows=True, AutoZonesCreated=..., AutoZoneRoadRejects=..., AutoZoneSmallRejects=..., RoadRects=...`

## Important

The log should include:

`AutoZonesCreated=...`

If `AutoZonesCreated` is 0 and `Zones` remains low, lower:

`Auto Zone Road Clearance`

from `0.75` to `0.25`

or increase:

`Auto Zone Minimum Desired Zones`

## Notes

Keep old `ZedRoadFrontageBuildingSpawner` disabled.
The separate `ZedAutoBuildableZoneCoverage` component is no longer required.
