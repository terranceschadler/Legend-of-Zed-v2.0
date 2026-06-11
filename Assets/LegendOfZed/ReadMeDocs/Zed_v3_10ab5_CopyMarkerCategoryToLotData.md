# v3.10AB5 — Copy Marker Category To LotData

## Actual Bug Found

`ZedAuthoredBuildingLot.buildingCategory` existed, but `CollectLots()` was not copying it into the spawner's internal `LotData`.

So runtime lots were effectively still:

`category = Any`

That is why corner lots could still spawn non-corner buildings.

## Fix

The spawner now does:

`LotData.category = marker.buildingCategory`

Legacy blgSpawn markers explicitly use:

`category = Any`

## Migration Helper

If an old authored lot marker still has `buildingCategory = Any` but also has a `ZedBuildingFootprint` with `sizeClass = Corner`, it is treated as `Corner`.

This reads explicit marker-authored data only. It does not use road detection.

## Verification

Generated building names now include the effective category:

`SM_Bld_Apartment_Corner_01_AuthoredLotBuilding_Category_Corner_Facing_East_Yaw_90`

If a generated building says:

`Category_Corner`

then non-corner prefabs should be rejected.

If it says:

`Category_Any`

then that source marker is not set to Corner yet.

## Expected Log

`V3.10AB5 COPY MARKER CATEGORY TO LOTDATA complete...`

Look for:

- `LotMarkerCategoryAnyCount`
- `LotMarkerCategoryCornerCount`
- `LotMarkerCategoryFootprintCornerFallbackCount`
- `CornerFinalPrefabRejected`
- `CornerOverridePrefabRejected`
