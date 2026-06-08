# v2.1 Spawn Gizmos and Level Placement

## Goal

Make zombie spawn points easier to see, select, and place in scenes.

## Added

- `Assets/LegendOfZed/Scripts/Spawning/ZedZombieSpawnPointMarker.cs`
- `Assets/LegendOfZed/Editor/ZedZombieSpawnPointMarkerEditor.cs`
- `Assets/LegendOfZed/Editor/ZedV21SpawnGizmosAndPlacementSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.1_SpawnGizmosAndLevelPlacement.md`

## New marker component

`ZedZombieSpawnPointMarker` draws:

- spawn radius wire sphere
- vertical marker line
- forward direction line
- editor label

## Menus

Add gizmos to existing spawn points:

`Legend of Zed/Setup/v2.1 Add Spawn Point Gizmos`

Rebuild a larger 8-point arena layout around the player:

`Legend of Zed/Setup/v2.1 Rebuild 8 Arena Spawn Points`

Select the spawn point root:

`Legend of Zed/Setup/v2.1 Select Spawn Point Root`

## Test checklist

- Scene view shows zombie spawn point gizmos.
- Spawn points have readable labels.
- Spawner still references the spawn points.
- Press Play.
- Wave spawning from v2.0 still works.
- Zombies still use prefab baseline and rebind correctly.
- Zombies still chase/attack/ragdoll.
- Player damage still works.

## Untouched

This pass does not alter:

- player controller
- shooter controller
- weapon setup
- BulletPoint
- ammo IDs
- projectile IDs
- zombie prefab internals
- combat behavior
