# Zed v3.10AF5 — Runtime Console Cleanup

## Goal
Keep the Play Mode console focused on actionable messages while the player/enemy/map integration is being tested.

## What changed
- Added a runtime console log filter that defaults Play Mode to Warning-or-higher.
- Normal `Debug.Log` success spam from generation passes is hidden by default.
- Warnings, errors, asserts, and exceptions still show.
- Removed the player rotation zero-vector warning path in `MovementCharacterController` by guarding direction changes before assigning `transform.forward`.

## Menu
Use these only if you need to temporarily change verbosity:

- `Legend Of Zed > Cleanup > Console Logs > Quiet Runtime Logs (Warnings And Errors Only)`
- `Legend Of Zed > Cleanup > Console Logs > Verbose Runtime Logs (Show Debug.Log)`

## Notes
This does not delete generation systems or their diagnostics. It only stops routine success logs from flooding the console during normal Play Mode testing.
