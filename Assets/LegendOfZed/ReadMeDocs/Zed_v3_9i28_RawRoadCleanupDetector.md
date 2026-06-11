# v3.9i28 — Raw Road Cleanup Detector

## Problem

v3.9i27 removed some road-overlapping buildings, but red-circle buildings still survived.

The likely reason is that final cleanup was using the same filtered road rects used for frontage placement. Some actual road meshes live under parent groups named like `Road_CENTER-lines`, so frontage detection can reject them while cleanup still needs them.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

- Adds a separate raw road cleanup rect list.
- Raw cleanup rect detection looks at the renderer's own name first.
- Actual `SM_Env_Road_*` meshes are accepted even if the parent contains `center-lines`.
- Final overlap cleanup checks buildings against raw cleanup road rects first.
- Falls back to normal road rects if raw cleanup rects are empty.

## Expected Log

`V3.9I28 RAW ROAD CLEANUP DETECTOR complete...`

Watch:

- `FinalRoadOverlapCleanupRemoved`
- `RawRoadCleanupRects`

## Goal

Remove remaining generated buildings that survive in road surfaces because the filtered frontage road list missed the road mesh.
