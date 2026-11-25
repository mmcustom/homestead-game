# Saw Pit, Sawmill and Salt Barrel

Design doc, drafted by Claude on 2026-11-23 for Mike's review.

## Mike's decisions (2026-11-23)

- "we need a barrel or something to store any salt surplus": a **Salt Barrel** for Salt and dry goods ("Salt and dry goods").
- "a lumber mill or something to turn logs into boards or lumber": **both, in order** ("Both, in order"). First a **Saw Pit**, and later a **water-powered Sawmill** as an upgrade.
- Build order: the Saw Pit, then the Salt Barrel, then tanning slices T1 to T4 (Tanning_Hides_and_Furs.md).

Everything marked *proposed* is Claude's suggestion for Mike to change as he likes.

---

## 1. The Saw Pit (build first)

A frontier saw pit is a trench with a frame of logs over it, so a log can be laid across and sawn lengthwise into boards.

- **Build** (*proposed*): a staged site. Mark it out (free), dig the trench with the Shovel (20 blows), deliver 6 Logs and 4 Branches with R in any order, and hammer it up (30 blows). It needs no Lumber, since this is how you make Lumber. Footprint about 3 m by 1.5 m, and it needs dry ground out of hollows.
- **Use** (*proposed*):
  - R loads Logs, up to 6. The Hand Cart can carry them.
  - With the **Saw** equipped (the Trading Post sells it), hold click on the log over the pit. The screen shows the progress, as with the Hammer.
  - Each pair of Logs takes 40 strokes and makes 3 Lumber (16 kg of logs make 15 kg of boards). A single Log left over waits for a second one.
  - The Lumber stacks beside the pit, and E takes it to your pack (5 kg each).
- **Why it pays:** the Trading Post sells Lumber, so sawing your own Logs saves money on the Smokehouse (6 Lumber) and the Root Cellar (4 Lumber). Lumber also sells back, for less than it costs to buy.
- **Dismantle:** the Hammer takes it down for half its materials, with the Logs and Lumber in it.
- **Know-how:** "Sawing Lumber in a Saw Pit".
- **Art:** structure_sawpit (the trench, the log frame and a log on it, sawdust), and its build icons.

### As built (2026-11-23)

Built as proposed. SawPitSite (51) and SawPit (52) are appended to PileKind; the rules live in SawPitRules.cs.

- Site: Other tab, free to mark. Shovel 20 blows, then 6 Logs and 4 Branches with R (any order), then the Hammer 30 blows.
- It can't go in a hollow or on a tree, and keeps the shed setbacks.
- Loading: R loads Logs, up to 6.
- Sawing: the Wood Saw, hold click within 1.7 m. Every 40 strokes take 2 Logs and add 3 Lumber, with a banner.
- Taking: E takes the Lumber to your pack.
- Dismantle: the Hammer gives back half the site cost plus everything left in the pit.
- Know-how entry 18, "Sawing Lumber in a Saw Pit", and a journal happening when the site is marked.
- Art: structure_sawpit (4,686 tris) with build icons build_sawpit and build_sawpitsite.

## 2. The Sawmill (an upgrade on the Saw Pit)

### Mike's decisions (2026-11-24)

- **Placement:** on the creek bank. It must stand within about 4 m of the creek's water, but not in the channel itself. The water wheel turns in the creek.
- **Cost:** the big build, with Lumber and Iron. You use the Saw Pit to make the Lumber first.
- **Speed:** **2 Logs an hour**, holding **20 Logs**. Every 2 Logs make 3 Lumber, so a full load of 20 Logs makes 30 Lumber in 10 game hours. It keeps running while you are away or asleep.
- **Floods:** it **stops during a flash flood** (the wheel is pulled out of gear) and starts again by itself when the flood passes. No damage.
- The Saw Pit stays: it is still the cheap way to start.

### Plan (*proposed* where marked)

- **Site** (staged like the Workshop): Other tab, free to mark on the creek bank. Deliver **12 Logs, 10 Lumber, 2 Iron Bars, 4 Cordage and 6 Stone** with R, in any order, then **60 blows** with the Hammer. Footprint about 4 m by 3 m (*proposed*), the wheel on the creek side.
- **Placement check** (*proposed*): the nearest point of the creek's channel must be within 4 m of the mill's wheel side, and the mill's own footprint must stay out of the channel. Not in a hollow, and the shed setbacks apply. The prompt says why when a spot won't do ("Too far from the creek", "In the creek").
- **Use:** R loads Logs from the pack, up to 20. While it holds 2 or more Logs and the creek isn't in flood, each game hour saws **2 Logs into 3 Lumber**. E takes the Lumber to the pack. The prompt shows the Logs, the Lumber waiting, and "running", "waiting for Logs" or "stopped: flash flood".
- **Dismantle:** the Hammer takes it down for half its materials (*proposed*), with the Logs and Lumber in it.
- **Know-how:** a new entry, "The Sawmill" (*proposed*).
- **Art:** structure_sawmill (a timber-framed open shed, the up-and-down saw frame inside, a water wheel on the creek side), structure_sawmillsite, and the build icons.

### As built (2026-11-24)

Built as planned. SawmillSite (56) and Sawmill (57) are appended to PileKind; the rules live in SawmillRules.cs.

- Site: Other tab, free to mark. Deliver 12 Logs, 10 Lumber, 2 Iron Bars, 4 Cordage and 6 Stone with R (any order), then the Hammer 60 blows. Footprint 4 m by 3 m.
- Placement: the mill's middle must be at least 1 m from the creek's water ("Too close: ... out of the creek itself") and no more than 4 m from it ("Too far from the creek"). It has its own check ahead of the usual flooding check, so the creek bank is allowed. No trees within reach, and the shed setbacks apply.
- Loading: R loads Logs from the pack, up to 20.
- Sawing: each game hour, with 2 or more Logs and no flash flood, 2 Logs become 3 Lumber. A banner says when the Logs run out.
- Taking: E takes the Lumber to your pack.
- Floods: it stops during a flash flood and restarts by itself afterwards. No damage.
- Dismantle: the Hammer takes it down, giving back the Logs and Lumber in it.
- Know-how entry 23, "The Sawmill", and a journal happening when the site is marked.
- The water wheel: the mill turns its wheel to the wall nearest the creek (in quarter turns) and spins it while sawing (SawmillWheel).
- Art: structure_sawmill (2,476 tris, the wheel a separate part so it can turn) with build icons build_sawmill and build_sawmillsite.

## 3. The Salt Barrel

- **Build** (*proposed*): 3 Logs and 2 Cordage, one click with the Hammer, like the Storage Bin. It is a lidded wooden barrel about 0.9 m tall.
- **Holds** (*proposed*): Salt and other dry goods that don't spoil (Tanbark, Cordage, Sinew and the like), up to 100 kg. It doesn't take food, wood, Stone, water or Tools.
- **Why have one when the Storage Bin also takes dry goods?** (*proposed*) The barrel keeps its contents dry. Salt left in the Storage Bin or on the ground slowly clumps in wet weather. That's a later rule, only if Mike wants it.
- **Art:** structure_saltbarrel and its build icon.
