# v3.0 Map Tile Generator Runtime Error Fix

Changed-files-only patch.

## Fixes

- Fixes `ArgumentOutOfRangeException` in `ZedLegacyRandomMapGenerator.FixedUpdate()` by replacing index-based mutation loops with safe pop-last processing.
- Prevents the legacy generator from continuing after its open spawn list is exhausted.
- Dead-end fill no longer consumes the main random tile budget.
- Fresh test scenes now get required tags through the setup menu.
- Runtime bridge now creates a fallback test player if no Player object or player prefab exists.
- Runtime bridge now creates a Main Camera if the fresh test scene has none.
- Editor setup assigns generator/camera/player prefab references to the bridge when possible.

## Use

1. Open `Zed_Overworld_Test`.
2. Run `Legend of Zed/Map Integration/Add Runtime Bridge To Scene`.
3. Save the scene.
4. Press Play.

Expected: no legacy generator index errors, no missing player warning, generated map is integrated by the bridge.
