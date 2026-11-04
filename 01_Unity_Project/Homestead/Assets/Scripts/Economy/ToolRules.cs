using System;

// Metalworking, phase M4 (Mike, 2026-11-04): the iron Shovel and the iron Pick Axe. This is the ONE place that says which item ids are a shovel and which are a pick axe, so that every
// place that used to ask "is the equipped tool the Shovel?" asks here, and a later tier (steel) is one more id in a list. Pure and engine-free so edit-mode tests cover it.
//
// The stone Shovel (id "shovel", shown as the Primitive Shovel) and the Stone Pick Axe (id "stone_pick_axe") keep their ids, recipes and every number. The iron ones are made at the
// Smithy (SmithyRules), are for the player's own use (no market row, so the Trading Post never lists them), and need IronToolBlowFactor times the blows of the stone ones on every dig and
// mine, the way the Wood Saw needs 0.65 times the Axe's (AxeRules.DefaultSawFactor). The Axe and the Saw are untouched. There is no wear or durability: no such system exists.
public static class ToolRules
{
    public const string StoneShovelId = "shovel";
    public const string IronShovelId = "iron_shovel";
    public const string StonePickAxeId = "stone_pick_axe";
    public const string IronPickAxeId = "iron_pick_axe";

    // Every id that digs like a shovel, and every id that mines like a pick axe, the stone one first. Add a later tier here and every check follows.
    public static readonly string[] ShovelIds = { StoneShovelId, IronShovelId };
    public static readonly string[] PickAxeIds = { StonePickAxeId, IronPickAxeId };
    public static readonly string[] IronToolIds = { IronShovelId, IronPickAxeId };

    // Iron needs this fraction of the stone tools' blows: 25% fewer. The AxeTool component carries the live value (a tunable, as the Saw's); this is its default.
    public const float DefaultIronToolBlowFactor = 0.75f;

    public static bool IsShovel(string itemId) => itemId != null && Array.IndexOf(ShovelIds, itemId) >= 0;

    public static bool IsPickAxe(string itemId) => itemId != null && Array.IndexOf(PickAxeIds, itemId) >= 0;

    public static bool IsDiggingTool(string itemId) => IsShovel(itemId) || IsPickAxe(itemId);

    public static bool IsIron(string itemId) => itemId != null && Array.IndexOf(IronToolIds, itemId) >= 0;

    // Whether the pack holds any shovel (the test is passed in so this stays engine-free).
    public static bool HasShovel(Func<string, bool> has)
    {
        foreach (string id in ShovelIds)
            if (has(id))
                return true;
        return false;
    }

    // The blows multiplier of a digging or mining tool: 1 for the stone ones (and for anything that is not a digging tool), the iron factor for the iron ones.
    public static float BlowFactor(string toolId, float ironFactor) => IsDiggingTool(toolId) && IsIron(toolId) ? ironFactor : 1f;

    // The blows one dig or mine takes with this tool: the base for the action times the tool's factor. Left fractional (4 x 0.75 = 3, 6 x 0.75 = 4.5): the swing adds a whole blow at full
    // work efficiency and the job completes once the sum reaches the number, so 4.5 lands on the 5th blow.
    public static float BlowsNeeded(float baseBlows, string toolId, float ironFactor) => baseBlows * BlowFactor(toolId, ironFactor);

    // How much one swing counts when the action's blow count is fixed elsewhere (a Well shaft or an Outhouse pit): the work efficiency divided by the factor, so 0.75 needs three quarters of the swings.
    public static float SwingWeight(float efficiency, string toolId, float ironFactor)
    {
        float factor = BlowFactor(toolId, ironFactor);
        return factor > 0f ? efficiency / factor : efficiency;
    }

    // The whole swings an action of this many base blows takes with this tool at full work efficiency (what a player counts): the least whole number that reaches the needed blows.
    public static int SwingsAtFullEfficiency(float baseBlows, string toolId, float ironFactor) =>
        Math.Max(1, (int)Math.Ceiling(BlowsNeeded(baseBlows, toolId, ironFactor) - 0.0001f));
}
