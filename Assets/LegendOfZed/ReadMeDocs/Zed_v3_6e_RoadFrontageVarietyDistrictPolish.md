# v3.6e — Road Frontage Variety + District Polish

## Purpose

Adds variety and district-style polish to the working road-frontage building spawner.

This keeps v3.6d placement behavior.

## Added

- anti-repeat prefab selection
- per-segment recent prefab memory
- simple position-based district style buckets
- larger anchor building chance on long frontage segments
- optional deterministic `varietySeed`

## New fields

- `avoidImmediatePrefabRepeats`
- `recentPrefabMemory`
- `useDistrictStyleBuckets`
- `districtCellSize`
- `anchorBuildingChance`
- `minAnchorSegmentLength`
- `districtCandidatePercent`
- `varietySeed`

## Still true

- road-frontage source of truth
- buildings face roads
- actual renderer bounds align to placement center
- magenta boxes use actual renderer bounds
- road overlap rejection
- generated-building overlap rejection
- round-robin street coverage
- alley gaps
