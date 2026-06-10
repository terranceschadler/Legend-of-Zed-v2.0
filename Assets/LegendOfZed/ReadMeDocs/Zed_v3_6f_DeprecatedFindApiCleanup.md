# v3.6f — Deprecated Find API Cleanup

## Purpose

Removes obsolete Unity find API usage from the warning list reported after v3.6e.

## Direct cleanup

Available files from the patch history were directly updated.

## One-time local cleanup menu

Because these enemy scripts were not present in the local patch history used to build this zip:

- `Assets/LegendOfZed/Scripts/Enemies/ZedZombieHitReactionMotor.cs`
- `Assets/LegendOfZed/Scripts/Enemies/ZedZombieSpawner.cs`
- `Assets/LegendOfZed/Scripts/Enemies/ZedPrototypeZombieEnemy.cs`

This package also adds:

`Legend of Zed / Tools / Cleanup Deprecated Find APIs`

Run that menu once after import. It scans `Assets/LegendOfZed` and replaces only these narrow obsolete patterns:

- `FindFirstObjectByType<T>()` -> `FindAnyObjectByType<T>()`
- `FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode.None)` -> `FindObjectsByType<T>(FindObjectsInactive)`

## Notes

This is an API cleanup only. It does not change road-frontage placement behavior.
