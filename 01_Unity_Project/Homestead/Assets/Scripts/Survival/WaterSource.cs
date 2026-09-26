using System;
using UnityEngine;

// Open water the player can use by looking at it within reach (First_Person_Controller.md's "Draw Water"
// interaction). With the Bucket equipped it fills the bucket (Water_System.md's Hand Carrying), otherwise it drinks on
// the spot. Needs a collider the interaction raycast can hit; on the property's water surface that collider sits on the
// Water layer, which doesn't collide with the player, so the player still wades through the creek rather than standing
// on it.
//
// One water surface covers the whole spring → creek → pond system, so quality is by zone: the nearest zone containing
// the player wins, otherwise the default. Drinking here carries the quality's illness risk, scaled to the amount drunk
// (Water Purification); collected water keeps its quality until it's boiled.
public class WaterSource : MonoBehaviour, IInteractable
{
    public const string BucketItemId = "bucket";

    [Serializable]
    public struct QualityZone
    {
        public string name;
        public Vector3 center;
        [Min(0f)] public float radius;
        public WaterQuality quality;
    }

    [Tooltip("Hydration restored per drink.")]
    [SerializeField, Min(0f)] float hydrationPerDrink = 20f;
    [Tooltip("Litres one Bucket holds (1 L of water = 1 kg).")]
    [SerializeField, Min(1)] int bucketLitres = 10;
    [Tooltip("Quality anywhere not covered by a zone (Water_System.md: fast-moving creeks are Good).")]
    [SerializeField] WaterQuality defaultQuality = WaterQuality.Good;
    [SerializeField] QualityZone[] zones = Array.Empty<QualityZone>();

    PlayerController lastPlayer;

    public int BucketLitres => bucketLitres;

    public string InteractionPrompt
    {
        get
        {
            WaterQuality quality = QualityAt(lastPlayer != null ? lastPlayer.transform.position : transform.position);
            int fill = FillableLitres();
            if (fill > 0)
                return $"Fill Bucket  ({quality} water, {fill} L)";
            return $"Drink  ({quality} water{RiskNote(quality)})";
        }
    }

    static string RiskNote(WaterQuality quality)
    {
        switch (quality)
        {
            case WaterQuality.Excellent: return "";
            case WaterQuality.Good: return ", slight risk of sickness";
            case WaterQuality.Questionable: return ", risk of sickness";
            default: return ", high risk of sickness";
        }
    }

    public void Configure(WaterQuality fallback, QualityZone[] qualityZones)
    {
        defaultQuality = fallback;
        zones = qualityZones ?? Array.Empty<QualityZone>();
    }

    // The quality zone a point is in (e.g. "Spring Hollow" or "Bass Hole pond"), or null for the default water.
    public string ZoneAt(Vector3 position)
    {
        string found = null;
        float best = float.MaxValue;
        foreach (QualityZone zone in zones)
        {
            float d = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(zone.center.x, zone.center.z));
            if (d <= zone.radius && d < best)
            {
                best = d;
                found = zone.name;
            }
        }
        return found;
    }

    public WaterQuality QualityAt(Vector3 position)
    {
        WaterQuality quality = defaultQuality;
        float best = float.MaxValue;
        foreach (QualityZone zone in zones)
        {
            float d = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(zone.center.x, zone.center.z));
            if (d <= zone.radius && d < best)
            {
                best = d;
                quality = zone.quality;
            }
        }
        return quality;
    }

    public bool CanInteract(PlayerController player)
    {
        lastPlayer = player;
        return FillableLitres() > 0 ||
               (SurvivalManager.Instance != null && SurvivalManager.Instance.Hydration < SurvivalManager.MaxValue);
    }

    public void Interact(PlayerController player)
    {
        lastPlayer = player;
        int fill = FillableLitres();
        if (fill > 0)
        {
            string itemId = WaterQualities.ItemId(QualityAt(player.transform.position));
            InventoryManager.Instance.AddToPlayer(itemId, fill);
            return;
        }

        if (SurvivalManager.Instance != null && SurvivalManager.Instance.Hydration < SurvivalManager.MaxValue)
        {
            SurvivalManager survival = SurvivalManager.Instance;
            survival.Drink(hydrationPerDrink);
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.Drink);

            // The item's chance is per litre (HydrationPerLitre); a drink of a different size scales it.
            WaterQuality quality = QualityAt(player.transform.position);
            float perLitre = WaterQualities.IllnessChance(quality);
            float chance = 1f - Mathf.Pow(1f - perLitre, hydrationPerDrink / WaterQualities.HydrationPerLitre);
            SurvivalManager.Sickness before = survival.SicknessLevel;
            if (survival.RollIllness(chance))
                ToolStatus.Flash($"The {quality.ToString().ToLower()} water — {Food.SickMessage(before, survival.SicknessLevel)}", 5f);
        }
    }

    // With the Bucket equipped: how many litres would go in — every carried bucket holds bucketLitres between them,
    // limited by what the player can still carry. 0 when no bucket is equipped or they're all full.
    int FillableLitres()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || inventory.EquippedTool == null || inventory.EquippedTool.Id != BucketItemId)
            return 0;

        int room = inventory.Player.Count(BucketItemId) * bucketLitres - WaterQualities.LitresCarried(inventory.Player);
        if (room <= 0)
            return 0;

        ItemDefinition water = ItemDatabase.Get(WaterQualities.ItemId(defaultQuality));
        return water != null ? Mathf.Min(room, inventory.Player.SpaceFor(water)) : 0;
    }
}
