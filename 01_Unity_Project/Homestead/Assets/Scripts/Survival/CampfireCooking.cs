using System.Collections.Generic;
using UnityEngine;

// Cooking and boiling take a little while (Mike, 2026-09-26: not too long, but not instant and nowhere near real
// time). Clicking Cook or Boil in the Inventory queues the item here; each piece then takes a few seconds at the fire
// — a fish about 4, a haunch of venison 8, a litre of water 6 — standing in for the minutes it would really take.
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

    [Header("Sound")]
    [Tooltip("Seconds to fade the sizzle or boiling in or out.")]
    [SerializeField, Min(0.01f)] float sizzleFade = 0.4f;

    readonly List<Job> queue = new List<Job>();
    float progress;
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

    // Queues count more of an item (int.MaxValue = the whole stack). Returns false if it can't be cooked here.
    public bool Enqueue(ItemDefinition item, int count)
    {
        if (item == null || count <= 0 || !(Cooking.IsCookable(item.Id) || Cooking.IsBoilable(item.Id)))
            return false;
        if (Cooking.IsBoilable(item.Id) && !Cooking.HasContainer)
        {
            ToolStatus.Flash("Boiling water needs a Bucket to boil it in");
            return false;
        }

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

    // How many of this item are still to cook, capped at what's actually carried.
    public int RemainingOf(ItemDefinition item)
    {
        Job job = Find(item);
        return job == null ? 0 : Mathf.Min(job.remaining, Carried(item));
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
        bool boiling = Cooking.IsBoilable(current.item.Id);
        string verb = boiling ? "Boiling" : "Cooking";
        int left = RemainingOf(current.item);

        CampfireState fire = Cooking.FireInReach(player);
        if (fire == null)
        {
            if (state == GameState.Playing)
                ToolStatus.Report($"{verb} paused — get back to a burning campfire", ProgressOf(current.item), false);
            return NoSound;
        }
        sizzleAt = fire.position;

        // Unscaled, so it carries on behind the Inventory screen (which stops the clock).
        progress += Time.unscaledDeltaTime;
        float seconds = SecondsFor(current.item);
        if (state == GameState.Playing)
        {
            string what = boiling ? $"{current.item.DisplayName}, {left} L to go" : $"{current.item.DisplayName}, {left} to go";
            ToolStatus.Report($"{verb} {what}", Mathf.Clamp01(progress / seconds), false);
        }

        int playing = boiling ? BoilSound : CookSound;
        if (progress < seconds)
            return playing;

        progress = 0f;
        if (Cooking.Cook(current.item, 1, player) > 0 && current.remaining != int.MaxValue)
            current.remaining--;
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
