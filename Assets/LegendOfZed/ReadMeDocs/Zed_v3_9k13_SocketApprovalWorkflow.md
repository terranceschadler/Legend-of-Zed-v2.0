# v3.9k13 — Socket Approval Workflow

## Problem

The automatic socket builder was enabling every generated socket. That caused buildings to spawn from valid and invalid sides, including back sides, courtyard lines, and places that should not spawn buildings.

## Fix

Sockets are now safe by default.

Runtime will only use a socket when BOTH are true:

- `Can Spawn Buildings = true`
- `Approved For Runtime Spawning = true`

The clean socket builder now creates disabled, unapproved sockets.

## New Workflow

1. Select a tile prefab asset.
2. Run:

`Legend of Zed / Map Integration / v3.9K / Rebuild CLEAN DISABLED Sockets From Merged Road Geometry On Selected Prefab Assets`

3. Open the prefab.
4. Select only the socket lines that are valid building frontage.
5. Click:

`APPROVE This Socket For Runtime Spawning`

or use menu:

`Legend of Zed / Map Integration / v3.9K / Approve Selected Socket Objects`

## Useful Cleanup Menu

`Legend of Zed / Map Integration / v3.9K / Disable ALL Sockets On Selected Prefab Assets`

Use this on prefabs that were previously auto-enabled.

## Expected Runtime Log

`V3.9K13 SOCKET APPROVAL WORKFLOW complete...`

Important markers:

- `UsingExplicitFrontageSockets=True`
- `ExplicitFrontageSocketsFound=...`
- `ExplicitFrontageStripsBuilt=...`
- `ExplicitFrontageSocketsUnapproved=...`

If buildings are spawning where they should not, that prefab has too many approved sockets.
