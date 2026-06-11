# v3.10AC2 — Legacy Room Tile Remove Lot Filler Dependency

## Problem

The cleanup pass removed the old experimental building lot filler script, but `ZedLegacyRoomTile` still referenced that removed type directly.

That caused CS0246 compile errors.

## Fix

`ZedLegacyRoomTile` no longer references the removed filler type.

Legacy room tiles now either:

- use their old fixed marker spawn path, or
- are suppressed by the authored-lot building pipeline.

## Expected Result

The CS0246 errors from `ZedLegacyRoomTile.cs` should be gone.
