using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Slice P1: the phone (Mike, 2026-11-27: start with it, no service; P raises a 3D phone in the hands; one $25 plan billed on the 15th; Contractors, Weather,
// Bills & Plan and Market apps plus Mike's own app icons; time, date and "No Service" without a plan; all hiring by phone).
public class PhoneP1Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");
    static GameCalendar Cal => CalendarRules.Real;

    // Day 0 is March 1 on the real calendar.
    [Test]
    public void BillsFallOnTheFifteenth()
    {
        Assert.That(PhoneRules.PlanCents, Is.EqualTo(2500L), "$25 a month");
        Assert.That(PhoneRules.BillDayOfMonth, Is.EqualTo(15));
        Assert.That(PhoneRules.NextBillDay(Cal, 0), Is.EqualTo(14), "Mar 1 -> Mar 15");
        Assert.That(Cal.DayOfMonth(PhoneRules.NextBillDay(Cal, 14)), Is.EqualTo(15));
        Assert.That(PhoneRules.NextBillDay(Cal, 14), Is.EqualTo(14 + 31), "Mar 15 -> Apr 15");
        Assert.That(PhoneRules.NextBillDay(Cal, 20), Is.EqualTo(45), "Mar 21 -> Apr 15");
    }

    [Test]
    public void StartingThePlanPaysTheFirstMonth()
    {
        var s = new PhoneState();
        long cash = 1000;
        bool Pay(long c) { if (cash < c) return false; cash -= c; return true; }
        Assert.That(PhoneRules.HasService(s), Is.False, "no plan, no service");
        Assert.That(PhoneRules.SignUp(s, Cal, 0, Pay), Is.False, "$10 isn't enough");
        cash = 10000;
        Assert.That(PhoneRules.SignUp(s, Cal, 0, Pay), Is.True);
        Assert.That(cash, Is.EqualTo(7500L));
        Assert.That(PhoneRules.HasService(s), Is.True);
        Assert.That(s.nextBillDay, Is.EqualTo(14));
        Assert.That(PhoneRules.SignUp(s, Cal, 1, Pay), Is.False, "already on");
    }

    [Test]
    public void AnUnpaidBillSuspendsTheServiceUntilItIsPaid()
    {
        var s = new PhoneState();
        long cash = 2500;
        bool Pay(long c) { if (cash < c) return false; cash -= c; return true; }
        PhoneRules.SignUp(s, Cal, 0, Pay);
        var results = PhoneRules.Advance(s, Cal, 14, Pay);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].paid, Is.False);
        Assert.That(s.suspended, Is.True);
        Assert.That(PhoneRules.HasService(s), Is.False);
        Assert.That(PhoneRules.Advance(s, Cal, 100, Pay), Is.Empty, "nothing more is added while suspended");
        Assert.That(PhoneRules.PayOverdue(s, Cal, 100, Pay), Is.False, "still no cash");
        cash = 3000;
        Assert.That(PhoneRules.PayOverdue(s, Cal, 100, Pay), Is.True);
        Assert.That(cash, Is.EqualTo(500L));
        Assert.That(PhoneRules.HasService(s), Is.True);
        Assert.That(Cal.DayOfMonth(s.nextBillDay), Is.EqualTo(15));
        Assert.That(s.nextBillDay, Is.GreaterThan(100));
    }

    [Test]
    public void PaidBillsCarryOnMonthByMonthAndAreKept()
    {
        var s = new PhoneState();
        long cash = 100000;
        bool Pay(long c) { if (cash < c) return false; cash -= c; return true; }
        PhoneRules.SignUp(s, Cal, 0, Pay);
        var results = PhoneRules.Advance(s, Cal, 14 + 31 + 30, Pay); // Mar 15, Apr 15, May 15
        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(results.All(r => r.paid), Is.True);
        Assert.That(cash, Is.EqualTo(100000L - 4 * 2500L));
        Assert.That(s.history.Count, Is.EqualTo(4));
        PhoneRules.Advance(s, Cal, 400, Pay);
        Assert.That(s.history.Count, Is.EqualTo(PhoneRules.HistoryKept), "only the last few are kept");
    }

    [Test]
    public void CancellingRunsToTheNextBillDayThenEnds()
    {
        var s = new PhoneState();
        long cash = 100000;
        bool Pay(long c) { if (cash < c) return false; cash -= c; return true; }
        PhoneRules.SignUp(s, Cal, 0, Pay);
        PhoneRules.Cancel(s);
        Assert.That(PhoneRules.HasService(s), Is.True, "paid up to the 15th");
        var results = PhoneRules.Advance(s, Cal, 14, Pay);
        Assert.That(results.Single().ended, Is.True);
        Assert.That(s.active, Is.False);
        Assert.That(cash, Is.EqualTo(97500L), "nothing more is charged");
        PhoneRules.SignUp(s, Cal, 20, Pay);
        PhoneRules.Cancel(s);
        Assert.That(PhoneRules.SignUp(s, Cal, 21, Pay), Is.True, "keeping it again");
        Assert.That(cash, Is.EqualTo(95000L), "and that costs nothing extra");
        Assert.That(s.cancelling, Is.False);
    }

    [Test]
    public void TheWords()
    {
        Assert.That(PhoneRules.Clock(0, 5), Is.EqualTo("12:05 AM"));
        Assert.That(PhoneRules.Clock(13, 30), Is.EqualTo("1:30 PM"));
        Assert.That(PhoneRules.Clock(12, 0), Is.EqualTo("12:00 PM"));
        Assert.That(PhoneRules.PlanLine, Does.Contain("$25.00 a month"));
        Assert.That(PhoneRules.Status(new PhoneState()), Is.EqualTo("No plan"));
        Assert.That(PhoneRules.Apps.Select(a => a.label), Is.EqualTo(new[] { "Contractors", "Weather", "Bills & Plan", "Market", "Power", "Brandy's Bakery", "Sweet Swap", "Fast Food Frenzy", "Starforge", "My Tech App" }));
        Assert.That(PhoneRules.Apps.Where(a => !a.needsService).Select(a => a.id), Is.EqualTo(new[] { "bills" }), "only Bills & Plan opens with no service");
        foreach (var app in PhoneRules.Apps)
            Assert.That(Resources.Load<Sprite>("Phone/app_" + app.id), Is.Not.Null, app.id + " has its icon");
    }

    [Test]
    public void BillsHaveTheirOwnLedgerLine()
    {
        Assert.That((int)LedgerCategory.Bills, Is.EqualTo(21));
        Assert.That(LedgerCategories.Count, Is.EqualTo(22));
        Assert.That(LedgerCategories.Name(LedgerCategory.Bills), Is.EqualTo("Bills"));
    }

    [Test]
    public void ThePhoneIsAnItemEveryoneStartsWith()
    {
        ScriptableObject item = Resources.Load<ScriptableObject>("Items/Materials/smartphone");
        Assert.That(item, Is.Not.Null);
        Assert.That(Resources.Load<GameObject>("Art/item_smartphone"), Is.Not.Null);
        Assert.That(Resources.Load<GameObject>("Art/item_smartphone").transform.Find("Screen"), Is.Not.Null, "PhoneView draws the apps over the Screen child");
        Assert.That(KnowHowRules.ItemTip(PhoneRules.PhoneId).Length, Is.InRange(1, 80));
        Assert.That(KnowHowRules.ForItem(PhoneRules.PhoneId), Is.EqualTo(new[] { KnowHowRules.Phone }));
        string kit = SourceText("Scripts/Managers/DifficultyManager.cs");
        Assert.That(kit.Split(new[] { "(PhoneRules.PhoneId, 1)" }, System.StringSplitOptions.None).Length - 1, Is.EqualTo(2), "in both kits");
    }

    [Test]
    public void ThePhoneIsWiredIn()
    {
        Assert.That(SourceText("Scripts/Player/PlayerController.cs"), Does.Contain("&& !PhoneView.IsOpen;"), "no walking, looking or tools with the phone up");
        Assert.That(SourceText("Scripts/Player/PlayerController.cs"), Does.Contain("!PhoneView.TookEscape"), "Esc puts the phone away without pausing");
        Assert.That(SourceText("Scripts/Player/PlayerHands.cs"), Does.Contain("phoning = PhoneView.IsOpen;"));
        Assert.That(SourceText("Scripts/UI/TradingPostScreen.cs"), Does.Not.Contain("HireBarnCrew"), "all hiring is by phone");
        Assert.That(SourceText("Scripts/UI/PhoneView.cs"), Does.Contain("post.HireBarnCrew(target, out string message);"));
        Assert.That(SourceText("Scripts/Managers/GameManager.cs"), Does.Contain("PhoneManager.Instance.ResetForLoad();"));
        Assert.That(SourceText("Scripts/Managers/PhoneManager.cs"), Does.Contain("LedgerManager.Instance.Record(LedgerCategory.Bills, cents);"));
    }

    [Test]
    public void TheHandsHoldItInFront()
    {
        HandPose r = HandPoseRules.PhoneTarget(HandSide.Right), l = HandPoseRules.PhoneTarget(HandSide.Left);
        Assert.That(r.X, Is.EqualTo(-l.X), "mirrored");
        Assert.That(r.Z, Is.LessThan(HandPoseRules.PhoneAt.z), "the hands are just behind the phone");
        Assert.That(HandPoseRules.PhoneAt.z, Is.LessThan(0.3f), "close enough to read");
    }
}
