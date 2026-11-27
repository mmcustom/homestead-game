# Greenhouses

Design doc, drafted by Claude on 2026-11-25; sizes, plastic wear, snow and order approved by Mike the same day (section 8). Nothing here is built. It comes before the gardening design, because greenhouse beds are garden beds under cover.

Everything marked *proposed* is Claude's suggestion for Mike to change as he likes.

---

## Mike's decisions (2026-11-25)

- **Three types, each in three sizes:** Cold Frame, Hoop House and Glass Greenhouse, each Small, Medium and Large.
- **Venting:** manual at first. An upgrade later keeps a set temperature, chosen for the crop inside.
- **Heat:** the larger units can have a heat source added to hold the temperature up.
- **Watering:** cheap and early versions are watered by hand. Upgrades water automatically when the setup allows it: a Water Barrel, a hose from the Trading Post, hand pumps, electric pumps, fuel pumps and so on.

---

## 1. Why grow under cover (the real-world case)

Research notes, 2026-11-25 (sources at the end):

- **It moves the garden south.** An unheated single-layer greenhouse feels about 1.5 USDA zones warmer (Eliot Coleman, Maine). A row cover laid over the plants inside adds about the same again. On a −8 °F night Coleman measured 2 °F inside one layer and 20 °F under the inner row cover.
- **A longer season:** about a month on each end of the outdoor season for an unheated tunnel; Cornell aims for mid-April to mid-November in New York.
- **Winter food** from cold-hardy crops (kale, spinach, mustard, arugula, winter lettuce, carrots left in the ground). Very little grows from December to February, when the days are shortest: plants mostly hold rather than grow.
- **Earlier and better crops:** tunnel tomatoes come 3 to 4 weeks earlier than field tomatoes, larger and more marketable, with less early blight.
- **Winter produce sells high** (Ohio State: high winter prices for fresh local produce are why growers try for it).

The catches:

- **Overheating.** A closed tunnel passed 120 °F on a sunny March day in Utah. Tomatoes drop their flowers above about 88 to 90 °F by day or 70 °F at night. Greens want 60 to 65 °F and stress above 75 °F.
- **No rain falls inside**, so beds must be watered.
- **Cost and size:** a field is cheap and big, but has one season and takes all the weather.

---

## 2. What a greenhouse does in the game (*proposed*)

Each greenhouse has its own **inside temperature**, worked out hourly from the outside air the weather system already has:

- **Sun gain:** on a sunny day with the vents shut, the inside climbs well above the outside (more for glass and plastic over a large volume, less under cloud). This is what cooks crops if nobody vents.
- **Night lift:** at night the inside stays warmer than the outside (*proposed*: about +10 °F for one layer). A **Row Cover** (bought, laid over a bed) adds about +10 °F more to that bed.
- **Venting** lets heat out (a lid propped, sides rolled up, doors and roof vents open).
- **Heat** (where fitted) adds warmth when the inside drops below the setting.

What it gives the crops:

- **Frost protection:** a frost that kills outdoor crops may not reach inside.
- **Season extension:** planting earlier and harvesting later, and cold-hardy crops through winter (slow growth in the dark months).
- **Shelter:** no hail, flood or storm damage, and fewer crop diseases (a quality bonus at sale).
- **Seedlings:** start plants early under cover and move them to the outdoor garden for a head start.
- **The risk:** if the inside goes past a crop's maximum (Heat Waves, a sunny spring day with the lid shut), the crop is stressed, then loses yield, then dies. If it drops below the crop's minimum, the same.

Crop temperature ranges come from the extension tables (tomato best 70 to 75 °F, maximum about 85 to 90 °F; leafy greens best 60 to 65 °F, maximum 75 °F, minimum about 55 °F to keep growing, though hardy greens survive frost). The full crop list belongs to the gardening design.

---

## 3. The nine builds (*proposed* sizes and beds)

A **bed square** is 4 ft by 4 ft (1.2 m by 1.2 m) of growing space, the unit the gardening design will use. Bed counts allow for paths inside walk-in houses.

