# v3.10u — Corner Prefab Side Metadata

## Problem Found

The diagnostics showed the spawner was assuming corner buildings expose their second street-facing side on local `+X`.

But `Z-forward` only defines the front/door direction.

It does not define whether the corner/window side is local `+X` or local `-X`.

## Fix

Added optional prefab metadata:

`ZedBuildingCornerSide`

Options:

- `RightPositiveX`
- `LeftNegativeX`

The spawner now orients corner buildings so:

- local `+Z` faces one street
- the configured corner side faces the other street

## New Editor Menu

Select a corner prefab asset, then run:

`Legend of Zed / Map Integration / v3.10U / Set Selected Prefabs Corner Side RIGHT (+X)`

or

`Legend of Zed / Map Integration / v3.10U / Set Selected Prefabs Corner Side LEFT (-X)`

## Practical Use

For a bad `Apartment_Corner_01` case, select its prefab asset and set the side to LEFT (-X) if its corner windows are on the left side when looking at the front.

## Patched Files

- `Assets/LegendOfZed/Scripts/MapIntegration/ZedAuthoredBuildingLotSpawner.cs`
- `Assets/LegendOfZed/Scripts/MapIntegration/ZedBuildingCornerSide.cs`
- `Assets/LegendOfZed/Editor/ZedAuthoredBuildingCornerSideMenu.cs`

## Expected Log

`V3.10U CORNER PREFAB SIDE METADATA complete...`

Look for:

- `CornerSideRightMetadataUsed`
- `CornerSideLeftMetadataUsed`
