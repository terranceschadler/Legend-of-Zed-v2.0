# v3.9i13 — Dead-End Frontage + Lane Marking Filter

## Problem

Dead ends still had bald spots, and 2-way straight tiles showed blue frontage/debug lines down the street center.

The spawner was still letting center-line/lane-marking renderers influence frontage detection, and dead-end road-only supplemental strips found roads but rejected all candidates.

## Fix

Patched:

`Assets/LegendOfZed/Scripts/MapIntegration/ZedFrontageStripBuildingSpawnerV39I.cs`

## Changes

- explicitly rejects road center-line/lane-marking renderers
- adds `RoadLineRendererRejects` diagnostic
- narrows dead-end supplemental strip probe depth
- allows dead-end supplemental probes to skip the broad road-overlap self-rejection
- adds `DeadEndRelaxedProbeAccepts` diagnostic

## Expected Log

`V3.9I13 DEAD-END FRONTAGE + LANE MARKING FILTER complete...`

Watch:

- `RoadLineRendererRejects`
- `DeadEndStripsAdded`
- `DeadEndRelaxedProbeAccepts`

## Goal

- no blue frontage line down the center of 2-way roads
- dead-end supplemental strips should actually add where valid
