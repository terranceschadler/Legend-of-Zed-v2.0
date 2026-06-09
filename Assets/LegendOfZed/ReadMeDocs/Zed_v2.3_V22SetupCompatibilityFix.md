# v2.3 v2.2 Setup Compatibility Fix

## Problem

v2.3 renamed the overworld manager visual fallback setting from:

`CreateDebugBlockVisuals`

to:

`CreateDebugFallbackVisuals`

The older v2.2 setup script still referenced the old field, causing:

`CS1061: ZedOverworldGenerationManager does not contain a definition for CreateDebugBlockVisuals`

## Fix

This package replaces:

`Assets/LegendOfZed/Editor/ZedV22SeededOverworldFoundationSetup.cs`

with a compatible version that uses the v2.3 manager fields.

It also switches obsolete `FindFirstObjectByType` editor calls to `FindAnyObjectByType` in this setup script.
