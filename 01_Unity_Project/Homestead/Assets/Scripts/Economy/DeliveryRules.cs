using System;
using System.Collections.Generic;

// Slice D1: delivery from the Trading Post (Modern_Systems_and_Services.md, "Deliveries and the Pole Barn"; Mike's answers 2026-11-27). Pure and
// engine-free so edit-mode tests cover it.
//
// At the Trading Post the player can switch from carrying to delivery: store rows then add to an order list instead of the pack, with no weight
// limit. Place Order pays for the goods and the fee at once. The order comes to the Delivery Pad (a free build, one at a time) at ArrivalHour,
// 1 to 3 days later by its weight, and waits there like a store with no limit (E takes, R puts back). No pad, no order.
public static class DeliveryRules
{
    // --- The fee (real lumber yards: a flat $25 to $75 a load, free over a set order size) ---

    public const long FeeCents = 5000;           // $50 a delivery
    public const long FreeFromCents = 100000;    // free on orders of $1,000 or more
    public static long FeeFor(long goodsCents) => goodsCents <= 0 ? 0 : goodsCents >= FreeFromCents ? 0 : FeeCents;

    // --- When it comes ---

    public const int ArrivalHour = 8;            // 8 a.m.
    public const float NextDayKg = 500f;         // up to this, the next morning
    public const float TwoDayKg = 2000f;         // up to this, the morning after; more takes 3 days

    public static int DaysFor(float kg) => kg <= NextDayKg ? 1 : kg <= TwoDayKg ? 2 : 3;

    // The game hour (days times 24 plus the hour) an order placed on this day arrives.
    public static double ArrivalHours(int orderedDay, float kg) => (orderedDay + DaysFor(kg)) * 24.0 + ArrivalHour;

    public static bool Due(double arrivalHours, double nowHours) => nowHours >= arrivalHours;

    // --- Words ---

    public const string PadName = "Delivery Pad";
    public const string NoPad = "Build a Delivery Pad or a Pole Barn first (Build tab, Other) - deliveries come to them";
    public const string OnlyOnePad = "You already have a Delivery Pad - empty it and take it up with the Hammer to move it";
    public const string EmptyOrder = "The order is empty - switch to Deliver and add things from the store";
    public const string Arrived = "Your Trading Post delivery is on the Delivery Pad";
    public const string Waiting = "A Trading Post delivery is waiting - build a Delivery Pad (Build tab, Other) to receive it";
    public const string CarryMode = "Buying to carry  -  switch to Deliver";
    public const string DeliverMode = "Buying for delivery  -  switch to Carry";

    public static string DayWord(int days) => days == 1 ? "tomorrow" : $"in {days} days";

    public static string Summary(long goodsCents, float kg) =>
        $"Goods {Money.Format(goodsCents)}  +  delivery {(FeeFor(goodsCents) == 0 ? "free" : Money.Format(FeeFor(goodsCents)))}  =  {Money.Format(goodsCents + FeeFor(goodsCents))}" +
        $"\n{Math.Round(kg):0} kg, arrives {DayWord(DaysFor(kg))} at {ArrivalHour} a.m.{(goodsCents > 0 && goodsCents < FreeFromCents ? $"  (free over {Money.Format(FreeFromCents)})" : "")}";

    // Slice B1b: with a Pole Barn standing, the order comes to the barn (what doesn't fit goes on the pad).
    public static string Placed(int days, bool toBarn) => toBarn ? $"Order placed - it arrives at your Pole Barn {DayWord(days)} at {ArrivalHour} a.m." : Placed(days);

    public static string Placed(int days) => $"Order placed - it arrives on the Delivery Pad {DayWord(days)} at {ArrivalHour} a.m.";

    public static string PendingLine(int count, int days) =>
        count == 1 ? $"1 delivery on its way ({(days <= 0 ? "arriving today" : "arrives " + DayWord(days))})" : $"{count} deliveries on their way";

    public static string Help() =>
        "a gravelled spot where Trading Post deliveries are left. Free, one at a time. At the Trading Post (T), switch to Deliver, add what you want " +
        $"with no weight limit, and Place Order: {Money.Format(FeeCents)} a delivery, free on {Money.Format(FreeFromCents)} or more. It arrives at {ArrivalHour} a.m. " +
        $"the next day (up to {NextDayKg:0} kg), in 2 days (up to {TwoDayKg:0} kg) or 3. Take what you want from the pad (E); it keeps the rest. Take it up with the Hammer once it is empty.";
}

// One line of a delivery order.
[Serializable]
public class DeliveryLine
{
    public string itemId;
    public int count;
}

// An order paid for and on its way, saved with the Trading Post's money.
[Serializable]
public class DeliveryOrder
{
    public List<DeliveryLine> lines = new List<DeliveryLine>();
    public double arriveHours;
    public int siteId = -1;   // a Pole Barn kit (B1a) goes onto its site (the pile id); -1 is the Delivery Pad
}

// A Pole Barn crew booked (B1a): the site, the size, the day booked and whether the balance is still owed. Saved with the money.
[Serializable]
public class CrewJob
{
    public int siteId;
    public int size;        // BarnRules.Size
    public int bookedDay;
    public bool balancePaid;
    public int job;         // what the crew builds (slice P2; appended, so an older save's crews are 0): JobBarn or JobDrill
    public long cents;      // the whole job's price, when it depends on the site (the Drilled Well's quote); 0 for a barn (BarnRules has it)

    public const int JobBarn = 0, JobDrill = 1;
}
