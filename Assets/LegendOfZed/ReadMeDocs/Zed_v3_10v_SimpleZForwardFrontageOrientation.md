# v3.10v — Simple Z-Forward Frontage Orientation

## Correction

The models are already correct.

- local `+Z` = front
- local `+X` = right

The spawner should not guess prefab axes, add name rules, or require corner-side metadata to solve basic orientation.

## Fix

The spawner now uses one simple rule:

1. Test the four legal cardinal rotations.
2. Center the building on the lot.
3. Reject any rotation where the full building overlaps road blockers.
4. Score only the model `+Z` front edge against nearby streets.
5. Use the rotation where `+Z` best faces a street.

No diagonal rotation.
No prefab-name orientation guessing.
No corner metadata required for the main orientation path.

## Corner behavior

The old corner side metadata path is disabled by default:

`orientCornerBuildingsToExposeFrontAndRightSide = false`

Corner buildings still use the same simple rule:

`+Z front faces the best street`

Since the models have `+X` on the right, the side wall naturally follows the rotation.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10V SIMPLE Z-FORWARD FRONTAGE ORIENTATION complete...`

Look for:

- `DirectionMode=SimpleZForwardFrontageOrientation`
- `ForceSimpleZForwardFrontageOrientation=True`
- `SimpleZForwardOrientationUsed`
- `SimpleZForwardOrientationFallbacks`
