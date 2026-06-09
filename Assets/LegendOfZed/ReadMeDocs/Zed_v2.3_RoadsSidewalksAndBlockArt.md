# v2.3 Roads, Sidewalks, and Block Art

## Goal

Start replacing the v2.2 debug foundation blocks with real project art where available.

This pass keeps the seeded layout and persistence system from v2.2.

## Added / updated

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldGenerationManager.cs`
- `Assets/LegendOfZed/Editor/ZedV23RoadsSidewalksBlockArtSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.3_RoadsSidewalksAndBlockArt.md`

## Menus

Auto-find Synty/project prefabs and assign them to the overworld manager:

`Legend of Zed/Setup/v2.3 Auto Assign Overworld Block Art`

Rebuild the overworld using assigned art, with debug fallback when missing:

`Legend of Zed/Setup/v2.3 Rebuild Overworld With Block Art`

Validate assignments:

`Legend of Zed/Setup/v2.3 Validate Overworld Block Art Assignment`

## Important

This pass still keeps debug fallback visuals enabled.

If a category has no matching Synty prefab, that block type will still appear as debug geometry instead of disappearing.

## Test checklist

- Unity compiles.
- Open `Zed_Overworld`.
- Run Auto Assign Overworld Block Art.
- Run Rebuild Overworld With Block Art.
- Confirm 49 blocks are generated.
- Some blocks should use assigned art if found.
- Missing categories should fall back to debug geometry.
- Same seed should preserve the same block layout.
- No player/combat/zombie systems are changed.

## Untouched

This pass does not alter:

- player controller
- shooter controller
- weapons
- bullets
- ammo IDs
- zombie prefab
- zombie spawner
- ragdoll
- combat systems
