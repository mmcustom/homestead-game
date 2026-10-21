using System;
using System.Collections.Generic;

// A pool's multiplier in a save.
[Serializable]
public struct MarketPoolSave
{
    public string pool;
    public int bp;
}

// The market in a save (player file, key "market"): the multiplier of every pool in basis points and the last game day the market was
// brought up to. Version 0 (no section, or an empty one) means none of it was saved.
[Serializable]
public struct MarketSaveData
{
    public const int CurrentVersion = 1;

    public int version;
    public int lastDay;
    public MarketPoolSave[] pools;
}

// The market's moving part: one multiplier per pool and the day it was last brought up to. Pure and engine-free. Time is state-based:
// Advance(today) applies recovery for every game day since the last one, in order, each with its own season's target, so a sleep that
// skips many days, a debug skip and a load all give the same answer as the days passing one at a time.
public sealed class MarketState
{
    readonly Dictionary<string, int> multipliers = new Dictionary<string, int>();
    bool startAtTargets;

    // The last game day processed, or -1 when none has been (a new state, or an old save with no market).
    public int LastDay { get; private set; } = -1;

    // A pool with no multiplier yet is at normal price.
    public int Get(string pool) => pool != null && multipliers.TryGetValue(pool, out int m) ? m : MarketRules.One;

    public void Set(string pool, int multiplierBp) => multipliers[pool] = multiplierBp;

    // Back to a blank state: every multiplier 1.0 and no day processed. A save with no market section stays like this.
    public void Clear()
    {
        multipliers.Clear();
        LastDay = -1;
        startAtTargets = false;
    }

    // A new game: the first Advance starts every pool at its current-season target.
    public void BeginNewGame()
    {
        Clear();
        startAtTargets = true;
    }

    public void Advance(MarketTable table, int today, int daysPerSeason, int recoveryPercent)
    {
        if (today < 0)
            today = 0;

        if (LastDay < 0)
        {
            if (startAtTargets)
            {
                int start = MarketRules.SeasonOf(today, daysPerSeason);
                foreach (string pool in table.Pools)
                {
                    MarketGroupRule rule = table.RuleOfPool(pool);
                    multipliers[pool] = MarketRules.Clamp(rule.TargetFor(start), rule.floorBp);
                }
                startAtTargets = false;
            }

            LastDay = today; // nothing recovers: there is no earlier day to recover from
            return;
        }

        if (today <= LastDay)
        {
            LastDay = today;
            return;
        }

        for (int day = LastDay + 1; day <= today; day++)
        {
            int season = MarketRules.SeasonOf(day, daysPerSeason);
            foreach (string pool in table.Pools)
            {
                MarketGroupRule rule = table.RuleOfPool(pool);
                multipliers[pool] = MarketRules.Recover(Get(pool), rule.TargetFor(season), recoveryPercent, rule.floorBp);
            }
        }

        LastDay = today;
    }

    // What selling count units would pay and leave the multiplier at, without selling anything.
    public SaleResult Quote(MarketTable table, string itemId, int count)
    {
        long baseCents = table.SellBaseCents(itemId);
        string pool = table.PoolOf(itemId);
        if (baseCents <= 0 || !table.HasPool(pool) || count <= 0)
            return default;
        MarketGroupRule rule = table.RuleOf(itemId);
        return MarketRules.SellStack(baseCents, Get(pool), count, rule.dropBp, rule.floorBp);
    }

    // Sells count units unit by unit and keeps the lower multiplier.
    public SaleResult Sell(MarketTable table, string itemId, int count)
    {
        SaleResult result = Quote(table, itemId, count);
        if (result.count > 0)
            Set(table.PoolOf(itemId), result.newMultiplierBp);
        return result;
    }

    public MarketSaveData Capture(MarketTable table)
    {
        var pools = new MarketPoolSave[table.Pools.Count];
        for (int i = 0; i < pools.Length; i++)
            pools[i] = new MarketPoolSave { pool = table.Pools[i], bp = Get(table.Pools[i]) };
        return new MarketSaveData { version = MarketSaveData.CurrentVersion, lastDay = LastDay, pools = pools };
    }

    // Loads a save: pools the table doesn't have are ignored, pools the save doesn't have stay at normal, every value is kept in range,
    // and a save with no market (version 0) leaves no day processed.
    public void Restore(MarketTable table, MarketSaveData data)
    {
        Clear();
        if (data.version < MarketSaveData.CurrentVersion)
            return;

        LastDay = Math.Max(-1, data.lastDay);
        if (data.pools == null)
            return;
        foreach (MarketPoolSave saved in data.pools)
        {
            if (!table.HasPool(saved.pool))
                continue;
            multipliers[saved.pool] = MarketRules.Clamp(saved.bp, table.RuleOfPool(saved.pool).floorBp);
        }
    }
}
