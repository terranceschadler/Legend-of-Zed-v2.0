# v2.6b Overworld One-Click Setup Consolidation

## Goal

Remove the current overworld setup menu clutter and replace the multi-step workflow with one menu item.

## New menu

`Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test`

## What the one-click setup does

- Creates/opens `Assets/LegendOfZed/Scenes/Zed_Overworld.unity`
- Creates/configures `Zed_Overworld_Generation_Manager`
- Auto-assigns available Synty/project art prefabs
- Generates the seeded 7x7 overworld
- Applies park polish
- Guarantees at least 2 enterable building candidates
- Applies building/door/portal candidate markers
- Creates/updates `Assets/LegendOfZed/Scenes/Zed_Interior_Test.unity`
- Adds return portal to the test interior
- Adds portal triggers to enterable markers
- Adds both scenes to Build Settings
- Saves all assets/scenes

## Retired menu scripts

The following editor setup scripts are replaced with no-menu stubs:

- `ZedV22SeededOverworldFoundationSetup.cs`
- `ZedV23RoadsSidewalksBlockArtSetup.cs`
- `ZedV24ParkBlockPolishSetup.cs`
- `ZedV25BuildingLotPlacementSetup.cs`
- `ZedV25bGuaranteedEnterableBuildingsSetup.cs`
- `ZedV26OverworldPortalPrototypeSetup.cs`
- legacy failed generator setup scripts if present

## Runtime scripts included

This package also includes/reincludes the current portal runtime scripts so the one-click setup has everything it needs:

- `ZedOverworldEnterableBuildingMarker`
- `ZedOverworldReturnState`
- `ZedOverworldPortalTrigger`
- `ZedOverworldReturnApplier`
- `ZedInteriorReturnPortal`

## Test

1. Let Unity compile.
2. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test`
3. Open `Zed_Overworld`.
4. Press Play.
5. Walk to an enterable marker.
6. Press E.
7. Confirm `Zed_Interior_Test` loads.
8. Walk to the return portal.
9. Press E.
10. Confirm the overworld reloads and the player returns near the original portal.

## Untouched

This pass does not alter:

- player controller
- shooter controller
- weapons
- bullets
- ammo IDs
- zombie prefab
- zombie spawner
- combat systems
