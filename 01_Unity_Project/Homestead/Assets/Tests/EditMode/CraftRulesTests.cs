using System.Collections.Generic;
using NUnit.Framework;

// EditMode tests for the Craft tab's "how many can I craft" rule (CraftRules, Homestead.Economy): materials, carry weight, alternative
// ingredient sets, and the stepper's maximum. Pure: no manager, no scene.
public class CraftRulesTests
{
    static CraftIngredient Ing(string id, int quantity, float kg) => new CraftIngredient(id, quantity, kg);

    static IList<IList<CraftIngredient>> One(params CraftIngredient[] set) => new List<IList<CraftIngredient>> { set };

    static Dictionary<string, int> Have(params (string id, int count)[] items)
    {
        var have = new Dictionary<string, int>();
        foreach ((string id, int count) in items)
            have[id] = count;
        return have;
    }

    [Test]
    public void TheMaterialsSetTheLimitWhenThereIsPlentyOfRoom()
    {
        // Kindling Bundle: 10 Sticks (0.1 kg each) into one 1 kg bundle.
        var sets = One(Ing("sticks", 10, 0.1f));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 35)), 1f, 50f, 99), Is.EqualTo(3));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 9)), 1f, 50f, 99), Is.EqualTo(0));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 10)), 1f, 50f, 99), Is.EqualTo(1));
        Assert.That(CraftRules.MaxCrafts(sets, Have(), 1f, 50f, 99), Is.EqualTo(0), "none carried");
    }

    [Test]
    public void TheCapStopsTheRun()
    {
        var sets = One(Ing("sticks", 1, 0.1f));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 500)), 0.1f, 50f, 99), Is.EqualTo(99));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 500)), 0.1f, 50f, 5), Is.EqualTo(5));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 500)), 0.1f, 50f, 0), Is.EqualTo(0));
    }

    [Test]
    public void ACraftThatMakesMoreWeightThanItUsesIsLimitedByTheRoomLeft()
    {
        // Two Clay (1.0 kg each) become a 4 kg lantern: each craft adds 2 kg.
        var sets = One(Ing("clay", 2, 1f));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("clay", 20)), 4f, 6f, 99), Is.EqualTo(3), "6 kg of room, 2 kg a craft");
        Assert.That(CraftRules.MaxCrafts(sets, Have(("clay", 20)), 4f, 5.9f, 99), Is.EqualTo(2));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("clay", 20)), 4f, 1.9f, 99), Is.EqualTo(0), "not even one fits");
        Assert.That(CraftRules.MaxCrafts(sets, Have(("clay", 20)), 4f, 0f, 99), Is.EqualTo(0));
    }

    [Test]
    public void ACraftThatNeverAddsWeightIsNotLimitedByRoom()
    {
        // Ten Sticks (1 kg) into a 1 kg bundle adds nothing; a craft that loses weight frees room.
        Assert.That(CraftRules.MaxCrafts(One(Ing("sticks", 10, 0.1f)), Have(("sticks", 50)), 1f, 0f, 99), Is.EqualTo(5));
        Assert.That(CraftRules.MaxCrafts(One(Ing("logs", 1, 8f)), Have(("logs", 4)), 3f, 0f, 99), Is.EqualTo(4), "it gets lighter");
    }

    [Test]
    public void FreedWeightMakesRoomForTheNextCraft()
    {
        // Each craft adds 0.5 kg (1.5 made, 1.0 used): with 1 kg of room that is 2 crafts, not more.
        var sets = One(Ing("a", 1, 1f));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("a", 10)), 1.5f, 1f, 99), Is.EqualTo(2));
    }

    [Test]
    public void AnExactFitIsNotRefusedByFloatNoise()
    {
        var sets = One(Ing("clay", 2, 1f));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("clay", 2)), 4f, 2f - 0.00005f, 99), Is.EqualTo(1));
    }

    [Test]
    public void AlternativeSetsAreTriedInOrderAndEachCraftUsesTheFirstAffordableOne()
    {
        // Cordage: 3 Tall Grass, or 2 Cattail, or 1 Sinew.
        var sets = new List<IList<CraftIngredient>>
        {
            new[] { Ing("tall_grass", 3, 0.1f) },
            new[] { Ing("cattail", 2, 0.1f) },
            new[] { Ing("sinew", 1, 0.05f) },
        };
        Assert.That(CraftRules.MaxCrafts(sets, Have(("tall_grass", 7)), 0.1f, 50f, 99), Is.EqualTo(2), "7 grass: two crafts, 1 left");
        Assert.That(CraftRules.MaxCrafts(sets, Have(("tall_grass", 7), ("cattail", 5), ("sinew", 3)), 0.1f, 50f, 99), Is.EqualTo(2 + 2 + 3),
                    "the grass runs out after 2, then the cattail 2, then the sinew 3");
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sinew", 4)), 0.1f, 50f, 99), Is.EqualTo(4));
    }

    [Test]
    public void ARecipeWithSeveralIngredientsIsLimitedByTheScarcestOne()
    {
        var sets = One(Ing("sticks", 2, 0.1f), Ing("cordage", 1, 0.05f), Ing("stone", 1, 1f));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 20), ("cordage", 3), ("stone", 9)), 0.5f, 50f, 99), Is.EqualTo(3));
        Assert.That(CraftRules.MaxCrafts(sets, Have(("sticks", 20), ("cordage", 3), ("stone", 2)), 0.5f, 50f, 99), Is.EqualTo(2));
    }

    [Test]
    public void BadInputGivesZeroInsteadOfThrowing()
    {
        Assert.That(CraftRules.MaxCrafts(null, Have(), 1f, 5f, 99), Is.EqualTo(0));
        Assert.That(CraftRules.MaxCrafts(One(Ing("a", 1, 1f)), null, 1f, 5f, 99), Is.EqualTo(0));
        Assert.That(CraftRules.MaxCrafts(new List<IList<CraftIngredient>> { new CraftIngredient[0] }, Have(("a", 5)), 1f, 5f, 99), Is.EqualTo(0),
                    "an empty ingredient set is not a free craft");
    }

    [Test]
    public void TheStepperOffersAtLeastOneWhileTheMaterialsAllowOne()
    {
        Assert.That(CraftRules.StepperMax(5, 3), Is.EqualTo(3));
        Assert.That(CraftRules.StepperMax(2, 7), Is.EqualTo(2));
        Assert.That(CraftRules.StepperMax(5, 0), Is.EqualTo(1), "nothing fits by weight: a single craft keeps the old leave-it-on-the-ground rule");
        Assert.That(CraftRules.StepperMax(0, 4), Is.EqualTo(0), "no materials: the control is greyed out");
        Assert.That(CraftRules.StepperMax(0, 0), Is.EqualTo(0));
    }
}
