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
