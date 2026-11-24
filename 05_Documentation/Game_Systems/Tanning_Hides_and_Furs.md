# Tanning Hides and Furs

Design doc, drafted by Claude on 2026-11-23 for Mike's review. Large text and plain language throughout, as for every system.

## Mike's decisions (2026-11-23)

1. **Method: both.** Brain tanning gives soft Buckskin and Tanned Furs in a few game days. Bark tanning gives stiff Leather over weeks.
2. **Use: sell and craft.** Tanned goods sell for well over the raw hide, and later recipes use them.
3. **Raw hides spoil, slowly.** A fresh hide goes bad in about a week unless it is salted or tanned.
4. **Structure: a Hide Frame.** It is a one-click pole frame built with the Hammer, like the Drying Rack. The smoking step uses the Smokehouse.

Everything below marked *proposed* is Claude's suggestion for Mike to change as he likes.

---

## 1. What the game has today

- **Deer Hide**, 3 kg, from dressing a deer (the Carcass). It sells for $8.
- **Small Furs**, 0.2 kg, from rabbits, squirrels and the traps. They sell for $3.
- **Sinew** from deer, which already makes Cordage.
- The Pouch is crafted from 1 Deer Hide or 4 Small Furs, plus 2 Cordage.
- Salt, the Crock Pot, the Drying Rack's daylight drying and the Smokehouse all exist now, and tanning reuses them.

## 2. Raw hides spoil (slice T1)

- Deer Hide and Small Furs get a shelf life on the outdoor freshness clock (*proposed*: 7 days for a Deer Hide, 5 for Small Furs). Like food, they age faster in heat and slower in the cold.
- A spoiled hide becomes Rot, which the Compost Bin takes.
- **Salting a hide** (*proposed*): with Salt in the pack, a raw hide gets a Salt button in the Inventory, the same way raw meat does. A Deer Hide takes 2 Salt and Small Furs take 1. The result is a Salted Deer Hide or Salted Furs that keeps for about a year (*proposed*: 360 days). Salted hides tan exactly like fresh ones; the frame rinses the salt out first.
- The Food Cache and the Root Cellar do not take hides; the Storage Bin does, as now.
- Old saves: hides already carried are stamped half-fresh, as food was in slice 1.

### As built (slice T1, 2026-11-23)

Built with the proposed numbers (Mike: "yes, commit, then proceed"). The rules live in HideRules.cs.

- Deer Hide keeps 7 days and Small Furs 5, on the outdoor freshness clock. The Inventory and storage screens show the time left, as for food. A spoiled hide turns to Rot.
- Salting needs only the Salt, no Crock Pot (the salt is rubbed into the flesh side). The Salt button shows on any raw hide row in the Inventory.
- New items: Salted Deer Hide (3 kg) and Salted Furs (0.2 kg), each keeping 360 days. The Trading Post buys them for $8 and $3, in the Hides group.
- Old saves: hides carried, stored or dropped are stamped half-fresh the first time the save loads (SpoilageSaveData.hidesStamped).
- Know-how entry 19, "Hides Spoil: Salt or Tan Them", unlocks when any raw or salted hide is in the pack.
- Hides are not food, so the Food Cache and the Root Cellar still refuse them; the Storage Bin and the Salt Barrel take them.
- Art: item_salted_deer_hide and its icon; item_salted_furs comes next.

## 3. The Hide Frame (slice T2): scraping and rawhide

