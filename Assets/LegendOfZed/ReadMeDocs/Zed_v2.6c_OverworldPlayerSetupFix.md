# v2.6c Overworld Player Setup Fix

## Problem

The one-click overworld setup created the generated city and portal markers, but `Zed_Overworld` had no player, so there was nothing to control or use for portal testing.

## Fix

Adds:

`Assets/LegendOfZed/Editor/ZedV26cOverworldPlayerSetupFix.cs`

## New menus

Add only the player:

`Legend of Zed/Setup/Overworld/Add Player To Overworld Test`

Run the existing one-click setup, then add the player:

`Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Player`

## Behavior

The setup tries to:

1. Reuse an existing player if one already exists.
2. Instantiate a project prefab that has `PlayerController` or `CharacterController`.
3. Copy the working player from `Zed_Controller_Test`.
4. If no gameplay player can be found, create a temporary fallback capsule player with WASD movement for portal testing.

## Test

1. Run:
   `Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Player`
2. Open `Zed_Overworld`.
3. Press Play.
4. Walk to an enterable marker.
5. Press E.
6. Confirm `Zed_Interior_Test` loads.
