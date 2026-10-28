using System.Collections.Generic;
using UnityEngine;

// Cooking and boiling take a little while (Mike, 2026-09-26: not too long, but not instant and nowhere near real
// time). Clicking Cook or Boil in the Inventory queues the item here; each piece then takes a few seconds at the fire
// — a fish about 4, a haunch of venison 8, a litre of water 6 — standing in for the minutes it would really take.
// Boiling pours the water from the Bucket into the Cooking Pot first (sfx_pour_water), a potful at a time.
// It keeps going while the Inventory is open, so the player can watch the row count down, and shows on the HUD with a
// progress bar once they close it. The raw item is only used up as each piece finishes, so walking away from the fire
// (or it going out) just pauses the queue and nothing is lost. Runs after the tools so its HUD line wins.
// While meat or fish is on the fire it sizzles (cooking.mp3), and water bubbles while it boils (boiling_water.wav) —
// both looped from the fire itself, crossfading when the queue moves from one to the other.
[DefaultExecutionOrder(100)]
public class CampfireCooking : MonoBehaviour
{
    class Job
    {
        public ItemDefinition item;
        public int remaining;
    }

    [Header("Seconds per piece")]
    [SerializeField, Min(0.1f)] float fishSeconds = 4f;
    [SerializeField, Min(0.1f)] float smallMeatSeconds = 5f;   // small game, chicken
    [SerializeField, Min(0.1f)] float birdSeconds = 6f;        // turkey, waterfowl
    [SerializeField, Min(0.1f)] float venisonSeconds = 8f;
    [SerializeField, Min(0.1f)] float boilSeconds = 6f;        // per litre
    [Tooltip("Litres the Cooking Pot holds: water is poured in this much at a time.")]
    [SerializeField, Min(1)] int potLitres = 4;

    [Header("Sound")]
    [Tooltip("Seconds to fade the sizzle or boiling in or out.")]
    [SerializeField, Min(0.01f)] float sizzleFade = 0.4f;

    readonly List<Job> queue = new List<Job>();
    float progress;
    int litresInPot; // poured in and waiting to boil
    PlayerController player;
    AudioSource sizzle;
    float sizzleLevel;   // 0-1 fade

    public static CampfireCooking Instance { get; private set; }

    public bool IsBusy => queue.Count > 0;

    void Awake() => player = GetComponent<PlayerController>();
    void OnEnable() => Instance = this;
    void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    public float SecondsFor(ItemDefinition item)
    {
        if (item == null) return 1f;
        if (Cooking.IsSapBoil(item.Id)) return MapleRules.SecondsPerSyrup; // a whole 10 L batch
        if (Cooking.IsLyeBoil(item.Id)) return PotashRules.BoilSeconds;    // a whole batch of 3 Lye
        if (Cooking.IsBoilable(item.Id)) return boilSeconds;
        switch (item.Id)
        {
            case "venison": return venisonSeconds;
            case "turkey_meat":
            case "waterfowl_meat": return birdSeconds;
            case "small_game_meat":
            case "chicken_meat": return smallMeatSeconds;
            default: return fishSeconds;
        }
    }

    // Queues count more of an item (int.MaxValue = the whole stack). Returns false if it can't be cooked here. For Maple Sap the count is
    // batches of 10 L (one Syrup each), not litres.
    public bool Enqueue(ItemDefinition item, int count)
    {
        if (item == null || count <= 0 || !(Cooking.IsCookable(item.Id) || Cooking.IsBoilable(item.Id) || Cooking.IsSapBoil(item.Id) || Cooking.IsLyeBoil(item.Id)))
            return false;
        if (Cooking.IsLyeBoil(item.Id))
        {
            if (!Cooking.CheckBoilVessel(player, "Boiling lye", TripodRules.KettleId))
                return false;
            if (PotashRules.PotashBatches(Carried(item), CarriedFirewood) <= 0)
            {
                ToolStatus.Flash("Boiling lye " + PotashRules.NeedMessage(Carried(item), CarriedFirewood));
                return false;
            }
        }
        if (Cooking.IsSapBoil(item.Id))
        {
            if (!Cooking.CheckBoilVessel(player, "Boiling sap"))
                return false;
            if (MapleRules.SyrupFrom(Carried(item)) <= 0)
            {
                ToolStatus.Flash("Boiling " + MapleRules.NeedMessage(Carried(item)));
                return false;
            }
        }
        if (Cooking.IsBoilable(item.Id) && !Cooking.CheckBoilVessel(player, "Boiling water"))
            return false;

        Job job = Find(item);
        if (job == null)
            queue.Add(job = new Job { item = item });
        job.remaining = count == int.MaxValue || job.remaining == int.MaxValue ? int.MaxValue : job.remaining + count;
        return true;
    }

