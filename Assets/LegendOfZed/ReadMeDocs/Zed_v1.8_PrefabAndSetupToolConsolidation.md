# v1.8 Prefab and Setup Tool Consolidation

## Goal

Create a safe consolidation checkpoint before retiring old setup tools or creating reusable combat prefabs.

This pass does not delete older setup tools yet. It adds one stable validation/consolidation menu and documents the current workflow.

## Added

- `Assets/LegendOfZed/Editor/ZedV18CombatSetupConsolidation.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v1.8_PrefabAndSetupToolConsolidation.md`

## Menus

### Validate

`Legend of Zed/Setup/v1.8 Validate Current Combat Setup`

Checks:

- Player has `ZedPlayerHealth`
- Player has audio feedback bridge/components
- Zombie prototype exists
- Zombie prototype has `ZedPrototypeZombieEnemy`
- Zombie Animator has `ZedZombieRootMotionRelay`
- Zombie has hit reaction and audio bridge components
- Zombie hierarchy does not use demo `TopDownShooter.HitPoint`
- Spawner exists and has prototype/spawn points

### Apply stable defaults

`Legend of Zed/Setup/v1.8 Apply Stable Combat Defaults`

Normalizes current component defaults without changing player/weapons/bullets/ammo IDs.

### Print workflow

`Legend of Zed/Setup/v1.8 Print Stable Setup Workflow`

Prints the stable setup order in the console.

## Current stable setup order

1. `Legend of Zed/Setup/v1.1 Rebuild Tuned Zombie Ragdoll`
2. `Legend of Zed/Setup/v1.2 Add Zombie Spawner`
3. `Legend of Zed/Setup/v1.3 Cleanup Zombie Scene Components`
4. `Legend of Zed/Setup/v1.4 Add Player Health`
5. `Legend of Zed/Setup/v1.5 Apply Zombie Attack Feel Defaults`
6. `Legend of Zed/Setup/v1.5b Apply Stronger Zombie Hit Reaction`
7. `Legend of Zed/Setup/v1.6 Add Combat Audio Feedback Components`
8. `Legend of Zed/Setup/v1.7 Clean Debug HUD And Logs`
9. `Legend of Zed/Setup/v1.8 Validate Current Combat Setup`

## Important rules

- Do not add `TopDownShooter.HitPoint` to zombies.
- Do not require `NavMeshAgent` until the scene has a valid NavMesh.
- Do not use missing Animator death triggers.
- Keep player controller, shooter controller, weapons, ammo IDs, projectile IDs, and BulletPoint untouched during setup cleanup.
- Use zip-delivered assets, test locally in Unity, then commit/push.
