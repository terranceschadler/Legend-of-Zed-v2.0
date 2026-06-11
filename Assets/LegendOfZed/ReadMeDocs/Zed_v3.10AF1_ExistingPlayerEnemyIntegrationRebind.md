# v3.10AF1 — Existing Player + Enemy Integration Rebind

This patch does **not** add a new generic player/enemy framework.

It reuses the systems that already exist in this project:

- `TopDownShooter.PlayerController`
- `TopDownShooter.MovementCharacterController`
- `TopDownShooter.ShooterController`
- `TopDownShooter.Damage`
- `LegendOfZed.Player.ZedPlayerHealth`
- `LegendOfZed.Enemies.ZedPrototypeZombieEnemy`
- `LegendOfZed.Enemies.ZedZombieSpawner`
- `LegendOfZed.Enemies.ZedZombieRootMotionRelay`

## What this adds

A single editor utility:

`Assets/LegendOfZed/Editor/ZedV310AF1ExistingPlayerEnemyRebindMenu.cs`

Menu path:

`Legend Of Zed > v3.10AF1 Player Enemy Integration`

Commands:

1. **Rebind Current Scene To Existing Player And Zombie**
   - Finds the existing `PlayerController`.
   - Adds/repairs `ZedPlayerHealth`.
   - Sets the player tag to `Player`.
   - Rebinds scene zombies to the player target.
   - Rebinds zombie animator/root-motion relay.
   - Rebinds zombie spawners to the player target.
   - Assigns the existing zombie baseline prefab to empty spawners when possible.

2. **Rebind Existing Player And Zombie Prefabs**
   - Repairs the existing player prefab:
     - `Assets/TopDownShooterController/Media/Prefabs/Player.prefab`
   - Repairs the existing zombie baseline prefab:
     - `Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab`

3. **Validate Current Scene Integration**
   - Logs a quick scene report:
     - Player found/missing.
     - Player health found/missing.
     - Zombie target assignments.
     - Zombie animator/root-motion relay assignments.
     - Spawner target assignments.

## Recommended use

1. Import this ZIP into the Unity project.
2. Open the gameplay scene you want to test.
3. Run:

   `Legend Of Zed > v3.10AF1 Player Enemy Integration > Rebind Existing Player And Zombie Prefabs`

4. Then run:

   `Legend Of Zed > v3.10AF1 Player Enemy Integration > Rebind Current Scene To Existing Player And Zombie`

5. Then run:

   `Legend Of Zed > v3.10AF1 Player Enemy Integration > Validate Current Scene Integration`

## Notes

- This patch is intentionally additive.
- It does not overwrite existing player, enemy, weapon, damage, input, or animation scripts.
- It only adds an editor repair/rebind menu and this README.
- Use Unity's undo if the current-scene rebind result is not what you expected.
