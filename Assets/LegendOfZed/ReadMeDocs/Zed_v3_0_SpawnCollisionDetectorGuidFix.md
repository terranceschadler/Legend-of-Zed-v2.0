# Zed v3.0 Spawn Collision Detector GUID Fix

## Purpose

Fixes missing script errors on map tile prefab `TileSpawn-*` objects.

## Cause

A previous changed-files package replaced:

`Assets/LegendOfZed/MapGeneratorImport/Scripts/ZedLegacySpawnCollisionDetector.cs.meta`

with a new GUID.

The map tile prefabs already reference the original imported script GUID:

`274ed2ccca4c879499202d00e7e4abe0`

When the GUID changed, Unity treated existing `ZedLegacySpawnCollisionDetector` components as missing scripts.

## Fix

This package restores the `.meta` GUID for:

`ZedLegacySpawnCollisionDetector.cs`

No generator behavior is changed beyond the intended registry recording in the script body.
