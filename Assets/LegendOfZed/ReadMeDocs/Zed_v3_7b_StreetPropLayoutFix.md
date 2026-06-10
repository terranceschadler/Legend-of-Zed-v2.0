# v3.7b — Street Prop Layout Fix

## Fixes

- Lamps now face the street.
- Props now validate against actual renderer bounds after spawning.
- Mailboxes, benches, lamps, trash cans, hydrants, and paper debris are rejected if their real bounds overlap road cells, generated buildings, or other generated props.
- Paper debris density is increased.
- Dumpster/trash-pile support added.
- Dumpsters are placed against building sides/backs using the nearest road direction, not along the street frontage.
- Trash piles are scattered around accepted dumpsters.

## Menu

`Legend of Zed / Map Integration / Add Road Frontage Street Prop Spawner`

The menu now also searches for:

- dumpster / skip prefabs
- trash pile / garbage / trash heap / debris / paper prefabs

## Notes

This patch does not touch the road-frontage building spawner or boundary wall builder.
