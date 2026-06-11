# v3.10AD5 — Remove Rejected Row Workflows

## Purpose

This removes the rejected AD row workflows from the project.

Rejected workflows:

- runtime density slot subdivision
- automatic dense street-row marker generator
- anchor-based street-row marker generator
- block row zone authoring workflow

## Kept Stable Systems

- `ZedAuthoredBuildingLot`
- `ZedAuthoredBuildingLotSpawner`
- `ZedLotMarkerAuthoringMenu`
- one marker = one building
- lot marker category/facing remains source of truth

## Cleanup Behavior

A one-time editor cleanup script auto-runs after import.

It removes:

- `ZedAuthoredBuildingRowZone.cs`
- `ZedAuthoredBuildingRowZoneMenu.cs`
- `ZedAuthoredStreetRowLotMarkerGenerator.cs`
- rejected AD docs
- generated row-zone scene objects
- generated dense-row marker roots in prefabs
- missing script components caused by deleted rejected workflows

Then it deletes itself.

## Expected Log

`v3.10AD5 rejected row workflow cleanup complete...`

## Next Exploration

The next idea is to explore corner buildings communicating across a block and spawning row markers between valid block-side endpoints, while respecting actual street/sidewalk space.
