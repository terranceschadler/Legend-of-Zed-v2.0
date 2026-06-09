# v2.4 Park Block Polish

## Goal

Polish generated park blocks without changing the core seeded overworld generator.

## Added

- `Assets/LegendOfZed/Editor/ZedV24ParkBlockPolishSetup.cs`
- `Assets/LegendOfZed/ReadMeDocs/Zed_v2.4_ParkBlockPolish.md`

## Menus

Apply park polish:

`Legend of Zed/Setup/v2.4 Apply Park Block Polish`

Clear park polish:

`Legend of Zed/Setup/v2.4 Clear Park Block Polish`

Validate park polish:

`Legend of Zed/Setup/v2.4 Validate Park Block Polish`

## What it does

For each generated park block:

- creates a dedicated `Zed_ParkPolishRoot`
- adds clear cross paths
- scatters trees away from paths
- scatters bushes away from paths
- keeps content inside park block boundaries
- uses assigned Synty/project tree/bush/sidewalk prefabs when available
- uses simple fallback markers when art is missing

## Recommended order

1. Open `Zed_Overworld`.
2. Run `v2.3 Rebuild Overworld With Block Art`.
3. Run `v2.4 Apply Park Block Polish`.
4. Run `v2.4 Validate Park Block Polish`.

## Untouched

This pass does not alter:

- core seeded layout rules
- roads
- building lots
- player controller
- shooter controller
- weapons
- bullets
- ammo IDs
- zombie prefab
- zombie spawner
- combat systems
