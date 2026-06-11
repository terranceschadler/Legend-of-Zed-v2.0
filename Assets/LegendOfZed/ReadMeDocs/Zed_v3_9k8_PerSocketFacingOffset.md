# v3.9k8 — Per-Socket Facing Offset

## Problem

Some buildings still face the wrong way, especially on dead-end frontage. The socket position is correct, but the facing needs a per-socket override.

## New Socket Field

`buildingYawOffsetDegrees`

## Inspector Buttons

- `Facing OK / 0°`
- `Flip Facing 180°`
- `Rotate Facing +90°`
- `Rotate Facing -90°`

## How To Use

If a socket spawns buildings in the right place but facing backward:

1. Open the tile prefab asset.
2. Select that `Buildings_*` socket.
3. Click `Flip Facing 180°`.
4. Save the prefab.
5. Press Play again.

For dead ends, try `Flip Facing 180°` first.

## Runtime

Socket mode now rotates buildings from:

`faceRoad = -socket.awayFromRoad`

then applies the socket yaw offset.

## Expected Log

`V3.9K8 PER-SOCKET FACING OFFSET complete...`

Watch:

`SocketFacingOffsetApplied=...`
