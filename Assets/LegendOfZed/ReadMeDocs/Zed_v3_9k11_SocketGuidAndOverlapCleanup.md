# v3.9k11 — Socket GUID + Overlap Cleanup

## Problems

The latest test showed two separate issues:

1. Unity reports missing scripts on generated `Buildings_*` socket objects.
2. Some spawned buildings visually overlap/intersect.

The missing socket scripts are the bigger issue because broken socket components can make placement/facing look random.

## Fixes

### Stable socket script GUID restored

`ZedRoadFrontageSocket.cs.meta` is restored to the original v3.9K GUID:

`f1d8cc2cdda24c1cbf1a5a88287a519a`

This should repair prefab socket components that were showing as missing scripts.

### Final building overlap cleanup

The V39I spawner now runs a final pass after spawning:

- calculates actual renderer bounds
- removes later buildings that overlap earlier kept buildings
- logs the removed count

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`
- `Assets/LegendOfZed/Scripts/MapIntegration/ZedRoadFrontageSocket.cs`
- `Assets/LegendOfZed/Scripts/MapIntegration/ZedRoadFrontageSocket.cs.meta`

## Expected Log

`V3.9K11 SOCKET GUID + OVERLAP CLEANUP complete...`

Important markers:

- `UsingExplicitFrontageSockets=True`
- `DeterministicSocketZForwardFacing=True`
- `FinalBuildingOverlapCleanupRemoved=...`

## After Import

If Unity still reports missing scripts on `Buildings_*` socket objects after this patch, rerun:

`Legend of Zed / Map Integration / v3.9K / Rebuild CLEAN Enabled Sockets From Merged Road Geometry On Selected Prefab Assets`

on the affected tile prefab assets so they are rebuilt against the restored socket script.