| Type | Size | Footprint | Bed squares | Walk-in | Build |
|---|---|---|---|---|---|
| Cold Frame | Small | 4 × 4 ft (1.2 × 1.2 m), 2 window sashes | 1 | No, reach in | One click with the Hammer |
| Cold Frame | Medium | 4 × 8 ft (1.2 × 2.4 m), 4 sashes | 2 | No | One click |
| Cold Frame | Large | 4 × 12 ft (1.2 × 3.6 m), 6 sashes | 3 | No | One click |
| Hoop House | Small | 10 × 20 ft (3 × 6 m) | 8 | Yes, stooping at the sides | Staged site |
| Hoop House | Medium | 14 × 30 ft (4.3 × 9 m) | 16 | Yes | Staged site |
| Hoop House | Large | 20 × 48 ft (6 × 14.6 m) | 36 | Yes | Staged site |
| Glass Greenhouse | Small | 8 × 12 ft (2.4 × 3.6 m) | 4, plus a seedling bench | Yes | Staged site |
| Glass Greenhouse | Medium | 12 × 20 ft (3.6 × 6 m) | 9, plus benches | Yes | Staged site |
| Glass Greenhouse | Large | 20 × 40 ft (6 × 12 m) | 30, plus benches | Yes | Staged site |

Why three types and not just three sizes:

- **Cold Frame:** the cheapest, early game. A wooden box with old window sashes for a lid, knee high, 4 ft deep so you can reach the back. Great for greens and for **seedlings**. Small volume, so it heats and cools fastest: it cooks quickly with the lid shut. Materials (*proposed*): Lumber and Window Sashes (Trading Post), small numbers.
- **Hoop House:** mid game, the most growing space for the money. Bent hoops covered with greenhouse plastic sheeting (Trading Post). **The plastic wears out** (*proposed*: replace every 4 game years, a new sheet from the Trading Post) and a heavy snow must be swept off or it can tear. Materials (*proposed*): Lumber for the end walls and base, hoops (Iron Bars from the Smithy, or bought steel hoops), Plastic Sheeting, Cordage.
- **Glass Greenhouse:** late game, the best and dearest. A timber frame on a stone foundation with glass panes. Lasts for good, holds heat best with a heater, has room for seedling benches. Materials (*proposed*): Lumber, Stone, Glass Panes (Trading Post), Iron Bars for fittings.

---

## 4. Venting: manual first, automatic later

| Level | How it works | Fits |
|---|---|---|
| Manual (built in) | The player opens and closes: the Cold Frame lid (propped), the Hoop House roll-up sides and doors, the Glass Greenhouse doors and roof vents. Use key on the vent. The prompt shows the inside temperature. | All nine |
| Automatic Vent Opener (*proposed* upgrade, no power) | A wax-cylinder opener (a real, unpowered device) that opens the vent as it warms. Opens at one fixed temperature; no choice of setting. | Cold Frames and Glass Greenhouses |
| Thermostat Controller (*proposed* upgrade) | Keeps a chosen temperature: the player sets it for the crop (a dial in the prompt). Opens and closes vents, and in the Hoop House and Glass Greenhouse runs an exhaust fan. The fan needs power (waits on Power_System; a hand-cranked or wind-up version could come first). | Hoop Houses and Glass Greenhouses |

The Weather Station already gives readings; a cheap **Thermometer** hung inside (*proposed*) would show the inside temperature without opening anything.

---

## 5. Heat (the larger units)

| Heat source | Fuel | Fits (*proposed*) | Notes |
|---|---|---|---|
| Hotbed | Compost or fresh manure dug in under the bed | Cold Frames | The 1800s way: rotting compost warms the soil for weeks. Uses the Compost Bin's output. |
| Water barrels (thermal mass) | None | Hoop Houses, Glass Greenhouses | Barrels of water soak up the day's heat and give it back at night: a small night lift, no fuel. |
| Wood Stove | Firewood, fed by hand like the cabin hearth | Medium and Large Hoop Houses, all Glass Greenhouses | Holds the inside up on cold nights while it burns. Ties into the Firewood economy. |
| Propane Heater | Propane bought at the Trading Post | Medium and Large Hoop Houses, Glass Greenhouses | Thermostat controlled; a running cost. |
| Electric Heater | Power | Glass Greenhouses | Waits on Power_System. |

With enough heat, the Glass Greenhouse is the only place that can grow warm crops (tomatoes, peppers) in winter. It costs fuel every cold night, which is the trade-off against the winter selling price.

---

## 6. Watering: by hand first, automatic later

