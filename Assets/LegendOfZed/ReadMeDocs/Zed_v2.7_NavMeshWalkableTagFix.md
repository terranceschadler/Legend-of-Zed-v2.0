# v2.7 NavMesh Walkable Tag Fix

## Problem

`ZedOverworldRuntimeNavMeshBuilder` called:

`go.CompareTag("Walkable")`

The project does not define a `Walkable` tag, so Unity spammed:

`Tag: Walkable is not defined.`

## Fix

This package replaces:

- `Assets/LegendOfZed/Scripts/Overworld/ZedOverworldRuntimeNavMeshBuilder.cs`
- `Assets/LegendOfZed/Editor/ZedV27OverworldNavMeshBakePrototypeSetup.cs`

The runtime builder now:

- does not require a `Walkable` tag
- defaults `WalkableTag` to blank
- uses object-name matching for walkable sources:
  - floor
  - road
  - sidewalk
  - path
  - park
  - walkable
- safely catches missing tag errors if a tag name is supplied later

The editor setup also stops trying to assign the `Walkable` tag.

## After importing

Let Unity compile, then run:

`Legend of Zed/Setup/Overworld/One Click Build NavMesh Prototype`

Then press Play and confirm:

`Zed overworld runtime NavMesh built.`
