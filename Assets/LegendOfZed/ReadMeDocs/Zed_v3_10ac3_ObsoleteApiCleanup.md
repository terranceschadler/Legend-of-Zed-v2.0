# v3.10AC3 — Obsolete API Cleanup

## Problem

Unity reported warnings from the v3.10AC cleanup pass:

- `FindObjectsSortMode` obsolete warnings
- `Object.FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode)` obsolete warnings
- `DestroyObject(Object)` name-hiding warning

## Fix

Updated project scripts to use non-obsolete API calls:

- `FindObjectsByType<T>(FindObjectsInactive.Include)`
- renamed local helper `DestroyObject(...)` to `DestroyUnityObject(...)`

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedLotMarkerAuthoringMenu.cs`
- `Assets/LegendOfZed/Editor/ZedV310ACProjectCleanupMenu.cs`

## Expected Result

These warnings should be gone:

- CS0108 from `DestroyObject`
- CS0618 from `FindObjectsSortMode`
- CS0618 from the obsolete `FindObjectsByType` overload
