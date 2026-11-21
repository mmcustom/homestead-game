# Homestead
## Steam store page text and press kit (Wave 10, 2026-11-20)

Status: first draft by Claude for Mike to edit. Nothing here has been published. Steam choices: developer and publisher MM Custom (Mike, 2026-11-20); store sizes for Steam.

---

# Store page

## Name
Homestead

## Short description (Steam allows 300 characters)
Start with five acres of Ohio woodland, a few tools and a loan. Find clean water, keep a fire, build a log cabin and put up enough food and firewood to get through the winter. A first-person homesteading game about learning the land, one season at a time.

(255 characters.)

## About this game (long description)

Five acres of hardwood forest, a creek, a spring and a loan you have to pay back. Homestead is a first-person homesteading game set in the Ohio Valley, where the land gives you everything you need, if you learn how to take it.

**Water first.** Find springs and creeks, learn which water is safe, boil what isn't, and later dig a well.

**Fire, shelter, food.** Light a fire with a bow drill or flint and steel. Sleep under a tarp, then a lean-to, then a log cabin you built yourself. Forage wild greens, nuts, fruit and mushrooms, fish the creek and the pond, set traps, and hunt deer, turkey, ducks, rabbits and squirrels.

**Make what you need.** Cut timber and split firewood. Dig clay and fire pottery in a kiln. Burn charcoal, smelt iron in a bloomery and forge tools at the smithy. Tap the sugar maples in late winter and boil the sap down to syrup. Leach ash into lye and boil it into potash to sell.

**Seasons that matter.** Spring brings the first greens and high water. Summer is for building and fishing. Fall is the harvest and the last chance to stock up. Winter is the test: snow, cold nights and short days, and the firewood you stacked in October is what keeps you warm.

**A body that feels the weather.** Hunger, thirst and stamina, and a body temperature that follows the wind, the rain, the sun and the fire. Get soaked in a cold rain and you will shiver; work hard in a heat wave and you will need shade and water.

**Pay your way.** Trade at the Trading Post, where prices rise and fall. Keep up with the payments on your land. Choose how hard the start is: Homesteader, Settler or Pioneer, from a full kit to almost nothing at all.

**Learn the land.** Every spring, stand, creek bend and berry patch you find goes into your Journal, with what you learned about it. By the second year the same five acres feel like a different place.

*Homestead is in development. Features, art and balance will change before release.*

## Key features (for the store's feature list)
- First-person homesteading in a realistic Ohio Valley woodland
- Water, fire, shelter and food first; winter preparation is the heart of the year
- Four seasons with real consequences for food, water, wildlife and work
- Body temperature, wetness, hunger, thirst and stamina that follow the weather
- Build a homestead: cabin, well, outhouse, workshop, kiln, potter's shed, smithy, outdoor kitchen and more
- Craft tools from stone and wood, then forge them in iron
- Hunting, fishing, trapping, foraging and maple sugaring
- A Trading Post with seasonal prices, and a land loan to pay off
- Three difficulty levels: Homesteader, Settler, Pioneer
- A Journal that records what you discover about your land

## Tags (suggested; Steam asks for up to 20)
Survival, Simulation, Open World Survival Craft, Crafting, Building, First-Person, Realistic, Nature, Hunting, Fishing, Base Building, Singleplayer, Sandbox, Life Sim, Exploration, Atmospheric, Relaxing, Resource Management, Economy, Indie

## Platform and requirements (fill in after testing a build)
- Windows PC only
- Minimum and recommended specs: TBD (Unity 6 HDRP needs a DirectX 12 capable GPU; measure on a low and a mid machine before publishing)

---

# Press kit (fact sheet)

| | |
|---|---|
| Developer | MM Custom |
| Publisher | MM Custom (self-published) |
| Based in | Bethel, Ohio, USA |
| Release date | TBA |
| Platform | Windows PC (Steam) |
| Price | TBA |
| Engine | Unity 6 (HDRP) |
| Players | Single player |
| Genre | First-person homesteading and survival simulation |
| Status | In development (Alpha 0.1) |
| Website | TBA |
| Press contact | TBA |

## Description
Homestead is a first-person homesteading game about turning five acres of Ohio woodland into a home. The player starts with a few tools and a land loan, and learns to find clean water, keep a fire, build shelter, gather and preserve food and prepare for winter. Every system follows the real world closely: water quality, body temperature, the seasons, the wildlife and the market. The game's motto is its loop: Survive. Prepare. Build. Prosper.

## History
Homestead began as a personal project by Mike Mack of MM Custom, set on land like his own near Bethel, Ohio. The design rule from the start has been that the land should feel learned rather than revealed: the player's second year on the same five acres should feel different because they understand it better, not because anything new appeared.

## Assets in this kit
- Logo: logo_lockup.png (emblem, HOMESTEAD and the motto), logo_emblem.png (the round badge), library_logo_1280x720.png (emblem and name, transparent)
- Key art: library_hero_3840x1240.png, title_background.png
- Screenshots: 7 at 1920 x 1080, straight from the game
- Teaser: homestead_teaser_1080p.mp4 (26 s)

All assets are made from the game itself and the Homestead logo. Press may use them to cover the game.

---

# Steam asset checklist (files in 07_Marketing/Steam)

| Asset | Size | File | Note |
|---|---|---|---|
| Header capsule (also library header) | 920 x 430 | header_capsule_920x430.png | Logo and art only |
| Small capsule | 462 x 174 | small_capsule_462x174.png | Logo fills it so it reads at 120 x 45 |
| Main capsule | 1232 x 706 | main_capsule_1232x706.png | |
| Vertical capsule | 748 x 896 | vertical_capsule_748x896.png | |
| Page background | 1438 x 810 | page_background_1438x810.png | Darkened and softened |
| Library capsule | 600 x 900 | library_capsule_600x900.png | |
| Library hero | 3840 x 1240 | library_hero_3840x1240.png | No logo or text, as Steam asks |
| Library logo | 1280 x 720 | library_logo_1280x720.png | Transparent PNG |
| Community (app) icon | 184 x 184 | community_icon_184x184.jpg | JPG |
| Client icon | 32 x 32 | client_icon_32x32.png | Steam wants a .ico; homestead_icon.ico in Logos has 16 to 256 |
| Screenshots | 1920 x 1080 | screenshots/*.png | 7 (Steam asks for at least 5) |
| Trailer | 1920 x 1080, 30 fps | ../Trailer/homestead_teaser_1080p.mp4 | 26 s teaser |

The capsules carry only the emblem and the name (Steam does not allow taglines or other text on capsules), so the motto stays on the logo lockup used elsewhere.

## Re-making them
- Stills and teaser shots: Play Mode, Assets/Scripts/Debug/MarketingCamera.cs (Editor only). It sets the day, hour, weather and snow per shot, flies its own camera with the HUD hidden and writes stills or MJPEG shot files to Temp/Marketing. Back up the saves first: the game autosaves while it runs.
- Capsules and screenshots: Art/Blender_Work/brand_logo/gen_store_art.py (Python, numpy, PIL).
- Teaser: Art/Blender_Work/brand_logo/make_teaser.py (Python and ffmpeg).
- Redo all of it when the world art changes noticeably; the pictures are only as good as the game looks today.
