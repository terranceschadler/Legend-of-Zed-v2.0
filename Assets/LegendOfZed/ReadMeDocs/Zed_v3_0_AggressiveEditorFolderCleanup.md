# Zed v3.0 Aggressive Editor Folder Cleanup

## Purpose

Deletes leftover editor setup/menu/helper scripts from:

`Assets/LegendOfZed/Editor`

This is more aggressive than the earlier cleanup, because old setup helpers were still left behind.

## How to run

After unzipping, run:

`Legend of Zed/Cleanup/Delete All Obsolete Editor Helpers`

## What it deletes

Every `.cs` file under:

`Assets/LegendOfZed/Editor`

except the whitelist below.

It also deletes matching `.meta` files and then deletes itself.

## Whitelist kept

- `ZedMapTileGeneratorIntegrationMenu.cs`
- `ZedMapTileBoundaryWallBuilderMenu.cs`

## Not touched

This cleanup does not touch:

- `Assets/LegendOfZed/Scripts`
- runtime scripts
- map generator fixes
- boundary wall runtime logic
- park prefabs
- scenes
- art assets
- `ReadMeDocs`

## Important

This is intended for the current v3 map generator baseline where old one-time setup menus are no longer needed.
