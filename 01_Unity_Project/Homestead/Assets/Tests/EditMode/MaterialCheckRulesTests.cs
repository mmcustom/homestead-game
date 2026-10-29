using System.Collections.Generic;
using NUnit.Framework;

// EditMode tests for the Craft and Build screen's have / need wording (MaterialCheckRules, Homestead.Economy): the plain-word status of each
// material (HAVE, SHORT n, NEED n), the order, the "Missing: n of m materials" heading, the closest of several ways to make something, the row
// marker, and a staged site's prompt line. Pure: no manager, no scene.
public class MaterialCheckRulesTests
{
    static IList<MaterialLine> Set(params MaterialLine[] lines) => lines;

    static IList<IList<MaterialLine>> Sets(params MaterialLine[][] sets)
    {
        var all = new List<IList<MaterialLine>>();
        foreach (MaterialLine[] set in sets)
            all.Add(set);
        return all;
    }

    [Test]
    public void EnoughInThePackIsHave()
    {
        var line = new MaterialLine("Logs", 3, 3);
        Assert.That(line.Status, Is.EqualTo(MaterialStatus.Have));
        Assert.That(MaterialCheckRules.StatusWord(line), Is.EqualTo("HAVE"));
        Assert.That(MaterialCheckRules.StatusWord(new MaterialLine("Logs", 9, 3)), Is.EqualTo("HAVE"), "more than enough is still HAVE");
    }

    [Test]
    public void SomeButNotEnoughIsShortWithHowManyAreShort()
    {
        var line = new MaterialLine("Logs", 2, 3);
        Assert.That(line.Status, Is.EqualTo(MaterialStatus.Short));
        Assert.That(MaterialCheckRules.StatusWord(line), Is.EqualTo("SHORT 1"));
        Assert.That(MaterialCheckRules.StatusWord(new MaterialLine("Stone", 1, 6)), Is.EqualTo("SHORT 5"));
    }

    [Test]
    public void NoneAtAllIsNeedWithHowManyAreMissing()
    {
        var line = new MaterialLine("Logs", 0, 3);
        Assert.That(line.Status, Is.EqualTo(MaterialStatus.Need));
        Assert.That(MaterialCheckRules.StatusWord(line), Is.EqualTo("NEED 3"));
    }

    [Test]
    public void ANeedOfZeroIsAlwaysHave()
    {
        Assert.That(new MaterialLine("Free", 0, 0).Status, Is.EqualTo(MaterialStatus.Have));
    }

    [Test]
    public void TheWordsNeverUseSymbols()
    {
        foreach (MaterialLine line in new[] { new MaterialLine("Logs", 3, 3), new MaterialLine("Logs", 2, 3), new MaterialLine("Logs", 0, 3), new MaterialLine("Logs", 1, 2, 3) })
            foreach (char c in MaterialCheckRules.Line(line))
                Assert.That(c, Is.LessThan('\u007F'), "plain ASCII only: " + MaterialCheckRules.Line(line));
    }

    [Test]
    public void ALineReadsStatusWordThenTheCounts()
    {
        Assert.That(MaterialCheckRules.Line(new MaterialLine("Logs", 2, 3)), Is.EqualTo("SHORT 1  Logs  2 / 3"));
        Assert.That(MaterialCheckRules.Line(new MaterialLine("Logs", 0, 3)), Is.EqualTo("NEED 3  Logs  0 / 3"));
        Assert.That(MaterialCheckRules.Line(new MaterialLine("Logs", 3, 3)), Is.EqualTo("HAVE  Logs  3 / 3"));
    }

    [Test]
    public void WithASiteNearbyShortIsNeedMinusWhatIsAtTheSite()
    {
        // 2 in the pack, 1 already at the site, 3 needed: short by 2 (the pack is not counted), and the pack covers it.
        var line = new MaterialLine("Logs", 2, 1, 3);
        Assert.That(line.Missing, Is.EqualTo(2));
        Assert.That(MaterialCheckRules.StatusWord(line), Is.EqualTo("SHORT 2"));
        Assert.That(MaterialCheckRules.Counts(line), Is.EqualTo("Logs  in pack 2, at site 1, need 3 (your pack covers it)"));
    }

