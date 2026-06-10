# v3.7c3 — Skip Spawn Timing + Order Fix

## Problem

Skips/dumpsters were still not spawning.

Likely cause:

- The street prop spawner could run before `Generated_RoadFrontage_Buildings` had children.
- Then `BuildingBlockers=0`, so no buildings existed to place skips against.
- Dumpsters were also attempted after small street props, letting smaller props block dumpster locations.

## Fix

- Waits for `Generated_RoadFrontage_Buildings` to exist and contain children before running.
- Adds `waitForGeneratedBuildings`.
- Adds `maxGeneratedBuildingWaitFrames`.
- Collects generated building blockers after buildings exist.
- Spawns dumpsters/skips before street lights, benches, mailboxes, etc.
- Loosens dumpster rejection so skips claim spots before small props.
- Keeps tuned prefab arrays preserved.
- Logs `BuildingRootReady=True/False`.

## Notes

This patch only changes the street prop spawner runtime.
It does not change building placement or boundary walls.
