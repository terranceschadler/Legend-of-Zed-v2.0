# v2.5 Building Lot Placement and Enterable Markers

## Goal

Make building lots less random and start preparing enterable buildings.

This is still not final art. It is a rules pass:

- buildings face the nearest road
- each building lot gets a clear path toward the road
- some buildings get visible enterable/portal candidate markers
- no actual scene loading yet

## Added

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldEnterableBuildingMarker.cs`
- `Assets/LegendOfZed/Editor/ZedV25BuildingLotPlacementSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.5_BuildingLotPlacementAndEnterableMarkers.md`

## Menus

Apply building lot polish:

`Legend of Zed/Setup/v2.5 Apply Building Lot Placement And Enterable Markers`

Clear building lot polish:

`Legend of Zed/Setup/v2.5 Clear Building Lot Placement Markers`

Validate:

`Legend of Zed/Setup/v2.5 Validate Building Lot Placement`

## Recommended order

1. Open `Zed_Overworld`.
2. Run `v2.3 Rebuild Overworld With Block Art`.
3. Run `v2.4 Apply Park Block Polish`.
4. Run `v2.5 Apply Building Lot Placement And Enterable Markers`.
5. Run `v2.5 Validate Building Lot Placement`.

## Notes

This pass intentionally does not make portals functional yet.

The visible portal marker is only a candidate marker for the later enterable-building scene transition pass.

## Untouched

This pass does not alter:

- core seeded layout rules
- park polish logic
- roads
- player controller
- shooter controller
- weapons
- bullets
- ammo IDs
- zombie prefab
- zombie spawner
- combat systems
