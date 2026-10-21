using UnityEngine;

// Trading_Post_Market.md's prices and group rules as one data asset (Resources/Market/MarketData), so they can be tuned without a script
// edit. Rows: every item the Trading Post stocks or buys, with its group, pool and base price in cents. Groups: the drop per unit sold, the
// floor and the four seasonal targets, all in basis points (10000 = 1.0). Seeded from MarketCatalog by the Homestead/Market menu command.
[CreateAssetMenu(fileName = "MarketData", menuName = "Homestead/Market Data")]
public class MarketData : ScriptableObject
{
    public const string ResourcePath = "Market/MarketData";

    [Tooltip("Every item the store stocks or only buys. baseCents is today's price; items with the same pool share one multiplier.")]
    public MarketRow[] rows;

    [Tooltip("Per group: dropBp per unit sold, floorBp, and the Spring/Summer/Fall/Winter targets (10000 = 1.0).")]
    public MarketGroupRule[] groups;

    public MarketTable Build(int sellBackPercent) => new MarketTable(rows ?? new MarketRow[0], groups ?? new MarketGroupRule[0], sellBackPercent);

    // Fills the asset with the default catalog.
    public void Seed()
    {
        rows = (MarketRow[])MarketCatalog.Rows.Clone();
        groups = (MarketGroupRule[])MarketCatalog.Groups.Clone();
    }
}
