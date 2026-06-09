# Zed v3.0 Auto Aggressive Editor Cleanup

## Purpose

Automatically removes leftover editor helper/setup scripts from:

`Assets/LegendOfZed/Editor`

## Auto-run behavior

After this package is unzipped and Unity compiles, the script auto-runs once.

It keeps only:

- `ZedMapTileGeneratorIntegrationMenu.cs`
- `ZedMapTileBoundaryWallBuilderMenu.cs`

It deletes every other `.cs` file in:

`Assets/LegendOfZed/Editor`

Then it deletes itself.

## Not touched

- runtime scripts
- map generator fixes
- boundary wall runtime logic
- park prefabs
- scenes
- art assets
- ReadMeDocs
