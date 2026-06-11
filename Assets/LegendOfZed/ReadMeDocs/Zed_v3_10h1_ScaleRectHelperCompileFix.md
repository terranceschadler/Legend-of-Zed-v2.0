# v3.10h1 — ScaleRect Helper Compile Fix

## Problem

`ZedAuthoredBuildingLotSpawner.cs` called:

`ScaleRectFromCenter(...)`

but the helper method was missing.

Unity error:

`CS0103: The name 'ScaleRectFromCenter' does not exist in the current context`

## Fix

Added the missing helper method to:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`

## Expected Result

The v3.10H tighter block packing pass compiles and keeps:

- centered buildings
- inward block setback
- tighter safety footprint
