# v2.7b NavMesh Mesh Read Access Warning Cleanup

## Problem

The v2.7 runtime NavMesh prototype built successfully, but Unity warned that imported Synty source meshes do not allow read access.

Example:

`RuntimeNavMeshBuilder: Source mesh SM_Gen_Env_Sidewalk_Edge_01 does not allow read access. This will work in playmode in the editor but not in player`

## Fix

This package replaces:

`Assets/LegendOfZed/Scripts/Overworld/ZedOverworldRuntimeNavMeshBuilder.cs`

The builder now defaults to:

`UseColliderBoundsOnly = true`

That means it builds NavMesh sources from collider bounds boxes instead of source mesh data.

## Why this is better for now

- No Synty import setting changes.
- No Read/Write warnings.
- No player-build risk from unreadable meshes.
- Still builds a usable prototype NavMesh.
- The hidden test floor remains the guaranteed walkable source.

## Test

1. Import this package.
2. Run:
   `Legend of Zed/Setup/Overworld/One Click Build NavMesh Prototype`
3. Press Play.
4. Confirm:
   `Zed overworld runtime NavMesh built.`
5. Confirm the old source mesh read access warnings are gone.
