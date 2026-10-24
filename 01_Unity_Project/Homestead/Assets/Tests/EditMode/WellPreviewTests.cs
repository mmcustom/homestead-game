using System.Collections.Generic;
using NUnit.Framework;

// EditMode tests for Well Water Table B2: the Well Site ghost's text and colour level (WellPreview, Homestead.Setbacks). Pure: the scene lookups
// (ground height, water surface, distances) are play-checked instead.
public class WellPreviewTests
{
    const float Reach = 40f; // WellsAndOuthouses.wellFloodReachMetres

    // A depth answer from a ground height above the water at the default slope: depth = 1.5 + 0.5 * above.
    static WellWaterResult AtDepth(float depth) => WellWater.Compute(10f + (depth - 1.5f) / 0.5f, 10f);

    static PreviewHazard Hazard(string name, float metres) => new PreviewHazard { name = name, metres = metres };

    [Test]
    public void ALegalSpotFarFromTheCreekIsClearWithTheDepthLine()
    {
        WellPreviewResult r = WellPreview.Build(null, AtDepth(4f), 120f, Reach, new List<PreviewHazard>());
        Assert.That(r.level, Is.EqualTo(PreviewLevel.Clear));
        Assert.That(r.text, Is.EqualTo("Depth to water: 4.0 m (Medium)"));
    }

    [Test]
    public void ALegalSpotThirtyMetresFromTheCreekIsAmberWithTheFloodWarning()
    {
        WellPreviewResult r = WellPreview.Build(null, AtDepth(4f), 30f, Reach, new List<PreviewHazard>());
        Assert.That(r.level, Is.EqualTo(PreviewLevel.Warning));
        Assert.That(r.text, Does.Contain("Floods can foul this well here"));
        Assert.That(r.text, Does.Contain("30 m away"));
        Assert.That(r.text, Does.Contain("reaches 40 m"));
        Assert.That(r.text, Does.Contain("Higher ground is safer but deeper."));
        Assert.That(r.text, Does.StartWith("Depth to water: 4.0 m (Medium)"));
    }

    [Test]
    public void ARefusedSpotIsRedAndKeepsItsOwnReason()
    {
        WellPreviewResult r = WellPreview.Build("This ground floods.", AtDepth(2f), 20f, Reach, new List<PreviewHazard>());
        Assert.That(r.level, Is.EqualTo(PreviewLevel.Refused));
        Assert.That(r.text, Is.EqualTo("This ground floods."));
    }

    [Test]
    public void ABrokenSetbackIsRedWithItsReasonWhateverElseIsTrue()
    {
        WellPreviewResult r = WellPreview.Build("Too close to the outhouse: 30 m needed.", AtDepth(4f), 120f, Reach, new List<PreviewHazard>());
        Assert.That(r.level, Is.EqualTo(PreviewLevel.Refused));
        Assert.That(r.text, Is.EqualTo("Too close to the outhouse: 30 m needed."));
    }

    [Test]
    public void TooDeepIsRedWithTheDrilledWellMessage()
    {
        WellWaterResult tooDeep = AtDepth(9.5f);
        Assert.That(tooDeep.tier, Is.EqualTo(WellTier.TooDeep));

        WellPreviewResult r = WellPreview.Build(null, tooDeep, 120f, Reach, new List<PreviewHazard>());
        Assert.That(r.level, Is.EqualTo(PreviewLevel.Refused));
        Assert.That(r.text, Is.EqualTo("The water is too deep to dig by hand here (a drilled well needs a rig)."));
        Assert.That(r.text, Is.EqualTo(WellPreview.TooDeepMessage));
    }

    [Test]
    public void ExactlyAtTheFloodReachLineStillWarnsAndJustBeyondDoesNot()
    {
        Assert.That(WellPreview.Build(null, AtDepth(4f), 40f, Reach, null).level, Is.EqualTo(PreviewLevel.Warning), "on the 40 m line");
        Assert.That(WellPreview.Build(null, AtDepth(4f), 40.01f, Reach, null).level, Is.EqualTo(PreviewLevel.Clear), "just past it");
        Assert.That(WellPreview.FloodWarning(40f, Reach), Is.True);
        Assert.That(WellPreview.FloodWarning(40.01f, Reach), Is.False);
    }

