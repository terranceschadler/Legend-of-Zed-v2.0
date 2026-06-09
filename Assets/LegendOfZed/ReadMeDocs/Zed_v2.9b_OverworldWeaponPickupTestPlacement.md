# v2.9b Overworld Weapon Pickup Test Placement

## Goal

Add nearby weapon pickups in `Zed_Overworld` so the real gameplay player can test pickup behavior, ammo behavior, and zombie combat without manually dragging objects around.

## Added

- `Assets/LegendOfZed/Editor/ZedV29bOverworldWeaponPickupTestPlacement.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.9b_OverworldWeaponPickupTestPlacement.md`

## Menus

Add/rebuild pickups:

`Legend of Zed/Setup/Overworld/v2.9b Add Weapon Pickups Near Player`

Validate:

`Legend of Zed/Setup/Overworld/v2.9b Validate Weapon Pickups`

Clear:

`Legend of Zed/Setup/Overworld/v2.9b Clear Weapon Pickup Test`

## Behavior

The setup tries to:

1. Copy working pickup objects from `Zed_Controller_Test`.
2. If none are found, instantiate pickup prefabs from the project.
3. Place up to 6 pickups near the current real/safe player.
4. Preserve existing pickup scripts and prefab references.
5. Ensure each pickup has a collider.

## Safety

This pass does not edit:

- `WeaponData`
- ammo IDs
- weapon prefab assets
- bullet prefab assets
- combat tuning
- zombie prefab assets
- portal scripts
- NavMesh builder

## Recommended test

1. Run:
   `Legend of Zed/Setup/Overworld/v2.9 Replace Safe Player With Real Gameplay Player`
2. Run:
   `Legend of Zed/Setup/Overworld/v2.9b Add Weapon Pickups Near Player`
3. Run:
   `Legend of Zed/Setup/Overworld/v2.9b Validate Weapon Pickups`
4. Press Play in `Zed_Overworld`.
5. Walk over / interact with the pickups.
6. Confirm weapon pickup behavior.
7. Confirm ammo count/type is correct.
8. Confirm zombies still spawn/path.
