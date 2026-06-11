# v3.9i31 — Collider Road Frontage

## Goal

Try collider-based road frontage generation.

Renderer-based road detection is too noisy because road surfaces, lane markings, center lines, sidewalks, and parent objects are fragmented. Road BoxColliders should provide cleaner rectangles.

## Patched File

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

- Adds collider-based road rectangle detection.
- Prefers BoxCollider road bounds for frontage generation.
- Falls back to renderer road rectangles if no valid road colliders are found.
- Keeps raw road cleanup active.
- Keeps authored spawn fallback disabled.
- Keeps bad tile-edge frontage disabled.

## Expected Log

`V3.9I31 COLLIDER ROAD FRONTAGE complete...`

Watch these values:

- `RoadColliders`
- `RoadColliderRects`
- `RendererRoadRectsForFallback`
- `UsingColliderRoadRects`

## Interpretation

- `UsingColliderRoadRects=True` means frontage lines are built from colliders.
- `UsingColliderRoadRects=False` means no usable road colliders were found and it fell back to renderer road rects.
