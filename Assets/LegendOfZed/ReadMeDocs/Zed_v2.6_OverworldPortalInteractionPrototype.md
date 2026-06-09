# v2.6 Overworld Portal Interaction Prototype

## Goal

Prototype the basic enter/return loop:

- Press E at an enterable building marker.
- Load a simple interior test scene.
- Press E at the return portal.
- Return to the same overworld scene.
- Restore player near the original portal location.
- Preserve the overworld seed/root behavior.

## Added

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldReturnState.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldPortalTrigger.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldReturnApplier.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedInteriorReturnPortal.cs`
- `Assets/LegendOfZed/Editor/ZedV26OverworldPortalPrototypeSetup.cs`

## Menus

Create the interior test scene:

`Legend of Zed/Setup/v2.6 Create Interior Test Scene`

Add portal triggers to the v2.5b enterable markers:

`Legend of Zed/Setup/v2.6 Add Portal Triggers To Enterable Markers`

Validate:

`Legend of Zed/Setup/v2.6 Validate Portal Prototype`

## Recommended order

1. Open `Zed_Overworld`.
2. Run v2.3 rebuild.
3. Run v2.4 park polish.
4. Run v2.5b guarantee and reapply building markers.
5. Run `v2.6 Create Interior Test Scene`.
6. Run `v2.6 Add Portal Triggers To Enterable Markers`.
7. Add both scenes to Build Settings:
   - `Zed_Overworld`
   - `Zed_Interior_Test`
8. Press Play in `Zed_Overworld`.
9. Walk to an enterable marker and press E.
10. In the test interior, walk to return marker and press E.

## Current limitations

- Uses temporary OnGUI prompts.
- Uses a simple placeholder interior scene.
- Does not create real dungeon interiors yet.
- Does not assign different interior types per building yet.
- Does not bake NavMesh yet.

## Untouched

This pass does not alter combat, zombies, player controller, weapons, bullets, ammo IDs, or zombie spawning.