    [Test]
    public void WithASiteNearbyAnEmptySiteIsNeedEvenIfThePackHasSome()
    {
        var line = new MaterialLine("Logs", 5, 0, 3);
        Assert.That(MaterialCheckRules.StatusWord(line), Is.EqualTo("NEED 3"));
        Assert.That(MaterialCheckRules.Counts(line), Does.EndWith("(your pack covers it)"));
    }

    [Test]
    public void ADeliveredSiteMaterialIsHaveAndThePackNoteIsLeftOff()
    {
        var line = new MaterialLine("Logs", 0, 3, 3);
        Assert.That(MaterialCheckRules.StatusWord(line), Is.EqualTo("HAVE"));
        Assert.That(MaterialCheckRules.Counts(line), Is.EqualTo("Logs  in pack 0, at site 3, need 3"));
    }

    [Test]
    public void ThePackNoteIsLeftOffWhenThePackCannotCoverTheGap()
    {
        var line = new MaterialLine("Logs", 1, 1, 3);
        Assert.That(MaterialCheckRules.Counts(line), Is.EqualTo("Logs  in pack 1, at site 1, need 3"));
    }

    [Test]
    public void WithoutASiteThePackAloneIsCounted()
    {
        var line = new MaterialLine("Logs", 2, 3);
        Assert.That(line.atSite, Is.False);
        Assert.That(line.Counted, Is.EqualTo(2));
    }

    [Test]
    public void ADepthDependentNeedShowsTheRangeButIsJudgedAgainstTheSmallest()
    {
        var line = new MaterialLine("Stone", 4, 6) { needText = "6, 10 or 14 by depth" };
        Assert.That(MaterialCheckRules.Counts(line), Is.EqualTo("Stone  have 4, need 6, 10 or 14 by depth"));
        Assert.That(MaterialCheckRules.StatusWord(line), Is.EqualTo("SHORT 2"));
        Assert.That(MaterialCheckRules.StatusWord(new MaterialLine("Stone", 6, 6) { needText = "6, 10 or 14 by depth" }), Is.EqualTo("HAVE"));
    }

    [Test]
    public void MissingLinesComeFirstThenTheOnesYouHaveEachInTheirOwnOrder()
    {
        var ordered = MaterialCheckRules.MissingFirst(Set(
            new MaterialLine("A", 5, 5), new MaterialLine("B", 0, 2), new MaterialLine("C", 3, 3), new MaterialLine("D", 1, 4)));
        Assert.That(ordered.ConvertAll(l => l.label), Is.EqualTo(new[] { "B", "D", "A", "C" }));
    }

    [Test]
    public void MissingFirstOfNothingIsEmpty()
    {
        Assert.That(MaterialCheckRules.MissingFirst(null), Is.Empty);
        Assert.That(MaterialCheckRules.MissingFirst(Set()), Is.Empty);
    }

    [Test]
    public void TheHeadingCountsTheMissingMaterials()
    {
        var lines = Set(new MaterialLine("A", 5, 5), new MaterialLine("B", 0, 2), new MaterialLine("C", 3, 3), new MaterialLine("D", 1, 4), new MaterialLine("E", 9, 1));
        Assert.That(MaterialCheckRules.MissingCount(lines), Is.EqualTo(2));
        Assert.That(MaterialCheckRules.Heading(lines), Is.EqualTo("Missing: 2 of 5 materials"));
    }

