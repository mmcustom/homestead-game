using System;
using UnityEngine;

public enum Difficulty { Homesteader, Settler, Pioneer }

[Serializable]
public struct DifficultySaveData
{
    public Difficulty difficulty;
    public bool saved; // false in saves from before difficulty existed, which play as Settler
}

// Difficulty_System.md (confirmed 2026-09-26): three named tiers, picked at New Game and locked for the life of the save.
// Each is a bundle of three things:
//   Starting kit — one full "packed for a week of camping" kit; each harder tier takes things out of it. The Knife comes
//     on every tier.
//       Homesteader: Tent, Sleeping Bag, Tarp, Cooking Pot, Bucket, Axe, Hammer, Pick Axe, Shovel, Canteen, Flint and
//                    Steel, 3 Cordage, Knife and a Rifle with 10 rounds — and $50.
//       Settler:     the same without the Hammer, the Tarp.
//       Pioneer:     a Bucket, 3 Cordage, the Knife and a Bow with 20 arrows. Nothing else, not even a way to light a fire — sell
//                    something at the Trading Post first.
//     The weapon is fixed by the tier, with no choice at New Game (StartingWeapons): the rifle for Homesteader and Settler, the
//     bow for Pioneer.
//   Trading Post — open on every tier (TradingPost); only the savings differ (they replaced the starting cash).
//   Survival severity — how forgiving the survival systems are, as multipliers on their own numbers:
//                  Hunger/Hydration/Warmth drain   weather           wildlife alertness   spoilage (not built yet)
//     Homesteader  x0.75                           +3°C, storms x0.6 x0.85                x0.75
//     Settler      x1 (the baseline)               as is             x1                   x1
//     Pioneer      x1.3                            -3°C, storms x1.5 x1.2                 x1.3
// "Pick Axe" and "Shovel" are the Stone Pick Axe and Primitive Shovel — the only ones that exist until Iron and Steel
// do. Flint and Steel isn't on the doc's list, but the easier kits can't light a fire without it. All numbers are
// Claude Code's first pass, pending Mike's playtest.
public class DifficultyManager : MonoBehaviour, ISaveable
{
    public static DifficultyManager Instance { get; private set; }

    Difficulty current = Difficulty.Settler;

    public Difficulty Current => current;

    // Static reads with the baseline when there's no manager (World played on its own).
    public static Difficulty Tier => Instance != null ? Instance.current : Difficulty.Settler;

    public static float DrainMultiplier => Pick(0.75f, 1f, 1.3f);
    public static float SpoilageMultiplier => Pick(0.75f, 1f, 1.3f);
    public static float TemperatureOffsetC => Pick(3f, 0f, -3f);
    public static float StormMultiplier => Pick(0.6f, 1f, 1.5f);
    public static float WildlifeAlertness => Pick(0.85f, 1f, 1.2f);
    // Savings at a new game, as a percentage of the land price (capped for every tier, LandRules): the player splits them between a
    // down payment and a cash reserve. This replaced the old starting cash ($50, $0, $0).
    public static int SavingsPercent(Difficulty tier) => LandRules.SavingsPercent((int)tier);

    static float Pick(float homesteader, float settler, float pioneer) =>
        Tier == Difficulty.Homesteader ? homesteader : Tier == Difficulty.Pioneer ? pioneer : settler;

    public static string Name(Difficulty tier) => tier.ToString();

    public static string Blurb(Difficulty tier)
    {
        switch (tier)
        {
            case Difficulty.Homesteader:
                return "Packed for a week of camping: tent, sleeping bag, tarp, cooking pot, tripod, bucket, axe, hammer, pick axe, " +
                       "shovel, canteen, flint and steel, knife, cordage and a rifle with 10 rounds — plus savings of 40% of your land's price (up to $30,000). " +
                       "Hunger, thirst and cold come on slowly; the weather is milder and the game less wary.";
            case Difficulty.Pioneer:
                return "A bucket, a knife, a little cordage and a bow with 20 arrows. No shelter, no fire-starter, no savings: the land is fully financed. " +
                       "Sell what you gather at the Trading Post to buy the rest. Hunger, thirst and cold bite harder; the weather is " +
                       "harsher and the game warier.";
            default:
                return "The camping kit, with its rifle and 10 rounds, without the hammer and tarp, and savings of 20% of your land's price (up to $30,000) to split " +
                       "between a down payment and a cash reserve. Survival as it's meant to be.";
        }
    }

    // What a new game on this tier starts carrying: item id and count. The weapon follows from the tier (StartingWeapons).
    public static (string item, int count)[] Kit(Difficulty tier)
    {
        StartingWeapon weapon = StartingWeapons.For((int)tier);
        if (tier == Difficulty.Pioneer)
        {
            var pioneer = new System.Collections.Generic.List<(string, int)> { (WaterSource.BucketItemId, 1), ("cordage", 3), (Knife.Id, 1) };
            pioneer.AddRange(StartingWeapons.Items(weapon));
            return pioneer.ToArray();
        }

        var kit = new System.Collections.Generic.List<(string, int)>
        {
            (WoodManager.TentId, 1), (SleepManager.SleepingBagId, 1), (Cooking.PotId, 1), (TripodRules.TripodId, 1), (WaterSource.BucketItemId, 1),
            (AxeTool.AxeId, 1), (AxeTool.PickAxeId, 1), (AxeTool.ShovelId, 1), (WaterQualities.CanteenId, 1),
            (FireManager.IgnitionId, 1), ("cordage", 3), (Knife.Id, 1),
        };
        kit.AddRange(StartingWeapons.Items(weapon));
        if (tier == Difficulty.Homesteader)
        {
            kit.Add((SleepManager.TarpId, 1));
            kit.Add(("hammer", 1)); // nothing to build with yet (Building_Housing_System.md) — it waits for Building
        }
        return kit.ToArray();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    // New Game only — the tier is locked from then on.
    public void Begin(Difficulty tier) => current = tier;

    // Before loading a save: saves from before difficulty existed have nothing to restore and play as Settler.
    public void ResetForLoad() => current = Difficulty.Settler;

    public DifficultySaveData CaptureState() => new DifficultySaveData { difficulty = current, saved = true };

    public void RestoreState(DifficultySaveData data) => current = data.saved ? data.difficulty : Difficulty.Settler;

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "difficulty";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<DifficultySaveData>(json));
}
