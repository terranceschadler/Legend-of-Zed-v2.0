# Zed v3.10AF3 — Player Asset Selection Fix

## Purpose

Fixes the v3.10AF1 player/enemy rebind tool so it no longer hard-codes or auto-instantiates the TopDownShooter demo `Player.prefab`.

The integration pass now uses the selected or existing scene player instead of guessing the player asset.

## Updated Tool Behavior

Menu location:

`Legend Of Zed > v3.10AF1 Player Enemy Integration`

Commands:

- `Rebind Current Scene To Selected Or Existing Player`
  - Prefers the selected scene player if one is selected.
  - Otherwise uses the tagged `Player` object.
  - Otherwise uses a single scene `PlayerController` if only one exists.
  - If there are multiple possible players, it stops and asks for the correct player to be selected.
  - It does not create a player prefab.

- `Rebind Selected Player Object Or Prefab`
  - Use this when you know exactly which player object or prefab should be repaired.

- `Rebind Existing Zombie Prefab`
  - Still repairs the existing zombie baseline prefab at:
    `Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab`

- `Validate Current Scene Integration`
  - Reports scene player candidates, the resolved player, prefab source, zombie targets, zombie animator links, root motion relay links, and spawner targets.

## Notes

This is a cleanup patch over v3.10AF2. It replaces only:

`Assets/LegendOfZed/Editor/ZedV310AF1ExistingPlayerEnemyRebindMenu.cs`

No gameplay controller rewrite and no duplicate player framework.
