# v1.7 Debug HUD and Log Cleanup

## Goal

Clean up temporary player-health debug noise after the v1.4-v1.6 combat passes.

## Changes

`ZedPlayerHealth` defaults are now cleaner:

- `ShowDebugHud = false`
- `LogDamageForTesting = false`
- `ShowDamageFlash = true`

The damage flash remains enabled because it is gameplay feedback, not debug spam.

## Setup menus

Use this for normal playtesting:

`Legend of Zed/Setup/v1.7 Clean Debug HUD And Logs`

Use this when debugging player damage:

`Legend of Zed/Setup/v1.7 Enable Player Debug HUD`

## Notes

This pass does not change:

- zombie movement
- zombie spawning
- zombie ragdoll
- zombie hit reaction
- player movement
- shooting
- weapons
- bullets
- ammo IDs
- projectile IDs