- **Build** (*proposed*): 4 Sticks, 2 Branches and 2 Cordage, one click with the Hammer (like the Drying Rack). It is a square pole frame about 2 m tall, with a hide laced into it.
- **Load:** R laces one hide into the frame: one Deer Hide, or up to 4 Small Furs as one bundle. Salted hides work too.
- **Scrape:** with the Knife equipped, hold click on the hide. Each stroke scrapes a little (*proposed*: 20 strokes for a Deer Hide, 8 for Furs). The screen shows the progress, like the Hammer on a building site. Scraping takes the flesh and fat off, and on a Deer Hide the hair too. Furs keep their hair.
- **Dry:** a scraped hide dries on the frame in daylight while it is not raining (the Drying Rack's rule; *proposed*: 12 daylight hours). It doesn't age on the frame.
- **Rawhide:** a dried scraped Deer Hide is **Rawhide** (*proposed*: 2 kg, keeps for years, sells for $12). E takes it off the frame. Rawhide is stiff, and it is the starting point for both kinds of tanning. Dried scraped furs are **Dried Furs** (*proposed*: $4 each, $1 more than raw).

### As built (slice T2, 2026-11-23)

Built with the proposed numbers (Mike: "commit t1 and start t2"). The rules live in HideFrameRules.cs.

- Build: 4 Sticks, 2 Branches and 2 Cordage, one click with the Hammer, under **Other** in the build list.
- R laces in one hide: a Deer Hide before furs, raw before salted. Furs go on as a bundle of up to 4, raw and salted mixed. E takes an unscraped hide back off.
- The Knife scrapes it: each Knife blow aimed at the frame (within 1.8 m) is one stroke, with the same stamina as a blow. The HUD shows the percent. 20 strokes for a Deer Hide, 8 for furs.
- Then it dries 12 hours on the Drying Rack's rule (daylight, no rain or snow). A banner says when it is ready, and E takes it.
- A hide on the frame does not spoil there (the frame is left out of the spoilage sweep).
- New items: Rawhide (2 kg, never spoils, $12) and Dried Furs (0.2 kg, never spoil, $4 each), in the Hides group.
- Dismantling with the Hammer gives back half the materials, plus the hide or what it has become.
- Know-how entry 20, "The Hide Frame: Scraping and Rawhide", unlocks when a frame is built or Rawhide or Dried Furs are first held.
- Art: structure_hideframe and its build icon; item_rawhide and item_dried_furs come next.

## 4. Brain tanning (slice T3): Buckskin and Tanned Furs

- **Soften:** with Rawhide or Dried Furs laced in the frame, a Crock Pot in the pack and water in it (from the Canteen or a bucket), hold click with the Knife to work the hide. The brain-and-water mix soaks in as you stretch and work it until it is soft (*proposed*: 30 strokes for a Deer Hide, 10 for Furs). Each animal's brains are enough for its own hide, so no new item is needed (*proposed*).
- The worked hide is a **Soft Hide** or **Soft Furs**, white and floppy. Left like that it would go stiff again, so it needs smoking.
- **Smoke:** a Soft Hide goes into the Smokehouse with the meat. It takes up the room of 2 pieces of meat (Furs take 1), smokes for the same 24 hours and burns the same Firewood. It comes out as **Buckskin** (golden brown, soft, keeps for years) or **Tanned Furs**.
- **Value** (*proposed*): Buckskin $24 (against $8 raw, $12 as Rawhide). Tanned Furs $6 each (against $3 raw).

## 5. Bark tanning (slice T4): Leather

- **Tanbark** (*proposed*): with the Knife, strip bark from a Log lying on the ground (a felled log or one in a pile). Each Log gives 2 Tanbark and stays a Log. Tanbark is 1 kg and keeps for years. Oak bark is the classic; if oaks are added to the forest later, oak logs could give more.
- **The Tanning Vat** (*proposed*): a sunken log trough with a lid, 3 Logs and 2 Clay, one click with the Hammer. R loads Rawhide (up to 4), Tanbark (4 per hide) and water (20 L per hide, from a bucket or the Water Barrel).
- **Soak:** the hides soak for 14 game days (*proposed*), counted hour by hour like the Smokehouse, so being away or asleep counts. The vat needs no tending, but if its water runs low (*proposed*: in long dry heat) it stops until topped up.
- **Leather:** dark brown, stiff and strong, keeps for years. *Proposed* value: $30 a hide. Leather pays the most, but it takes two weeks and the bark.

## 6. Crafting with tanned goods (slice T5, later)

These are proposals only, to come once the goods exist:

- **Rawhide Lashing** from Rawhide: it binds tool heads and frames, and is stronger than Cordage.
- **Moccasins** from Buckskin and Sinew: less stamina lost walking.
- **Buckskin Coat** from Buckskin and Tanned Furs: warmer in cold weather.
- **Leather Pouch** from Leather and Sinew: a bigger Pouch.
- **Fur Hat and Mittens** from Tanned Furs: winter warmth.
- **Saddle, harness and belts** later, with horses and livestock.

## 7. The Trading Post (proposed)

| Item | Raw | Step 1 | Step 2 |
|---|---|---|---|
| Deer Hide | $8 raw, $8 salted | Rawhide $12 | Buckskin $24 (brain), Leather $30 (bark) |
| Small Furs | $3 raw, $3 salted | Dried Furs $4 | Tanned Furs $6 |

Each step pays more than the last, even after the price drops 5% with each one sold (a test will check it, as it does for meat). Tanbark is not bought. The Hides group's existing rule sets the seasonal prices.

## 8. The Journal

New Know-how entries, unlocked the first time each thing happens: **Hides Spoil: Salt or Tan Them**, **The Hide Frame: Scraping and Rawhide**, **Brain Tanning: Buckskin and Furs** and **Bark Tanning: Leather**.

## 9. Art needed

- **Structures:** the Hide Frame (empty, with a fresh hide, a scraped hide and rawhide) and the Tanning Vat (lid open and shut).
- **Items, each with its icon:** Salted Deer Hide, Salted Furs, Rawhide, Dried Furs, Soft Hide, Soft Furs, Buckskin, Tanned Furs, Leather and Tanbark.
- One piece at a time, each approved by Mike.

## 10. Build order (proposed)

1. **T1:** hides spoil, and salting hides.
2. **T2:** the Hide Frame, scraping, drying and Rawhide.
3. **T3:** brain tanning: softening on the frame, then smoking in the Smokehouse, for Buckskin and Tanned Furs.
4. **T4:** bark tanning: Tanbark, the Tanning Vat and Leather.
5. **T5:** crafting with tanned goods (later, its own decisions).

Each slice is played and approved by Mike before the next one starts, with its art made in Blender as part of the slice.

## 11. Open questions for Mike

1. The shelf lives (7 days for a hide, 5 for furs) and the salted life (about a year).
2. Whether the frame takes a bundle of 4 Small Furs, or one fur at a time.
3. Whether brain tanning should need a separate Brains item from dressing the deer, or (as proposed) need nothing extra.
4. The Tanning Vat: a separate one-click build as proposed, or the existing Water Barrel used as the vat.
5. The prices in section 7.
