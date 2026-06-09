# v2.8 Overworld Zombie Spawn / Path Test

## Goal

Prove that zombies can spawn on the overworld NavMesh and move toward the current safe portal test player.

## Added

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldZombiePathTestSpawner.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedSimpleNavMeshChaseTestZombie.cs`
- `Assets/LegendOfZed/Editor/ZedV28OverworldZombieSpawnPathTestSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.8_OverworldZombieSpawnPathTest.md`

## Menu

Build/setup:

`Legend of Zed/Setup/Overworld/One Click Build Zombie Spawn Path Test`

Validate:

`Legend of Zed/Setup/Overworld/Validate Zombie Spawn Path Test`

## What it does

- Creates `Zed_Overworld_ZombiePathTest`.
- Creates four spawn points near the portal test area.
- Adds `Zed_Overworld_ZombiePathTestSpawner`.
- Tries to find and assign the existing zombie prefab asset.
- Spawns 3 zombies on Play.
- Samples spawn positions onto the NavMesh.
- Assigns the safe portal player as target.
- Adds a `NavMeshAgent` if the prefab does not already have one.
- Uses reflection to assign common target fields on existing zombie scripts.
- Uses fallback capsule test zombies only if no zombie prefab can be found.

## Recommended run order

1. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Immediate Portal Test`
2. Run:
   `Legend of Zed/Setup/Overworld/One Click Build NavMesh Prototype`
3. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Zombie Spawn Path Test`
4. Run:
   `Legend of Zed/Setup/Overworld/Validate Zombie Spawn Path Test`
5. Press Play.

## Test checklist

- Confirm log:
  `Zed overworld runtime NavMesh built.`
- Confirm log:
  `v2.8 overworld zombie path test spawned 3 zombie(s).`
- Confirm zombies appear near the test area.
- Confirm zombies move toward `Zed_Portal_Test_Player`.
- Confirm immediate portal loop still works.

## Untouched

This pass does not alter:

- portal scripts
- real gameplay player
- weapon/combat tuning
- bullet/ammo IDs
- zombie prefab asset internals
- existing zombie combat behavior
