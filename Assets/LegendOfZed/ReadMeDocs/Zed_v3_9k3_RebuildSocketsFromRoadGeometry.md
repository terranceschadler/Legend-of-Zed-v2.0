# v3.9k3 — Rebuild Sockets From Road Geometry

## Problem

The disabled placeholder sockets from v3.9k1 used the full prefab bounds, so they created giant invalid frontage lines.

## Fix

Adds a better menu:

`Legend of Zed / Map Integration / v3.9K / Rebuild Sockets From Road Geometry On Selected Prefab Assets`

This menu:

- works on selected prefab assets in the Project window
- clears old placeholder sockets under `Zed_ExplicitFrontageSockets`
- scans road renderers by road names/materials
- creates short road-edge sockets from each road renderer
- leaves all created sockets disabled until manually approved

## How To Use

1. Select the road tile prefab asset in the Project window.
2. Run `Rebuild Sockets From Road Geometry On Selected Prefab Assets`.
3. Open the prefab.
4. Delete/leave disabled any bad sockets.
5. For valid sides only:
   - select the socket
   - click Buildings North/South/East/West if needed
   - click Enable This Socket

## Enable Rule

Only enable a socket if:

- the cyan line sits on a valid sidewalk/building frontage edge
- the line is not across the road
- the line is not inside an intersection
- the line is not on a dead-end cap unless buildings really belong there

## Notes

The old `Add 4 Disabled Socket Placeholders` menu now only logs a warning and does not create full-prefab placeholder sockets.