| Level | How it works | Needs |
|---|---|---|
| By hand (built in) | Carry water in a bucket (or a Watering Can, *proposed* Trading Post item) and water each bed. Dry beds stress and then lose yield. | A water source |
| Hose from a barrel | A Garden Hose (Trading Post) linked from a Water Barrel or Rain Barrel within reach. Use key waters every bed in one go, drawing the barrel down. | A barrel with water in it |
| Hand Pump | Pumps from the creek, a pond or the Well into the hose. Work the pump to water. | A water source nearby |
| Drip lines and a timer | Waters automatically each morning while the source has water. | A hose source, plus a pump below |
| Fuel Pump | Pumps automatically from creek or pond; burns gasoline (Trading Post). | Fuel |
| Electric Pump | Pumps automatically. | Power_System |

Automatic watering only runs while its criteria are met: water in the barrel or source, fuel in the pump, power on. When it fails, the prompt says why ("barrel empty", "out of fuel").

---

## 7. Build order (*proposed*)

1. **Gardening basics first** (beds, seeds, crops, watering, frost), outdoors. A greenhouse is a covered garden bed, so it needs the crop rules.
2. **G1: Cold Frames** (three sizes), manual lid, hand watering, the inside temperature model, Row Cover.
3. **G2: Hoop Houses** (three sizes), roll-up sides, plastic wear, snow load.
4. **G3: Glass Greenhouses** (three sizes), seedling benches.
5. **G4: Heat:** Hotbed, water barrels, Wood Stove, Propane Heater.
6. **G5: Watering upgrades:** hose, hand pump, drip and timer, fuel pump.
7. **G6: Automatic venting:** the wax openers, then the Thermostat Controller.
8. **Later, with Power_System:** fans, electric heat, electric pumps.

Each slice is played and approved by Mike before the next one starts, with its art made in Blender as part of the slice.

---

## 8. Mike's answers (2026-11-25)

1. **The nine sizes and bed squares** in section 3: approved as proposed.
2. **Hoop House plastic wears out:** yes, replaced every 4 game years with a new sheet from the Trading Post.
3. **Snow on the Hoop House:** yes, a chore. Heavy snow must be swept off or the plastic tears.
4. **Order:** gardening basics first (outdoors), then the greenhouse slices G1 to G6 in section 7.

## 9. Mike's answers for slice G1, Cold Frames (2026-11-26)

1. **A Cold Frame is its own build with soil inside.** Small holds 1 bed square, Medium 2, Large 3. Each square inside is planted, watered and harvested like a garden bed, under the lid.
2. **Cost:** old Window Sashes bought at the Trading Post (about $8 each, 2 per bed square) plus 2 Lumber per square for the box.
3. **Overheating stresses, then harms:** past the crop's heat limit it stops growing, then takes damage over a few hours. A whole sunny day with the lid shut can kill tender greens.
4. **Warnings:** yes, banners. "A cold frame is overheating - prop the lid open", and in the evening "Frost tonight - close the cold frame lids".

## 10. As built: G1 Cold Frames (2026-11-26)

- **Build:** Small, Medium and Large Cold Frames on the Build tab (under Other), placed with the Hammer like the fences; the front (the low side) faces where the player stands. Cost per bed square: 2 Window Sashes and 2 Lumber. Building one makes its bed squares inside it at once: Garden Beds already dug, in a row along the frame, marked as in a frame.
- **Window Sash:** a new Trading Post item, $8, 3.5 kg, sells back like other stocked goods.
- **The lid:** E on the frame's front board props the lid open or shuts it; the sashes swing up 60 degrees. The frame's header shows the lid and the inside and outside temperatures; each bed inside shows "in a cold frame: lid shut, 9 C inside".
- **Inside temperature (each game hour):** lid shut, the air plus 5.5 C (about 10 F) plus up to 25 C in full midday sun; lid open, plus 1.5 C plus up to 4 C of sun. The sun follows the time of day, the season's midday sun height and the cloud. Crops inside grow, take frost and take heat at that temperature, and the frost alert counts the frame.
- **Rain:** none reaches the beds with the lid shut, half with it open, so framed beds need watering.
- **Heat:** past a crop's heat limit it stalls; more than 4 C past it, it is harmed every hour (0.03 plus 0.02 per degree further). A whole sunny day shut can kill tender greens.
- **What fits:** lettuce, kale, carrots and beans. Taller crops are not offered with the Hoe in a framed bed.
- **Raiders:** deer and rabbits cannot get into a frame.
- **Banners:** "A cold frame is overheating (41 C inside) - prop the lid open (E on its front)" at most every 3 game hours per frame; in the evening before frost, "Frost tonight (about -3 C) - close the cold frame lids (E on the front)" when a frame is open; and a banner when heat kills a crop.
- **Taking it down:** R with the Hammer, half the materials back; the beds inside must be empty first ("Harvest or clear the beds in the cold frame first"). The Shovel and the Hammer do nothing to the framed beds themselves.
- **Art:** structure_coldframesmall, structure_coldframemedium, structure_coldframelarge (the sashes are hinged children named Sash_0 and on, swung open in game) and item_window_sash, with build and item icons.
- **Code:** Economy/ColdFrameRules.cs (every number), World/ColdFrameVisuals.cs, WoodManager (kinds 64 to 66, appended), GardenBeds, PlacedStructure; tests in GreenhouseG1Tests.

