using System.Collections.Generic;
using UnityEngine;

// A downed animal, ready for field dressing (Hunting_System.md): hold E to dress it and take the meat and whatever
// else it yields — hide and antlers from deer, feathers from birds, fur from small game (the species sheets in
// 05_Documentation/Wildlife). Meat quality depends on how soon it's dressed, on the in-game clock:
//   within 1 hour — full meat;  within 3 — three quarters, and a day's less shelf life;
//   within 8 — half, two days less;  later — the meat's spoiled, only hide, antlers, feathers or fur are left.
// A wounding hit that still brought the animal down halves the meat. A killing arrow is recovered. Anything that
// doesn't fit in the pack stays on the carcass for another trip. Carcasses rot away after a day (and aren't saved).
public class Carcass : MonoBehaviour, IInteractable
{
    const float RotHours = 24f;

    Animal animal;
    bool reduced;
    double diedAt;
    bool dressed;
    int spoilageShift; // days of shelf life lost to late dressing
    bool driftedAshore;
    float landTravel;
    public bool KilledByArrow { get; set; }
    readonly List<(string item, int count)> remaining = new List<(string, int)>();

    public void Begin(Animal deadAnimal, bool reducedHarvest)
    {
        animal = deadAnimal;
        reduced = reducedHarvest;
        diedAt = TimeManager.Instance != null ? TimeManager.Instance.TotalHours : 0;
    }

    double HoursSinceDeath => TimeManager.Instance != null ? TimeManager.Instance.TotalHours - diedAt : 0;

    void Update()
    {
        if (animal == null)
            return;

        if (HoursSinceDeath > RotHours)
        {
            Destroy(gameObject);
            return;
        }

        // Downed waterfowl drift in to the bank nearest the hunter and up onto it within easy reach, then settle there.
        if (animal.Swims && WildlifeManager.Player != null && !driftedAshore)
        {
            Vector3 toward = WildlifeManager.Player.transform.position - transform.position;
            toward.y = 0f;
            bool overWater = Animal.IsOverWater(transform.position, out float surface);
            if (!overWater)
                landTravel += 1.2f * Time.deltaTime;

            if (toward.magnitude <= 1.2f || landTravel >= 1.5f)
            {
                Settle();
                driftedAshore = true;
            }
            else
            {
                transform.position += toward.normalized * 1.2f * Time.deltaTime;
                if (overWater)
                    transform.position = new Vector3(transform.position.x, surface, transform.position.z);
                else
                    Settle();
            }
        }
    }

    void Settle()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null && !Animal.IsOverWater(transform.position, out _))
            transform.position = new Vector3(transform.position.x, terrain.SampleHeight(transform.position) + terrain.transform.position.y,
                                             transform.position.z);
    }

    string Freshness
    {
        get
        {
            double hours = HoursSinceDeath;
            if (hours <= 1) return "fresh";
            if (hours <= 3) return "meat still good";
            if (hours <= 8) return "some meat spoiling";
            return "meat spoiled";
        }
    }

    public string InteractionPrompt
    {
        get
        {
            if (animal == null)
                return "";
            if (!dressed)
                return $"Field Dress {animal.DisplayName}  ({Freshness})";
            if (remaining.Count == 0)
                return "";
            (string item, int count) next = remaining[0];
            return $"Take remaining {ItemDatabase.Get(next.item)?.DisplayName ?? next.item}  ({next.count})";
        }
    }

    public bool CanInteract(PlayerController player) => animal != null && (!dressed || remaining.Count > 0);

    public void Interact(PlayerController player)
    {
        if (animal == null)
            return;

        if (!dressed)
        {
            dressed = true;
            BuildYield();
        }

        TakeWhatFits();
        if (remaining.Count == 0)
            Destroy(gameObject, 0.1f);
    }

    void BuildYield()
    {
        WildlifeManager wildlife = WildlifeManager.Instance;
        WildlifeManager.Yield yield = wildlife != null ? wildlife.YieldFor(animal.SpeciesId) : null;
        if (yield == null)
            return;

        double hours = HoursSinceDeath;
        float quality = hours <= 1 ? 1f : hours <= 3 ? 0.75f : hours <= 8 ? 0.5f : 0f;
        if (reduced)
            quality *= 0.5f;
        int meat = quality > 0f ? Mathf.Max(1, Mathf.RoundToInt(yield.meat * quality)) : 0;
        if (meat > 0)
            remaining.Add((yield.meatItem, meat));

        TimeManager time = TimeManager.Instance;
        bool rut = time != null && time.CurrentSeason == Season.Fall && time.SeasonProgress >= 0.5f;

        if (animal.SpeciesId == "deer")
        {
            // Large_Game.md: hide and antlers year-round, likelier in the Fall Rut; antlers only from bucks.
            if (Random.value < (rut ? 0.85f : 0.45f))
                remaining.Add(("deer_hide", 1));
            if (animal.IsBuck && Random.value < (rut ? 0.8f : 0.35f))
                remaining.Add(("deer_antlers", 1));
        }
        else if (!string.IsNullOrEmpty(yield.extraItem) && yield.extraCount > 0)
        {
            remaining.Add((yield.extraItem, yield.extraCount));
        }

        if (KilledByArrow)
            remaining.Add(("arrows", 1));

        // Late dressing: the meat has less shelf life left (Hunting_System.md: delayed dressing accelerates spoilage).
        spoilageShift = hours <= 1 ? 0 : hours <= 3 ? 1 : 2;

        string note = meat == 0 ? "the meat had spoiled" : quality < 1f ? $"{meat} meat (it waited {hours:0.#} h)" : $"{meat} meat";
        ToolStatus.Flash($"Field dressed the {animal.DisplayName.ToLower()} — {note}");
    }

    void TakeWhatFits()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;

        int today = TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;
        bool leftSomething = false;
        for (int i = remaining.Count - 1; i >= 0; i--)
        {
            (string item, int count) entry = remaining[i];
            bool perishable = entry.item.EndsWith("meat") || entry.item == "venison";
            int added = perishable ? inventory.AddToPlayer(entry.item, entry.count, today - spoilageShift)
                                   : inventory.AddToPlayer(entry.item, entry.count);
            if (added >= entry.count)
                remaining.RemoveAt(i);
            else
            {
                remaining[i] = (entry.item, entry.count - added);
                leftSomething = true;
            }
        }

        if (leftSomething)
            ToolStatus.Flash("No room to carry everything — the rest is still here");
    }
}
