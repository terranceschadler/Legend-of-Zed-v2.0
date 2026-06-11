# v3.10q — Corner Lot Prefab Preference

## Goal

Certain buildings are better for corner lots.

Dense packing should not treat corner lots exactly the same as regular frontage lots.

## Corner Detection

A lot is considered a corner lot when its lot footprint is close to roads on two or more cardinal sides.

Default:

`cornerRoadTouchDistance = 6`

## Corner Prefab Preference

Corner lots prefer prefabs whose names contain one of:

- `corner`
- `apartment`
- `shop`
- `office`
- `hotel`

This is a first-pass heuristic. Add or remove keywords in the inspector if needed.

## Density Preserved

Regular prefabs can still be used as fallback:

`allowRegularPrefabsOnCornerFallback = true`

So density is preserved even if no matching corner-friendly prefab fits.

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10Q CORNER LOT PREFAB PREFERENCE complete...`

Look for:

- `CornerLotsDetected`
- `CornerPrefabChoices`
- `CornerFallbackChoices`
- `PreferCornerPrefabsForCornerLots=True`

If corner lots are not being detected enough, increase `cornerRoadTouchDistance`.
