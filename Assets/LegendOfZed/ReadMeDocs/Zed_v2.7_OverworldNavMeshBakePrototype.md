# v2.7 Overworld NavMesh Bake Prototype

## Goal

Prototype overworld NavMesh build support after the generated city/portal loop is confirmed.

## Added

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldRuntimeNavMeshBuilder.cs`
- `Assets/LegendOfZed/Editor/ZedV27OverworldNavMeshBakePrototypeSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.7_OverworldNavMeshBakePrototype.md`

## Menus

Build/setup the prototype:

`Legend of Zed/Setup/Overworld/One Click Build NavMesh Prototype`

Validate:

`Legend of Zed/Setup/Overworld/Validate NavMesh Prototype`

## What it does

- Ensures the hidden overworld walkable floor exists.
- Tags that floor as `Walkable`.
- Adds `Zed_Overworld_NavMesh_Prototype`.
- Adds a runtime NavMesh builder.
- Adds six overworld zombie spawn points.
- Connects an existing `ZedZombieSpawner` if one exists in the scene.
- Keeps the immediate portal test intact.

## Runtime behavior

At Play, `ZedOverworldRuntimeNavMeshBuilder` collects walkable collider sources and builds a runtime NavMesh.

The hidden test floor is the guaranteed source for this prototype.

## Test checklist

1. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Immediate Portal Test`
2. Run:
   `Legend of Zed/Setup/Overworld/One Click Build NavMesh Prototype`
3. Run:
   `Legend of Zed/Setup/Overworld/Validate NavMesh Prototype`
4. Press Play.
5. Confirm log:
   `Zed overworld runtime NavMesh built.`
6. If a `ZedZombieSpawner` exists in the scene, confirm zombies spawn/path.
7. Confirm portal loop still works.

## Untouched

This pass does not alter:

- portal loop
- safe portal test player
- combat behavior
- zombie prefab internals
- weapons
- bullets
- ammo IDs
