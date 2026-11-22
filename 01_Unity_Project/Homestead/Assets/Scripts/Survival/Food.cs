using UnityEngine;

// Eating and drinking carried items (the Eating entry, 2026-09-25). Any item with a food effect on its ItemDefinition
// can be consumed from the Inventory screen: one is used up, its Hunger and Hydration go to SurvivalManager, and a
// risky one — raw meat or fish, unpurified water — rolls its chance of making the player sick. Nothing is used up when
// it would do nothing (not hungry, not thirsty).
public static class Food
{
    // Whether consuming this now would restore anything.
    public static bool WouldHelp(ItemDefinition item)
    {
        SurvivalManager survival = SurvivalManager.Instance;
        if (survival == null || item == null || !item.IsFood)
            return false;
        return (item.HungerRestored > 0f && survival.Hunger < SurvivalManager.MaxValue) ||
               (item.HydrationRestored > 0f && survival.Hydration < SurvivalManager.MaxValue);
    }

    // Eats or drinks one of this item from the player's inventory. Returns whether one was consumed.
    public static bool Consume(ItemDefinition item)
    {
        SurvivalManager survival = SurvivalManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        if (survival == null || inventory == null || item == null || !item.IsFood)
            return false;

        if (!WouldHelp(item))
        {
            ToolStatus.Flash(item.IsDrink ? "You're not thirsty" : "You're not hungry");
            return false;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOnConsume(item.IsDrink ? SoundCue.Drink : SoundCue.Eat); // replaces the drop sound
        // The oldest batch is eaten first; its age adds a chance of falling ill (Food_Spoilage_and_Preservation.md: a slight chance,
        // rising the closer it is to spoiling).
        int stamp = -1;
        if (inventory.Player.RemoveBatches(item.Id, 1, (day, count) => stamp = day) != 1)
            return false;
        double ageRisk = item.IsPerishable && stamp >= 0 ? SpoilageRules.IllnessChance(SpoilageManager.FractionUsed(item, stamp)) : 0.0;
        float chance = (float)SpoilageRules.Combine(item.IllnessChance, ageRisk);

        survival.Eat(item.HungerRestored);
        survival.Drink(item.HydrationRestored);

        SurvivalManager.Sickness before = survival.SicknessLevel;
        string verb = item.IsDrink ? "Drank" : "Ate";
        if (survival.RollIllness(chance))
            ToolStatus.Flash($"{verb} {item.DisplayName}{(ageRisk > 0 ? " (it was going off)" : "")} — {SickMessage(before, survival.SicknessLevel)}", 5f);
        else
        {
            // Eaten under a Covered Outdoor Kitchen's roof: a sheltered meal, a small comfort bonus (WellsAndOuthouses.ShelteredMeal).
            string sheltered = WellsAndOuthouses.ShelteredMeal(item);
            ToolStatus.Flash(sheltered == null ? $"{verb} {item.DisplayName}" : $"{verb} {item.DisplayName} - {sheltered}: you feel settled", sheltered == null ? 3f : 4f);
        }
        return true;
    }

    // What falling ill (again) feels like, by how sick the player was and is now.
    public static string SickMessage(SurvivalManager.Sickness before, SurvivalManager.Sickness after)
    {
        if (after == SurvivalManager.Sickness.Severe)
            return before == SurvivalManager.Sickness.Severe ? "you feel even worse" : "you're very sick";
        return before == SurvivalManager.Sickness.None ? "your stomach turns. You're a little sick" : "you feel worse";
    }

    // One word for what the item risks, for the Inventory screen: "raw" meat and fish, "unpurified" water.
    public static string RiskTag(ItemDefinition item)
    {
        if (item == null || item.IllnessChance <= 0f)
            return null;
        return WaterQualities.IsRawWater(item.Id) ? "unpurified" : "raw";
    }
}
