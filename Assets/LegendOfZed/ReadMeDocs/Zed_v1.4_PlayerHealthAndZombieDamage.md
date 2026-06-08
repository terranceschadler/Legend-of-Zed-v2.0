# v1.4 Player Health and Zombie Damage

## Goal

Zombies already send damage messages:

- `ApplyDamage`
- `TakeDamage`
- `Damage`

`ZedPlayerHealth` receives all three safely, so zombie attacks can damage the player without using the demo `TopDownShooter.HitPoint` health bar system.

## Added files

- `Assets/LegendOfZed/Scripts/Player/ZedPlayerHealth.cs`
- `Assets/LegendOfZed/Editor/ZedV14PlayerHealthSetup.cs`

## Setup

Run:

`Legend of Zed/Setup/v1.4 Add Player Health`

This adds `ZedPlayerHealth` to the player in:

`Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity`

## Test checklist

- Press Play.
- Let zombies reach the player.
- Health should decrease.
- Red flash should appear when damaged.
- Damage should respect invulnerability timing and not spam every frame.
- At 0 health, movement/shooting should stop.
- `PLAYER DOWN` should appear.
- No `TopDownShooter.HitPoint.UpdatePointsBars` errors.

## Notes

This pass does not touch:

- player controller source
- shooter controller source
- weapons
- bullets
- ammo IDs
- projectile IDs
- zombie spawner
- zombie ragdoll
