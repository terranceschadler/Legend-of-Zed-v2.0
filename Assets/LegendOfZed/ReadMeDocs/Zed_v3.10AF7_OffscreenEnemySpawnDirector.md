# v3.10AF7 — Offscreen Enemy Spawn Director

Adds a clean runtime enemy spawn director for the generated map.

## Design locked in

- Spawn source: valid NavMesh positions outside the active camera view.
- Minimum distance from player: 22 world units by default, tunable in inspector.
- NavMesh validation: sample candidate position and require a complete path to the player.
- Camera validation: candidate must be outside the gameplay camera viewport with padding.
- Startup: waits for legacy map generation to finish, then waits through a short grace period.
- Pacing: controlled trickle after map generation.
- Limits: max alive zombie count and max spawned per minute.
- Cleanup: despawn very far zombies only when they are also off-screen.
- Pooling: despawned/disabled spawned zombies return to a small reuse pool.

## Setup

Run:

`Legend Of Zed > Enemies > Setup Offscreen Enemy Spawn Director`

This creates/selects `Zed_Offscreen_Enemy_Spawn_Director` and attempts to assign:

- active `ZedLegacyRandomMapGenerator`
- `Camera.main`
- current `PlayerController` or Player-tagged object
- `Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab`

## Important

This does not create a new enemy system. It reuses:

- `ZedPrototypeZombieEnemy`
- `ZedZombieRootMotionRelay`
- `ZedZombieHitReactionMotor`
- `ZedZombieAudioBridge`
- existing bullet/player damage messaging

## Defaults

- InitialGraceSeconds: 5
- MinPlayerDistance: 22
- MaxPlayerDistance: 65
- Group size: 1–3
- Spawn interval: 3–6 seconds
- MaxAliveZombies: 10
- MaxSpawnedPerMinute: 24
- FarDespawnDistance: 90

Keep `VerboseLogs` off during normal playtesting.
