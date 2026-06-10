# v3.7c1 — Preserve Tuned Street Prop Arrays

## Problem

The street prop spawner menu auto-populated prefab arrays every time it was run, overwriting manually tuned asset arrays.

## Fix

The menu now only auto-fills a prefab array when that array is empty.

If an array already has tuned assets assigned, it is preserved.

## Applies to

- street lights
- benches
- trash cans
- hydrants
- mailboxes
- paper debris
- dumpsters
- trash piles

## Notes

This patch changes only the menu behavior.
It does not change runtime placement rules.
