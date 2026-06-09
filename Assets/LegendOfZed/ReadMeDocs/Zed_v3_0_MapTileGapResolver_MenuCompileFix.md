# Zed v3.0 Map Tile Gap Resolver Menu Compile Fix

Fixes `ZedMapTileGapResolverMenu.cs` so it matches the current `ZedMapTileGapResolver` fields.

Removed old references to:

- `runAutomatically`
- `maxPasses`
- `positionTolerance`
- `logDetails`

The menu now assigns the current fields:

- tile prefab references
- `gridSize`
- `waitTimeoutSeconds`
- `logDebugDetails`

No generator behavior changed.
