using System.Collections.Generic;
using NUnit.Framework;

// EditMode tests for Maple D: which Maples get a marker, how it fades and when it shows. All pure (MapleMarkerRules, Homestead.Economy).
public class MapleMarkerTests
{
    // Maples along the x axis at the given distances from the origin.
    static List<(float x, float z)> Line(params float[] distances)
    {
        var list = new List<(float x, float z)>();
        foreach (float d in distances)
            list.Add((d, 0f));
        return list;
    }

    static List<int> Near(List<(float x, float z)> positions, int max = 24, System.Func<int, bool> excluded = null) =>
        MapleMarkerRules.Nearest(positions, 0f, 0f, 0f, 50f, max, excluded);

    [Test]
    public void NoMaplesGiveNoMarkers()
    {
        Assert.That(Near(new List<(float x, float z)>()), Is.Empty);
        Assert.That(MapleMarkerRules.Nearest(null, 0f, 0f, 0f, 50f, 24), Is.Empty);
    }

    [Test]
    public void OnlyMaplesWithinTheRadiusAreMarkedAndTheEdgeCounts()
    {
        List<int> found = Near(Line(10f, 49.9f, 50f, 50.1f, 200f));
        Assert.That(found, Is.EquivalentTo(new[] { 0, 1, 2 }));
    }

    [Test]
    public void TheResultIsNearestFirst()
    {
        Assert.That(Near(Line(30f, 5f, 20f, 45f, 10f)), Is.EqualTo(new[] { 1, 4, 2, 0, 3 }));
    }

    [Test]
    public void NoMoreThanMaxAreMarkedAndTheNearestWin()
    {
        var far = new List<float>();
        for (int i = 1; i <= 40; i++)
            far.Add(i);
        List<int> found = Near(Line(far.ToArray()), 24);
        Assert.That(found.Count, Is.EqualTo(24));
        Assert.That(found[0], Is.EqualTo(0));
        Assert.That(found[23], Is.EqualTo(23));
        Assert.That(Near(Line(5f, 6f), 24).Count, Is.EqualTo(2), "fewer in range than the cap: all of them");
        Assert.That(Near(Line(5f, 6f), 0), Is.Empty);
    }

    [Test]
    public void ExcludedMaplesNeverAppearAndFartherOnesTakeTheirPlace()
    {
        List<(float x, float z)> line = Line(5f, 10f, 15f, 20f);
        List<int> found = Near(line, 2, i => i == 0);
        Assert.That(found, Is.EqualTo(new[] { 1, 2 }), "the tapped nearest is skipped and the next two fill the cap");
    }

    [Test]
    public void EqualDistancesGoToTheLowerIndex()
    {
        var ring = new List<(float x, float z)> { (0f, 20f), (20f, 0f), (-20f, 0f), (0f, -20f) };
        Assert.That(Near(ring, 2), Is.EqualTo(new[] { 0, 1 }));
        Assert.That(Near(ring, 2), Is.EqualTo(Near(ring, 2)));
    }

    [Test]
    public void OnlyFlatDistanceCounts()
    {
        // The positions carry no height at all: a Maple 30 m away on the ground plane is 30 m away.
        var positions = new List<(float x, float z)> { (18f, 24f) };
        Assert.That(MapleMarkerRules.Nearest(positions, 0f, 0f, 0f, 30f, 5), Is.EqualTo(new[] { 0 }));
        Assert.That(MapleMarkerRules.Nearest(positions, 0f, 0f, 0f, 29.9f, 5), Is.Empty);
    }

    [Test]
    public void AMapleCloserThanTheMinimumIsLeftOut()
    {
        List<(float x, float z)> line = Line(2f, 3.9f, 4f, 9f);
        List<int> found = MapleMarkerRules.Nearest(line, 0f, 0f, MapleMarkerRules.HideUnderMetres, MapleMarkerRules.RadiusMetres, 24);
        Assert.That(found, Is.EqualTo(new[] { 2, 3 }), "exactly 4 m still counts; 3.9 m does not");
    }

