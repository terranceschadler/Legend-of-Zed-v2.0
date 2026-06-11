# v3.9i12.1 — Dead-End Road-Only Compile Fix

## Problem

v3.9i12 failed compile because the fallback tile-edge dead-end helper methods were removed while references to them remained.

Compiler errors included:

- `MakeDeadEndNorthStrip` missing
- `MakeDeadEndSouthStrip` missing
- `MakeDeadEndEastStrip` missing
- `MakeDeadEndWestStrip` missing
- `MakeDeadEndStrip` missing

## Fix

Restored those helper methods.

The default runtime behavior is still:

`Dead End Use Road Rects Only = true`

So the tile-edge fallback compiles but is not used unless manually disabled.

## Expected Log

`V3.9I12.1 DEAD-END ROAD-ONLY COMPILE FIX complete...`
