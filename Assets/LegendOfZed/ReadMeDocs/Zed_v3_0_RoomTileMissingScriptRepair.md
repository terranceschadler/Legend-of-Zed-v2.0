# Zed v3.0 Room Tile Missing Script Repair

## Purpose

Repairs room tile prefabs after a bad script GUID caused `TileSpawn-*` objects to show:

`The referenced script (Unknown) on this Behaviour is missing!`

## What this does

Adds editor menu items:

- `Legend of Zed/Map Integration/Report Room Tile Missing Scripts`
- `Legend of Zed/Map Integration/Repair Room Tile Missing Scripts`

The repair tool:

1. Loads prefabs under:
   `Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles`

2. Removes missing MonoBehaviour components.

3. Ensures every object tagged `TileSpawn` has:
   `ZedLegacySpawnCollisionDetector`

4. Ensures TileSpawn colliders are triggers.

## What this does not do

- Does not change generator logic.
- Does not change dead-end rules.
- Does not delete any map art.