## 11. Mike's answers for slice G2, Hoop Houses (2026-11-26)

1. **A staged site:** placed free, the materials delivered over trips, then finished by pounding with the Hammer, like the Cabin and the Kitchen.
2. **Hoops:** bought as Steel Hoops at the Trading Post, or forged from Iron Bars at the Smithy as the cheaper homemade route.
3. **Beds made with the house:** the bed squares come with it, dug and laid out with paths (8, 16 or 36). Walk in and plant.
4. **Snow:** a banner warns of snow load; E at the house sweeps it off (an hour of work). Left too long, the plastic tears and the house gives no cover until a new sheet is fitted.

## 12. As built: G2 Hoop Houses (2026-11-26)

- **Sizes:** Small 10 x 20 ft (3 x 6 m, 2.1 m high, 8 squares in 2 rows), Medium 14 x 32 ft (4.3 x 9.8 m, 2.5 m high, 16 squares in 2 rows) and Large 20 x 50 ft (6 x 15.4 m, 3 m high, 36 squares in 3 rows). Medium and Large are 2 ft longer than first proposed so the approved bed squares fit with a way in; the Large house keeps a cross aisle inside the door because its middle row lines up with the door.
- **Build:** a staged site on the Build tab (under Other), marked out free and turned with the wheel so the door faces you. Deliver with R, in any order: Small 8 Lumber, 6 Greenhouse Hoops, 1 Greenhouse Plastic, 2 Cordage; Medium 12, 9, 2, 3; Large 20, 13, 4, 4. Then hold click with the Hammer (40, 60 or 90 blows). The site needs dry, fairly level ground (no more than 1 m of rise across it) with no trees, and nothing else may stand on it.
- **Hoops:** Greenhouse Hoops are $12 at the Trading Post, or forged at the Smithy from 1 Iron Bar and 1 Charcoal in 2 hours. The Smithy list now has seven rows, so its rows are a little shorter (76 px with 6 px gaps; the list takes the same height as before).
- **Plastic:** Greenhouse Plastic is $20 a sheet at the Trading Post.
- **Beds:** finishing the house lays out its bed squares inside, dug and ready, with paths between the rows. Any crop grows in them.
- **Walls:** the end walls and long sides are solid; walk in through the door. Aiming at any wall shows the house's prompt.
- **E at the door:** rolls the sides up or down. With 2 cm or more of snow on it, E sweeps it off instead (the clock moves on an hour). With torn plastic, E fits new sheets from the pack (1, 2 or 4).
- **Inside temperature:** sides down, the air plus 5.5 C at night and up to 22 C more in full midday sun; sides up, plus 1 C and up to 3 C. No rain falls on the beds either way, so water them. Overheating and the frost alert work as in the Cold Frames, with their own banners ("roll the sides up", "roll down the sides").
- **Snow:** 2 cm builds up for each hour it snows and melts at 0.5 cm per degree above freezing each hour. A banner warns at 15 cm. At 30 cm the plastic tears.
- **Wear:** the plastic wears out 4 game years after it was fitted (a banner says so). The header shows the years left.
- **Torn or worn out:** the house gives no cover at all (the beds are as outdoors, rain and raiders included) until new plastic is fitted.
- **Taking it down:** R with the Hammer gives half the materials back, once the beds inside are empty.
- **Art:** structure_hoophousesmall, _medium, _large (children Plastic, Sides_Down, Sides_Up, Torn, Snow, switched by state), item_greenhouse_hoop, item_greenhouse_plastic, build and item icons.
- **Code:** Economy/HoopHouseRules.cs (every number), World/HoopHouseVisuals.cs, WoodManager (kinds 67 to 72, appended), WellsAndOuthouses (the site), GardenBeds (covers and the hourly snow and wear), PlacedStructure, SmithyRules; tests in GreenhouseG2Tests.

