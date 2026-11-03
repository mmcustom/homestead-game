using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// EditMode tests for the creek bank look (Mike, 2026-11-04): Wet Clay and Creek Sand painted into the play-time copy of the terrain along the water, snow and leaf litter keeping
// only a share of them, the footstep and minimap handling, and the "Sandy bank" prompt. The rules are engine-free (CreekBankRules, GroundCoverRules); the main-assembly parts
// are reached by reflection as the other world tests do. The end-to-end tests build a tiny terrain and two box colliders on the Water layer in memory and delete them afterwards.
// Nothing here touches the Saves folder, a scene file or the terrain asset.
public class CreekBankTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    static readonly float[] Noises = { 0f, 0.25f, 0.5f, 1f };

    // --- Distance to weights ---

    [Test]
    public void NothingIsPaintedBeyondTheBankReach()
    {
        foreach (float distance in new[] { PotteryRules.BankReachMetres, PotteryRules.BankReachMetres + 0.01f, 4f, 10f, 100f })
            foreach (float inner in Noises)
                foreach (float outer in Noises)
                {
                    CreekBankRules.Weights(CreekBankRules.Water.Dry, distance, inner, outer, out float clay, out float sand);
                    Assert.That(clay, Is.EqualTo(0f), $"clay at {distance} m");
                    Assert.That(sand, Is.EqualTo(0f), $"sand at {distance} m");
                }
    }

    [Test]
    public void DryGroundInsideTheWetBankIsFullClay()
    {
        float certain = PotteryRules.WetBankMetres - CreekBankRules.InnerFadeMaxMetres; // the soft edge never starts before this
        foreach (float distance in new[] { 0f, 0.5f, 1f, certain })
            foreach (float inner in Noises)
                foreach (float outer in Noises)
                {
                    CreekBankRules.Weights(CreekBankRules.Water.Dry, distance, inner, outer, out float clay, out float sand);
                    Assert.That(clay, Is.EqualTo(1f).Within(1e-5f), $"clay at {distance} m");
                    Assert.That(sand, Is.EqualTo(0f).Within(1e-5f), $"sand at {distance} m");
                }
    }

    [Test]
    public void SandRunsFromTheWetBankOutToTheBankReach()
    {
        foreach (float inner in Noises)
            foreach (float outer in Noises)
            {
                CreekBankRules.Weights(CreekBankRules.Water.Dry, PotteryRules.WetBankMetres, inner, outer, out float clay, out float sand);
                Assert.That(clay, Is.EqualTo(0f).Within(1e-5f), "no Clay from the wet bank's limit outward");
                Assert.That(sand, Is.GreaterThan(0.99f), "Sand starts where the Clay stops");

                CreekBankRules.Weights(CreekBankRules.Water.Dry, PotteryRules.WetBankMetres + 0.1f, inner, outer, out clay, out sand);
                Assert.That(clay, Is.EqualTo(0f).Within(1e-5f));
                Assert.That(sand, Is.GreaterThan(0.85f));

                CreekBankRules.Weights(CreekBankRules.Water.Dry, PotteryRules.BankReachMetres - 0.01f, inner, outer, out clay, out sand);
                Assert.That(sand, Is.LessThan(0.05f), "the Sand has faded out by the bank's reach");
            }
    }

    [Test]
    public void TheWeightsAreNeverNegativeAndNeverAddUpToMoreThanOne()
    {
        foreach (CreekBankRules.Water water in new[] { CreekBankRules.Water.Dry, CreekBankRules.Water.Shallow, CreekBankRules.Water.Deep })
            for (float distance = 0f; distance <= 5f; distance += 0.05f)
                foreach (float inner in Noises)
                    foreach (float outer in Noises)
                    {
                        CreekBankRules.Weights(water, distance, inner, outer, out float clay, out float sand);
                        Assert.That(clay, Is.InRange(0f, 1f));
                        Assert.That(sand, Is.InRange(0f, 1f));
                        Assert.That(clay + sand, Is.LessThanOrEqualTo(1f + 1e-5f), $"{water} at {distance} m");
                    }
    }

    [Test]
    public void ShallowWaterIsClayAndDeepWaterIsLeftAlone()
    {
        CreekBankRules.Weights(CreekBankRules.Water.Shallow, 0f, 0.5f, 0.5f, out float clay, out float sand);
        Assert.That(clay, Is.EqualTo(1f));
        Assert.That(sand, Is.EqualTo(0f));
        CreekBankRules.Weights(CreekBankRules.Water.Deep, 0f, 0.5f, 0.5f, out clay, out sand);
        Assert.That(clay, Is.EqualTo(0f));
        Assert.That(sand, Is.EqualTo(0f));
    }

    [Test]
    public void TheReachComesFromThePotteryRulesAndIsNotCopied()
    {
        string text = SourceText("Scripts/Economy/CreekBankRules.cs");
        Assert.That(text, Does.Contain("PotteryRules.WetBankMetres"));
        Assert.That(text, Does.Contain("PotteryRules.BankReachMetres"));
        Assert.That(text, Does.Contain("PotteryRules.WadeDigDepthMetres"));
        Assert.That(PotteryRules.WetBankMetres, Is.EqualTo(2f));
        Assert.That(PotteryRules.BankReachMetres, Is.EqualTo(3f));
    }

    [Test]
    public void TheEdgesAreIrregularButSteady()
    {
        // The width of the soft edge follows the noise, so the edge is not a straight line; the same place always gives the same value.
        CreekBankRules.Weights(CreekBankRules.Water.Dry, 1.6f, 0f, 0.5f, out float narrow, out _);
        CreekBankRules.Weights(CreekBankRules.Water.Dry, 1.6f, 1f, 0.5f, out float wide, out _);
        Assert.That(narrow, Is.GreaterThan(wide), "a wider fade has already started at 1.6 m");
        Assert.That(CreekBankRules.HashNoise(10.3f, 44.7f, 3f, 5), Is.EqualTo(CreekBankRules.HashNoise(10.3f, 44.7f, 3f, 5)));
        float low = 1f, high = 0f;
        for (int i = 0; i < 400; i++)
        {
            float n = CreekBankRules.HashNoise(i * 0.7f, i * 1.3f, 3f, 5);
            Assert.That(n, Is.InRange(0f, 1f));
            low = Math.Min(low, n);
            high = Math.Max(high, n);
        }
        Assert.That(high - low, Is.GreaterThan(0.5f), "the noise actually varies");
    }

    // --- The water rule ---

    [Test]
    public void WaterIsJudgedLikeTheDigRuleDoes()
    {
        Assert.That(CreekBankRules.WaterAt(false, 10f, 5f), Is.EqualTo(CreekBankRules.Water.Dry), "no ray hit");
        Assert.That(CreekBankRules.WaterAt(true, 10f, 9.97f), Is.EqualTo(CreekBankRules.Water.Dry), "within 5 cm of the ground is dry");
        Assert.That(CreekBankRules.WaterAt(true, 10f, 10.5f), Is.EqualTo(CreekBankRules.Water.Dry), "ground above the surface");
        Assert.That(CreekBankRules.WaterAt(true, 10f, 9.9f), Is.EqualTo(CreekBankRules.Water.Shallow));
        Assert.That(CreekBankRules.WaterAt(true, 10f, 10f - (PotteryRules.WadeDigDepthMetres - 0.01f)), Is.EqualTo(CreekBankRules.Water.Shallow), "just under 0.3 m deep is still the shallows");
        Assert.That(CreekBankRules.WaterAt(true, 10f, 10f - (PotteryRules.WadeDigDepthMetres + 0.01f)), Is.EqualTo(CreekBankRules.Water.Deep));
        Assert.That(CreekBankRules.RayStartAboveMetres, Is.EqualTo(30f), "Animal.IsOverWater starts 30 m above");
        Assert.That(CreekBankRules.RayLengthMetres, Is.EqualTo(80f), "and looks 80 m down");
        Assert.That(CreekBankRules.WaterMarginMetres, Is.EqualTo(0.05f));
        string animal = SourceText("Scripts/Wildlife/Animal.cs");
        Assert.That(animal, Does.Contain("at.y + 30f").And.Contain("80f").And.Contain("surface - 0.05f"), "the numbers copied from Animal.IsOverWater still match it");
    }

    // --- The distance pass ---

    [Test]
    public void TheDistancePassMatchesAPlainSearchOnASmallMask()
    {
        const int W = 9, H = 6;
        const float Cell = 2f;
        var water = new bool[W * H];
        water[1 * W + 1] = true;
        water[4 * W + 7] = true;
        water[4 * W + 8] = true;

        float[] field = CreekBankRules.DistanceToWater(water, W, H, Cell);
        for (int z = 0; z < H; z++)
            for (int x = 0; x < W; x++)
            {
                float best = float.PositiveInfinity;
                for (int wz = 0; wz < H; wz++)
                    for (int wx = 0; wx < W; wx++)
                        if (water[wz * W + wx])
                            best = Math.Min(best, (float)Math.Sqrt((x - wx) * (x - wx) + (z - wz) * (z - wz)) * Cell);
                Assert.That(field[z * W + x], Is.EqualTo(best).Within(1e-3f), $"square {x},{z}");
            }
        Assert.That(field[1 * W + 1], Is.EqualTo(0f), "water is at distance 0");
        Assert.That(field[1 * W + 3], Is.EqualTo(4f).Within(1e-4f), "two squares of 2 m");
    }

    [Test]
    public void WithNoWaterEveryDistanceIsInfinite()
    {
        float[] field = CreekBankRules.DistanceToWater(new bool[5 * 4], 5, 4, 1f);
        Assert.That(field.All(float.IsPositiveInfinity), Is.True);
    }

    [Test]
    public void TheEdgeIsHalfASquareNearerThanTheNextCentre()
    {
        Assert.That(CreekBankRules.EdgeDistance(1f, 1f), Is.EqualTo(0.5f));
        Assert.That(CreekBankRules.EdgeDistance(0f, 1f), Is.EqualTo(0f));
        Assert.That(CreekBankRules.EdgeDistance(3f, 2f), Is.EqualTo(2f));
    }

    // --- Painting a whole terrain ---

    // A 32 m terrain, flat at height 0, with shallow water across x 10 to 14 and deep water across x 4 to 6 (two box colliders on the Water layer).
    static GameObject MakeTerrain(string[] layerNames, out TerrainData data, bool withWater, int alphaRes = 64)
    {
        data = new TerrainData { heightmapResolution = 33, alphamapResolution = alphaRes };
        data.size = new Vector3(32f, 10f, 32f);
        data.terrainLayers = layerNames.Select(n => new TerrainLayer { name = n }).ToArray();
        var alphas = new float[alphaRes, alphaRes, layerNames.Length];
        for (int z = 0; z < alphaRes; z++)
            for (int x = 0; x < alphaRes; x++)
                alphas[z, x, 0] = 1f;
        data.SetAlphamaps(0, 0, alphas);

        GameObject terrain = Terrain.CreateTerrainGameObject(data);
        terrain.name = "Creek bank test terrain";
        if (withWater)
        {
            AddWater(terrain.transform, new Vector3(12f, 0.05f, 16f), new Vector3(4f, 0.1f, 32f)); // top at 0.1 m: 10 cm deep
            AddWater(terrain.transform, new Vector3(5f, 0.5f, 16f), new Vector3(2f, 1f, 32f));     // top at 1 m: deep
        }
        Physics.SyncTransforms();
        return terrain;
    }

    static void AddWater(Transform parent, Vector3 centre, Vector3 size)
    {
        var water = new GameObject("Creek bank test water") { layer = LayerMask.NameToLayer("Water") };
        water.transform.SetParent(parent, false);
        water.transform.position = centre;
        water.AddComponent<BoxCollider>().size = size;
    }

    static readonly string[] BaseLayers = { "Grass", "Dirt", "Gravel" };

    [Test]
    public void BuildPaintsTheBanksIntoTheTerrainAndLeavesFarGroundAlone()
    {
        GameObject go = MakeTerrain(BaseLayers, out TerrainData data, withWater: true);
        try
        {
            Assert.That(LayerMask.NameToLayer("Water"), Is.GreaterThanOrEqualTo(0), "the project has a Water layer");
            var banks = (Component)go.AddComponent(Find("CreekBanks"));
            Find("CreekBanks").GetMethod("Build").Invoke(banks, new object[] { go.GetComponent<Terrain>(), data });

            Assert.That((bool)Find("CreekBanks").GetProperty("Built").GetValue(banks), Is.True);
            Assert.That(data.alphamapLayers, Is.EqualTo(5), "the three painted layers and the two bank layers");
            Assert.That(data.terrainLayers[3].name, Is.EqualTo(CreekBankRules.ClayLayerName));
            Assert.That(data.terrainLayers[4].name, Is.EqualTo(CreekBankRules.SandLayerName));

            object field = Find("CreekBanks").GetProperty("LastField").GetValue(banks);
            Assert.That((int)field.GetType().GetField("waterCells").GetValue(field), Is.EqualTo(4 * 32 + 2 * 32), "4 shallow and 2 deep columns of 1 m squares");
            Assert.That((int)field.GetType().GetField("shallowCells").GetValue(field), Is.EqualTo(4 * 32));

            float[,,] a = data.GetAlphamaps(0, 0, 64, 64);
            int row = 32;
            // Pixels are 0.5 m. Water's far edge is at x = 14 m.
            Assert.That(a[row, 24, 3], Is.GreaterThan(0.95f), "the shallow bed is Clay");
            Assert.That(a[row, 29, 3], Is.GreaterThan(0.95f), "x 14.75: 0.75 m from the water, inside the wet bank");
            Assert.That(a[row, 31, 3] + a[row, 31, 4], Is.GreaterThan(0.9f), "x 15.75: 1.75 m out, Clay fading to Sand");
            Assert.That(a[row, 32, 4], Is.GreaterThan(0.75f), "x 16.25: 2.25 m out, Sand");
            Assert.That(a[row, 32, 3], Is.LessThan(0.05f), "and no Clay out there");
            Assert.That(a[row, 40, 0], Is.EqualTo(1f).Within(0.01f), "x 20.25: 6 m from the water, the old painting is untouched");
            Assert.That(a[row, 40, 3] + a[row, 40, 4], Is.LessThan(0.01f));
            Assert.That(a[row, 60, 0], Is.EqualTo(1f).Within(0.01f), "far ground is untouched");

            for (int z = 0; z < 64; z += 7)
                for (int x = 0; x < 64; x++)
                {
                    float sum = 0f;
                    for (int l = 0; l < 5; l++)
                        sum += a[z, x, l];
                    Assert.That(sum, Is.EqualTo(1f).Within(0.03f), $"weights at {x},{z}");
                }

            int bankCells = (int)Find("CreekBanks").GetProperty("BankCells").GetValue(banks);
            Assert.That(bankCells, Is.GreaterThan(1000));

            // Build paints only the rectangle near the water; the result must equal painting the whole map at once (Compose).
            var plain = new float[64, 64, 3];
            for (int z = 0; z < 64; z++)
                for (int x = 0; x < 64; x++)
                    plain[z, x, 0] = 1f;
            object[] args = { plain, field, new Vector3(32f, 10f, 32f), 0 };
            var whole = (float[,,])Find("CreekBanks").GetMethod("Compose").Invoke(null, args);
            Assert.That((int)args[3], Is.EqualTo(bankCells), "the same cells are painted either way");
            for (int z = 0; z < 64; z++)
                for (int x = 0; x < 64; x++)
                    for (int l = 0; l < 5; l++)
                        Assert.That(a[z, x, l], Is.EqualTo(whole[z, x, l]).Within(0.01f), $"layer {l} at {x},{z}");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    [Test]
    public void BuildDoesNothingWhenThereIsNoWater()
    {
        GameObject go = MakeTerrain(BaseLayers, out TerrainData data, withWater: false);
        try
        {
            var banks = (Component)go.AddComponent(Find("CreekBanks"));
            Find("CreekBanks").GetMethod("Build").Invoke(banks, new object[] { go.GetComponent<Terrain>(), data });
            Assert.That((bool)Find("CreekBanks").GetProperty("Built").GetValue(banks), Is.False);
            Assert.That(data.alphamapLayers, Is.EqualTo(3), "the terrain keeps its own three layers");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    [Test]
    public void BuildRefusesWhenTheLayersWouldGoPastEight()
    {
        GameObject go = MakeTerrain(new[] { "A", "B", "C", "D", "E" }, out TerrainData data, withWater: true);
        try
        {
            var banks = (Component)go.AddComponent(Find("CreekBanks"));
            Find("CreekBanks").GetMethod("Build").Invoke(banks, new object[] { go.GetComponent<Terrain>(), data });
            Assert.That(data.alphamapLayers, Is.EqualTo(5), "5 + 2 + SeasonalGround's 2 would be 9: left alone");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    // --- The layer budget ---

    [Test]
    public void TheRuntimeTotalIsSevenAndTheSavedTerrainStillHasThree()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/Art/Terrain/PropertyTerrainData.asset");
        Assert.That(asset, Is.Not.Null, "the saved terrain asset is where it was");
        Assert.That(asset.terrainLayers.Length, Is.EqualTo(3), "the asset is not changed: Grass, Dirt, Gravel");
        Assert.That(asset.alphamapLayers, Is.EqualTo(3));

        int total = asset.terrainLayers.Length + CreekBankRules.BankLayers + GroundCoverRules.SeasonalLayers;
        Assert.That(total, Is.EqualTo(7));
        Assert.That(total, Is.LessThanOrEqualTo(GroundCoverRules.MaxTerrainLayers));
        Assert.That(GroundCoverRules.MaxTerrainLayers, Is.EqualTo(8), "TerrainLit's _TERRAIN_8_LAYERS");
    }

    [Test]
    public void TheBudgetCheckAllowsUpToEightAndRefusesMore()
    {
        Assert.That(CreekBankRules.CanAddBankLayers(3), Is.True);
        Assert.That(CreekBankRules.CanAddBankLayers(4), Is.True, "4 + 2 + 2 = 8");
        Assert.That(CreekBankRules.CanAddBankLayers(5), Is.False);
        Assert.That(CreekBankRules.CanAddBankLayers(8), Is.False);
    }

    [Test]
    public void ComposeAddsExactlyTwoLayersAndSeasonalGroundThenMakesSeven()
    {
        var field = new CreekBanksField(40, 40, 1f, 20f);
        var source = new float[40, 40, 3];
        for (int z = 0; z < 40; z++)
            for (int x = 0; x < 40; x++)
                source[z, x, 2] = 1f;

        object[] args = { source, field.Build(), new Vector3(40f, 10f, 40f), 0 };
        var painted = (float[,,])Find("CreekBanks").GetMethod("Compose").Invoke(null, args);
        Assert.That(painted.GetLength(2), Is.EqualTo(5));
        Assert.That(painted.GetLength(2) + GroundCoverRules.SeasonalLayers, Is.EqualTo(7));
        Assert.That((int)args[3], Is.GreaterThan(0));

        for (int z = 0; z < 40; z++)
            for (int x = 0; x < 40; x++)
            {
                float sum = 0f;
                for (int l = 0; l < 5; l++)
                    sum += painted[z, x, l];
                Assert.That(sum, Is.EqualTo(1f).Within(1e-5f));
            }
        Assert.That(painted[20, 20, 3], Is.GreaterThan(0.9f), "the water column is Clay (shallow)");
        Assert.That(painted[20, 2, 2], Is.EqualTo(1f), "ground far from the water keeps the old painting exactly");
    }

    // A column of shallow water down the middle of a made-up field, built the way CreekBanks.Scan builds it.
    sealed class CreekBanksField
    {
        readonly int cellsX, cellsZ;
        readonly float cell, waterX;

        public CreekBanksField(int cellsX, int cellsZ, float cell, float waterX)
        {
            this.cellsX = cellsX;
            this.cellsZ = cellsZ;
            this.cell = cell;
            this.waterX = waterX;
        }

        public object Build()
        {
            Type type = Find("CreekBanks").GetNestedType("Field");
            object field = Activator.CreateInstance(type);
            var water = new CreekBankRules.Water[cellsX * cellsZ];
            var isWater = new bool[water.Length];
            for (int z = 0; z < cellsZ; z++)
            {
                int x = (int)(waterX / cell);
                water[z * cellsX + x] = CreekBankRules.Water.Shallow;
                isWater[z * cellsX + x] = true;
            }
            type.GetField("cellsX").SetValue(field, cellsX);
            type.GetField("cellsZ").SetValue(field, cellsZ);
            type.GetField("cellMetres").SetValue(field, cell);
            type.GetField("water").SetValue(field, water);
            type.GetField("distance").SetValue(field, CreekBankRules.DistanceToWater(isWater, cellsX, cellsZ, cell));
            type.GetField("waterCells").SetValue(field, cellsZ);
            type.GetField("shallowCells").SetValue(field, cellsZ);
            return field;
        }
    }

    // --- SeasonalGround's weights with banks ---

    [Test]
    public void WithNoBankTheCoverIsExactlyWhatSeasonalGroundAlwaysPainted()
    {
        for (float snowAmount = 0f; snowAmount <= 1f; snowAmount += 0.125f)
            for (float mask = 0f; mask <= 1f; mask += 0.1f)
                for (float litterAmount = 0f; litterAmount <= 1f; litterAmount += 0.25f)
                    for (float canopy = 0f; canopy <= 1f; canopy += 0.2f)
                    {
                        // The formulas as they were before the banks existed.
                        float snow = snowAmount * mask;
                        float leaves = litterAmount * canopy;
                        float keep = (1f - snow) * (1f - leaves);
                        float litter = leaves * (1f - snow);

                        GroundCoverRules.Cover(snowAmount * mask, litterAmount * canopy, 0f, 0.25f, 0.25f, out float newKeep, out float newLitter, out float newSnow);
                        Assert.That(newKeep, Is.EqualTo(keep));
                        Assert.That(newLitter, Is.EqualTo(litter));
                        Assert.That(newSnow, Is.EqualTo(snow));
                    }
    }

    [Test]
    public void ABankCellKeepsOnlyAShareOfTheSnowAndTheLitter()
    {
        GroundCoverRules.Cover(1f, 0f, 1f, 0.25f, 0.25f, out _, out _, out float snow);
        Assert.That(snow, Is.EqualTo(0.25f).Within(1e-6f), "a full bank cell keeps 25 percent of full snow");

        GroundCoverRules.Cover(0f, 1f, 1f, 0.25f, 0.25f, out _, out float litter, out _);
        Assert.That(litter, Is.EqualTo(0.25f).Within(1e-6f), "and 25 percent of full litter");

        GroundCoverRules.Cover(1f, 0f, 0.5f, 0.25f, 0.25f, out _, out _, out snow);
        Assert.That(snow, Is.EqualTo(0.625f).Within(1e-6f), "half a bank cell sits in between");

        GroundCoverRules.Cover(1f, 0f, 1f, 1f, 1f, out _, out _, out snow);
        Assert.That(snow, Is.EqualTo(1f), "a share of 1 means no difference at all");

        GroundCoverRules.Cover(1f, 1f, 1f, 0f, 0f, out float keep, out litter, out snow);
        Assert.That(snow, Is.EqualTo(0f));
        Assert.That(litter, Is.EqualTo(0f));
        Assert.That(keep, Is.EqualTo(1f), "a share of 0 keeps the bank bare");
    }

    [Test]
    public void TheCoverWeightsAlwaysAddUpToOneWithSnowOverLitterOverThePainting()
    {
        for (float snowWanted = 0f; snowWanted <= 1f; snowWanted += 0.2f)
            for (float litterWanted = 0f; litterWanted <= 1f; litterWanted += 0.2f)
                for (float bank = 0f; bank <= 1f; bank += 0.25f)
                {
                    GroundCoverRules.Cover(snowWanted, litterWanted, bank, 0.25f, 0.25f, out float keep, out float litter, out float snow);
                    Assert.That(keep + litter + snow, Is.EqualTo(1f).Within(1e-5f), $"snow {snowWanted} litter {litterWanted} bank {bank}");
                    Assert.That(keep, Is.InRange(0f, 1f));
                    Assert.That(litter, Is.InRange(0f, 1f));
                    Assert.That(snow, Is.InRange(0f, 1f));
                }

        GroundCoverRules.Cover(0.5f, 0.8f, 0f, 0.25f, 0.25f, out _, out float under, out float over);
        Assert.That(under, Is.EqualTo(0.8f * (1f - 0.5f)).Within(1e-6f), "litter shows only through what the snow leaves");
        Assert.That(over, Is.EqualTo(0.5f));
    }

    [Test]
    public void SeasonalGroundReadsTheBankWeightsFromItsPaintingByLayerName()
    {
        string text = SourceText("Scripts/World/SeasonalGround.cs");
        Assert.That(text, Does.Contain("CreekBankRules.ClayLayerName").And.Contain("CreekBankRules.SandLayerName"));
        Assert.That(text, Does.Contain("baseAlphas[row, x, bankClayLayer]").And.Contain("baseAlphas[row, x, bankSandLayer]"));
        Assert.That(text, Does.Contain("GroundCoverRules.Cover("));
        Assert.That(text, Does.Contain("GroundCoverRules.SeasonalLayers"));

        Type type = Find("SeasonalGround");
        var host = new GameObject("Seasonal ground defaults");
        try
        {
            Component ground = host.AddComponent(type);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            Assert.That((float)type.GetField("bankSnowShare", flags).GetValue(ground), Is.EqualTo(0.25f), "25 percent of the snow");
            Assert.That((float)type.GetField("bankLitterShare", flags).GetValue(ground), Is.EqualTo(0.25f), "and of the litter");
            Assert.That(type.GetField("bankSnowShare", flags).GetCustomAttributes(typeof(SerializeField), false), Is.Not.Empty, "tunable on the component");
            Assert.That(type.GetField("bankLitterShare", flags).GetCustomAttributes(typeof(SerializeField), false), Is.Not.Empty);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    // --- The look ---

    [Test]
    public void TheLayerColoursAreTheOnesAskedForAndTunableOnTheComponent()
    {
        Type type = Find("CreekBanks");
        var host = new GameObject("Creek banks defaults");
        try
        {
            Component banks = host.AddComponent(type);
            var clay = (Color)type.GetField("wetClayColour").GetValue(banks);
            var sand = (Color)type.GetField("creekSandColour").GetValue(banks);
            Assert.That(clay.r, Is.EqualTo(0.42f).Within(0.001f));
            Assert.That(clay.g, Is.EqualTo(0.24f).Within(0.001f));
            Assert.That(clay.b, Is.EqualTo(0.16f).Within(0.001f));
            Assert.That(sand.r, Is.EqualTo(0.76f).Within(0.001f));
            Assert.That(sand.g, Is.EqualTo(0.68f).Within(0.001f));
            Assert.That(sand.b, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That((float)type.GetField("wetClayTileMetres").GetValue(banks), Is.InRange(2f, 3f));
            Assert.That((float)type.GetField("creekSandTileMetres").GetValue(banks), Is.InRange(2f, 3f));
            Assert.That((float)type.GetField("wetClaySmoothness").GetValue(banks), Is.GreaterThan((float)type.GetField("creekSandSmoothness").GetValue(banks)), "the clay is slightly smooth");
            Assert.That(type.GetField("wetClayColour").IsPublic && type.GetField("creekSandColour").IsPublic && type.GetField("wetClayTileMetres").IsPublic &&
                        type.GetField("creekSandTileMetres").IsPublic, Is.True, "named public fields");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void TheTexturesKeepTheirColourAndTileWithoutASeam()
    {
        MethodInfo make = Find("CreekBanks").GetMethod("MakeTexture");
        foreach (Color colour in new[] { new Color(0.42f, 0.24f, 0.16f), new Color(0.76f, 0.68f, 0.5f) })
        {
            var texture = (Texture2D)make.Invoke(null, new object[] { colour, 0.1f, 99 });
            try
            {
                Color[] pixels = texture.GetPixels();
                float r = pixels.Average(p => p.r), g = pixels.Average(p => p.g), b = pixels.Average(p => p.b);
                Assert.That(r, Is.EqualTo(colour.r).Within(colour.r * 0.06f));
                Assert.That(g, Is.EqualTo(colour.g).Within(colour.g * 0.06f));
                Assert.That(b, Is.EqualTo(colour.b).Within(colour.b * 0.06f));
                Assert.That(pixels.Max(p => p.r) - pixels.Min(p => p.r), Is.GreaterThan(0.01f), "a gentle noise, not flat");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
        for (float v = 0f; v < 8f; v += 0.37f)
            Assert.That(CreekBankRules.PeriodicNoise(0f, v, 8, 3), Is.EqualTo(CreekBankRules.PeriodicNoise(8f, v, 8, 3)).Within(1e-6f), "left and right edges meet");
    }

    [Test]
    public void TheClayAndTheSandAreEasyToTellFromTheGravelAndFromEachOther()
    {
        var clay = new Color(0.42f, 0.24f, 0.16f);
        var sand = new Color(0.76f, 0.68f, 0.5f);
        var gravel = new Color(0.64f, 0.61f, 0.55f); // the minimap's Gravel colour; the terrain's own Gravel texture is a similar grey
        Assert.That(Distance(clay, gravel), Is.GreaterThan(0.25f));
        Assert.That(Distance(sand, gravel), Is.GreaterThan(0.1f));
        Assert.That(Distance(clay, sand), Is.GreaterThan(0.4f));
        Assert.That(sand.r - sand.b, Is.GreaterThan(0.2f), "the sand is a warm tan, the gravel a neutral grey");
    }

    static float Distance(Color a, Color b) => Mathf.Sqrt((a.r - b.r) * (a.r - b.r) + (a.g - b.g) * (a.g - b.g) + (a.b - b.b) * (a.b - b.b));

    // --- Footsteps ---

    static void MapSurfaces(GameObject go, TerrainData data)
    {
        Type groundType = Find("GroundSurface");
        Type pair = Find("TerrainLayerSurface");
        Type surfaceType = Find("SurfaceType");
        Component ground = go.AddComponent(groundType);
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(pair));
        string[] names = { "Grass", "Dirt", "Gravel" };
        for (int i = 0; i < 3; i++)
        {
            object entry = Activator.CreateInstance(pair);
            pair.GetField("layer").SetValue(entry, data.terrainLayers[i]);
            pair.GetField("surface").SetValue(entry, Enum.Parse(surfaceType, names[i]));
            list.Add(entry);
        }
        groundType.GetField("terrainLayers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ground, list);
    }

    static string SurfaceAt(GameObject go, float x, float z) =>
        Find("GroundSurface").GetMethod("SurfaceAt").Invoke(go.GetComponent(Find("GroundSurface")), new object[] { new Vector3(x, 0f, z) }).ToString();

    [Test]
    public void TheBankLayersCountAsGravelUnderFoot()
    {
        GameObject go = MakeTerrain(new[] { "Grass", "Dirt", "Gravel", CreekBankRules.ClayLayerName, CreekBankRules.SandLayerName }, out TerrainData data, withWater: false);
        try
        {
            var a = new float[64, 64, 5];
            for (int z = 0; z < 64; z++)
                for (int x = 0; x < 64; x++)
                {
                    if (x < 16) { a[z, x, 0] = 0.2f; a[z, x, 3] = 0.8f; }       // mostly Wet Clay
                    else if (x < 32) { a[z, x, 0] = 0.3f; a[z, x, 4] = 0.7f; }  // mostly Creek Sand
                    else a[z, x, 0] = 1f;                                       // grass
                }
            data.SetAlphamaps(0, 0, a);
            MapSurfaces(go, data);

            Assert.That(SurfaceAt(go, 4f, 10f), Is.EqualTo("Gravel"), "Wet Clay sounds like gravel");
            Assert.That(SurfaceAt(go, 12f, 10f), Is.EqualTo("Gravel"), "Creek Sand sounds like gravel");
            Assert.That(SurfaceAt(go, 24f, 10f), Is.EqualTo("Grass"), "grass is still grass");
            Assert.That(SourceText("Scripts/World/GroundSurface.cs"), Does.Contain("CreekBankRules.IsBankLayer"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    [Test]
    public void SnowAndLitterLayersAreStillIgnoredUnderFoot()
    {
        GameObject go = MakeTerrain(new[] { "Grass", "Dirt", "Gravel", "Leaf Litter (runtime)", "Snow (runtime)" }, out TerrainData data, withWater: false);
        try
        {
            var a = new float[64, 64, 5];
            for (int z = 0; z < 64; z++)
                for (int x = 0; x < 64; x++)
                {
                    a[z, x, 2] = 0.1f;   // a little Gravel
                    a[z, x, 4] = 0.9f;   // under a lot of snow
                }
            data.SetAlphamaps(0, 0, a);
            MapSurfaces(go, data);
            Assert.That(SurfaceAt(go, 8f, 8f), Is.EqualTo("Gravel"), "the snow does not decide the sound; the gravel under it does");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    // --- The minimap ---

    [Test]
    public void TheMinimapGivesTheBankLayersTheirOwnColoursAndIgnoresTheWeatherLayers()
    {
        MethodInfo layerColor = Find("MinimapTerrain").GetMethod("LayerColor");
        Color Colour(string name, int index, out bool counts)
        {
            object[] args = { name == null ? null : new TerrainLayer { name = name }, index, false };
            var colour = (Color)layerColor.Invoke(null, args);
            counts = (bool)args[2];
            return colour;
        }

        Color grass = Colour("Grass", 0, out bool grassCounts);
        Color gravel = Colour("Gravel", 2, out bool gravelCounts);
        Color clay = Colour(CreekBankRules.ClayLayerName, 3, out bool clayCounts);
        Color sand = Colour(CreekBankRules.SandLayerName, 4, out bool sandCounts);
        Colour("Leaf Litter (runtime)", 5, out bool litterCounts);
        Colour("Snow (runtime)", 6, out bool snowCounts);

        Assert.That(grassCounts && gravelCounts && clayCounts && sandCounts, Is.True);
        Assert.That(litterCounts, Is.False, "leaf litter is weather, not ground");
        Assert.That(snowCounts, Is.False, "and so is snow");
        Assert.That(clay.r, Is.GreaterThan(clay.b).And.GreaterThan(clay.g), "clay brown");
        Assert.That(sand.r, Is.GreaterThan(sand.b), "sand tan");
        Assert.That(Distance(clay, gravel), Is.GreaterThan(0.15f));
        Assert.That(Distance(sand, gravel), Is.GreaterThan(0.08f));
        Assert.That(Distance(clay, sand), Is.GreaterThan(0.2f));
        Assert.That(Distance(grass, clay), Is.GreaterThan(0.1f));
    }

    [Test]
    public void TheMinimapPaintsTheBanksAndSkipsTheSnowWhenItIsBuilt()
    {
        GameObject go = MakeTerrain(new[] { "Grass", "Dirt", "Gravel", CreekBankRules.ClayLayerName, "Snow (runtime)" }, out TerrainData data, withWater: false);
        try
        {
            var a = new float[64, 64, 5];
            for (int z = 0; z < 64; z++)
                for (int x = 0; x < 64; x++)
                {
                    if (x < 32) { a[z, x, 3] = 1f; }    // Wet Clay on the left half
                    else { a[z, x, 0] = 0.4f; a[z, x, 4] = 0.6f; } // grass under 60 percent snow on the right
                }
            data.SetAlphamaps(0, 0, a);

            var image = (Texture2D)Find("MinimapTerrain").GetMethod("Build").Invoke(null, new object[] { go.GetComponent<Terrain>(), Vector2.zero, 32f, 32 });
            try
            {
                Texture2D readable = Readable(image);
                Color left = readable.GetPixel(8, 16), right = readable.GetPixel(24, 16);
                UnityEngine.Object.DestroyImmediate(readable);
                Assert.That(left.r, Is.GreaterThan(left.b + 0.1f), "the clay shows brown on the map");
                Assert.That(right.g, Is.GreaterThan(right.r), "the snowy grass shows as grass: the map does not show weather");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(image);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    // The minimap image is not readable from scripts; copy it through a render texture.
    static Texture2D Readable(Texture2D source)
    {
        RenderTexture target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
            copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            copy.Apply();
            return copy;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
        }
    }

    // --- Prompt text ---

    [Test]
    public void TheDryBankIsNowTheSandyBank()
    {
        Assert.That(PotteryRules.PlaceName(PotteryRules.Dig.DryBank), Is.EqualTo("Sandy bank"));
        Assert.That(PotteryRules.DigText(PotteryRules.Dig.DryBank), Is.EqualTo("Sandy bank - hold click to dig Sand"));
        Assert.That(PotteryRules.PlaceName(PotteryRules.Dig.WetBank), Is.EqualTo("Wet clay bank"), "left as it was");
        Assert.That(PotteryRules.PlaceName(PotteryRules.Dig.CreekBed), Is.EqualTo("Creek bed"), "left as it was");

        string scripts = Path.Combine(Application.dataPath, "Scripts");
        foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
            Assert.That(File.ReadAllText(file), Does.Not.Contain("Dry creek bank"), file);
    }

    // --- Dev-only timing, and no saved fields ---

    [Test]
    public void TheTimingLineIsLoggedOnlyInEditorAndDevelopmentBuilds()
    {
        string text = SourceText("Scripts/World/CreekBanks.cs");
        int log = text.IndexOf("[CreekBanks] built in", StringComparison.Ordinal);
        Assert.That(log, Is.GreaterThan(0));
        string before = text.Substring(0, log);
        int guard = before.LastIndexOf("#if UNITY_EDITOR || DEVELOPMENT_BUILD", StringComparison.Ordinal);
        int end = before.LastIndexOf("#endif", StringComparison.Ordinal);
        Assert.That(guard, Is.GreaterThan(end), "the log line sits inside the dev-only guard");
        Assert.That(text, Does.Contain("water hits").And.Contain("bank cells"));
    }

    [Test]
    public void NothingNewIsSavedAndTheSaveVersionsStayAtOne()
    {
        Assert.That(Find("ISaveable").IsAssignableFrom(Find("CreekBanks")), Is.False, "the banks are derived at load, never saved");
        Assert.That(Find("ISaveable").IsAssignableFrom(Find("RuntimeTerrain")), Is.False);

        FieldInfo[] snow = Find("GroundSnowSaveData").GetFields(BindingFlags.Public | BindingFlags.Instance);
        Assert.That(snow.Select(f => f.Name).ToArray(), Is.EqualTo(new[] { "amount" }), "SeasonalGround still saves only the snow amount");

        string[] pile = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(pile.Length, Is.EqualTo(17), "WoodPileState gained nothing");
        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    // --- The dev key for the ground snow (Ctrl+Home) ---

    [Test]
    public void TheSnowKeyCyclesZeroHalfFullAndBackToZero()
    {
        float amount = 0f;
        var seen = new List<float>();
        for (int i = 0; i < 7; i++)
        {
            amount = GroundCoverRules.NextDebugSnow(amount);
            seen.Add(amount);
        }
        Assert.That(seen, Is.EqualTo(new[] { 0.5f, 1f, 0f, 0.5f, 1f, 0f, 0.5f }));
        Assert.That(GroundCoverRules.NextDebugSnow(0.2f), Is.EqualTo(0.5f), "a melting in-between amount goes to the next step up");
        Assert.That(GroundCoverRules.NextDebugSnow(0.6f), Is.EqualTo(1f));
        Assert.That(GroundCoverRules.NextDebugSnow(0.9f), Is.EqualTo(0f));
        Assert.That(GroundCoverRules.DebugSnowHoldGameHours, Is.EqualTo(24f));
    }

    [Test]
    public void TheSnowKeySetsTheAmountOnSeasonalGroundWithinZeroAndOne()
    {
        Type type = Find("SeasonalGround");
        var host = new GameObject("Seasonal ground for the snow key");
        try
        {
            Component ground = host.AddComponent(type);
            MethodInfo set = type.GetMethod("DebugSetSnow");
            PropertyInfo amount = type.GetProperty("Amount");
            set.Invoke(ground, new object[] { 0.5f, 24f });
            Assert.That((float)amount.GetValue(ground), Is.EqualTo(0.5f));
            set.Invoke(ground, new object[] { 7f, 24f });
            Assert.That((float)amount.GetValue(ground), Is.EqualTo(1f), "clamped to 1");
            set.Invoke(ground, new object[] { -3f, 24f });
            Assert.That((float)amount.GetValue(ground), Is.EqualTo(0f), "clamped to 0");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void TheSnowKeyIsDevOnlyFreeOfHomeAndHoldsTheAmountAgainstTheWeather()
    {
        string keys = SourceText("Scripts/Debug/TimeDebugControls.cs");
        Assert.That(keys, Does.StartWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the whole key file is compiled out of release builds");
        Assert.That(keys, Does.Contain("GroundCoverRules.NextDebugSnow(ground.Amount)"));
        Assert.That(keys.IndexOf("keyboard.homeKey.wasPressedThisFrame && (keyboard.leftCtrlKey.isPressed", StringComparison.Ordinal),
                    Is.LessThan(keys.IndexOf("keyboard.homeKey.wasPressedThisFrame && TapManager.Instance != null", StringComparison.Ordinal)),
                    "Ctrl+Home is handled first and returns, so it never toggles the sap");
        Assert.That(keys, Does.Not.Contain("Key.Home"));

        string ground = SourceText("Scripts/World/SeasonalGround.cs");
        int method = ground.IndexOf("public void DebugSetSnow", StringComparison.Ordinal);
        Assert.That(method, Is.GreaterThan(0));
        Assert.That(ground.Substring(0, method).LastIndexOf("#if UNITY_EDITOR || DEVELOPMENT_BUILD", StringComparison.Ordinal),
                    Is.GreaterThan(ground.Substring(0, method).LastIndexOf("#endif", StringComparison.Ordinal)), "dev-only in SeasonalGround too");
        Assert.That(ground, Does.Contain("held = hours < holdUntilHours"));

        // The saved field is unchanged.
        Assert.That(Find("GroundSnowSaveData").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray(), Is.EqualTo(new[] { "amount" }));
    }

    [Test]
    public void TheDigRuleAndTheTerrainAssetAreNotTouchedByTheBankCode()
    {
        Assert.That(SourceText("Scripts/Player/AxeTool.cs"), Does.Not.Contain("CreekBanks").And.Not.Contain("CreekBankRules"));
        Assert.That(SourceText("Scripts/Editor/PropertyTerrainBuilder.cs"), Does.Not.Contain("CreekBanks").And.Not.Contain("CreekBankRules"));
        string runtime = SourceText("Scripts/World/RuntimeTerrain.cs");
        Assert.That(runtime.IndexOf("Instantiate(terrain.terrainData)", StringComparison.Ordinal),
                    Is.LessThan(runtime.IndexOf("Build(terrain, owner.copy)", StringComparison.Ordinal)), "the banks go into the copy, after it is made");
    }
}