    [Test]
    public void TheHeadingSaysHaveAllWhenCoveredAndIsEmptyWithNoMaterials()
    {
        Assert.That(MaterialCheckRules.Heading(Set(new MaterialLine("A", 1, 1), new MaterialLine("B", 2, 2))), Is.EqualTo("HAVE all 2 materials"));
        Assert.That(MaterialCheckRules.Heading(Set(new MaterialLine("A", 1, 1))), Is.EqualTo("HAVE all 1 material"));
        Assert.That(MaterialCheckRules.Heading(Set(new MaterialLine("A", 0, 1))), Is.EqualTo("Missing: 1 of 1 material"));
        Assert.That(MaterialCheckRules.Heading(Set()), Is.EqualTo(""));
        Assert.That(MaterialCheckRules.Heading(null), Is.EqualTo(""));
    }

    [Test]
    public void TheClosestSetIsTheFirstCoveredOne()
    {
        var sets = Sets(
            new[] { new MaterialLine("Tall Grass", 1, 3) },
            new[] { new MaterialLine("Cattail", 2, 2) },
            new[] { new MaterialLine("Sinew", 1, 1) });
        Assert.That(MaterialCheckRules.Closest(sets), Is.EqualTo(1));
    }

    [Test]
    public void WhenNoSetIsCoveredTheClosestHasTheFewestItemsMissing()
    {
        var sets = Sets(
            new[] { new MaterialLine("Tall Grass", 0, 3) },
            new[] { new MaterialLine("Cattail", 1, 2) },
            new[] { new MaterialLine("Sinew", 0, 1) });
        Assert.That(MaterialCheckRules.Closest(sets), Is.EqualTo(1), "1 Cattail short beats 1 Sinew short only on a tie, and the first of a tie wins");
    }

    [Test]
    public void ATieForClosestGoesToTheFirstSet()
    {
        var sets = Sets(new[] { new MaterialLine("A", 0, 2) }, new[] { new MaterialLine("B", 0, 2) });
        Assert.That(MaterialCheckRules.Closest(sets), Is.EqualTo(0));
    }

    [Test]
    public void ClosestOfNoSetsIsMinusOne()
    {
        Assert.That(MaterialCheckRules.Closest(null), Is.EqualTo(-1));
        Assert.That(MaterialCheckRules.Closest(new List<IList<MaterialLine>>()), Is.EqualTo(-1));
    }

    [Test]
    public void TheRowMarkerFollowsTheClosestSet()
    {
        Assert.That(MaterialCheckRules.RowMarker(Sets(new[] { new MaterialLine("A", 2, 2), new MaterialLine("B", 1, 1) })), Is.EqualTo("HAVE"));
        Assert.That(MaterialCheckRules.RowMarker(Sets(new[] { new MaterialLine("A", 2, 2), new MaterialLine("B", 0, 1) })), Is.EqualTo("SHORT"),
            "something is there but not everything");
        Assert.That(MaterialCheckRules.RowMarker(Sets(new[] { new MaterialLine("A", 0, 2), new MaterialLine("B", 0, 1) })), Is.EqualTo("NEED"),
            "none of it");
        // Either / or: the marker is for the way you are closest to.
        Assert.That(MaterialCheckRules.RowMarker(Sets(new[] { new MaterialLine("Firewood", 0, 3) }, new[] { new MaterialLine("Sticks", 4, 4) })), Is.EqualTo("HAVE"));
    }

    [Test]
    public void ARowWithNothingToCheckHasNoMarker()
    {
        Assert.That(MaterialCheckRules.RowMarker(null), Is.EqualTo(""));
        Assert.That(MaterialCheckRules.RowMarker(Sets()), Is.EqualTo(""));
        Assert.That(MaterialCheckRules.RowMarker(Sets(new MaterialLine[0])), Is.EqualTo(""));
        Assert.That(MaterialCheckRules.Overall(Sets()), Is.Null);
    }

    [Test]
    public void ASitePromptLineShowsDeliveredAndWhatThePackHolds()
    {
        Assert.That(MaterialCheckRules.SiteProgress("Logs", 2, 3, 1), Is.EqualTo("Logs: 2 of 3 delivered, 1 in your pack"));
        Assert.That(MaterialCheckRules.SiteProgress("Logs", 0, 3, 0), Is.EqualTo("Logs: 0 of 3 delivered, none in your pack"));
    }
}