## 13. Mike's answers for slice G3, Glass Greenhouses (2026-11-26)

1. **Materials: no Iron Bars.** Lumber, Stone and Glass Panes, with Nails (bought or forged) for the fittings, so it can be built without running a Bloomery.
2. **Seedling benches start transplants:** sow seed in a tray on a bench; it grows under glass, then E takes up transplants to plant outdoors with a head start (like the bought Tomato Plants).
3. **The glass lasts for good:** no wear and no breakage; its trade-off is the cost.

## 14. As built: G3 Glass Greenhouses and seedling benches (2026-11-26)

- **Sizes:** Small 10 x 12 ft (3 x 3.6 m; 4 squares in 2 rows; 1 bench), Medium 16 x 20 ft (4.9 x 6.1 m; 9 squares in 3 rows; 2 benches) and Large 20 x 45 ft (6 x 13.7 m; 30 squares in 3 rows; 3 benches). Each is a little bigger than first proposed (8 x 12, 12 x 20, 20 x 40) so the approved squares, the paths, a step in at the door and the benches fit. Walls 1.6 m, ridge 2.5, 3.0 and 3.4 m.
- **Build:** a staged site on the Build tab (under Other), placed like the Hoop House. Deliver with R, in any order: Small 16 Lumber, 10 Stone, 24 Glass Panes, 1 Box of Nails; Medium 28, 18, 48, 2; Large 60, 36, 110, 4. Then hold click with the Hammer (60, 90 or 140 blows). No Iron Bars.
- **Glass Panes:** $3 each at the Trading Post.
- **Nails:** the Box of Nails is now also on the shelf at $40 (the homemade 2.5x, like the Fired Crock). The store still pays $16 for forged ones.
- **Inside temperature:** vents shut, the air plus 7 C at night (about 12 F) and up to 20 C more in full sun; vents open, plus 1.5 C and up to 3 C. E at the door opens or shuts the roof vents and the door. No rain inside, so water the beds. Any crop grows. The overheat and frost banners work as for the other covers. The glass never wears or breaks.
- **Seedling benches:** along the back wall, one tray each.
  - E with seed in the pack sows a tray (1 seed). R picks which seed when you carry more than one.
  - The tray grows at the greenhouse's inside temperature, as a bed would. It needs no water, but frost or great heat can kill it.
  - When it's ready, E takes up the transplants:
    - 1 Tomato Seed â†’ 12 Tomato Plants (30 days on the bench).
    - 1 Lettuce Seed â†’ 3 Lettuce Starts (20 days).
    - 1 Kale Seed â†’ 3 Kale Starts (20 days).
    - 1 Squash Seed â†’ 3 Squash Starts (25 days).
  - Starts are planted with the Hoe like seed, one to a square, already that many days grown. Like Tomato Plants, they wilt after 7 days unplanted.