    public void Cancel(ItemDefinition item)
    {
        Job job = Find(item);
        if (job == null)
            return;
        if (queue.IndexOf(job) == 0)
            progress = 0f;
        queue.Remove(job);
    }

    public bool IsQueued(ItemDefinition item) => Find(item) != null;

    // 0-1 through the piece being cooked if this item is first in line, else -1.
    public float ProgressOf(ItemDefinition item)
    {
        if (queue.Count == 0 || queue[0].item != item)
            return -1f;
        return Mathf.Clamp01(progress / SecondsFor(item));
    }

    // How many of this item are still to cook, capped at what's actually carried (for sap: the whole batches it still makes).
    public int RemainingOf(ItemDefinition item)
    {
        Job job = Find(item);
        if (job == null)
            return 0;
        if (Cooking.IsLyeBoil(item.Id))
            return Mathf.Min(job.remaining, PotashRules.PotashBatches(Carried(item), CarriedFirewood)); // batches of 3 Lye and 2 Firewood
        return Mathf.Min(job.remaining, Cooking.IsSapBoil(item.Id) ? MapleRules.SyrupFrom(Carried(item)) : Carried(item));
    }

    Job Find(ItemDefinition item)
    {
        foreach (Job job in queue)
            if (job.item == item)
                return job;
        return null;
    }

    static int Carried(ItemDefinition item) =>
        InventoryManager.Instance != null ? InventoryManager.Instance.Player.Count(item.Id) : 0;

    static int CarriedFirewood => InventoryManager.Instance != null ? InventoryManager.Instance.Player.Count(AshRules.FirewoodId) : 0;

    // What the queued boil at this vessel is doing now, for the tripod's prompt: "boiling Lye, 2 Potash to go" (null when nothing is queued for it).
    public string JobTextFor(string vesselId)
    {
        if (queue.Count == 0 || vesselId == null || Cooking.BoilVessel(queue[0].item.Id) != vesselId)
            return null;
        ItemDefinition item = queue[0].item;
        int left = RemainingOf(item);
        if (left <= 0)
            return null;
        string unit = Cooking.IsLyeBoil(item.Id) ? "Potash" : Cooking.IsSapBoil(item.Id) ? "Syrup" : "L";
        return $"boiling {item.DisplayName}, {left} {unit} to go";
    }

    void Update()
    {
        int sound = Tick();
        AudioManager audio = AudioManager.Instance;
        UpdateSizzle(audio == null || sound == NoSound ? null : sound == BoilSound ? audio.Boiling : audio.CookingSizzle);
    }

    const int NoSound = 0, CookSound = 1, BoilSound = 2;

