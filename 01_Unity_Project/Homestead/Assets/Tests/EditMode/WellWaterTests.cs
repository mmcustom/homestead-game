using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Well Water Table B1: the pure depth-to-water helper (Homestead.Setbacks). The Medium tier must equal the numbers the
// Well has today, which live on the WellsAndOuthouses component (main assembly, reached by reflection on a throwaway object).
public class WellWaterTests
{
    // Ground height above the water that gives an exact depth at the default slope: depth = 1.5 + 0.5 * above.
    static WellWaterResult AtDepth(float depth) => WellWater.Compute(10f + (depth - 1.5f) / 0.5f, 10f);

    [Test]
    public void DepthIsOnePointFiveAtTheWaterPlusHalfAMetrePerMetreAbove()
    {
        Assert.That(WellWater.Compute(10f, 10f).depthToWater, Is.EqualTo(1.5f));
        Assert.That(WellWater.Compute(14f, 10f).depthToWater, Is.EqualTo(3.5f).Within(0.0001f));
        Assert.That(WellWater.Compute(24.2f, 10f).depthToWater, Is.EqualTo(8.6f).Within(0.0001f), "the cabin ridge, about 14.2 m up");
    }

    [Test]
    public void GroundAtOrBelowTheWaterGetsTheBaseDepth()
    {
        Assert.That(WellWater.Compute(9.5f, 10f).depthToWater, Is.EqualTo(1.5f));
        Assert.That(WellWater.Compute(-3f, 10f).depthToWater, Is.EqualTo(1.5f));
        Assert.That(WellWater.Compute(9.5f, 10f).tier, Is.EqualTo(WellTier.Shallow));
    }

    [Test]
    public void TierBoundariesAreExactlyThreeAndSixMetresOfDepth()
    {
        Assert.That(AtDepth(2.999f).tier, Is.EqualTo(WellTier.Shallow));
        Assert.That(AtDepth(3f).tier, Is.EqualTo(WellTier.Medium), "exactly 3 m is Medium");
        Assert.That(AtDepth(5.999f).tier, Is.EqualTo(WellTier.Medium));
        Assert.That(AtDepth(6f).tier, Is.EqualTo(WellTier.Deep), "exactly 6 m is Deep");
        Assert.That(AtDepth(3f).depthToWater, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(AtDepth(6f).depthToWater, Is.EqualTo(6f).Within(0.0001f));
    }

    [Test]
    public void TheCapIsNineMetresAndOnlyBeyondItIsTooDeep()
    {
        Assert.That(AtDepth(9f).tier, Is.EqualTo(WellTier.Deep), "exactly the cap can still be dug");
        Assert.That(AtDepth(9f).Diggable, Is.True);
        WellWaterResult deeper = AtDepth(9.01f);
        Assert.That(deeper.tier, Is.EqualTo(WellTier.TooDeep));
        Assert.That(deeper.Diggable, Is.False);
        Assert.That(deeper.depthToWater, Is.EqualTo(9.01f).Within(0.0001f), "the real depth is reported, not the cap");
        Assert.That(deeper.digBlows, Is.EqualTo(0));
        Assert.That(deeper.liningStone, Is.EqualTo(0));
        Assert.That(deeper.bufferLitres, Is.EqualTo(0));
        Assert.That(deeper.recoveryLitresPerHour, Is.EqualTo(0f));
    }

    [Test]
    public void EachTierHasTheDocumentedNumbers()
    {
        WellWaterResult shallow = AtDepth(2f);
        Assert.That((shallow.digBlows, shallow.liningStone, shallow.bufferLitres, shallow.recoveryLitresPerHour), Is.EqualTo((20, 6, 200, 8f)));

        WellWaterResult medium = AtDepth(4.5f);
        Assert.That((medium.digBlows, medium.liningStone, medium.bufferLitres, medium.recoveryLitresPerHour), Is.EqualTo((40, 10, 150, 6f)));

        WellWaterResult deep = AtDepth(7.5f);
        Assert.That((deep.digBlows, deep.liningStone, deep.bufferLitres, deep.recoveryLitresPerHour), Is.EqualTo((60, 14, 120, 4.5f)));
        Assert.That(deep.droughtRecoveryLitresPerHour, Is.EqualTo(4.5f), "Mike's Deep row, 2026-10-24");
    }

    [Test]
    public void DroughtLowersTheShallowAndMediumRatesAndLeavesTheDeepOneAlone()
    {
        WellWaterResult shallow = AtDepth(2f), medium = AtDepth(4.5f), deep = AtDepth(7.5f);
        Assert.That(shallow.droughtRecoveryLitresPerHour, Is.LessThan(shallow.recoveryLitresPerHour));
        Assert.That(medium.droughtRecoveryLitresPerHour, Is.EqualTo(3f), "today's drought rate");
        Assert.That(deep.droughtRecoveryLitresPerHour, Is.EqualTo(deep.recoveryLitresPerHour), "a deep well keeps working in a drought");
    }

    [Test]
    public void ADepthOfZeroIsAnOldWellWithTodaysFixedNumbers()
    {
        foreach (WellWaterResult old in new[] { WellWater.ForDepth(0f), WellWater.Legacy(), WellWater.ForDepth(-1f) })
        {
            Assert.That(old.tier, Is.EqualTo(WellTier.Legacy));
            Assert.That(old.depthToWater, Is.EqualTo(0f));
            Assert.That(old.Diggable, Is.True);
            Assert.That((old.digBlows, old.liningStone, old.bufferLitres, old.recoveryLitresPerHour, old.droughtRecoveryLitresPerHour),
                        Is.EqualTo((40, 10, 150, 6f, 3f)));
        }
    }

    [Test]
    public void AComputedDepthIsNeverZeroSoItNeverLooksLikeAnOldWell()
    {
        Assert.That(WellWater.Compute(0f, 100f).depthToWater, Is.GreaterThan(0f));
        Assert.That(WellWater.Compute(0f, 100f).tier, Is.Not.EqualTo(WellTier.Legacy));
    }

    [Test]
    public void TheMediumAndLegacyTiersEqualWhatTheWellComponentHasToday()
    {
        Type wells = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
            .First(t => t.Name == "WellsAndOuthouses");
        var go = new GameObject("test wells");
        try
        {
            Component c = go.AddComponent(wells);
            Func<string, object> field = name => wells.GetField(name, BindingFlags.Public | BindingFlags.Instance).GetValue(c);
            WellWaterRules rules = WellWaterRules.Defaults;
            foreach (WellTierNumbers tier in new[] { rules.medium, rules.legacy })
            {
                Assert.That(tier.digBlows, Is.EqualTo(field("wellDigBlows")));
                Assert.That(tier.liningStone, Is.EqualTo(field("wellLiningStone")));
                Assert.That(tier.bufferLitres, Is.EqualTo(field("wellBufferLitres")));
                Assert.That(tier.recoveryLitresPerHour, Is.EqualTo(field("wellRecoveryLitresPerHour")));
                Assert.That(tier.droughtRecoveryLitresPerHour, Is.EqualTo(field("wellDroughtRecoveryLitresPerHour")));
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void TheSlopeIsTunableThroughTheRulesTable()
    {
        WellWaterRules steeper = WellWaterRules.Defaults;
        steeper.slopeMetresPerMetre = 0.8f;
        Assert.That(WellWater.Compute(24.2f, 10f, steeper).tier, Is.EqualTo(WellTier.TooDeep), "0.8 refuses the cabin ridge, as the read-only pass found");
        Assert.That(WellWater.Compute(24.2f, 10f).tier, Is.EqualTo(WellTier.Deep), "0.5 does not");
    }
}
