# v3.10v1 — Signature Compile Fix

## Problem

Unity compile error:

`CS1503: Argument 2: cannot convert from 'UnityEngine.Vector3' to 'ZedAuthoredBuildingLotSpawner.LotData'`

## Cause

The new simple Z-forward helper still had a fallback call to the old front-edge scorer using the old method signature.

## Fix

Removed that stale fallback call.

The simple Z-forward orientation behavior remains active.

## Expected Log

`V3.10V1 SIGNATURE COMPILE FIX complete...`
