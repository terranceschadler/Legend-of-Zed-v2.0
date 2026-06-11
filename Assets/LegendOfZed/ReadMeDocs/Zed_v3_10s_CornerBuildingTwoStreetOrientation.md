# v3.10s — Corner Building Two-Street Orientation

## Goal

Corner buildings should be oriented properly:

- model local `+Z` front / door side faces one street
- model local `+X` right-side / window wall faces the other street

## Fix

For detected corner lots, the spawner now:

1. Detects adjacent road sides around the lot.
2. Builds valid corner-facing candidates.
3. Tests the candidate with full road blockers.
4. Scores both the front edge and right-side edge against nearby roads.
5. Uses the orientation that exposes both street-facing sides.

No diagonal rotations.

## Assumption

All building models are still treated as Z-forward.

## New Setting

`orientCornerBuildingsToExposeFrontAndRightSide = true`

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Log

`V3.10S CORNER BUILDING TWO-STREET ORIENTATION complete...`

Look for:

- `CornerBuildingsOrientedToTwoStreets`
- `CornerBuildingsNoAdjacentRoadPair`
- `OrientCornerBuildingsToExposeFrontAndRightSide=True`
