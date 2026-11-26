using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Gardening basics, slice GB1 (Gardening_Basics.md): the nine crops, growing in real time with the warmth and the soil's moisture, frost by hardiness, picks, and the wiring.
public class GardenTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    static GardenRules.Crop Crop(string id) => GardenRules.Find(id);

    // Runs a planted, watered bed for some days at a steady temperature (moisture kept up, as if watered).
    static GardenRules.HourResult Grow(GardenRules.Crop crop, float airC, int days, bool dryBeans = false, float moisture = 0.8f, bool keepWatered = true)
    {
        var r = new GardenRules.HourResult { moisture = moisture };
        for (int h = 0; h < days * 24 && !r.died; h++)
        {
            r = GardenRules.Hour(crop, r.growingDays, r.damage, keepWatered ? 0.8f : r.moisture, dryBeans, false, airC, 0f);
        }
        return r;
    }

    [Test]
    public void NineCropsMikesStaplesWithBothCorns()
    {
        Assert.That(GardenRules.Crops.Select(c => c.id), Is.EqualTo(new[] { "potatoes", "field_corn", "sweet_corn", "beans", "tomatoes", "lettuce", "carrots", "squash", "kale" }));
        Assert.That(GardenRules.Crops.Select(c => c.days), Is.EqualTo(new[] { 100f, 95f, 78f, 55f, 75f, 50f, 70f, 100f, 60f }), "the extension tables' days to harvest");
        foreach (GardenRules.Crop crop in GardenRules.Crops)
        {
            Assert.That(Resources.Load("Items/Resources/" + crop.produceId) ?? Resources.Load("Items/Materials/" + crop.produceId), Is.Not.Null, crop.produceId);
            Assert.That(MarketCatalog.Table.SellBaseCents(crop.produceId), Is.GreaterThan(0L), crop.produceId + " sells");
            Assert.That(MarketCatalog.Table.BuyBaseCents(crop.seedId), Is.GreaterThan(0L), crop.seedId + " is on the shelf");
            Assert.That(GardenRules.ForSeed(crop.seedId), Is.SameAs(crop));
        }
        Assert.That(MarketCatalog.Table.SellBaseCents(GardenRules.DryBeansId), Is.EqualTo(GardenRules.DryBeanCents));
        Assert.That(MarketCatalog.Table.BuyBaseCents(GardenRules.HoeId), Is.EqualTo(1000L));
        Assert.That(MarketCatalog.Table.BuyBaseCents(GardenRules.WateringCanId), Is.EqualTo(800L));
        Assert.That(Resources.Load("Items/Tools/hoe"), Is.Not.Null);
        Assert.That(Resources.Load("Items/Tools/watering_can"), Is.Not.Null);
        Assert.That(MarketCatalog.Table.SellBaseCents(GardenRules.PotatoesId), Is.EqualTo(25L), "the store pays 25 cents a potato and sells them at 60");
    }

    [Test]
    public void ProducePaysMostInWinterAndLeastInTheSummerGlut()
    {
        Assert.That(MarketCatalog.Table.TryGetRule(MarketGroup.Produce, out MarketGroupRule rule), Is.True);
        Assert.That(rule.TargetFor(3), Is.GreaterThan(rule.TargetFor(0)));
        Assert.That(rule.TargetFor(1), Is.LessThan(rule.TargetFor(2)));
        Assert.That(MarketCatalog.Table.Problems, Is.Empty);
    }

    [Test]
    public void GrowthFollowsTheWarmthAndStopsWhenCold()
    {
        GardenRules.Crop tomato = Crop("tomatoes"), lettuce = Crop("lettuce");
        Assert.That(GardenRules.GrowthRate(tomato, 8f), Is.EqualTo(0f));
        Assert.That(GardenRules.GrowthRate(tomato, 14f), Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(GardenRules.GrowthRate(tomato, 24f), Is.EqualTo(1f));
        Assert.That(GardenRules.GrowthRate(tomato, 34f), Is.EqualTo(0.3f), "tomatoes stall above 32 C (flowers drop)");
        Assert.That(GardenRules.GrowthRate(lettuce, 10f), Is.GreaterThan(0.5f), "a cool crop grows in cool weather");

        GardenRules.HourResult r = Grow(tomato, 22f, 75);
        Assert.That(r.growingDays, Is.EqualTo(75f).Within(0.1f), "a steady 22 C: one growing day a day, ready in 75");
        Assert.That(GardenRules.PicksReady(tomato, false, r.growingDays + 0.01f, 0), Is.EqualTo(1));
        Assert.That(Grow(tomato, 14f, 75).growingDays, Is.EqualTo(37.5f).Within(0.1f), "half speed at 14 C");
    }

    [Test]
    public void DrySoilSlowsThenKillsAndRainWetsIt()
    {
        Assert.That(GardenRules.WaterFactor(0.5f), Is.EqualTo(1f));
        Assert.That(GardenRules.WaterFactor(0.05f), Is.EqualTo(0f));
        Assert.That(GardenRules.AfterRain(0.2f, 10f), Is.EqualTo(0.6f).Within(0.001f), "10 mm of rain");
        Assert.That(GardenRules.AfterWatering(0.2f, 10), Is.EqualTo(0.6f).Within(0.001f), "a full 10 L Bucket");
        Assert.That(GardenRules.LitresToSoak(0.6f), Is.EqualTo(10));
        Assert.That(GardenRules.DryingPerHour(25f) * 24f, Is.EqualTo(0.132f).Within(0.001f), "about 0.13 a day at 25 C");
        Assert.That(GardenRules.DryingPerHour(-3f), Is.EqualTo(0f), "frozen soil does not dry");

        GardenRules.HourResult r = Grow(Crop("beans"), 26f, 30, moisture: 0.5f, keepWatered: false);
        Assert.That(r.died, Is.True, "a month of summer with no rain or watering kills the beans");
    }

    [Test]
    public void FrostKillsByHardiness()
    {
        Assert.That(GardenRules.KillC(GardenRules.Hardiness.Tender), Is.EqualTo(0f));
        Assert.That(GardenRules.KillC(GardenRules.Hardiness.Hardy), Is.EqualTo(-12f));
        GardenRules.HourResult tomato = Grow(Crop("tomatoes"), -2f, 1);
        Assert.That(tomato.died, Is.True, "a night at -2 C kills tender tomatoes");
        GardenRules.HourResult kale = Grow(Crop("kale"), -8f, 5);
        Assert.That(kale.died, Is.False, "kale takes -8 C");
        Assert.That(kale.damage, Is.EqualTo(0f));
        Assert.That(GardenRules.FrostDamage(Crop("potatoes"), -5f, true), Is.EqualTo(0f), "ready potatoes are safe under the ground");
        Assert.That(GardenRules.FrostDamage(Crop("potatoes"), -5f, false), Is.GreaterThan(0f));
    }

    [Test]
    public void LettuceBoltsInHeatAndWhenLeftTooLong()
    {
        GardenRules.Crop lettuce = Crop("lettuce");
        Assert.That(Grow(lettuce, 30f, 5).damage, Is.GreaterThan(0.9f), "five days above 27 C nearly finishes it");
        GardenRules.HourResult r = GardenRules.Hour(lettuce, lettuce.days + lettuce.boltAfterDays - 0.001f, 0f, 0.8f, false, false, 18f, 0f);
        Assert.That(r.bolted && r.died, Is.True, "20 growing days past ready it bolts");
    }

    [Test]
    public void PickedCropsGiveAgainAndDamageCutsTheYield()
    {
        GardenRules.Crop tomato = Crop("tomatoes");
        Assert.That(GardenRules.PicksReady(tomato, false, 74f, 0), Is.EqualTo(0));
        Assert.That(GardenRules.PicksReady(tomato, false, 75f, 0), Is.EqualTo(1));
        Assert.That(GardenRules.PicksReady(tomato, false, 85f, 1), Is.EqualTo(2), "a pick every 5 days");
        Assert.That(GardenRules.PicksReady(tomato, false, 200f, 0), Is.EqualTo(7), "seven picks at most");
        Assert.That(GardenRules.PickYield(tomato, false, 0f), Is.EqualTo(10), "70 tomatoes over 7 picks");
        Assert.That(GardenRules.PickYield(tomato, false, 0.5f), Is.EqualTo(5));
        Assert.That(GardenRules.Spent(tomato, false, 7), Is.True);

        GardenRules.Crop beans = Crop("beans");
        Assert.That(GardenRules.DaysToReady(beans, true), Is.EqualTo(90f));
        Assert.That(GardenRules.ProduceOf(beans, true), Is.EqualTo("dry_beans"));
        Assert.That(GardenRules.Picks(beans, true), Is.EqualTo(1));
        Assert.That(GardenRules.PickYield(beans, true, 0f), Is.EqualTo(7));
        Assert.That(GardenRules.PickYield(beans, false, 0f), Is.EqualTo(2), "8 portions over 4 picks");
        Assert.That(GardenRules.PickYield(Crop("potatoes"), false, 0f), Is.EqualTo(36));
    }

    [Test]
    public void StagesForTheLookOfTheBed()
    {
        GardenRules.Crop kale = Crop("kale");
        Assert.That(GardenRules.StageOf(null, false, 0f, 0, false), Is.EqualTo(GardenRules.Stage.Bare));
        Assert.That(GardenRules.StageOf(kale, false, 5f, 0, false), Is.EqualTo(GardenRules.Stage.Sprouting));
        Assert.That(GardenRules.StageOf(kale, false, 20f, 0, false), Is.EqualTo(GardenRules.Stage.Growing));
        Assert.That(GardenRules.StageOf(kale, false, 40f, 0, false), Is.EqualTo(GardenRules.Stage.Maturing));
        Assert.That(GardenRules.StageOf(kale, false, 60f, 0, false), Is.EqualTo(GardenRules.Stage.Ready));
        Assert.That(GardenRules.StageOf(kale, false, 60f, 9, false), Is.EqualTo(GardenRules.Stage.Spent));
        Assert.That(GardenRules.StageOf(kale, false, 60f, 0, true), Is.EqualTo(GardenRules.Stage.Dead));
    }

    [Test]
    public void TheBedIsWiredIntoTheWorld()
    {
        Assert.That(SourceText("Scripts/Managers/SpoilageManager.cs"), Does.Contain("GardenBeds.Hour((float)AirC,"));
        string axe = SourceText("Scripts/Player/AxeTool.cs");
        Assert.That(axe, Does.Contain("|| site.State.kind == PileKind.GardenBed) && site.State.digProgress < 1f"));
        Assert.That(SourceText("Scripts/Managers/WellsAndOuthouses.cs"), Does.Contain("well.kind == PileKind.GardenBed ? GardenRules.DigBlows"));
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("case PileKind.GardenBed: GardenBeds.Build(parent, state, pieces, out boxCenter, out boxSize); return;"));
        Assert.That(placed, Does.Contain("case PileKind.GardenBed: return GardenBeds.ActionE(state);"));
        Assert.That(placed, Does.Contain("case PileKind.GardenBed: GardenBeds.Interact(state, transform); break;"));
        Assert.That(placed, Does.Contain("GardenBeds.Secondary(state);"));
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("PileKind.SawPitSite, PileKind.SawmillSite, PileKind.GardenBed,"), "on the Build tab");
        Assert.That(wood, Does.Contain("if (IsBedKind(kind) && IsBedKind(other.kind))"), "beds stand side by side (a Raised Bed Site too, GB2)");
        Assert.That(SourceText("Scripts/UI/CraftingScreen.cs"), Does.Contain("case PileKind.GardenBed:\n            case PileKind.RaisedBedSite:\n            case PileKind.HideFrame:"), "under Other (the Raised Bed Site too, GB2)");
        Assert.That(SourceText("Scripts/Survival/WaterQuality.cs"), Does.Contain("container.Count(GardenRules.WateringCanId) * GardenRules.WateringCanLitres"));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.GardenBedPlaced), Is.EqualTo(new[] { KnowHowRules.Garden }));
        Assert.That(KnowHowRules.Entries.Last().title, Is.EqualTo("The Kitchen Garden"));
    }

    // Mike, 2026-11-25: "Sell them only in Spring". Months count from March (0) in the game's calendar.
    [Test]
    public void TomatoPlantsAreOnTheShelfFromMidAprilToEarlyJune()
    {
        GameCalendar cal = CalendarRules.Real;
        int Day(int month, int date) => GardenRules.DayOfYearOf(cal, month, date);
        Assert.That(cal.MonthName(cal.MonthOfYear(Day(1, 15))), Is.EqualTo("Apr"));
        Assert.That(GardenRules.TomatoPlantsInSeason(cal, Day(1, 14)), Is.False, "April 14");
        Assert.That(GardenRules.TomatoPlantsInSeason(cal, Day(1, 15)), Is.True, "April 15");
        Assert.That(GardenRules.TomatoPlantsInSeason(cal, Day(2, 20)), Is.True, "May");
        Assert.That(GardenRules.TomatoPlantsInSeason(cal, Day(3, 10)), Is.True, "June 10");
        Assert.That(GardenRules.TomatoPlantsInSeason(cal, Day(3, 11)), Is.False, "June 11");
        Assert.That(GardenRules.TomatoPlantsInSeason(cal, Day(7, 1)), Is.False, "October");
        Assert.That(GardenRules.TomatoPlantsInSeason(cal, Day(1, 20) + cal.DaysPerYear), Is.True, "every year");
        Assert.That(SourceText("Scripts/Managers/TradingPost.cs"), Does.Contain("if (OnShelf(row.itemId))"), "off the shelf out of season");
        Assert.That(GardenRules.Help, Does.Contain("mid-April to early June"));
    }

    [Test]
    public void PlantingTooEarlyOrTooLateForFrostWarns()
    {
        GameCalendar cal = CalendarRules.Real;
        int Day(int month, int date) => GardenRules.DayOfYearOf(cal, month, date);
        GardenRules.Crop tomatoes = Crop("tomatoes");
        Assert.That(GardenRules.TimingWarning(tomatoes, cal, Day(1, 20)), Does.Contain("Early"), "a tender crop before the last spring frost");
        Assert.That(GardenRules.TimingWarning(tomatoes, cal, Day(2, 20)), Is.Null, "late May is right");
        Assert.That(GardenRules.TimingWarning(tomatoes, cal, Day(5, 20)), Does.Contain("Late"), "August 20 + 75 days runs past mid-October");
        Assert.That(GardenRules.TimingWarning(tomatoes, cal, Day(7, 20)), Does.Contain("Too late"), "after the first frost");
        Assert.That(GardenRules.TimingWarning(Crop("kale"), cal, Day(7, 20)), Is.Null, "kale takes a hard frost");
        Assert.That(GardenRules.TimingWarning(Crop("carrots"), cal, Day(0, 20)), Is.Null, "carrots too");
        Assert.That(GardenRules.TimingWarning(Crop("potatoes"), cal, Day(1, 20)), Is.Null, "potatoes take a light frost, so April is fine");
        Assert.That(GardenRules.PlantPrompt(tomatoes, 4), Does.StartWith("Plant Tomatoes"), "the HUD adds the key; the text does not say E");
    }

    // Mike, 2026-11-25: the Hide Frame and Tanning Vat prompts were confusing (the same text on the E and R lines). The status now sits above the keys
    // (IPromptHeader) and each key line says only what that key does.
    [Test]
    public void TheHideFrameAndVatPromptsSayWhatEachKeyDoes()
    {
        string scraping = HideFrameRules.Header(HideFrameRules.Phase.Scraping, HideFrameRules.Kind.Deer, 1, 0f, false, true, null, 0);
        Assert.That(scraping, Does.Contain("scraped 0 of 20").And.Contain("Equip the Knife, then hold click to scrape it"));
        Assert.That(scraping, Does.Not.Contain("(E)").And.Not.Contain("(R)"), "no keys in the status");
        Assert.That(HideFrameRules.ActionE(HideFrameRules.Phase.Scraping, 0f, null), Is.EqualTo("Take the hide back off"));
        Assert.That(HideFrameRules.ActionE(HideFrameRules.Phase.Scraping, 3f, null), Is.Null, "once scraping has started E does nothing");
        Assert.That(HideFrameRules.ActionE(HideFrameRules.Phase.Drying, 0f, null), Is.Null);
        string done = HideFrameRules.Header(HideFrameRules.Phase.Done, HideFrameRules.Kind.Deer, 0, 0f, false, true, "Rawhide", 1, true);
        Assert.That(done, Does.Contain("For Leather: take it to the Tanning Vat").And.Contain("For Buckskin").And.Contain("Crock Pot"));
        Assert.That(HideFrameRules.ActionE(HideFrameRules.Phase.Done, 0f, "Rawhide"), Is.EqualTo("Take the Rawhide"));
        Assert.That(TanningVatRules.Header(TanningVatRules.Phase.Loading, 1, 2, 20, 0, 0f), Does.Contain("Still needs 2 more Tanbark"));
        Assert.That(TanningVatRules.ActionE(TanningVatRules.Phase.Loading), Is.Null);
        Assert.That(TanningVatRules.ActionE(TanningVatRules.Phase.Ready), Is.EqualTo("Start the soak (7 days)"));
        Assert.That(TanningVatRules.LoadPrompt(new TanningVatRules.LoadPlan { hides = 1, tanbark = 4, water = 20 }), Is.EqualTo("Load 1 Rawhide, 4 Tanbark, 20 litres of water"));
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("IInteractable, ISecondaryInteractable, IPromptHeader"));
        Assert.That(SourceText("Scripts/UI/InteractionPromptHud.cs"), Does.Contain("focus is IPromptHeader withHeader"));
    }
}
