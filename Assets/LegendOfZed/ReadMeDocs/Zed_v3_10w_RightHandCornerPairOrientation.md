# v3.10w — Right-Hand Corner Pair Orientation

## Correction

The model contract is:

- local `+Z` = front
- local `+X` = right side

The screenshot shows a correct corner case where the lot is on west + north streets:

- `+Z` faces west
- `+X` faces north
- Unity rotation Y = -90

## Fix

For corner lots, the spawner now applies the right-hand corner-pair rule before the generic frontage scorer.

Rules:

- north + east roads => `+Z` north, `+X` east
- east + south roads => `+Z` east, `+X` south
- south + west roads => `+Z` south, `+X` west
- west + north roads => `+Z` west, `+X` north

No name rules. No left-side metadata. No diagonal rotation.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Log

`V3.10W RIGHT-HAND CORNER PAIR ORIENTATION complete...`

Look for:

- `UseRightHandCornerPairOrientation=True`
- `RightHandCornerPairOrientationUsed`
- `RightHandCornerPairOrientationFallbacks`
