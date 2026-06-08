# v1.9b Zombie Prefab Spawn Rebind

## Problem

The v1.9 prefab existed, but spawned zombies could still carry copied references from the scene prototype, such as an Animator reference pointing at `Zed_Prototype_Zombie_Visual`.

Runtime spawned prefab instances always appear in the Hierarchy, but their references should point to their own cloned children.

## Fix

`ZedZombieSpawner` now rebinds every spawned zombie after instantiation:

- assigns the clone's own child Animator
- ensures the clone's Animator has `ZedZombieRootMotionRelay`
- assigns `relay.Owner` to the clone's own `ZedPrototypeZombieEnemy`
- removes demo `TopDownShooter.HitPoint`
- removes `NavMeshAgent`
- assigns Player target
- ensures hit reaction/audio bridge components exist

## Setup

Run:

`Legend of Zed/Setup/v1.9b Reassign Prefab Spawner And Rebind Rules`

## Test

After pressing Play, select a `Zed_Spawned_Zombie`.

The `ZedPrototypeZombieEnemy.Animator` field should point to that spawned zombie's own child Animator, not to `Zed_Prototype_Zombie_Visual` in the scene.
