using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

// Gardening slice GB4a (Mike, 2026-11-25, Gardening_Basics.md 9d): fences built panel by panel (wattle keeps rabbits out, deer fence and the Gate both), a bed protected
// only inside a closed ring, and light nibbling by rabbits and deer at dawn and dusk.
public class GardenGB4Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");
    static FenceRules.Point P(float x, float z) => new FenceRules.Point(x, z);
    static FenceRules.Segment Seg(float x, float z, float yaw, float length, FenceRules.Type type = FenceRules.Type.Deer) => FenceRules.SegmentOf(x, z, yaw, length, type);

    // A 2.4 m square round the origin: south, east, north, west.
    static List<FenceRules.Segment> Square(FenceRules.Type west = FenceRules.Type.Deer) => new List<FenceRules.Segment>
    {
        Seg(0f, -1.2f, 0f, 2.4f), Seg(1.2f, 0f, 90f, 2.4f), Seg(0f, 1.2f, 0f, 2.4f), Seg(-1.2f, 0f, 90f, 2.4f, west),
    };

    [Test]
    public void APanelRunsAlongItsYawAsUnityTurnsIt()
    {
        FenceRules.Point d0 = FenceRules.Direction(0f), d90 = FenceRules.Direction(90f);
        Assert.That(d0.x, Is.EqualTo(1f).Within(1e-5f));
        Assert.That(d0.z, Is.EqualTo(0f).Within(1e-5f));
        Assert.That(d90.x, Is.EqualTo(0f).Within(1e-5f));
        Assert.That(d90.z, Is.EqualTo(-1f).Within(1e-5f), "Quaternion.Euler(0, 90, 0) * right is -Z");
        Vector3 unity = Quaternion.Euler(0f, 37f, 0f) * Vector3.right;
        FenceRules.Point d37 = FenceRules.Direction(37f);
        Assert.That(d37.x, Is.EqualTo(unity.x).Within(1e-5f));
        Assert.That(d37.z, Is.EqualTo(unity.z).Within(1e-5f));
        Assert.That(FenceRules.YawOf(P(0f, 0f), P(0f, -2f)), Is.EqualTo(90f).Within(1e-4f));
        FenceRules.Segment s = Seg(1f, 2f, 0f, 2.4f);
        Assert.That(s.a.x, Is.EqualTo(-0.2f).Within(1e-5f));
        Assert.That(s.b.x, Is.EqualTo(2.2f).Within(1e-5f));
    }

    [Test]
    public void TheSizesAndCosts()
    {
        Assert.That(FenceRules.PanelLength, Is.EqualTo(2.4f));
        Assert.That(FenceRules.GateLength, Is.EqualTo(FenceRules.PanelLength), "Mike, 2026-11-26: as wide as a section");
        Assert.That(FenceRules.GateHeight(false, true), Is.EqualTo(FenceRules.WattleHeight), "between wattle sections, wattle height");
        Assert.That(FenceRules.GateHeight(true, false), Is.EqualTo(FenceRules.DeerHeight), "in deer fence, deer-fence height, so the ring stays deer-proof");
        Assert.That(FenceRules.GateHeight(true, true), Is.EqualTo(FenceRules.DeerHeight), "between the two, the taller");
        Assert.That(FenceRules.GateHeight(false, false), Is.EqualTo(FenceRules.DeerHeight), "on its own");
        Assert.That(FenceRules.Height(FenceRules.Type.Wattle), Is.EqualTo(1.0f));
        Assert.That(FenceRules.Height(FenceRules.Type.Deer), Is.EqualTo(2.1f));
        Assert.That(FenceRules.DeerProof(FenceRules.Type.Wattle), Is.False);
        Assert.That(FenceRules.DeerProof(FenceRules.Type.Gate), Is.True);
        Assert.That(FenceRules.LengthOf(FenceRules.Type.Deer, 0f), Is.EqualTo(2.4f), "a panel from before it was cut is full length");
        Assert.That(FenceRules.LengthOf(FenceRules.Type.Deer, 0.7f), Is.EqualTo(0.7f));
        Assert.That(FenceRules.WireMeshStoreCents, Is.EqualTo(1500L));
        Assert.That(MarketCatalog.Table.BuyBaseCents(FenceRules.WireMeshId), Is.EqualTo(1500L));
        Assert.That(SourceText("Scripts/Economy/Ledger.cs"), Does.Contain("{ FenceRules.WireMeshId, LedgerCategory.BuyOtherGoods }"));
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("GardenBed, TanningVatSite, RaisedBedSite, WattleFence, DeerFence, FenceGate, ColdFrameSmall, ColdFrameMedium, ColdFrameLarge, HoopHouseSiteSmall, HoopHouseSiteMedium, HoopHouseSiteLarge, HoopHouseSmall, HoopHouseMedium, HoopHouseLarge, GlassHouseSiteSmall, GlassHouseSiteMedium, GlassHouseSiteLarge, GlassHouseSmall, GlassHouseMedium, GlassHouseLarge, SeedlingBench, GreenhouseStove, PropaneHeater, WaterBarrels, HoseBib, HoseReel, HandPump, FuelPump, VentOpener, DeliveryPad, PoleBarnSiteSmall"), "appended, so saves keep their numbers");
        Assert.That(wood, Does.Contain("count = FenceRules.WattleSticks }, new WoodStack { itemId = FenceRules.BranchesId, count = FenceRules.WattleBranches }"));
        Assert.That(wood, Does.Contain("count = FenceRules.DeerLogs }, new WoodStack { itemId = FenceRules.WireMeshId, count = FenceRules.DeerWire }"));
        Assert.That(wood, Does.Contain("count = FenceRules.GateLumber }, new WoodStack { itemId = FenceRules.CordageId, count = FenceRules.GateCordage }"));
    }

    [Test]
    public void ANewPanelSnapsToAFreeEndAndRunsTowardTheAim()
    {
        var standing = new List<FenceRules.Segment> { Seg(0f, -1.2f, 0f, 2.4f) };
        Assert.That(FenceRules.FreeEnds(standing).Count, Is.EqualTo(2));
        FenceRules.Placement p = FenceRules.Place(standing, P(1.5f, -0.2f), FenceRules.Type.Deer, 0f);
        Assert.That(p.snapped, Is.True);
        Assert.That(p.closes, Is.False);
        Assert.That(p.length, Is.EqualTo(2.4f).Within(1e-4f));
        FenceRules.Segment placed = FenceRules.SegmentOf(p.x, p.z, p.yaw, p.length, FenceRules.Type.Deer);
        bool startsAtEnd = Vector2.Distance(new Vector2(placed.a.x, placed.a.z), new Vector2(1.2f, -1.2f)) < 1e-3f ||
                           Vector2.Distance(new Vector2(placed.b.x, placed.b.z), new Vector2(1.2f, -1.2f)) < 1e-3f;
        Assert.That(startsAtEnd, Is.True, "it starts at the end it snapped to");

        FenceRules.Placement alone = FenceRules.Place(standing, P(20f, 20f), FenceRules.Type.Wattle, 45f);
        Assert.That(alone.snapped, Is.False, "nothing near: it stands on its own");
        Assert.That(alone.x, Is.EqualTo(20f));
        Assert.That(alone.yaw, Is.EqualTo(45f), "turned by the mouse wheel");
    }

    [Test]
    public void TheLastPanelIsCutToFitAndClosesTheRing()
    {
        List<FenceRules.Segment> three = Square();
        three.RemoveAt(3);
        FenceRules.Placement p = FenceRules.Place(three, P(-1.3f, 0.3f), FenceRules.Type.Deer, 0f);
        Assert.That(p.closes, Is.True);
        Assert.That(p.length, Is.EqualTo(2.4f).Within(1e-3f));
        Assert.That(Mathf.Abs(Mathf.DeltaAngle(p.yaw, 90f)) % 180f, Is.LessThan(0.01f));

        // A short gap: 0.6 m left between a cut panel and the corner.
        three.Add(FenceRules.SegmentOf(-1.2f, 0.3f, 90f, 1.8f, FenceRules.Type.Deer));
        FenceRules.Placement cut = FenceRules.Place(three, P(-1.2f, -0.9f), FenceRules.Type.Deer, 0f);
        Assert.That(cut.closes, Is.True);
        Assert.That(cut.length, Is.EqualTo(0.6f).Within(1e-3f));
        three.Add(FenceRules.SegmentOf(cut.x, cut.z, cut.yaw, cut.length, FenceRules.Type.Deer));
        Assert.That(FenceRules.FreeEnds(three).Count, Is.EqualTo(0), "the ring is closed");
        Assert.That(FenceRules.Inside(FenceRules.Rings(three), P(0f, 0f)), Is.True);
    }

    [Test]
    public void OnlyAClosedRingProtects()
    {
        List<FenceRules.Segment> ring = Square();
        Assert.That(FenceRules.Inside(FenceRules.Rings(ring), P(0f, 0f)), Is.True);
        Assert.That(FenceRules.Inside(FenceRules.Rings(ring), P(0.9f, -0.9f)), Is.True);
        Assert.That(FenceRules.Inside(FenceRules.Rings(ring), P(3f, 0f)), Is.False);
        Assert.That(FenceRules.Inside(FenceRules.Rings(ring), P(0f, 1.2f + 0.3f)), Is.False);

        List<FenceRules.Segment> open = Square();
        open.RemoveAt(1);
        Assert.That(FenceRules.Rings(open).Count, Is.EqualTo(0), "with a side missing, every panel dangles");
        Assert.That(FenceRules.Inside(FenceRules.Rings(open), P(0f, 0f)), Is.False);

        // A panel running off a corner does not spoil the ring.
        List<FenceRules.Segment> spur = Square();
        spur.Add(FenceRules.SegmentOf(2.4f, 1.2f, 0f, 2.4f, FenceRules.Type.Wattle));
        Assert.That(FenceRules.Rings(spur).Count, Is.EqualTo(4));
        Assert.That(FenceRules.Inside(FenceRules.Rings(spur), P(0f, 0f)), Is.True);
    }

    [Test]
    public void WattleKeepsRabbitsOutButDeerJumpIt()
    {
        FenceRules.Protection allDeer = FenceRules.ProtectionAt(Square(), P(0f, 0f));
        Assert.That(allDeer.rabbits && allDeer.deer, Is.True);
        FenceRules.Protection wattleSide = FenceRules.ProtectionAt(Square(FenceRules.Type.Wattle), P(0f, 0f));
        Assert.That(wattleSide.rabbits, Is.True);
        Assert.That(wattleSide.deer, Is.False, "one wattle side and deer jump in");

        // A gate in the west side keeps deer out like the rest.
        List<FenceRules.Segment> gated = Square();
        gated.RemoveAt(3);
        gated.Add(FenceRules.SegmentOf(-1.2f, 0.6f, 90f, 1.2f, FenceRules.Type.Gate));
        gated.Add(FenceRules.SegmentOf(-1.2f, -0.6f, 90f, 1.2f, FenceRules.Type.Deer));
        FenceRules.Protection withGate = FenceRules.ProtectionAt(gated, P(0f, 0f));
        Assert.That(withGate.rabbits && withGate.deer, Is.True);
        Assert.That(FenceRules.ProtectionText(withGate), Is.EqualTo("fenced against deer and rabbits"));
        Assert.That(FenceRules.ProtectionText(wattleSide), Is.EqualTo("fenced against rabbits (deer jump wattle)"));
        Assert.That(FenceRules.ProtectionText(default), Does.StartWith("not fenced"));
    }

    [Test]
    public void RaidsAreLightNibblingAtDawnAndDusk()
    {
        Assert.That(FenceRules.RabbitBite, Is.EqualTo(0.05f), "Mike: a twentieth of a square");
        Assert.That(FenceRules.DeerBite, Is.EqualTo(0.10f), "Mike: a tenth");
        Assert.That(FenceRules.IsRaidHour(6), Is.True);
        Assert.That(FenceRules.IsRaidHour(19), Is.True);
        Assert.That(FenceRules.IsRaidHour(12), Is.False);
        // An unfenced bed loses about a quarter of a 70-day crop on average: light.
        float perDay = 2f * (FenceRules.RabbitChance * FenceRules.RabbitBite + FenceRules.DeerChance * FenceRules.DeerBite);
        Assert.That(perDay * 70f, Is.InRange(0.15f, 0.35f));
        Assert.That(FenceRules.RabbitEats("lettuce", false), Is.True);
        Assert.That(FenceRules.RabbitEats("squash", false), Is.False);
        Assert.That(FenceRules.RabbitEats("squash", true), Is.True, "young plants of anything");
        Assert.That(FenceRules.DeerEats("potatoes"), Is.False);
        Assert.That(FenceRules.DeerEats("sweet_corn"), Is.True);
        Assert.That(FenceRules.RaidBanner(2, 0, true), Is.EqualTo("Rabbits nibbled 2 garden beds at dawn - a fence keeps them out"));
        Assert.That(FenceRules.RaidBanner(0, 1, false), Does.StartWith("A deer browsed a garden bed at dusk"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("FenceRules.ProtectionAt(fences, new FenceRules.Point(bed.position.x, bed.position.z))"));
        Assert.That(beds, Does.Contain("FenceRules.ProtectionText(WoodManager.Instance.FenceProtectionAt(s.position))"));
        string place = SourceText("Scripts/Player/PlacementMode.cs");
        Assert.That(place, Does.Contain("FenceRules.Place(WoodManager.Instance.FenceSegments()"));
        Assert.That(place, Does.Contain("WoodManager.Instance.BuildPile(kind, player, spot, fenceYaw, fenceLength)"));
        string ps = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(ps, Does.Contain("gateLeaf = FenceVisuals.GateLeaf(transform, state, gateHeight);"));
        Assert.That(ps, Does.Contain("WoodManager.Instance.RefreshGatesBeside(state);"), "a gate follows a section taken down beside it");
        Assert.That(SourceText("Scripts/World/FenceVisuals.cs"), Does.Contain("Resources.Load<Material>(\"Art/structure_plank\")"), "the leaf uses the door planks");
        Assert.That(SourceText("Scripts/World/FenceVisuals.cs"), Does.Contain("Resources.Load<GameObject>(\"Art/\" + ArtId(state.kind))"), "the swap point");
    }
}