- **Taking it down:** R with the Hammer gives half the materials back, once the beds and benches are empty.
- **Art:** structure_glasshousesmall, _medium, _large (children Vents_Shut and Vents_Open), structure_seedlingbench, item_glass_pane, item_starts_lettuce, _kale, _squash, and the build and item icons.
- **Code (G3):** Economy/GlassHouseRules.cs (every number and the trays), World/GlassHouseVisuals.cs, World/GreenhouseBenches.cs, WoodManager (kinds 73 to 79 appended; the Hoop House code is now shared as "walk-in houses"), WellsAndOuthouses, GardenBeds, PlacedStructure, GardenRules (the starts' sowings), MarketCatalog (the pane, and the nails on the shelf); tests in GreenhouseG3Tests.


## 15. Mike's answers for slice G4, Heat (2026-11-26)

1. **Built in a slot inside:** like the Kitchen add-ons, each greenhouse has a heater spot and a barrel spot; the Wood Stove, Propane Heater or Water Barrels are built there with the Hammer.
2. **The Wood Stove: load and it burns.** R loads Firewood (up to 8); E lights it; it burns one an hour only while the house is below a set warmth, so a full load lasts a cold night.
3. **The Propane Heater: fixed, keeps frost off.** It holds the inside at 5 C or above, using propane only when needed. A choosable setting comes with G6 (the Thermostat Controller).
4. **Hotbeds as proposed:** Fertilizer (the Compost Bin's output) dug into a Cold Frame's bed warms its soil about 8 C for about 4 weeks, then it is spent (and the bed is better fed).

## 16. As built: G4 Heat (2026-11-26)

- **Spots:** each greenhouse has a heater spot and a barrel spot inside, clear of the beds, benches and doorway. The Build tab's ghost snaps to them. The Small Hoop House has no room for a heater, and the Small Glass Greenhouse has none for barrels; its bench is now 1.6 m wide to leave the back corner for a heater.
- **Wood Stove:** 1 Greenhouse Stove ($40, Trading Post) and 2 Stone, built with the Hammer.
  - R loads Firewood (up to 8); E lights it or lets it go out.
  - Each hour the greenhouse would be below 8 C it burns one Firewood and adds 18, 14 or 10 C (small, medium, large house). Warm hours cost nothing.
  - When the wood runs out it goes out, with a banner. The stovepipe reaches the roof.
- **Propane Heater:** 1 Propane Heater ($35), built with the Hammer.
  - R loads Propane Tanks ($12, up to 2); E turns it on or off.
  - It keeps the greenhouse at 5 C or above, adding up to 20, 16 or 12 C, and burns only for the warmth it adds. A tank gives 30, 20 or 12 hours of full output.
  - When the propane runs out it stops, with a banner.
- **Water Barrels:** 4 Logs, built with the Hammer. They add 2.5 C on a sunless hour and take 2 C off in strong sun. No fuel.
- **The frost alert** counts the barrels and any stove or heater that is on and has fuel.
- **Taking them out:** R with the Hammer gives half the materials back, plus all the fuel in it. A greenhouse with heat in it can't be taken down until they're out.
- **Hotbeds:** in a Cold Frame's bed, E with 2 Fertilizer digs in a hotbed. It warms the soil 8 C (about 14 F) for 28 days and feeds the bed as Fertilizer does. The bed's line shows the days left.
- **Art:** structure_greenhousestove (Pipe and State_Burning children), structure_propaneheater (State_Burning), structure_waterbarrels, item_greenhouse_stove, item_propane_heater, item_propane_tank, and the build and item icons.
- **Code:** Economy/GreenhouseHeatRules.cs (every number and the spots), World/GreenhouseHeat.cs (the hour, the warmth, prompts, E, R and the look), WoodManager (kinds 80 to 82 appended; the spots), PlacementMode (the snap), GardenBeds (heat in the inside temperature, hotbeds), PlacedStructure; tests in GreenhouseG4Tests.

## 17. Mike's answers for slice G5, Watering (2026-11-27)

1. **Outdoor beds too**, not only greenhouse beds.
2. **Two kinds of fitting:** a Hose Bib fitted inside each greenhouse for its beds, and a separate outdoor Hose Reel for garden beds nearby. Each draws from a Water Barrel or Rain Barrel (or a pump) within reach.
3. **All three pumps as planned:** a Hand Pump (work it to fill the line from the creek, a pond or the Well), Drip Lines with a timer (water every morning from the barrel or pump), and a Fuel Pump (automatic from the creek or a pond, burns gasoline).
4. **Failures:** a banner the morning it fails ("drip lines dry - the barrel is empty"), and the fitting's prompt says why.

## 18. As built: G5 Watering (2026-11-27)

- **Hose Bib:** 1 Garden Hose ($8). It is fitted on the inside of a greenhouse's front wall, left of the door; the build ghost snaps there. It waters that greenhouse's beds.
- **Hose Reel:** 1 Garden Hose and 1 Lumber, built anywhere outdoors. It waters the garden beds within 12 m that are not under cover.
- **Water source:** each fitting draws from the nearest Water Barrel, Rain Barrel, Hand Pump or Fuel Pump within 15 m that has water.
- **E at a fitting:** waters all its beds to soaked at once. The prompt says how many litres and from where, or why it can't ("no barrel or pump within 15 m", "the Rain Barrel is empty").
- **Drip Kit ($15):** E with one in the pack fits it to a bib or reel. From then on it waters its beds by itself every morning at 6 a.m. If it can't, a banner that morning says why.
- **Hand Pump:** 1 Hand Pump ($30) and 2 Lumber, built within 5 m of the creek or a pond, or beside a Well. E pumps 20 L into its 200 L tank.
- **Fuel Pump:** 1 Fuel Pump ($90) and 2 Lumber, built within 5 m of the creek or a pond. R loads Gasoline ($3 a can, up to 4); one can pumps about 400 L. Fittings draw as much as they want from it while it has fuel.
- **Taking them down:** R with the Hammer gives half the materials back, plus any Gasoline. A greenhouse with a Hose Bib in it can't be taken down until the bib is out.
- **Art:** structure_hosebib and structure_hosereel (each with a Drip child, shown once a Drip Kit is fitted), structure_handpump, structure_fuelpump, item_garden_hose, item_drip_kit, item_hand_pump, item_fuel_pump, item_gasoline_can, and the build and item icons.
- **Code:** Economy/WateringRules.cs, World/WateringFittings.cs, WoodManager (kinds 83 to 86 appended; the bib shares the heat spots' code), GardenBeds (the morning drip), PlacedStructure; tests in GreenhouseG5Tests.

## 19. Mike's answers for slice G6, Automatic venting (2026-11-27)

1. **The wax Vent Opener fits all three kinds:** Cold Frame lids, Glass Greenhouse roof vents, and Hoop House sides (a wax-driven roll-up winder).
2. **The Thermostat Controller waits for power:** only the Vent Opener is built now; the controller (and a choosable heater setting) comes with Power_System.
3. **Built in a spot:** the opener is a one-step build with the Hammer into its own spot, like the stove.

## 20. As built: G6 Automatic venting (2026-11-27)

- **Vent Opener:** 1 Vent Opener ($35 at the Trading Post), built with the Hammer. The build ghost snaps to its spot: the middle of a Cold Frame's back wall (under the lid), or the inside of a greenhouse's front wall, right of the door (across the door from the Hose Bib).
- **What it does:** every game hour it works out what the inside would reach shut, sun and heat included. At 24 C (75 F) or above it opens the cover; at 18 C (64 F) or below it shuts it; in between it leaves it as it is, so it doesn't flap open and shut. On a Cold Frame it props the lid, in a Hoop House it rolls the sides, in a Glass Greenhouse it opens the vents. It skips a Hoop House whose plastic is torn.
- **By hand:** E at the cover still opens or shuts it, but the opener has its say again the next hour. E at the opener says what it is doing ("opens the lid above 24 C and shuts it below 18 C (open now)").
- **Order in the hour:** the openers go before the stoves and heaters, so a heater sees the house with the vents as the opener left them.
- **Taking it down:** R with the Hammer (half the materials back, rounded down, so nothing for a single opener). A Cold Frame or greenhouse with an opener on it can't be taken down until the opener is off.
- **Thermostat Controller:** not built; it waits for Power_System.
- **Art:** structure_ventopener (a steel wall plate, a brass wax cylinder and a steel lifting arm; State_Open and State_Shut children show the arm up or down), item_vent_opener, and the build and item icons.
- **Code:** Economy/VentRules.cs, World/VentOpeners.cs, WoodManager (kind 87 appended; the opener shares the heat spots' code, and a Cold Frame can host it), GardenBeds (InsideIf, and the hourly call), PlacedStructure; tests in GreenhouseG6Tests.

---

## Sources

- Eliot Coleman, The Winter Harvest Handbook (Chelsea Green excerpts): https://www.chelseagreen.com/2021/winter-gardening-without-heated-greenhouses/
- Farmers' Almanac, hoop house winter growing: https://www.farmersalmanac.com/extend-your-harvest-grow-undercover
- Cornell, unheated high tunnels: https://news.cornell.edu/node/276853
- SARE GS07-060, high tunnel vs field tomatoes: https://sare.org/wp-content/uploads/GS07-060-system-comparison.pdf
- Ohio State ANR-98, heating costs for protected cultivation: https://ohioline.osu.edu/factsheet/anr-98
- SARE, temperature management in high tunnels: https://sare.org/wp-content/uploads/Temperature-Management-in-High-Tunnels.pdf
- NRCS, controlling the high tunnel environment: https://www.nrcs.usda.gov/sites/default/files/2022-10/Controlling-High-Tunnel-Environment.pdf
- University of Maryland, pollination in high tunnel tomatoes: https://extension.umd.edu/resource/problems-pollination-high-tunnel-tomatoes
- Hudson Valley winter growing project: https://www.slideshare.net/LDHV/the-winter-bounty-project
