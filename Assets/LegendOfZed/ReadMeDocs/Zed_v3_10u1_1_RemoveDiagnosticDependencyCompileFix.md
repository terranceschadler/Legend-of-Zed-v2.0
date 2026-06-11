# v3.10u1.1 — Remove Diagnostic Dependency Compile Fix

## Problem

`ZedAuthoredBuildingLotSpawner.cs` called:

`LogCornerOrientationDiagnostic(...)`

but that helper was not present in the current spawner file.

Unity error:

`CS0103: The name 'LogCornerOrientationDiagnostic' does not exist in the current context`

## Fix

Removed the diagnostic call from the forced corner side rotation path.

The actual v3.10U1 behavior is preserved:

- corner lots use the detected road side pair
- `ZedBuildingCornerSide` controls whether the second street-facing side is `+X` or `-X`
- model `+Z` remains the front

## Expected Log

`V3.10U1.1 REMOVE DIAGNOSTIC DEPENDENCY COMPILE FIX complete...`