    [Test]
    public void TheReachIsPassedInNotBakedIn()
    {
        Assert.That(WellPreview.Build(null, AtDepth(4f), 50f, 60f, null).level, Is.EqualTo(PreviewLevel.Warning));
        Assert.That(WellPreview.Build(null, AtDepth(4f), 50f, 40f, null).level, Is.EqualTo(PreviewLevel.Clear));
    }

    [Test]
    public void AShallowSpotGetsTheHintEvenWithoutAFloodWarningAndADeepOneDoesNot()
    {
        WellPreviewResult shallow = WellPreview.Build(null, AtDepth(2f), 120f, Reach, null);
        Assert.That(shallow.level, Is.EqualTo(PreviewLevel.Clear));
        Assert.That(shallow.text, Does.Contain("Higher ground is safer but deeper."));

        WellPreviewResult deep = WellPreview.Build(null, AtDepth(7.5f), 120f, Reach, null);
        Assert.That(deep.text, Does.Not.Contain("Higher ground"));
        Assert.That(deep.text, Is.EqualTo("Depth to water: 7.5 m (Deep)"));
    }

    [Test]
    public void NearbyHazardsListTheNearestThreeWithinRangeNearestFirst()
    {
        var hazards = new List<PreviewHazard>
        {
            Hazard("the creek", 31f), Hazard("outhouse", 42f), Hazard("Bass Hole pond", 80f), Hazard("Spring Hollow", 95f), Hazard("animal pen", 150f),
        };
        Assert.That(WellPreview.NearbyLine(hazards), Is.EqualTo("Nearby: the creek 31 m, outhouse 42 m, Bass Hole pond 80 m"));
        Assert.That(WellPreview.NearbyLine(new List<PreviewHazard> { Hazard("animal pen", 150f) }), Is.EqualTo(""), "out of range");
        Assert.That(WellPreview.NearbyLine(null), Is.EqualTo(""));
    }

    [Test]
    public void TheNearbyLineSitsBetweenTheDepthAndTheWarning()
    {
        WellPreviewResult r = WellPreview.Build(null, AtDepth(4f), 30f, Reach, new List<PreviewHazard> { Hazard("the creek", 30f) });
        string[] lines = r.text.Split('\n');
        Assert.That(lines.Length, Is.EqualTo(4));
        Assert.That(lines[0], Does.StartWith("Depth to water"));
        Assert.That(lines[1], Does.StartWith("Nearby: the creek 30 m"));
        Assert.That(lines[2], Does.StartWith("Floods can foul"));
        Assert.That(lines[3], Does.StartWith("Higher ground"));
    }

    [Test]
    public void WithNoWaterFoundTheDepthLineIsLeftOutAndTheRestStillWorks()
    {
        WellPreviewResult r = WellPreview.Build(null, null, 30f, Reach, null);
        Assert.That(r.level, Is.EqualTo(PreviewLevel.Warning));
        Assert.That(r.text, Does.Not.Contain("Depth to water"));
        Assert.That(r.text, Does.StartWith("Floods can foul"));
    }

    [Test]
    public void ThePreviewNeverShowsTheDigStoneOrYieldNumbersThatB3HasNotWiredYet()
    {
        foreach (float depth in new[] { 2f, 4f, 7.5f })
        {
            string text = WellPreview.Build(null, AtDepth(depth), 30f, Reach, new List<PreviewHazard> { Hazard("outhouse", 50f) }).text.ToLowerInvariant();
            Assert.That(text, Does.Not.Contain("blow"));
            Assert.That(text, Does.Not.Contain("stone"));
            Assert.That(text, Does.Not.Contain("per hour"));
            Assert.That(text, Does.Not.Contain(" l "));
        }
    }
}
