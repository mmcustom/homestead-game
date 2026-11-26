using System;
using System.Collections.Generic;

// Garden fences and the deer and rabbits they keep out (Gardening_Basics.md section 6; slice GB4a, Mike's answers 2026-11-25 in 9d). Pure and engine-free so edit-mode
// tests cover it: positions come in as plain x and z (metres, on the ground plane) and yaw in degrees, as Unity's.
//
// A fence is built panel by panel, each a placed structure: a Wattle Fence panel (Sticks and Branches woven between stakes, waist high: keeps rabbits out, deer jump
// it), a Deer Fence panel (a Log for posts and a roll of Wire Mesh from the Trading Post, about 7 ft: keeps both out) and a Gate (a Lumber frame as tall as the deer
// fence, swinging open as the player walks into it). A new panel snaps to the free end of a panel already standing, and the panel that closes a ring is cut to fit
// the gap. A bed is protected only once a ring round it is closed: its fence is the panels left once every dangling one is pruned away, and the bed is inside if a
// line from it crosses them an odd number of times. A ring of deer fence and gates keeps deer and rabbits out; any wattle panel in it lets deer jump in.
//
// Raids (light nibbling, Mike): at dawn and at dusk each open bed with a crop a rabbit or a deer eats may be visited, and a visit nibbles a share of the crop
// (cropDamage). Worked out by the hour while the player is away or asleep; GB4b will show the raiders when the player is near.
public static class FenceRules
{
    public enum Type { Wattle, Deer, Gate }

    // --- Sizes (metres) ---

    public const float PanelLength = 2.4f;   // 8 ft between posts
    public const float GateLength = 1.2f;    // a 4 ft garden gate
    public const float MinLength = 0.3f;     // the shortest a panel may be cut to close a ring
    public const float JoinMetres = 0.25f;   // panel ends this close are joined
    public const float SnapMetres = 1.6f;    // a new panel snaps to a free end this close to where the player aims
    public const float CloseMetres = 0.8f;   // and its far end snaps onto another free end this close, cut to fit
    public const float WattleHeight = 1.0f;
    public const float DeerHeight = 2.1f;

    public static float FullLength(Type type) => type == Type.Gate ? GateLength : PanelLength;
    public static float Height(Type type) => type == Type.Wattle ? WattleHeight : DeerHeight;
    public static bool DeerProof(Type type) => type != Type.Wattle;

    // A pile's length: saved in its jobHours, which a fence has no other use for; 0 (a panel from before it was cut) is the full length.
    public static float LengthOf(Type type, float saved) => saved > 0f ? saved : FullLength(type);

    // --- Costs ---

    public const string SticksId = "sticks";
    public const string BranchesId = "branches";
    public const string LogsId = "logs";
    public const string LumberId = "lumber";
    public const string CordageId = "cordage";
    public const string WireMeshId = "wire_mesh";
    public const long WireMeshStoreCents = 1500;   // a 7 ft by 8 ft roll of welded wire fencing, one panel's worth

    public const int WattleSticks = 4, WattleBranches = 6;
    public const int DeerLogs = 1, DeerWire = 1;
    public const int GateLumber = 2, GateCordage = 1;

    // --- Geometry ---

