# v3.9i15.1 — Frontage Gap Fill Compile Fix

## Problem

v3.9i15 failed compile because `Candidate` does not have `width` or `depth` fields.

Compiler errors:

- `Candidate.width` does not exist
- `Candidate.depth` does not exist

## Fix

Changed the gap-fill small-prefab sorter to use the existing footprint field:

- `candidate.size.x`
- `candidate.size.y`

## Expected Log

`V3.9I15.1 FRONTAGE GAP FILL COMPILE FIX complete...`
