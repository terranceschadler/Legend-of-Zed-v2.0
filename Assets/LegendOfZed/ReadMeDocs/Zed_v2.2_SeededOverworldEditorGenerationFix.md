# v2.2 Seeded Overworld Editor Generation Fix

## Fix

`DontDestroyOnLoad` can only be used in Play Mode. The first v2.2 generator called it from an editor menu, which caused generation to stop and left an empty `Zed_Seeded_Overworld_Root`.

This replacement only calls `DontDestroyOnLoad` when `Application.isPlaying`.

It also refuses to reuse an empty generated root. If the root has zero generated blocks, it clears it and regenerates.

## Test

1. Replace `ZedOverworldGenerationManager.cs`.
2. Let Unity compile.
3. Run `Legend of Zed/Setup/v2.2 Generate Seeded Overworld Now`.
4. Confirm `Zed_Seeded_Overworld_Root` has block children.
5. Validate again.
