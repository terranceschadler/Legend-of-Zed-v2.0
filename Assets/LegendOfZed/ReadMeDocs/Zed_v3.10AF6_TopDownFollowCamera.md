# v3.10AF6 — Top Down Follow Game Camera

Purpose: replace the static one-shot camera placement with a real top-down perspective game camera that follows the active Legend of Zed player after map generation.

## Changed

- `TopDownCamera` now:
  - uses perspective projection by default,
  - auto-finds the `Player` tag or `TopDownShooter.PlayerController`,
  - follows in `LateUpdate`,
  - keeps a top-down angled offset,
  - snaps cleanly on start,
  - stops warning-spamming when no target exists yet.

- `ZedMapTileGeneratorRuntimeBridge` now:
  - places/spawns the player on the generated start tile,
  - binds the Main Camera to the spawned/placed player,
  - adds `TopDownCamera` to the gameplay camera if missing,
  - snaps the camera after player spawn.

## Menu

Use:

`Legend Of Zed > Camera > Setup Top Down Follow Camera`

Select the real player first when possible. If no player is selected, the camera will auto-find the `Player` tag or `PlayerController` at runtime.

## Notes

This patch reuses the existing `TopDownShooter.TopDownCamera` instead of adding a parallel camera framework.
