# Zed v3.0 Map Tile Gap Resolver Compile Fix

Fixes a C# compile error in `ZedMapTileGapResolver.cs` caused by reusing the local variable name `directions` inside `BuildRequiredConnectionMap`.

No generator behavior changed.
