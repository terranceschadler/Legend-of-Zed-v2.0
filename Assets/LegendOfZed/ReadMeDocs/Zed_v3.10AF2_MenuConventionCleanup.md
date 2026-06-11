# Legend of Zed v3.10AF2 — Menu Convention Cleanup

This patch removes the duplicate top-level Unity editor menu caused by mixed menu roots.

## Convention

Use only:

```text
Legend Of Zed
```

Do not use:

```text
Legend of Zed
```

## Files Updated

- `Assets/LegendOfZed/Editor/ZedLotMarkerAuthoringMenu.cs`
  - Moves Map Authoring menu items under `Legend Of Zed`.
- `Assets/LegendOfZed/Editor/ZedV310ACProjectCleanupMenu.cs`
  - Moves Cleanup menu items under `Legend Of Zed`.
- `Assets/LegendOfZed/Editor/ZedV310AF1ExistingPlayerEnemyRebindMenu.cs`
  - Confirms the AF1 player/enemy integration tools live under `Legend Of Zed`.
  - Fixes the editor script brace cleanup so the file stays clean and compile-safe.

## Expected Result

After Unity finishes compiling, there should be one top-level project menu:

```text
Legend Of Zed
```

The player/enemy integration commands should appear at:

```text
Legend Of Zed > v3.10AF1 Player Enemy Integration
```

The map authoring commands should appear at:

```text
Legend Of Zed > Map Authoring
```

The cleanup commands should appear at:

```text
Legend Of Zed > Cleanup
```
