# v3.10s1 — Corner Test All Cardinal Facings

## Problem

A corner building was still not oriented correctly.

The previous corner pass limited candidate rotations based on detected road sides around the lot. If that lot-side detection was slightly wrong, it could choose the wrong rotation.

## Fix

Corner buildings now test all four legal cardinal rotations:

- +Z
- +X
- -Z
- -X

For each rotation:

1. Center the Z-forward building on the lot.
2. Reject if the full body overlaps road blockers.
3. Score the +Z/front edge against roads.
4. Score the +X/right-side edge against roads.
5. Choose the best combined front+side score.

## Assumption

All building models are still treated as Z-forward.

## Expected Log

`V3.10S1 CORNER TEST ALL CARDINAL FACINGS complete...`

Look for:

`CornerCandidateMode=AllFourCardinal`
