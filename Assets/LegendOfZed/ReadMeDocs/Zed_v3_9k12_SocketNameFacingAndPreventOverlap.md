# v3.9k12 — Socket Name Facing + Pre-Accept Overlap Check

## Problems

The last test showed:

- some buildings still not facing the street
- buildings still visually intersecting/stacking
- `FinalBuildingOverlapCleanupRemoved=148`, which means too many buildings were accepted first and deleted later

## Fix 1 — Socket name drives facing

In socket mode, deterministic facing now uses the socket object name first:

- `Buildings_North_*` -> building local +Z faces south/toward road
- `Buildings_South_*` -> building local +Z faces north/toward road
- `Buildings_East_*` -> building local +Z faces west/toward road
- `Buildings_West_*` -> building local +Z faces east/toward road

This avoids any bad local-away vector causing rotation mistakes.

## Fix 2 — Pre-accept actual bounds overlap check

After a building is instantiated and front-anchored, the spawner now calculates actual renderer bounds and rejects the building before accepting it if it overlaps existing accepted buildings.

This should reduce the need for the final cleanup pass and prevent stacked/intersecting buildings from surviving.

## Expected Log

`V3.9K12 SOCKET NAME FACING + PRE-ACCEPT OVERLAP CHECK complete...`

Important markers:

- `UsingExplicitFrontageSockets=True`
- `UseSocketNameForDeterministicFacing=True`
- `SocketNameFacingApplied=...`
- `PreAcceptedBuildingOverlapRejects=...`
- `FinalBuildingOverlapCleanupRemoved=...`

`FinalBuildingOverlapCleanupRemoved` should drop substantially compared to the previous `148`.
