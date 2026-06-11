# v3.10u2 — Name-Based Corner Side Resolver

## Problem

The prior corner-side metadata had no visible effect.

The log showed:

`CornerSideRightMetadataUsed=33`
`CornerSideLeftMetadataUsed=0`

So the spawned instances were never resolving to LEFT-side corner behavior.

## Fix

`ResolveCornerStreetSide` now checks, in order:

1. `ZedBuildingCornerSide` metadata on the spawned instance.
2. Name-based rules.
3. Default corner side.

## Defaults

LEFT side keywords:

- `Apartment_Corner`

RIGHT side keywords:

- `Shop_Corner`
- `SM_Bld_Shop_Corner`

This means `Apartment_Corner_01_AuthoredLotBuilding` should now use `LeftNegativeX` even if the metadata component was not present on the spawned instance.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10U2 NAME-BASED CORNER SIDE RESOLVER complete...`

Look for:

- `CornerSideLeftMetadataUsed`
- `CornerSideNameRuleUsed`

For Apartment_Corner cases, `CornerSideNameRuleUsed` and `CornerSideLeftMetadataUsed` should be greater than zero.
