# v2.2 Seeded Overworld Generation Foundation

## Goal

Start the clean overworld generator inside this controller project.

This is the foundation for:

- seeded city generation
- roads / sidewalks / building lots / parks as block types
- persistent overworld root
- no regeneration after entering and leaving interiors
- later NavMesh build
- later Synty art population
- later portals to dungeon/interior scenes

## Added

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldBlockType.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldGeneratedBlock.cs`
- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldGenerationManager.cs`
- `Assets/LegendOfZed/Editor/ZedV22SeededOverworldFoundationSetup.cs`

## Menus

Create the clean overworld scene:

`Legend of Zed/Setup/v2.2 Create Seeded Overworld Scene`

Generate immediately in editor:

`Legend of Zed/Setup/v2.2 Generate Seeded Overworld Now`

Validate:

`Legend of Zed/Setup/v2.2 Validate Seeded Overworld Foundation`

## Scene

The setup creates:

`Assets/LegendOfZed/Scenes/Zed_Overworld.unity`

## Current block types

- Road
- BuildingLot
- Park

## Persistence

The generated root is named:

`Zed_Seeded_Overworld_Root`

At runtime the manager can reuse this root instead of regenerating. This is the first step toward leaving/entering buildings without changing the overworld.

## Important limit of this pass

This pass uses debug foundation visuals only so we can confirm seed/layout/persistence rules first.

Do not judge final art from this pass.

The next passes should replace block visuals with real Synty roads, sidewalks, buildings, parks, props, and NavMesh collection.

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
