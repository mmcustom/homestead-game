using System;
using System.Collections.Generic;

// Money_System.md Phase C: the loss, pure and engine-free so edit-mode tests cover it. Money is long cents (Money); nothing here uses
// float or double.

public static class LossRules
{
    // Three missed payments open at once lose the game.
    public const int DefaultThreshold = 3;

    // The loss: the number of missed payments that are STILL OPEN (LoanState.MissedCount) reaches the threshold. Catching up clears a
    // missed payment, so this is open payments, not payments ever missed. A threshold of 0 or less turns the loss off.
    public static bool IsLoss(int openMissedCount, int threshold) => threshold > 0 && openMissedCount >= threshold;

    // The line under "Mother Nature beat you.": "Three payments on the land went unpaid."
    public static string MissLine(int threshold)
    {
        string[] words = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
        string count = threshold >= 0 && threshold < words.Length ? words[threshold] : threshold.ToString();
        return $"{count} payment{(threshold == 1 ? "" : "s")} on the land went unpaid.";
    }
}

// Money_System.md "savings go to the debt on a loss": on a loss all the cash goes against everything owed on the loan, in the order a
// payment is applied (late fees, interest, principal, then any rest as extra principal). It is worked out on a COPY of the loan, for
// display only: the real loan, the real cash and the ledger never change.
public struct LossSettlement
{
    public long cashHad;       // the cash at the moment of the loss
    public long taken;         // what the cash paid against the debt
    public long stillOwed;     // everything still owed after that: principal plus the interest and late fees on open payments
    public long cashLeft;      // cash the debt could not take (only when the cash was more than everything owed)

    public static LossSettlement Of(LoanState loan, long cashCents)
    {
        long cash = Math.Max(0L, cashCents);
        if (loan == null)
            return new LossSettlement { cashHad = cash, cashLeft = cash };

        LoanState copy = loan.Copy();
        PayResult paid = copy.Pay(cash, cash); // exactly what a payment of all the cash would do
        return new LossSettlement
        {
            cashHad = cash,
            taken = paid.applied,
            stillOwed = copy.MaxPayableCents,
            cashLeft = cash - paid.applied,
        };
    }
}

// What the loss screen says about the run, built from plain numbers at the moment of the loss.
public struct RunSummary
{
    public int daysSurvived;        // game days since the start (TotalDays)
    public int seasonsCompleted;    // whole seasons behind the player
    public long incomeCents;        // everything earned since the start (the ledger)
    public long expensesCents;      // everything spent since the start (the ledger), a positive amount
    public long balanceCents;       // the land loan still owed
    public long cashCents;          // cash at the end
    public int[] missedDueDays;     // the due days (TotalDays) of the missed payments still open, oldest first
    public long takenCents;         // the cash that went against the debt on the loss (LossSettlement), display only
    public long owedAfterCents;     // everything still owed after that
    public long cashLeftCents;      // cash the debt could not take

    // The same summary with the loss settlement filled in: cash had, taken, still owed, cash left.
    public RunSummary WithSettlement(LossSettlement s)
    {
        RunSummary copy = this;
        copy.cashCents = s.cashHad;
        copy.takenCents = s.taken;
        copy.owedAfterCents = s.stillOwed;
        copy.cashLeftCents = s.cashLeft;
        return copy;
    }

    public static RunSummary Build(int totalDays, int daysPerSeason, long incomeCents, long expensesCents, long balanceCents,
                                   IEnumerable<int> missedDueDays, long cashCents)
    {
        var days = new List<int>();
        if (missedDueDays != null)
            days.AddRange(missedDueDays);
        days.Sort();

        return new RunSummary
        {
            daysSurvived = Math.Max(0, totalDays),
            seasonsCompleted = LedgerState.SeasonOf(totalDays, daysPerSeason),
            incomeCents = Math.Max(0L, incomeCents),
            expensesCents = Math.Max(0L, expensesCents),
            balanceCents = Math.Max(0L, balanceCents),
            cashCents = cashCents,
            missedDueDays = days.ToArray(),
        };
    }
}

// How a game began, so Restart can start the same one again: the difficulty picked and the land purchase (parcel and the savings
// split). Difficulty is the game's enum as a number (Homesteader 0, Settler 1, Pioneer 2), because the enum lives in the main assembly.
// The starting weapon is not recorded: it follows from the difficulty (StartingWeapons). A setup section saved with an older "weapon"
// field still loads: JsonUtility ignores the field.
[Serializable]
public struct NewGameSetup
{
    public int difficulty;
    public bool hasLand;
    public int parcelIndex;
    public long downCents;
    public long reserveCents;

    public static NewGameSetup Of(int difficulty, bool hasLand, int parcelIndex, long downCents, long reserveCents) => new NewGameSetup
    {
        difficulty = difficulty,
        hasLand = hasLand,
        parcelIndex = hasLand ? parcelIndex : 0,
        downCents = hasLand ? downCents : 0,
        reserveCents = hasLand ? reserveCents : 0,
    };

    // Whether the numbers could have come from a real New Game (a bad or hand-edited save is not used to restart).
    public bool IsValid =>
        difficulty >= 0 && difficulty <= 2 &&
        (!hasLand || (parcelIndex >= 0 && parcelIndex < LandRules.Starters.Length && downCents >= 0 && reserveCents >= 0));
}

// The setup in a save (player file, key "setup"). A save with no such section, or hasSetup false, has none: Restart then falls back
// to the New Game screens.
[Serializable]
public struct NewGameSetupSaveData
{
    public const int CurrentVersion = 1;

    public int version;
    public bool hasSetup;
    public NewGameSetup setup;

    public static NewGameSetupSaveData Of(NewGameSetup? setup) => new NewGameSetupSaveData
    {
        version = CurrentVersion,
        hasSetup = setup.HasValue,
        setup = setup ?? default,
    };

    // The setup to restore, or null when the save has none (or a bad one).
    public NewGameSetup? ToSetup() => version >= CurrentVersion && hasSetup && setup.IsValid ? setup : (NewGameSetup?)null;
}
