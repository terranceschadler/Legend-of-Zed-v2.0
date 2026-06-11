# v3.9k — Explicit Tile Frontage Socket System

## Why

The renderer/collider frontage experiments proved geometry inference is not stable enough. It cannot reliably know which road edge is valid building frontage, which edge is an intersection, or which line belongs to a tile.

The new direction is semantic frontage:

`tile instance -> explicit ZedRoadFrontageSocket children -> exact frontage line`

## New Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedRoadFrontageSocket.cs`
- `Assets/LegendOfZed/Editor/ZedExplicitTileFrontageSocketSetupMenu.cs`

## Patched File

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Runtime Behavior

The V39I spawner now:

1. Detects road geometry for safety cleanup.
2. Looks for `ZedRoadFrontageSocket` components under `Generated_Map_Tiles`.
3. If sockets exist, uses them for frontage lines.
4. If sockets do not exist, falls back to the current collider/renderer fallback.
5. Forces experimental supplemental systems off so they do not interfere.

## Forced Off Experimental Systems

- intersection corner supplemental frontage
- endpoint extension frontage
- tile-edge frontage
- dead-end supplemental frontage
- frontage gap fill
- endpoint filler pass
- slot filler pass
- authored spawn fallback
- median collider corridor experiment

## Editor Menus

`Legend of Zed / Map Integration / v3.9K / Create Frontage Socket Holder On Selected Tiles`

Adds a `Zed_ExplicitFrontageSockets` holder under selected generated tile instances.

`Legend of Zed / Map Integration / v3.9K / Remove Experimental Frontage Components From Scene`

Removes old experimental frontage components from the open scene by type name.

`Legend of Zed / Map Integration / v3.9K / Disable Experimental Settings On V39I Spawners`

Forces existing V39I spawner objects to use the new socket-first workflow and disables experimental passes.

## Expected Log

`V3.9K EXPLICIT TILE FRONTAGE SOCKET SYSTEM complete...`

Important counters:

- `UsingExplicitFrontageSockets`
- `ExplicitFrontageSocketsFound`
- `ExplicitFrontageStripsBuilt`
- `ExplicitFrontageSocketRejects`

If sockets are not authored yet:

- `UsingExplicitFrontageSockets=False`
- fallback collider frontage is used

## Next Step

Author sockets per road tile prefab/instance:

- socket line start/end should match the exact sidewalk/building frontage edge
- `localAwayFromRoad` should point away from the street toward building placement
- `canSpawnBuildings=false` for intersections, road caps, parks, or blocked sides
