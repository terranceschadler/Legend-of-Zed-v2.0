# v1.9 Zombie Prefab Baseline

## Goal

Turn the working scene zombie prototype into a reusable prefab baseline.

## Added / updated

- `Assets/LegendOfZed/Scripts/Enemies/ZedZombieSpawner.cs`
- `Assets/LegendOfZed/Editor/ZedV19ZombiePrefabBaselineSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v1.9_ZombiePrefabBaseline.md`

## New generated prefab

The setup tool creates:

`Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab`

This prefab is created locally in Unity from the working scene prototype.

## Menus

Create the prefab and assign it to the scene spawner:

`Legend of Zed/Setup/v1.9 Create Zombie Prefab Baseline`

Validate the generated prefab:

`Legend of Zed/Setup/v1.9 Validate Zombie Prefab Baseline`

Reassign the prefab to the spawner later:

`Legend of Zed/Setup/v1.9 Assign Zombie Prefab To Spawner`

## Prefab rules

The zombie prefab should include:

- `ZedPrototypeZombieEnemy`
- child `Animator`
- `ZedZombieRootMotionRelay` on the Animator object
- tuned ragdoll rigidbodies/colliders/joints
- `ZedZombieHitReactionMotor`
- `ZedZombieAudioBridge`
- `ZedAudioFeedback`

The zombie prefab should not include:

- `TopDownShooter.HitPoint`
- required `NavMeshAgent`

## Test checklist

After running the create menu:

- Press Play.
- Spawner should spawn from the prefab asset.
- 5 zombies should still spawn/chase.
- Shooting zombies should still create blood splats and hit shove.
- Zombies should still ragdoll on death.
- Player damage/death should still work.
- No `HitPoint.UpdatePointsBars` errors.