    // Runs the queue; returns what's on the fire right now, for its sound.
    int Tick()
    {
        // Drop finished jobs and ones whose item has run out (eaten, dropped).
        while (queue.Count > 0 && RemainingOf(queue[0].item) <= 0)
        {
            queue.RemoveAt(0);
            progress = 0f;
        }
        if (queue.Count == 0)
            return NoSound;

        GameState state = GameManager.Instance != null ? GameManager.Instance.State : GameState.Playing;
        if (state != GameState.Playing && state != GameState.Menu)
            return NoSound; // paused or loading

        Job current = queue[0];
        bool sap = Cooking.IsSapBoil(current.item.Id);
        bool lye = Cooking.IsLyeBoil(current.item.Id); // like sap, queued by batches, not litres
        bool boiling = Cooking.IsBoilable(current.item.Id) || sap || lye;
        string verb = boiling ? "Boiling" : "Cooking";
        int left = RemainingOf(current.item);

        // A boil needs its vessel hung on a tripod at a lit fire in reach; cooking meat and fish needs any lit fire. A boil whose vessel was
        // swapped or taken down stays queued but loses the batch's progress (the items are only used up when a batch finishes).
        string vessel = boiling ? Cooking.BoilVessel(current.item.Id) : null;
        CampfireState fire = vessel != null ? Cooking.VesselFireInReach(player, vessel) : Cooking.FireInReach(player);
        if (fire == null)
        {
            if (vessel != null && Cooking.FireInReach(player) != null)
            {
                progress = 0f;
                litresInPot = 0;
                if (state == GameState.Playing)
                    ToolStatus.Report(TripodRules.PausedMessage(verb, Cooking.VesselName(vessel)), -1f, false);
                return NoSound;
            }
            if (state == GameState.Playing)
                ToolStatus.Report($"{verb} paused — get back to a burning campfire", ProgressOf(current.item), false);
            return NoSound;
        }
        sizzleAt = fire.position;

        // Pour the next potful in before boiling it (sap: the pot is poured as each 10 L batch starts, and topped up as it boils down).
        if (boiling && progress <= 0f && (sap || lye || litresInPot <= 0))
        {
            if (!sap && !lye)
                litresInPot = Mathf.Min(potLitres, left);
            if (!lye && AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.Pour);
        }

        // Unscaled, so it carries on behind the Inventory screen (which stops the clock).
        progress += Time.unscaledDeltaTime;
        float seconds = SecondsFor(current.item);
        if (state == GameState.Playing)
        {
            string what = sap ? $"{current.item.DisplayName}, {left} Syrup to go"
                        : lye ? $"{current.item.DisplayName}, {left} Potash to go"
                        : boiling ? $"{current.item.DisplayName}, {left} L to go" : $"{current.item.DisplayName}, {left} to go";
            ToolStatus.Report($"{verb} {what}", Mathf.Clamp01(progress / seconds), false);
        }

        int playing = boiling ? BoilSound : CookSound;
        if (progress < seconds)
            return playing;

        progress = 0f;
        if (Cooking.Cook(current.item, 1, player) > 0)
        {
            if (current.remaining != int.MaxValue)
                current.remaining--;
            if (boiling && !sap && !lye)
                litresInPot--;
        }
        else if (sap || lye)
        {
            queue.Remove(current); // no vessel or no room: stop rather than try again every batch (the sap, or the Lye and Firewood, are untouched)
        }
        return playing;
    }

    Vector3 sizzleAt;

    Sound sizzleSound; // what the source is set up to play

    // Fades the wanted loop in at the fire and out when it stops, looping only its steady stretch. Changing sound
    // (cooking to boiling) fades the old one out before the new one fades in.
    void UpdateSizzle(Sound want)
    {
        if (want != null && want.clip == null)
            want = null;
        AudioManager audio = AudioManager.Instance;
        if (audio == null)
            return;

        if (sizzle == null)
        {
            if (want == null)
                return;
            var go = new GameObject("Cooking Sizzle");
            go.transform.SetParent(transform, false);
            sizzle = go.AddComponent<AudioSource>();
            sizzle.loop = true;
            sizzle.playOnAwake = false;
            sizzle.spatialBlend = 1f; // from the fire
            sizzle.rolloffMode = AudioRolloffMode.Linear;
            sizzle.minDistance = 2f;
            sizzle.maxDistance = 15f;
            sizzle.dopplerLevel = 0f;
            sizzle.outputAudioMixerGroup = audio.GetGroup(AudioChannel.Sfx);
        }

        // Swap sounds only once the old one has faded out.
        if (want != sizzleSound && sizzleLevel <= 0f && want != null)
        {
            sizzle.Stop();
            sizzleSound = want;
            sizzle.clip = want.clip;
        }
        Sound sound = sizzleSound;
        if (sound == null)
            return;

        bool on = want != null && want == sound;
        sizzleLevel = Mathf.MoveTowards(sizzleLevel, on ? 1f : 0f, Time.unscaledDeltaTime / sizzleFade);
        sizzle.volume = sound.volume * sizzleLevel;
        if (on)
            sizzle.transform.position = sizzleAt + Vector3.up * 0.3f;

        if (sizzleLevel > 0f && !sizzle.isPlaying)
        {
            if (sizzle.time < sound.startTime || sizzle.time >= sound.clip.length - 0.1f)
                sizzle.time = Mathf.Min(sound.startTime, sound.clip.length - 0.1f);
            sizzle.Play();
        }
        else if (sizzleLevel <= 0f && sizzle.isPlaying)
        {
            sizzle.Pause(); // resumes mid-sizzle next time rather than from the top
        }

        if (sizzle.isPlaying && sound.loopEnd > sound.startTime && sizzle.time >= sound.loopEnd)
            sizzle.time = sound.startTime;
    }
}
