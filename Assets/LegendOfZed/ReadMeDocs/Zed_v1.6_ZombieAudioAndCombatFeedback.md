# v1.6 Zombie Audio and Combat Feedback

## Goal

Add audio feedback hooks without requiring audio clips yet.

## Added / updated

- `ZedAudioFeedback`
- `ZedZombieAudioBridge`
- `ZedPlayerAudioBridge`
- `ZedZombieHitReactionMotor` now optionally plays hit audio.
- `ZedPlayerHealth` now optionally plays hurt audio.
- `ZedV16CombatAudioSetup`

## Setup

Run:

`Legend of Zed/Setup/v1.6 Add Combat Audio Feedback Components`

Then assign clips in the `ZedAudioFeedback` components:

Zombie prototype:
- Idle Groans
- Attack Clips
- Hit Clips
- Death Clips

Player:
- Hurt Clips

## Notes

This pass compiles and works without clips assigned. No audio will play until clips are assigned.

## Untouched

This pass does not alter:
- Player controller source
- Shooter controller source
- Weapon setup
- BulletPoint
- Ammo IDs
- Projectile IDs
- Zombie movement/ragdoll baseline
