# v3.9k1 — Prefab Asset Socket Authoring

## Correction

Generated road tiles only exist during Play. So the first v3.9K instructions were wrong for authoring.

Correct workflow:

`Project prefab asset -> author sockets -> runtime generated tile inherits sockets`

## Patched File

`Assets/LegendOfZed/Editor/ZedExplicitTileFrontageSocketSetupMenu.cs`

## New / Renamed Menus

`Legend of Zed / Map Integration / v3.9K / Create Frontage Socket Holder On Selected Prefab Assets`

Use this on selected prefab assets in the Project window.

`Legend of Zed / Map Integration / v3.9K / Add 4 Disabled Socket Placeholders To Selected Prefab Assets`

Adds four disabled placeholder sockets to selected prefab assets. They are disabled on purpose. Enable and adjust only the sides that are valid for that tile.

`Legend of Zed / Map Integration / v3.9K / Create Frontage Socket Holder On Selected Scene Tiles`

This scene-object version is only for debugging already-generated scene instances.

## Correct Socket Authoring Workflow

1. In the Project window, select one or more map tile prefab assets.
2. Run `Create Frontage Socket Holder On Selected Prefab Assets`.
3. Optionally run `Add 4 Disabled Socket Placeholders To Selected Prefab Assets`.
4. Open the prefab asset.
5. Enable only valid sockets by setting `Can Spawn Buildings=true`.
6. Move each socket line to the exact frontage edge.
7. Set `Local Away From Road` to point away from the street toward where buildings should spawn.
8. Press Play and check the spawner log:

`UsingExplicitFrontageSockets=True`
`ExplicitFrontageSocketsFound=...`
`ExplicitFrontageStripsBuilt=...`

## Do Not

Do not author sockets on runtime-generated scene tiles unless you are only debugging. Those disappear when Play stops.
