# v1.5 Zombie Attack Feel and Hit Reaction

## Goal

Make zombie attacks feel more readable and make bullet hits slightly affect zombie motion.

## Changes

`ZedPrototypeZombieEnemy` now supports:

- attack windup before player damage is applied
- optional range check at the actual hit moment
- brief bullet-hit stagger
- preserved ragdoll death
- preserved root-motion locomotion
- preserved no-NavMesh fallback

## Setup

Run:

`Legend of Zed/Setup/v1.5 Apply Zombie Attack Feel Defaults`

## Test checklist

- Spawned zombies still chase the player.
- Zombie attack animation starts before player health drops.
- If the player moves away during the windup, damage should not apply.
- Shooting zombies should briefly interrupt their movement without breaking chase.
- Zombies still ragdoll on death.
- Player health/damage from v1.4 still works.
- No `TopDownShooter.HitPoint.UpdatePointsBars` errors.

## Defaults

- `AttackCooldown = 1.45`
- `AttackWindupSeconds = 0.35`
- `AttackRangeGrace = 0.35`
- `HitStaggerSeconds = 0.16`

## Untouched

This pass does not alter:

- Player controller source
- Shooter controller source
- Weapon setup
- BulletPoint
- Ammo IDs
- Projectile IDs
