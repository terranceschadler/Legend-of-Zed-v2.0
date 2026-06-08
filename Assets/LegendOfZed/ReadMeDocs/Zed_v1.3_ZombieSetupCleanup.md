# v1.3 Zombie Setup Cleanup

## Current working zombie baseline

Use this branch as the cleanup/stabilization branch after v1.2 zombie spawning and combat polish.

Working systems:

- `ZedPrototypeZombieEnemy` owns zombie health, movement, death, and ragdoll activation.
- `ZedZombieRootMotionRelay` lives on the Animator object and relays root motion to the zombie root.
- `ZedV11ZombieRagdollSetup` rebuilds the tuned humanoid ragdoll.
- `ZedZombieSpawner` spawns multiple zombies from a scene prototype.
- `ZedV12ZombieSpawnerSetup` creates the scene spawner and spawn points.
- `Damage.cs` routes direct bullet damage to `ZedPrototypeZombieEnemy` and adds cleanup to impact FX.
- `ZedAutoDestroyAfterSeconds` cleans up BloodSplat and impact FX.

## Important rules

Do not add `TopDownShooter.HitPoint` to zombies. That demo component expects health bar references and can throw `HitPoint.UpdatePointsBars` null reference errors.

Do not require `NavMeshAgent` on zombies yet. Current zombie movement has a direct fallback and works without a NavMesh.

Do not use a missing Animator `Dead` trigger for death. Death currently stops AI/animation and enables ragdoll.

Do not edit player, weapons, ammo IDs, projectile IDs, or BulletPoint while doing zombie cleanup.

## Setup menu order

For a clean scene:

1. `Legend of Zed/Setup/v1.1 Rebuild Tuned Zombie Ragdoll`
2. `Legend of Zed/Setup/v1.2 Add Zombie Spawner`
3. `Legend of Zed/Setup/v1.3 Cleanup Zombie Scene Components`

## v1.3 cleanup menu

`Legend of Zed/Setup/v1.3 Cleanup Zombie Scene Components` does this:

- Finds all `ZedPrototypeZombieEnemy` objects in the open scene.
- Removes demo `TopDownShooter.HitPoint` components from zombie hierarchies.
- Removes `NavMeshAgent` from zombie roots if no usable NavMesh exists.
- Ensures the Animator has `ZedZombieRootMotionRelay`.
- Forces ragdoll death settings to the safe test defaults.
