# v2.7 Spawner Compatibility Compile Fix

## Problem

`ZedV27OverworldNavMeshBakePrototypeSetup.cs` directly referenced newer spawner fields:

- `UseNavMeshAgentWhenAvailable`
- `NavMeshSampleDistance`

The current project's `ZedZombieSpawner` does not expose those members, causing compile errors.

## Fix

This package replaces:

`Assets/LegendOfZed/Editor/ZedV27OverworldNavMeshBakePrototypeSetup.cs`

The replacement:

- still creates the NavMesh prototype
- still creates overworld zombie spawn points
- still connects an existing `ZedZombieSpawner`
- only sets optional spawner fields through reflection if they exist
- supports both `UseNavMeshAgentWhenAvailable` and older `UseNavMeshAgents` when present
- skips missing fields safely

## After importing

Let Unity compile, then run:

`Legend of Zed/Setup/Overworld/One Click Build NavMesh Prototype`
