# v2.0 Enemy Wave and Spawn Control

## Goal

Move from one immediate spawn burst to controlled wave spawning.

## Updated

- `Assets/LegendOfZed/Scripts/Enemies/ZedZombieSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedV20EnemyWaveSpawnControlSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.0_EnemyWaveAndSpawnControl.md`

## New spawner controls

`ZedZombieSpawner` now supports:

- `ZombiesPerWave`
- `MaxAliveZombies`
- `InitialSpawnDelay`
- `DelayBetweenSpawns`
- `DelayBetweenWaves`
- `LoopWaves`
- context menu: `Start Waves`
- context menu: `Stop Waves`
- context menu: `Spawn One Wave Now`
- context menu: `Clear Spawned Zombies`

## Setup

Run:

`Legend of Zed/Setup/v2.0 Apply Enemy Wave Spawn Defaults`

Then validate:

`Legend of Zed/Setup/v2.0 Validate Enemy Wave Spawner`

## Default values

- `SpawnOnStart = true`
- `LoopWaves = false`
- `ZombiesPerWave = 5`
- `MaxAliveZombies = 5`
- `InitialSpawnDelay = 0.25`
- `DelayBetweenSpawns = 0.45`
- `DelayBetweenWaves = 4`

## Test checklist

- Press Play.
- Zombies should spawn one at a time instead of all instantly.
- Total alive zombies should respect `MaxAliveZombies`.
- Spawned zombies should still use the prefab baseline.
- Spawned zombies should still rebind their own Animator/root-motion/audio refs.
- Zombies should chase/attack.
- Hit shove should still work.
- Ragdoll death should still work.
- Player damage/death should still work.
- No demo `HitPoint.UpdatePointsBars` errors.

## Untouched

This pass does not alter:

- Player controller source
- Shooter controller source
- Weapon setup
- BulletPoint
- Ammo IDs
- Projectile IDs
