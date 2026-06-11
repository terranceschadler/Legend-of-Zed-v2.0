# v3.10w1 — Corner Prefab Nearest Pair Orientation

## Problem

The selected `Apartment_Corner_01` is a corner prefab, but the corner rule only fired for detected corner lots.

If the old lot-corner detector misses that lot, the building falls back to generic +Z frontage scoring and can choose the wrong street.

## Fix

If the spawned prefab name contains `Corner`, it now forces the right-hand corner orientation path even if the old corner-lot detector misses it.

The corner path now finds the nearest perpendicular road pair around the lot position:

- north + east => +Z north, +X east
- east + south => +Z east, +X south
- south + west => +Z south, +X west
- west + north => +Z west, +X north

## Model Rule

- local `+Z` = front
- local `+X` = right side

## New Settings

- `forceCornerPrefabPairOrientation = true`
- `cornerPrefabOrientationKeywords = ["Corner"]`
- `cornerPrefabNearestRoadSearchDistance = 24`

## Expected Log

`V3.10W1 CORNER PREFAB NEAREST PAIR ORIENTATION complete...`

Look for:

- `CornerPrefabPairOrientationUsed`
- `CornerPrefabPairOrientationFallbacks`
- `ForceCornerPrefabPairOrientation=True`
