using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the calendar gate on saves (Phase 1): calendarVersion in meta.json and slotinfo.json, an old-calendar save labeled and refused, a warning before anything writes over
// it; and for the loan-banner fix (a settled payment takes down its own reminders, tagged, and nothing else).
public class CalendarSaveGateTests
{
    // --- The rules ---

    [Test]
    public void OnlyTheCurrentCalendarVersionIsCurrent()
    {
        Assert.That(CalendarRules.SaveVersion, Is.EqualTo(1));
        Assert.That(CalendarSaveRules.IsCurrent(CalendarRules.SaveVersion), Is.True);
        Assert.That(CalendarSaveRules.IsLegacy(0), Is.True, "a save from before the field existed reads 0: the old calendar");
        Assert.That(CalendarSaveRules.IsLegacy(CalendarRules.SaveVersion + 1), Is.True, "a save from a later calendar is not this one either");
        Assert.That(CalendarSaveRules.IsLegacy(CalendarRules.SaveVersion), Is.False);
    }

    [Test]
    public void AnEmptySlotIsNeverLegacyAndAnOldSaveCannotBeLoaded()
    {
        Assert.That(CalendarSaveRules.IsLegacySlot(false, 0), Is.False, "an empty slot has no calendar");
        Assert.That(CalendarSaveRules.IsLegacySlot(true, 0), Is.True);
        Assert.That(CalendarSaveRules.IsLegacySlot(true, CalendarRules.SaveVersion), Is.False);
        Assert.That(CalendarSaveRules.CanLoad(true, CalendarRules.SaveVersion), Is.True);
        Assert.That(CalendarSaveRules.CanLoad(true, 0), Is.False);
        Assert.That(CalendarSaveRules.CanLoad(false, CalendarRules.SaveVersion), Is.False, "nothing to load");
    }

    [Test]
    public void TheSlotListShowsOldCalendarInPlaceOfTheDate()
    {
        Assert.That(CalendarSaveRules.DateColumn(true, 0, "Summer 11, Year 1 · 20:41"), Is.EqualTo("old calendar"));
        Assert.That(CalendarSaveRules.DateColumn(true, CalendarRules.SaveVersion, "Jul 3, Year 1 · 20:41"), Is.EqualTo("Jul 3, Year 1 · 20:41"));
        Assert.That(CalendarSaveRules.DateColumn(false, 0, "Empty"), Is.EqualTo("Empty"), "an empty slot is not labeled");
        Assert.That(CalendarSaveRules.DateColumn(true, CalendarRules.SaveVersion, null), Is.EqualTo(""));
    }

    [Test]
    public void SavingOverAnOldCalendarSaveAlwaysAsksWithAPlainWarning()
    {
        Assert.That(CalendarSaveRules.NeedsOverwriteWarning(true, 0), Is.True);
        Assert.That(CalendarSaveRules.NeedsOverwriteWarning(true, CalendarRules.SaveVersion), Is.False);
        Assert.That(CalendarSaveRules.NeedsOverwriteWarning(false, 0), Is.False);

        string warning = CalendarSaveRules.OverwriteWarning("Slot 4", "Summer 11, Year 1 · 20:41");
        StringAssert.Contains("Slot 4", warning);
        StringAssert.Contains("Summer 11, Year 1", warning);
        StringAssert.Contains("old calendar", warning);
        StringAssert.Contains("_old_calendar", warning, "says where the copy is kept");
        StringAssert.Contains("Slot 2", CalendarSaveRules.RefusalMessage("Slot 2"));
        StringAssert.Contains("old calendar", CalendarSaveRules.RefusalMessage("Slot 2"));
    }

    // --- The save format (SaveManager's private classes, read by reflection) ---