    [Test]
    public void TheMarkersFollowThePlayer()
    {
        List<(float x, float z)> line = Line(0f, 100f);
        Assert.That(MapleMarkerRules.Nearest(line, 0f, 0f, 0f, 50f, 5), Is.EqualTo(new[] { 0 }));
        Assert.That(MapleMarkerRules.Nearest(line, 90f, 0f, 0f, 50f, 5), Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void TheMarkerIsFullStrengthToFiftyFiveMetresThenFadesToNothingAtEighty()
    {
        Assert.That(MapleMarkerRules.Fade(0f), Is.EqualTo(1f));
        Assert.That(MapleMarkerRules.Fade(55f), Is.EqualTo(1f));
        Assert.That(MapleMarkerRules.Fade(67.5f), Is.EqualTo(0.5f).Within(1e-4f));
        Assert.That(MapleMarkerRules.Fade(80f), Is.EqualTo(0f));
        Assert.That(MapleMarkerRules.Fade(120f), Is.EqualTo(0f));
        Assert.That(MapleMarkerRules.Fade(60f), Is.GreaterThan(MapleMarkerRules.Fade(70f)));
    }

    [Test]
    public void ATapMarkerIsFullStrengthToSeventyFiveMetresThenFadesToNothingAtAHundred()
    {
        Assert.That(MapleMarkerRules.TapFade(0f), Is.EqualTo(1f));
        Assert.That(MapleMarkerRules.TapFade(75f), Is.EqualTo(1f));
        Assert.That(MapleMarkerRules.TapFade(87.5f), Is.EqualTo(0.5f).Within(1e-4f));
        Assert.That(MapleMarkerRules.TapFade(100f), Is.EqualTo(0f));
        Assert.That(MapleMarkerRules.TapFade(250f), Is.EqualTo(0f));
        Assert.That(MapleMarkerRules.TapFade(80f), Is.GreaterThan(MapleMarkerRules.TapFade(90f)));
    }

    [Test]
    public void TheFadeTakesAnyPairOfLimits()
    {
        Assert.That(MapleMarkerRules.Fade(15f, 10f, 20f), Is.EqualTo(0.5f).Within(1e-4f));
        Assert.That(MapleMarkerRules.Fade(10f, 10f, 20f), Is.EqualTo(1f));
        Assert.That(MapleMarkerRules.Fade(20f, 10f, 20f), Is.EqualTo(0f));
    }

    // --- A tap's marker state ---

    [TestCase(0f, false, MapleMarkerRules.TapMarkerState.Empty)]
    [TestCase(0.01f, false, MapleMarkerRules.TapMarkerState.Filling)]
    [TestCase(2f, false, MapleMarkerRules.TapMarkerState.Filling)]
    [TestCase(3.99f, false, MapleMarkerRules.TapMarkerState.Filling)]
    [TestCase(4f, false, MapleMarkerRules.TapMarkerState.Ready)]
    [TestCase(8f, false, MapleMarkerRules.TapMarkerState.Ready)]
    [TestCase(0f, true, MapleMarkerRules.TapMarkerState.Sour)]
    [TestCase(2f, true, MapleMarkerRules.TapMarkerState.Sour)]
    public void ATapsMarkerFollowsItsLitresAndTheSourFlag(float litres, bool sour, MapleMarkerRules.TapMarkerState expected)
    {
        Assert.That(MapleMarkerRules.StateOf(litres, sour), Is.EqualTo(expected));
    }

    [Test]
    public void SourWinsOverReadyEvenWithAFullBucket()
    {
        Assert.That(MapleMarkerRules.StateOf(8f, true), Is.EqualTo(MapleMarkerRules.TapMarkerState.Sour));
        Assert.That(MapleMarkerRules.StateOf(TapRules.ReadyLitres, true), Is.EqualTo(MapleMarkerRules.TapMarkerState.Sour));
    }

    [Test]
    public void ReadyStartsAtTheSameLitresAsTheSapReadyMessage()
    {
        Assert.That(MapleMarkerRules.StateOf(TapRules.ReadyLitres, false), Is.EqualTo(MapleMarkerRules.TapMarkerState.Ready));
        Assert.That(MapleMarkerRules.StateOf(TapRules.ReadyLitres - 0.01f, false), Is.EqualTo(MapleMarkerRules.TapMarkerState.Filling));
        Assert.That(TapRules.BecameReady(3.9f, 4.1f), Is.True);
    }

    [Test]
    public void TheTapRangeIsLongerThanTheUntappedRangeAndHidesVeryNear()
    {
        Assert.That(MapleMarkerRules.TapRadiusMetres, Is.EqualTo(100f));
        Assert.That(MapleMarkerRules.TapRadiusMetres, Is.GreaterThan(MapleMarkerRules.RadiusMetres));
        Assert.That(MapleMarkerRules.TapFadeStartMetres, Is.LessThan(MapleMarkerRules.TapRadiusMetres));
        Assert.That(MapleMarkerRules.TapHideUnderMetres, Is.EqualTo(MapleMarkerRules.HideUnderMetres));
    }

    [Test]
    public void ATappedTreeIsNeverInTheUntappedSet()
    {
        // The HUD excludes a tapped tree's index, so one tree can never carry a droplet and a pail.
        List<(float x, float z)> line = Line(10f, 20f, 30f);
        List<int> untapped = Near(line, 30, i => i == 1);
        Assert.That(untapped, Has.No.Member(1));
        Assert.That(untapped, Is.EqualTo(new[] { 0, 2 }));
    }

    [Test]
    public void MarkersShowOnlyWithASpileToolsUsableAndATapToSpare()
    {
        Assert.That(MapleMarkerRules.ShouldShow(true, true, 0), Is.True);
        Assert.That(MapleMarkerRules.ShouldShow(true, true, 11), Is.True);
        Assert.That(MapleMarkerRules.ShouldShow(false, true, 0), Is.False, "no Spile equipped");
        Assert.That(MapleMarkerRules.ShouldShow(true, false, 0), Is.False, "a menu, pause or placing something");
        Assert.That(MapleMarkerRules.ShouldShow(true, true, MapleRules.MaxTaps), Is.False, "the 12-tap cap hides them");
        Assert.That(MapleMarkerRules.ShouldShow(true, true, MapleRules.MaxTaps + 3), Is.False);
    }

    [Test]
    public void AMarkerIsNotDrawnBehindTheCameraNearAnEdgeOrOnTheCrosshair()
    {
        const float w = 1920f, h = 1080f;
        Assert.That(MapleMarkerRules.ScreenVisible(0.8f, 0.6f, 20f, w, h), Is.True);
        Assert.That(MapleMarkerRules.ScreenVisible(0.8f, 0.6f, -5f, w, h), Is.False, "behind the camera");
        Assert.That(MapleMarkerRules.ScreenVisible(0.8f, 0.6f, 0f, w, h), Is.False);
        Assert.That(MapleMarkerRules.ScreenVisible(0.02f, 0.6f, 20f, w, h), Is.False, "inside the left margin");
        Assert.That(MapleMarkerRules.ScreenVisible(0.5f, 0.98f, 20f, w, h), Is.False, "inside the top margin");
        Assert.That(MapleMarkerRules.ScreenVisible(1.2f, 0.5f, 20f, w, h), Is.False, "off the screen");
        Assert.That(MapleMarkerRules.ScreenVisible(0.5f, 0.5f, 20f, w, h), Is.False, "on the crosshair");
        Assert.That(MapleMarkerRules.ScreenVisible(0.5f + 40f / w, 0.5f, 20f, w, h), Is.False, "40 px from the middle");
        Assert.That(MapleMarkerRules.ScreenVisible(0.5f + 80f / w, 0.5f, 20f, w, h), Is.True, "80 px from the middle");
    }

    [Test]
    public void TheConstantsAreInOrder()
    {
        Assert.That(MapleMarkerRules.MaxMarkers, Is.EqualTo(30));
        Assert.That(MapleMarkerRules.RadiusMetres, Is.EqualTo(80f));
        Assert.That(MapleMarkerRules.HideUnderMetres, Is.LessThan(MapleMarkerRules.FadeStartMetres));
        Assert.That(MapleMarkerRules.FadeStartMetres, Is.LessThan(MapleMarkerRules.RadiusMetres));
        Assert.That(MapleMarkerRules.FallbackTreeHeightMetres, Is.GreaterThan(0f));
    }
}
