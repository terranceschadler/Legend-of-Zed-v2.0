# v1.5b Stronger Zombie Hit Reaction

## Goal

The first v1.5 hit stagger was too subtle. This pass adds a visible hit shove while preserving the working zombie AI/ragdoll script.

## Added

- `ZedZombieHitReactionMotor`
- `ZedV15bZombieHitReactionSetup`
- `Damage.cs` now calls the hit reaction motor after damaging a `ZedPrototypeZombieEnemy`

## Defaults

- `HitStaggerSeconds = 0.45`
- `ShoveDistance = 0.55`
- `ShoveSeconds = 0.14`

## Setup

Run:

`Legend of Zed/Setup/v1.5b Apply Stronger Zombie Hit Reaction`

## Test checklist

- Shoot a chasing zombie.
- Zombie should visibly pause and shove backward slightly.
- Zombie should resume chase after the hit reaction.
- Zombies still ragdoll on death.
- Player damage from v1.4 still works.
- Blood splats still clean up.
