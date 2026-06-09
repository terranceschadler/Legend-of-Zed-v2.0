# v2.9c WeaponDrop Folder Pickup Placement

## Goal

Use the exact weapon pickup folder requested by the project owner:

`Assets/TopDownShooterController/Media/Prefabs/Items/WeaponDrop`

## Replaces

`Assets/LegendOfZed/Editor/ZedV29bOverworldWeaponPickupTestPlacement.cs`

## Behavior

The pickup placement menu now:

- stops guessing pickup prefabs
- does not copy random scene objects
- loads only prefabs from:
  `Assets/TopDownShooterController/Media/Prefabs/Items/WeaponDrop`
- places up to 8 WeaponDrop prefabs near the current overworld player
- preserves prefab scripts/references
- ensures each placed pickup has at least one collider

## Menu

Add/rebuild pickups:

`Legend of Zed/Setup/Overworld/v2.9b Add Weapon Pickups Near Player`

Validate:

`Legend of Zed/Setup/Overworld/v2.9b Validate Weapon Pickups`

Clear:

`Legend of Zed/Setup/Overworld/v2.9b Clear Weapon Pickup Test`

## Safety

This pass does not edit:

- weapon pickup prefab assets
- WeaponData
- ammo IDs
- weapon prefab assets
- bullet prefab assets
- zombies
- NavMesh
- portals

## Test

1. Import this package.
2. Run:
   `Legend of Zed/Setup/Overworld/v2.9b Add Weapon Pickups Near Player`
3. Run:
   `Legend of Zed/Setup/Overworld/v2.9b Validate Weapon Pickups`
4. Press Play in `Zed_Overworld`.
5. Test the WeaponDrop pickups near the player.
