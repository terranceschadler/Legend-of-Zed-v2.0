# Zed v3.0 PolygonCity Duplicate Dependency Cleanup

Purpose: clean up the duplicate PolygonCity dependency risk without changing map generation behavior.

## Problem

The project currently has PolygonCity assets in two places:

- `Assets/LegendOfZed/MapGeneratorImport/ThirdParty/PolygonCity`
- `Assets/Synty/PolygonCity`

Unity treats these as different assets because they have different GUIDs, even when filenames match. This can make map tile prefabs reference the wrong duplicate mesh/material/prefab copy.

## Canonical source

Use this as the real source:

`Assets/Synty/PolygonCity`

Do not delete this duplicate folder yet:

`Assets/LegendOfZed/MapGeneratorImport/ThirdParty/PolygonCity`

Only delete or move it after validation reports zero dependencies.

## Menus added

`Legend of Zed/Map Integration/PolygonCity Dependency Cleanup/1 Report Duplicate PolygonCity Dependencies`

Scans map-generator prefabs under:

`Assets/LegendOfZed/MapGeneratorImport/Prefabs`

and writes a report to:

`Assets/LegendOfZed/ReadMeDocs/Zed_v3_0_PolygonCity_DuplicateDependencyReport.md`

`Legend of Zed/Map Integration/PolygonCity Dependency Cleanup/2 Remap Map Generator Prefabs To Assets/Synty/PolygonCity`

Edits only prefab YAML files under:

`Assets/LegendOfZed/MapGeneratorImport/Prefabs`

It replaces GUIDs that point into:

`Assets/LegendOfZed/MapGeneratorImport/ThirdParty/PolygonCity`

with matching assets at the same relative path under:

`Assets/Synty/PolygonCity`

It does not delete assets, move folders, or change generator logic.

`Legend of Zed/Map Integration/PolygonCity Dependency Cleanup/3 Validate Canonical PolygonCity Dependencies`

Runs the scan again and reports whether any map-generator prefab still depends on the duplicate PolygonCity folder.

## Safe workflow

1. Run menu 1 and read the report.
2. Run menu 2 to remap prefab GUIDs.
3. Run menu 3 to validate.
4. Only after validation reports clean, test the legacy map generator scene.
5. Only after the generator still works, quarantine or delete `Assets/LegendOfZed/MapGeneratorImport/ThirdParty/PolygonCity`.

## Notes

This package intentionally does not modify `ZedLegacyRandomMapGenerator` or dead-end spawning.
