# v3.10b — Authored Lot Menu Deprecated API Cleanup

## Problem

`ZedAuthoredBuildingLotSpawnerMenu.cs` used Unity APIs that are obsolete/deprecated in Unity 6.4:

- `FindFirstObjectByType<T>()`
- `FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode)`

## Fix

Updated the menu script to use current Unity-safe calls:

- `FindAnyObjectByType<T>()`
- `FindObjectsByType<T>(FindObjectsInactive)`

## Patched File

`Assets/LegendOfZed/Editor/ZedAuthoredBuildingLotSpawnerMenu.cs`

## Expected Result

The CS0618 warnings from this menu script should be gone.
