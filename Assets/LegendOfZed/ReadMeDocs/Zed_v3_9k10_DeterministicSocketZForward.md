# v3.9k10 — Deterministic Socket Z-Forward Facing

## Correction

Per-prefab facing correction was the wrong direction for this project.

All active building prefabs use local `+Z` as the storefront/front. Therefore socket mode should be deterministic:

`building local +Z -> toward the road`

## Fix

When a building is spawned from an explicit socket:

- use the socket `awayFromRoad` direction for placement
- set building rotation so local `+Z` faces `-awayFromRoad`
- disable auto prefab-facing correction for socket buildings
- per-socket yaw offset is available but off by default

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Expected Log

`V3.9K10 DETERMINISTIC SOCKET Z-FORWARD FACING complete...`

Important markers:

- `UsingExplicitFrontageSockets=True`
- `DeterministicSocketZForwardFacing=True`
- `SocketDeterministicFacingApplied=...`
- `UsePerSocketFacingOffset=False`

## Meaning

Buildings on the north side of a road face south.
Buildings on the south side face north.
Buildings on the east side face west.
Buildings on the west side face east.
