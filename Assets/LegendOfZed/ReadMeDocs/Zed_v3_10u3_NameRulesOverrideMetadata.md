# v3.10u3 — Name Rules Override Metadata

## Problem

The log after v3.10U2 showed:

- `CornerSideRightMetadataUsed=23`
- `CornerSideLeftMetadataUsed=4`
- `CornerSideNameRuleUsed=9`

That means some corner prefabs are still resolving through metadata/default RIGHT before the name rule can force them LEFT.

This can happen if a prefab has `ZedBuildingCornerSide` with the default RIGHT value.

## Fix

The resolver order is now:

1. Name rules first when `cornerSideNameRulesOverrideMetadata = true`
2. Metadata component
3. Fallback default side

## Defaults

- `Apartment_Corner` => `LeftNegativeX`
- `Shop_Corner` / `SM_Bld_Shop_Corner` => `RightPositiveX`
- `cornerSideNameRulesOverrideMetadata = true`

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10U3 NAME RULES OVERRIDE METADATA complete...`

Look for:

- `CornerSideNameRulesOverrideMetadata=True`
- `CornerSideNameRuleUsed`
- `CornerSideMetadataRuleUsed`

For `Apartment_Corner`, the name rule should win even if metadata exists.
