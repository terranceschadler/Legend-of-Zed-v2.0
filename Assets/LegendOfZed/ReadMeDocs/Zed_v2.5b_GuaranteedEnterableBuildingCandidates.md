# v2.5b Guaranteed Enterable Building Candidates

## Goal

Make sure every generated overworld has enough enterable building candidates for portal testing.

The current seed produced zero enterable candidates, so v2.5 had no portal markers to place.

## Added

- `Assets/LegendOfZed/Editor/ZedV25bGuaranteedEnterableBuildingsSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.5b_GuaranteedEnterableBuildingCandidates.md`

## Menus

Guarantee candidates only:

`Legend of Zed/Setup/v2.5b Guarantee Enterable Building Candidates`

Guarantee candidates and immediately reapply v2.5 markers:

`Legend of Zed/Setup/v2.5b Guarantee And Reapply Building Markers`

Validate:

`Legend of Zed/Setup/v2.5b Validate Enterable Building Candidates`

## Behavior

- Finds generated BuildingLot blocks.
- Counts existing enterable candidates.
- If fewer than 2 exist, promotes deterministic building lots based on seed.
- Reuses v2.5 marker placement so portal markers appear consistently.

## Recommended order

1. Open `Zed_Overworld`.
2. Run `v2.3 Rebuild Overworld With Block Art`.
3. Run `v2.4 Apply Park Block Polish`.
4. Run `v2.5b Guarantee And Reapply Building Markers`.
5. Run `v2.5b Validate Enterable Building Candidates`.

## Untouched

This pass does not alter:

- portals / scene loading
- player controller
- shooter controller
- weapons
- bullets
- ammo IDs
- zombie prefab
- zombie spawner
- combat systems