    static Type Find(string name)
    {
        Assembly[] all = AppDomain.CurrentDomain.GetAssemblies();
        Type[] TypesOf(Assembly a) { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } }
        Type own = all.Where(a => a.GetName().Name == "Assembly-CSharp").SelectMany(TypesOf).FirstOrDefault(t => t.Name == name);
        return own ?? all.SelectMany(TypesOf).First(t => t.Name == name);
    }

    [Test]
    public void MetaAndSlotInfoCarryTheCalendarVersionAndAnOldFileReadsZero()
    {
        Type manager = Find("SaveManager");
        Type meta = manager.GetNestedType("SaveMeta", BindingFlags.NonPublic | BindingFlags.Public);
        Type info = manager.GetNestedType("SlotInfoData", BindingFlags.NonPublic | BindingFlags.Public);
        Assert.That(meta.GetField("calendarVersion"), Is.Not.Null);
        Assert.That(info.GetField("calendarVersion"), Is.Not.Null);

        // meta.json and slotinfo.json from before the field: it is simply absent, and JsonUtility leaves 0.
        object oldMeta = JsonUtility.FromJson("{\"version\":1,\"savedAtUtc\":\"2026-11-08T18:04:53.3180059Z\"}", meta);
        Assert.That((int)meta.GetField("calendarVersion").GetValue(oldMeta), Is.EqualTo(0));
        object oldInfo = JsonUtility.FromJson("{\"name\":\"Slot 4\",\"inGameDate\":\"Summer 11, Year 1\",\"hour\":20,\"minute\":41,\"year\":1,\"season\":\"Summer\",\"difficulty\":\"Pioneer\",\"savedAtUtc\":\"x\",\"lastSlot\":\"slot4\"}", info);
        Assert.That((int)info.GetField("calendarVersion").GetValue(oldInfo), Is.EqualTo(0));
        Assert.That(CalendarSaveRules.IsLegacy((int)meta.GetField("calendarVersion").GetValue(oldMeta)), Is.True);

        // And one written with the field reads it back.
        object newMeta = JsonUtility.FromJson("{\"version\":1,\"savedAtUtc\":\"x\",\"calendarVersion\":1}", meta);
        Assert.That(CalendarSaveRules.IsCurrent((int)meta.GetField("calendarVersion").GetValue(newMeta)), Is.True);
    }

    [Test]
    public void SaveManagerKnowsWhichSlotsCanBeLoaded()
    {
        Type manager = Find("SaveManager");
        Type slotInfo = manager.GetNestedType("SlotInfo");
        Assert.That(slotInfo.GetProperty("IsLegacy"), Is.Not.Null);
        Assert.That(slotInfo.GetProperty("CanLoad"), Is.Not.Null);
        Assert.That(slotInfo.GetProperty("DateColumn"), Is.Not.Null);
        Assert.That(manager.GetMethod("IsLegacySlot"), Is.Not.Null);
        Assert.That(manager.GetProperty("HasAnySlot"), Is.Not.Null);

        object old = Activator.CreateInstance(slotInfo);
        slotInfo.GetField("Exists").SetValue(old, true);
        slotInfo.GetField("InGame").SetValue(old, "Summer 11, Year 1 · 20:41");
        Assert.That((bool)slotInfo.GetProperty("IsLegacy").GetValue(old), Is.True, "no calendarVersion: old calendar");
        Assert.That((bool)slotInfo.GetProperty("CanLoad").GetValue(old), Is.False);
        Assert.That((string)slotInfo.GetProperty("DateColumn").GetValue(old), Is.EqualTo("old calendar"));

        slotInfo.GetField("CalendarVersion").SetValue(old, CalendarRules.SaveVersion);
        Assert.That((bool)slotInfo.GetProperty("IsLegacy").GetValue(old), Is.False);
        Assert.That((bool)slotInfo.GetProperty("CanLoad").GetValue(old), Is.True);
        Assert.That((string)slotInfo.GetProperty("DateColumn").GetValue(old), Is.EqualTo("Summer 11, Year 1 · 20:41"));
    }

    // --- Loan banners ---

    [Test]
    public void ATaggedBannerIsTakenDownAndOthersStay()
    {
        var queue = new BannerQueue();
        queue.Add("Your land payment is due today.", 8f, "land payment");
        queue.Add("Your account statement is ready.", 8f);
        queue.Add("The creek is in flood.", 8f);
        queue.Add("Your land payment is overdue.", 8f, "land payment");
        queue.Tick(true, 0.1f);
        Assert.That(queue.Current, Is.EqualTo("Your land payment is due today."));
        Assert.That(queue.CurrentTag, Is.EqualTo("land payment"));

        int removed = queue.ClearTagged("land payment");

        Assert.That(removed, Is.EqualTo(2), "the one on show and the one waiting");
        Assert.That(queue.Current, Is.Null, "the one on show goes at once");
        Assert.That(queue.Waiting, Is.EqualTo(2), "the statement and the flood are still waiting");
        queue.Tick(true, 0.1f);
        Assert.That(queue.Current, Is.EqualTo("Your account statement is ready."), "the next in line takes its place");
        queue.Tick(true, 8f);
        Assert.That(queue.Current, Is.EqualTo("The creek is in flood."));
    }

    [Test]
    public void ClearingATagNothingUsesLeavesEveryBanner()
    {
        var queue = new BannerQueue();
        queue.Add("A", 8f);
        queue.Add("B", 8f, "other");
        queue.Tick(true, 0.1f);
        Assert.That(queue.ClearTagged("land payment"), Is.EqualTo(0));
        Assert.That(queue.ClearTagged(null), Is.EqualTo(0));
        Assert.That(queue.ClearTagged(""), Is.EqualTo(0));
        Assert.That(queue.Current, Is.EqualTo("A"));
        Assert.That(queue.Waiting, Is.EqualTo(1));
    }

    [Test]
    public void ABannerWithoutATagIsNeverClearedByATag()
    {
        var queue = new BannerQueue();
        queue.Add("Congratulations! The land is paid off.", 12f);
        queue.Tick(true, 0.1f);
        queue.ClearTagged("land payment");
        Assert.That(queue.Current, Is.EqualTo("Congratulations! The land is paid off."));
    }

    [Test]
    public void TheSameMessageAskedForAgainKeepsItsTagAndClearAllStillClearsEverything()
    {
        var queue = new BannerQueue();
        queue.Add("Pay up.", 8f, "land payment");
        queue.Add("Pay up.", 8f); // asked again, no tag given: it is the same banner
        queue.Add("Waiting one", 8f, "land payment");
        queue.Add("Waiting one", 8f);
        queue.Tick(true, 0.1f);
        Assert.That(queue.ClearTagged("land payment"), Is.EqualTo(2));

        queue.Add("X", 8f, "land payment");
        queue.Add("Y", 8f);
        queue.Clear();
        Assert.That(queue.Current, Is.Null);
        Assert.That(queue.CurrentTag, Is.Null);
        Assert.That(queue.Waiting, Is.EqualTo(0));
    }

    [Test]
    public void TheLoanBannerTagIsSharedByMoneyManagerAndToolStatus()
    {
        Type money = Find("MoneyManager");
        Assert.That(money.GetField("LoanBannerTag").GetRawConstantValue(), Is.EqualTo("land payment"));
        Type status = Find("ToolStatus");
        Assert.That(status.GetMethod("ClearBanners", new[] { typeof(string) }), Is.Not.Null);
    }
}
