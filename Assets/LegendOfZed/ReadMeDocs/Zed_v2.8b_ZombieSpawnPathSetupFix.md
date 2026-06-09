# v2.8b Zombie Spawn Path Setup Fix

## Problem

The v2.8 editor setup crashed with:

`ArgumentException: Gameobject is not a root in a scene`

The setup was parenting the spawn root under the test root, then calling:

`EditorSceneManager.MoveGameObjectToScene(...)`

Unity only allows `MoveGameObjectToScene` on root GameObjects.

Because the setup crashed before finishing, no zombie path test spawner was properly created, so no zombies spawned.

## Fix

This package replaces:

`Assets/LegendOfZed/Editor/ZedV28OverworldZombieSpawnPathTestSetup.cs`

The fixed setup:

- only moves root objects to the scene
- creates child spawn roots normally under the test root
- creates the path test spawner under the test root
- assigns 4 spawn points
- assigns the safe portal player as target
- assigns an existing zombie prefab if found
- falls back to capsule zombies if no zombie prefab is found
- adds a clean rebuild menu for partial failed v2.8 setups

## Menus

Clean rebuild:

`Legend of Zed/Setup/Overworld/Rebuild Zombie Spawn Path Test Clean`

Normal build:

`Legend of Zed/Setup/Overworld/One Click Build Zombie Spawn Path Test`

Validate:

`Legend of Zed/Setup/Overworld/Validate Zombie Spawn Path Test`

## Recommended use after the failed setup

1. Import this package.
2. Let Unity compile.
3. Run:
   `Legend of Zed/Setup/Overworld/Rebuild Zombie Spawn Path Test Clean`
4. Run:
   `Legend of Zed/Setup/Overworld/Validate Zombie Spawn Path Test`
5. Press Play.

Expected logs:

`Zed overworld runtime NavMesh built.`

`v2.8 overworld zombie path test spawned 3 zombie(s).`