    public struct Point
    {
        public float x, z;
        public Point(float x, float z) { this.x = x; this.z = z; }
        public static float Distance(Point a, Point b) => (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));
    }

    public struct Segment
    {
        public Point a, b;
        public Type type;
    }

    // The direction a panel runs for a yaw (its local +X): yaw 0 runs along +X, yaw 90 along -Z, as Unity's rotation does.
    public static Point Direction(float yaw)
    {
        double r = yaw * Math.PI / 180.0;
        return new Point((float)Math.Cos(r), (float)-Math.Sin(r));
    }

    // The yaw for a panel running from a to b.
    public static float YawOf(Point a, Point b) => (float)(Math.Atan2(-(b.z - a.z), b.x - a.x) * 180.0 / Math.PI);

    public static Segment SegmentOf(float x, float z, float yaw, float length, Type type)
    {
        Point d = Direction(yaw);
        float h = length / 2f;
        return new Segment { a = new Point(x - d.x * h, z - d.z * h), b = new Point(x + d.x * h, z + d.z * h), type = type };
    }

    public static float DistanceToSegment(Point p, Segment s)
    {
        float dx = s.b.x - s.a.x, dz = s.b.z - s.a.z;
        float len2 = dx * dx + dz * dz;
        float t = len2 <= 0f ? 0f : Math.Max(0f, Math.Min(1f, ((p.x - s.a.x) * dx + (p.z - s.a.z) * dz) / len2));
        return Point.Distance(p, new Point(s.a.x + t * dx, s.a.z + t * dz));
    }

    // Ends of the standing panels with no other panel end joined to them: where a new panel can start.
    public static List<Point> FreeEnds(IList<Segment> segments)
    {
        var ends = new List<Point>();
        for (int i = 0; i < segments.Count; i++)
            foreach (Point end in new[] { segments[i].a, segments[i].b })
                if (Joins(segments, i, end) == 0)
                    ends.Add(end);
        return ends;
    }

    // How many other panels have an end at this one's end.
    static int Joins(IList<Segment> segments, int self, Point end)
    {
        int n = 0;
        for (int j = 0; j < segments.Count; j++)
            if (j != self && (Point.Distance(segments[j].a, end) <= JoinMetres || Point.Distance(segments[j].b, end) <= JoinMetres))
                n++;
        return n;
    }

    // Where a new panel goes for an aim point: from the free end nearest the aim (within SnapMetres) toward the aim, its full length, unless another free end lies within
    // CloseMetres of that line, when it runs to that end instead, cut to fit (MinLength up to a little over full). With no free end near, it stands on its own,
    // centred on the aim and turned by freeYaw (the mouse wheel). Returns whether it snapped.
    public struct Placement
    {
        public float x, z, yaw, length;
        public bool snapped, closes;
    }

    public static Placement Place(IList<Segment> standing, Point aim, Type type, float freeYaw)
    {
        float full = FullLength(type);
        List<Point> free = FreeEnds(standing);
        int start = -1;
        float best = SnapMetres;
        for (int i = 0; i < free.Count; i++)
        {
            float d = Point.Distance(free[i], aim);
            if (d <= best)
            {
                best = d;
                start = i;
            }
        }
        if (start < 0)
            return new Placement { x = aim.x, z = aim.z, yaw = freeYaw, length = full };

        Point a = free[start];
        float dx = aim.x - a.x, dz = aim.z - a.z;
        float len = (float)Math.Sqrt(dx * dx + dz * dz);
        // Aiming right at the end: carry on the way its own panel runs.
        Point dir = len > 0.05f ? new Point(dx / len, dz / len) : AwayFrom(standing, a);
        var b = new Point(a.x + dir.x * full, a.z + dir.z * full);
        // Another free end lying along the way (within CloseMetres of the full panel's line) is where it stops: the panel is cut to reach it and closes the gap.
        var line = new Segment { a = a, b = b };
        bool closes = false;
        float bestClose = CloseMetres;
        Point end = b;
        for (int i = 0; i < free.Count; i++)
        {
            if (i == start)
                continue;
            float gap = Point.Distance(a, free[i]);
            float d = DistanceToSegment(free[i], line);
            if (d <= bestClose && gap >= MinLength && gap <= full + CloseMetres)
            {
                bestClose = d;
                end = free[i];
                closes = true;
            }
        }
        b = end;
        float length = Point.Distance(a, b);
        return new Placement { x = (a.x + b.x) / 2f, z = (a.z + b.z) / 2f, yaw = YawOf(a, b), length = length, snapped = true, closes = closes };
    }

    // The way the panel ending at this free end runs, carried on past it.
    static Point AwayFrom(IList<Segment> standing, Point end)
    {
        foreach (Segment s in standing)
        {
            Point other = Point.Distance(s.a, end) <= JoinMetres ? s.b : Point.Distance(s.b, end) <= JoinMetres ? s.a : new Point(float.NaN, 0f);
            if (float.IsNaN(other.x))
                continue;
            float dx = end.x - other.x, dz = end.z - other.z, len = (float)Math.Sqrt(dx * dx + dz * dz);
            if (len > 0.01f)
                return new Point(dx / len, dz / len);
        }
        return new Point(1f, 0f);
    }

    // --- Closed rings ---

    // The panels left once every dangling one (with an end joined to nothing) is taken away, again and again until none is: the rings and what joins them.
    public static List<Segment> Rings(IList<Segment> segments)
    {
        var left = new List<Segment>(segments);
        bool pruned = true;
        while (pruned)
        {
            pruned = false;
            for (int i = left.Count - 1; i >= 0; i--)
            {
                if (Joins(left, i, left[i].a) == 0 || Joins(left, i, left[i].b) == 0)
                {
                    left.RemoveAt(i);
                    pruned = true;
                }
            }
        }
        return left;
    }

    // Whether a point is inside the rings: a line from it (slightly skewed, so it never runs along a panel or through a post) crosses them an odd number of times.
    public static bool Inside(IList<Segment> rings, Point p)
    {
        const float dirX = 1f, dirZ = 0.01371f;
        int crossings = 0;
        foreach (Segment s in rings)
        {
            // Solve p + t*dir = a + u*(b - a), t > 0, 0 <= u < 1.
            float ex = s.b.x - s.a.x, ez = s.b.z - s.a.z;
            float det = dirX * (-ez) - dirZ * (-ex);
            if (Math.Abs(det) < 1e-9f)
                continue;
            float rx = s.a.x - p.x, rz = s.a.z - p.z;
            float t = (rx * (-ez) - rz * (-ex)) / det;
            float u = (dirX * rz - dirZ * rx) / det;
            if (t > 0f && u >= 0f && u < 1f)
                crossings++;
        }
        return crossings % 2 == 1;
    }

    public struct Protection
    {
        public bool rabbits, deer;   // kept out
    }

    // What the fences standing keep out at a bed: rabbits inside any closed ring; deer only inside a ring of deer fence and gates.
    public static Protection ProtectionAt(IList<Segment> fences, Point bed)
    {
        var deerOnly = new List<Segment>();
        foreach (Segment s in fences)
            if (DeerProof(s.type))
                deerOnly.Add(s);
        return new Protection { rabbits = Inside(Rings(fences), bed), deer = Inside(Rings(deerOnly), bed) };
    }

    // --- Raids (light nibbling) ---

    public const int DawnHour = 6, DuskHour = 19;
    public const float RabbitChance = 0.02f;   // each open bed with a crop a rabbit eats, at dawn and at dusk
    public const float DeerChance = 0.008f;    // the same for a deer
    public const float RabbitBite = 0.05f;     // a twentieth of the square
    public const float DeerBite = 0.10f;       // a tenth

    // What they eat (Gardening_Basics.md 6): rabbits the greens and young plants, deer almost everything but potatoes and squash.
    public static bool RabbitEats(string cropId, bool young) =>
        young || cropId == "lettuce" || cropId == "kale" || cropId == "beans" || cropId == "carrots";

    public static bool DeerEats(string cropId) =>
        cropId == "beans" || cropId == "field_corn" || cropId == "sweet_corn" || cropId == "tomatoes" || cropId == "kale" || cropId == "lettuce" || cropId == "carrots";

    public static bool IsRaidHour(int hour) => hour == DawnHour || hour == DuskHour;

    // The banner after a raid hour: "Rabbits nibbled 2 garden beds at dawn - a fence keeps them out".
    public static string RaidBanner(int rabbitBeds, int deerBeds, bool dawn)
    {
        string when = dawn ? "at dawn" : "at dusk";
        if (rabbitBeds > 0 && deerBeds > 0)
            return $"Deer and rabbits got into the garden {when}: {deerBeds + rabbitBeds} beds nibbled - a deer fence keeps both out";
        if (deerBeds > 0)
            return deerBeds == 1 ? $"A deer browsed a garden bed {when} - a deer fence (7 ft) keeps them out" : $"Deer browsed {deerBeds} garden beds {when} - a deer fence (7 ft) keeps them out";
        return rabbitBeds == 1 ? $"A rabbit nibbled a garden bed {when} - a fence keeps them out" : $"Rabbits nibbled {rabbitBeds} garden beds {when} - a fence keeps them out";
    }

    // The bed's fence, for its second line.
    public static string ProtectionText(Protection p) =>
        p.deer ? "fenced against deer and rabbits" : p.rabbits ? "fenced against rabbits (deer jump wattle)" : "not fenced - deer and rabbits can get at it";

    // --- Words ---

    public static string Name(Type type) => type == Type.Wattle ? "Wattle Fence" : type == Type.Deer ? "Deer Fence" : "Gate";

    public static string Header(Type type, float length) =>
        type == Type.Gate ? "Gate - walk into it to open it; it shuts behind you"
        : type == Type.Wattle ? $"Wattle Fence ({length:0.0} m) - keeps rabbits out; deer jump it"
        : $"Deer Fence ({length:0.0} m) - keeps deer and rabbits out";

    public static string DismantlePrompt(Type type) => $"Take the {Name(type)} down  (half the materials back, rounded down)";

    public const string SnapNote = "joined to the fence";
    public const string CloseNote = "closes the ring - cut to fit";

    public static readonly string WattleHelp =
        $"one 8 ft section of woven fence, waist high: {WattleSticks} Sticks for stakes and {WattleBranches} Branches woven between them. It keeps rabbits out of a garden, but deer jump it. " +
        "Build sections end to end round your beds (each snaps to the last one's free end) and close the ring: the last section is cut to fit. Leave room for a Gate. The Hammer builds it.";

    public static readonly string DeerHelp =
        $"one 8 ft section of 7 ft deer fence: {DeerLogs} Log for the posts and {DeerWire} roll of Wire Mesh (Trading Post). It keeps deer and rabbits out. " +
        "Build sections end to end round your beds (each snaps to the last one's free end) and close the ring; a ring with any wattle in it lets deer jump in. The Hammer builds it.";

    public static readonly string GateHelp =
        $"a 4 ft gate as tall as the deer fence, from {GateLumber} Lumber and {GateCordage} Cordage for the hinges. Put it in a fence ring like a section: it swings open as you walk into it and shuts behind you, and it counts as deer fence. The Hammer builds it.";
}
