# v3.5c — CityBlock Tag Only Detection

## Purpose

Simplifies the city block building filler.

There is only one tag now:

`CityBlock`

No `BlockSurface`.

## What to tag

Tag the actual sidewalk/block surface renderer object or a parent of that renderer:

`CityBlock`

Do not tag:

- road meshes
- curbs
- lane markings
- buildings
- trees
- spawn markers
- TileSpawn objects
- boundary walls

## Runtime behavior

The filler:

1. scans all renderers
2. accepts renderers whose object or parent has tag `CityBlock`
3. merges adjacent tagged surfaces into blocks
4. places buildings around the merged block perimeter

## Defaults

These are disabled by default:

- name/material fallback detection
- tile-footprint fallback detection

So if no `CityBlock` surfaces are found, it spawns zero buildings and logs a clear warning.

## Cleanup

This package also removes the old experimental `ZedCityBlockTagEnsure.cs` helper if it exists.
That helper was only for the unwanted `BlockSurface` experiment.
