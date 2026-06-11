# v3.10n — Front Edge Road Facing Score

## Important

This patch is built from v3.10L. Skip v3.10M.

## Problem

Most buildings face correctly, but some do not.

This is not a prefab axis issue. The models are still treated as Z-forward.

The real issue is that nearest road to the lot center is not always the intended frontage road. A lot can be closer to a side/back road rectangle than to the road its facade should face.

## Fix

The spawner now tests the four cardinal Z-forward facings:

- +Z
- -Z
- +X
- -X

For each facing, it:

1. Rotates the Z-forward model cardinally.
2. Centers the renderer bounds on the lot.
3. Rejects the candidate if the full body overlaps road space.
4. Scores how close the building's FRONT edge is to a road.
5. Chooses the facing with the best front-edge road score.

No diagonal rotation. No prefab-axis guessing.

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10N FRONT EDGE ROAD FACING SCORE complete...`

Look for:

- `DirectionMode=FrontEdgeRoadFacingScore`
- `ChooseFacingByFrontEdgeRoadScore=True`
- `BuildingsFacedByFrontEdgeScore`
- `BuildingsFrontEdgeScoreFallbacks`
- `TryFourCardinalPrefabRotationsForRoadFit=False`
