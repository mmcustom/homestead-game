# Tree Regrowth (design, 2026-10-31, approved, not built yet)

Mike's direction: trees must regrow so wood is always available. Decided: a felled tree regrows to full size in about one game year (112 days); a stump left standing regrows; a stump dug out with the Shovel clears the spot for good; natural regrowth only, no planting for now.

## Code facts (Claude Code read-only report)

- Trees are terrain instances, not objects. PropertyTerrainBuilder bakes 8,852 TreeInstances into PropertyTerrainData.asset (prototype 0 broad hardwood, 1 tall hardwood, 2 shrub). At World load a copy is made and WoodManager.originalTrees holds the pristine list; the asset is never touched. A tree's id is its index in that list.
- Felling appends FelledTree{index, stump} and calls RebuildTrees(), which rebuilds the list from originalTrees minus the felled set, calls SetTreeInstances and toggles the TerrainCollider off and on (the collider only sees tree changes after a rebuild). The API call is about 0.2 ms for 8,852 instances; the collider toggle is the real cost and already runs on every felling.
- Hardwoods leave a stump (a cylinder with a Stump component, not saved, respawned on load from stump == true); shrubs leave none. Only the Shovel digs stumps (6 blows, AxeTool.DigStump, RemoveStump sets stump = false). A hardwood entry with stump = false already means dug out for good. The Pick Axe only breaks rock outcrops and stays that way.
- Time: TimeManager stores totalDays and minuteOfDay, fires HourChanged, DayChanged, SeasonChanged, YearChanged for every boundary crossed (skips and sleep too). One game year is 112 days. There is no catch-up for time away. GroundWoodManager already does day-stamped regrowth ({id, takenDay}); regrowth copies that pattern.
- Trees within range of the cabin site (18, -68): within 50 m 31 trees (67 Logs), 100 m 312 trees (686 Logs), 150 m 855 (1,903 Logs), 250 m 4,962 (11,181 Logs), whole map 8,852 trees (20,019 Logs). At 14 Logs a day (the market ceiling) the 686 Logs within 100 m last about 7 weeks, and the whole map about 13 game years (the plan's "about 4 years" was a slip). So the real cost today is walking time as the near trees thin out.
- Sustainable yield with a 112-day cycle: about 6 Logs a day within 100 m, 17 within 150 m, 100 within 250 m.

## Model

Each felled spot has one saved stamp (fellStamp = day felled + 1; 0 = unset, additive field on FelledTree, no save version bump) and today's date gives its state: Cut (before day 14, no tree, stump if hardwood), Growing (day 14 to 111, a scaled tree in the terrain list), Full (day 112, the entry is dropped and the original tree is back), Cleared (hardwood with stump = false, permanent). Growth is stepped every 7 days, g = 0.08 at sprout rising linearly to 1.0 at day 112; height and width are both scaled by s = g^(2/3) so tree size (height x sqrt(width)) equals g times full size, which makes blows and Logs proportional to size. originalTrees is never changed. Stateless daily tick with a dirty flag, one rebuild in the next Update, so a multi-day skip rebuilds once.

Average tree (size 1.12), Axe / Saw blows and Logs: day 14 g 0.08: 1 / 1, 0 Logs; day 35: 3 / 2 broad (1 Log), 4 / 3 tall (1 Log); day 56: 5 / 3 broad, 7 / 4 tall; day 77: 8 / 5 broad, 10 / 6 tall (2 Logs); day 98: 10 / 6 broad, 13 / 8 tall; day 112: 11 / 7 broad (2 Logs), 15 / 9 tall (3 Logs). Logs lose their forced minimum of 1 (Branches and Sticks keep it), so cutting saplings is no faster per Log than cutting full trees.

Stump rules: a left stump stays until g >= 0.5 (about day 60), then the growing tree swallows it and the object is destroyed. The Shovel can dig a stump and its sprout before that point and the spot is cleared for good. After that the tree must be felled again (new stump, new 112-day clock) and then dug. Felling a sapling destroys any old stump first.

Obstruction (checked at the sprout moment, waits and retries on the next daily tick): a built structure within 1.5 m, a Cabin or Cabin Site within 3.2 m, the Covered Kitchen roof radius plus 3 m, a campfire within 2 m, the player within 1.5 m, over water, or inside a registered cleared-ground zone (a small hook for future farming, no farming rules now).

Old saves: existing felled entries load with stamp 0 and take stamp = first-tick day + 1, so existing stumps start their 112 days on first load (Mike's saves: autosave 24 felled with 3 stumps, slot1 25 with 13, slot2 42 with 19, slot4 24 with 3). Dug-out hardwoods never regrow; shrubs do.

Other rules: shrubs regrow on the same schedule; no season gating; a regrowing maple is not a Maple (not tappable) until full height; Windthrow picks only trees with g >= 0.75; the falling-tree animation uses the scaled instance. Dev key Ctrl+F5 regrows every felled tree to full size now (the F5 skip-a-day handler must ignore F5 with Ctrl held).

Wood Saw and Hand Cart are unaffected (they read live HeightScale, WidthScale and Prototype, and FindTree).

## Economy

Income is unchanged: the market ceiling of about 14 Logs a day and its floor price do not move. Regrowth renews supply and removes the walking penalty of depleted near trees. Honest limit: it will not keep the near-cabin forest full if the player cuts more than about 6 Logs a day within 100 m; the far trees (and the Hand Cart) carry the rest. Re-measure: Logs sold a day, average minutes per Log including walking over year 2, Log stock within 100 m of the cabin over time, and the "Firewood, logs and stone" income line against the $20.48 a day payment.

## Build plan (one build)

New Economy/RegrowRules.cs (about 110 lines, pure: GrowthFraction, Stage, ScaleOf, CanRegrow, StampFor, BlockRadius, IsBlocked, WindthrowEligible, LogsFor); WoodManager.cs about +130; TimeDebugControls.cs about +25; RegrowTests.cs about 250 (35 to 40 cases); MapleMarkerHud.cs about +3 (TreeRevision counter). Risks to check in Play Mode: the collider rebuild hitch on the daily tick, whether the Shovel can hit a stump under a sapling collider, stale wildlife habitat grid.

## BUILT: Tree Regrowth (2026-10-31, playtest 1 to 17 passed, committed 79eedb3)

Built as one build exactly to the plan, with these facts and departures. New Economy/RegrowRules.cs (pure: calendar, growth curve, scale, obstruction numbers, Windthrow and stump thresholds, LogsFor); WoodManager.cs (+250/-24): FelledTree.fellStamp (the only added saved field; save version stays 1), a daily tick that sets a flag and one rebuild in the next Update, live HeightScale/WidthScale so blows, Logs, trunk, height and Maple identity follow growth, sapling felling at its own size with a new year started on the same entry, stump swallowed at g 0.5 (stage 8, day 63), Windthrow only from g 0.75 (stage 12), AddClearedGround/RemoveClearedGround (not saved, no farming rules), TreeRevision counter (MapleMarkerHud rebuilds its list when it changes); TimeDebugControls.cs: Ctrl+F5 regrows every felled tree that can grow back now (plain F5 ignores Ctrl). Tests: 51 new cases, suite 1183.

Departure: a sprout that is blocked holds its clock at day 13 (RegrowRules.HeldStamp), so its year starts when the spot is clear instead of appearing nearly full the moment it clears. Last growing step is stage 14 at day 105 (growth 0.934); the full tree returns at day 112. DebugRegrowAll is not compiled out of release builds (TimeDebugControls itself is dev-only).

Measured in play (dev [Trees] log): every tree rebuild takes about 24 to 50 ms (loads 24 to 33, fellings 26 to 36, growth ticks 31 to 49 ms, max 49.0 ms), flat whatever the number of spots changed, because the cost is the fixed collider rebuild a felling already had. Console 0 errors. Old saves: slot2 and the autosave stamped all 20 stumps on first load; slot1 and slot4 unchanged until loaded; dug-out hardwoods carry no stamp and never regrow. Not yet checked: the wildlife habitat grid still reads the tree list only once (stale as it always was after fellings), the economy re-measures (Logs sold a day, minutes per Log in year 2, Log stock within 100 m, "Firewood, logs and stone" against the $20.48 payment).
