# v3.9i22 — Endpoint Filler Pass

## Problem

v3.9i21 created endpoint extension strips:

`FrontageEndpointExtensionsAdded=120`

but very few buildings appeared in the gaps because those endpoint strips were processed like normal frontage and rejected by duplicate/overlap rules.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## What Changed

- Endpoint extension strips are skipped during the normal frontage pass.
- A dedicated endpoint filler pass runs after normal/gap-fill placement.
- Endpoint filler uses smallest prefabs first.
- Endpoint filler allows only 1–2 buildings per endpoint strip.
- Endpoint filler can relax same-frontage duplicate rejection.
- Final road/overlap/boundary checks still protect placement.

## Expected Log

`V3.9I22 ENDPOINT FILLER PASS complete...`

Watch:

- `FrontageEndpointExtensionsAdded`
- `EndpointFillerPasses`
- `EndpointFillerBuildings`
- `Buildings`
- `RoadRejects`
- `OverlapRejects`

## Goal

Turn the endpoint extension strips into actual small filler buildings at 3-way/corner frontage gaps.
