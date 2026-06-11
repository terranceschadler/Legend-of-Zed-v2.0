# v3.9k2 — Socket Inspector Direction Buttons

## Problem

`Local Away From Road` is confusing as a raw Vector3.

## Fix

Adds a custom inspector for `ZedRoadFrontageSocket`.

New buttons:

- `Buildings North`
- `Buildings South`
- `Buildings East`
- `Buildings West`
- `Enable This Socket`
- `Disable This Socket`

## How To Use

Open a road tile prefab asset.

Select one of the socket objects:

- `Frontage_North_DISABLED`
- `Frontage_South_DISABLED`
- `Frontage_East_DISABLED`
- `Frontage_West_DISABLED`

Click the button that points toward where buildings should appear.

Examples:

- If buildings should spawn above/north of the road edge, click `Buildings North`.
- If buildings should spawn below/south of the road edge, click `Buildings South`.
- If buildings should spawn to the right/east, click `Buildings East`.
- If buildings should spawn to the left/west, click `Buildings West`.

Then click `Enable This Socket` only if that side is valid frontage.

## Important

The direction means:

`away from the street toward building placement`

It does not mean the direction the road travels.
