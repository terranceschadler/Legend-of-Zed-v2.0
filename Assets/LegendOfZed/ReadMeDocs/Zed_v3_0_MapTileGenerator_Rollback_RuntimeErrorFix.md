# Zed v3.0 Map Tile Generator Rollback — RuntimeErrorFix Backout

This package backs out the bad RuntimeErrorFix pass.

Restored behavior:

- Restores `ZedLegacyRandomMapGenerator.cs` to the original imported legacy generator from `v3.0-map-tile-generator-reimport`.
- Restores the runtime bridge to the earlier bridge-only version.
- Keeps the fresh-scene menu helper so `Legend of Zed/Map Integration/Add Runtime Bridge To Scene` still exists.

Reason:

The RuntimeErrorFix changed dead-end spawning behavior and left visible tile-spawn artifacts. This rollback removes that change so we can fix the index error with a smaller targeted pass next.
