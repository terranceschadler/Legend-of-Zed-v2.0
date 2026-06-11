# v3.9k2.1 — Socket Editor Namespace Compile Fix

## Problem

`ZedRoadFrontageSocketEditor.cs` failed compile:

`CS0118: 'Editor' is a namespace but is used like a type`

## Fix

The custom inspector now explicitly inherits from:

`UnityEditor.Editor`

and uses:

`UnityEditor.CustomEditor`

## Patched File

`Assets/LegendOfZed/Editor/ZedRoadFrontageSocketEditor.cs`
