using System;
using System.Collections.Generic;

// The Craft and Build screen's "what am I missing" wording, pure and engine-free so edit-mode tests cover it (Mike's measuring run, 2026-10:
// "knowing what I was missing from the recipe"). One MaterialLine is one material of a recipe or a build: how many are in the pack, how many are
// already at a staged site (when one is placed nearby), and how many it takes. The status is plain words, never a symbol: HAVE when it is
// covered, SHORT n when some but not enough (n is how many are short), NEED n when there is none at all. The screen adds colour on top, but
// the words carry the meaning alone. Nothing here changes a cost, a recipe or a save: counts and names come in, text comes out.
public enum MaterialStatus { Have, Short, Need }

public struct MaterialLine
{
    public string label;
    public int pack;        // carried in the pack
    public int site;        // already delivered to the staged site (only counted when atSite)
    public bool atSite;     // a site of this kind is placed nearby, so "short" is need minus what is at the site
    public int need;        // the smallest amount that covers it
    public string needText; // when the need depends on something not known yet ("6, 10 or 14 by depth"): shown instead of the number

    public MaterialLine(string label, int pack, int need)
    {
        this.label = label;
        this.pack = pack;
        site = 0;
        atSite = false;
        this.need = need;
        needText = null;
    }

    public MaterialLine(string label, int pack, int site, int need)
    {
        this.label = label;
        this.pack = pack;
        this.site = site;
        atSite = true;
        this.need = need;
        needText = null;
    }

    // What counts toward the need: what is at the site when there is one, otherwise what is carried.
    public int Counted => atSite ? site : pack;

    public int Missing => Math.Max(0, need - Counted);

    public MaterialStatus Status => Missing == 0 ? MaterialStatus.Have : Counted > 0 ? MaterialStatus.Short : MaterialStatus.Need;
}

public static class MaterialCheckRules
{
    public const string CanMakeWithThisSet = "You can make this with this set";

    // "HAVE", "SHORT 1" or "NEED 3" (the number is how many are still missing).
    public static string StatusWord(MaterialLine line)
    {
        switch (line.Status)
        {
            case MaterialStatus.Have: return "HAVE";
            case MaterialStatus.Short: return "SHORT " + line.Missing;
            default: return "NEED " + line.Missing;
        }
    }

    // "Logs  2 / 3", "Logs  in pack 2, at site 1, need 3" or "Stone  have 4, need 6, 10 or 14 by depth". The label and counts only; the
    // status word is added by Line.
    public static string Counts(MaterialLine line)
    {
        string need = line.needText ?? line.need.ToString();
        if (line.atSite)
            return $"{line.label}  in pack {line.pack}, at site {line.site}, need {need}" + (line.Missing > 0 && line.pack >= line.Missing ? " (your pack covers it)" : "");
        if (line.needText != null)
            return $"{line.label}  have {line.pack}, need {need}";
        return $"{line.label}  {line.pack} / {need}";
    }

    // The whole line, status word first: "SHORT 1  Logs  2 / 3".
    public static string Line(MaterialLine line) => StatusWord(line) + "  " + Counts(line);

    // The lines that are not covered first, then the covered ones, each group keeping its own order.
    public static List<MaterialLine> MissingFirst(IList<MaterialLine> lines)
    {
        var ordered = new List<MaterialLine>();
        if (lines == null)
            return ordered;
        foreach (MaterialLine l in lines)
            if (l.Status != MaterialStatus.Have)
                ordered.Add(l);
        foreach (MaterialLine l in lines)
            if (l.Status == MaterialStatus.Have)
                ordered.Add(l);
        return ordered;
    }

    public static int MissingCount(IList<MaterialLine> lines)
    {
        int n = 0;
        if (lines != null)
            foreach (MaterialLine l in lines)
                if (l.Status != MaterialStatus.Have)
                    n++;
        return n;
    }

    // Every material covered (an empty list is covered: nothing is needed).
    public static bool Covered(IList<MaterialLine> lines) => MissingCount(lines) == 0;

    // "Missing: 2 of 5 materials", or "HAVE all 5 materials" when covered. Empty for a list with no materials.
    public static string Heading(IList<MaterialLine> lines)
    {
        int total = lines != null ? lines.Count : 0;
        if (total == 0)
            return "";
        int missing = MissingCount(lines);
        string noun = total == 1 ? "material" : "materials";
        return missing == 0 ? $"HAVE all {total} {noun}" : $"Missing: {missing} of {total} {noun}";
    }

    // The set a player is closest to: the first fully covered one, otherwise the one with the fewest items still missing (the first on a tie).
    // -1 when there are no sets.
    public static int Closest(IList<IList<MaterialLine>> sets)
    {
        if (sets == null || sets.Count == 0)
            return -1;
        int best = 0, bestMissing = int.MaxValue;
        for (int s = 0; s < sets.Count; s++)
        {
            int total = 0;
            if (sets[s] != null)
                foreach (MaterialLine l in sets[s])
                    total += l.Missing;
            if (total == 0)
                return s;
            if (total < bestMissing)
            {
                bestMissing = total;
                best = s;
            }
        }
        return best;
    }

    // The overall state of an entry from its closest set: HAVE when covered, SHORT when something is there but not enough, NEED when
    // none of it is. Null when the entry has no materials to check.
    public static MaterialStatus? Overall(IList<IList<MaterialLine>> sets)
    {
        int closest = Closest(sets);
        if (closest < 0 || sets[closest] == null || sets[closest].Count == 0)
            return null;
        IList<MaterialLine> lines = sets[closest];
        if (Covered(lines))
            return MaterialStatus.Have;
        foreach (MaterialLine l in lines)
            if (l.Counted > 0)
                return MaterialStatus.Short;
        return MaterialStatus.Need;
    }

    // A staged site's prompt line for one material: "Logs: 2 of 3 delivered, 1 in your pack" (or "none in your pack").
    public static string SiteProgress(string label, int delivered, int need, int inPack) =>
        $"{label}: {delivered} of {need} delivered, " + (inPack > 0 ? $"{inPack} in your pack" : "none in your pack");

    // "HAVE", "SHORT" or "NEED" for the list row, or "" when there is nothing to check.
    public static string RowMarker(IList<IList<MaterialLine>> sets)
    {
        MaterialStatus? status = Overall(sets);
        if (status == null)
            return "";
        switch (status.Value)
        {
            case MaterialStatus.Have: return "HAVE";
            case MaterialStatus.Short: return "SHORT";
            default: return "NEED";
        }
    }
}
