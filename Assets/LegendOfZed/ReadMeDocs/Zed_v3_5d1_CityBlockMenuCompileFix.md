# v3.5d1 — CityBlock Menu Compile Fix

## Fix

Removes the obsolete editor menu reference:

`filler.useCityBlockTag = true;`

That field was removed in v3.5d because the filler now always uses CityBlock-only detection.

## Changed

- `Assets/LegendOfZed/Editor/ZedCityBlockBuildingFillerMenu.cs`

## No prefab changes

This package does not touch:

- room tiles
- generated map prefabs
- CityBlock tags
- building prefabs
