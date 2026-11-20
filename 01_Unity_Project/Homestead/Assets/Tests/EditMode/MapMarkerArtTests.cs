using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Wave 4 map icons (Mike, 2026-11-20): the minimap and world map discovery markers come from Resources/Art/map_marker_*
// (128 px art) through MapIcons.Marker, at the same on-screen size as the 64 px markers it used to draw.
public class MapMarkerArtTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    static Sprite Marker(string category, string icon)
    {
        Type categories = Find("DiscoveryCategory");
        MethodInfo marker = Find("MapIcons").GetMethod("Marker");
        return (Sprite)marker.Invoke(null, new object[] { Enum.Parse(categories, category), icon });
    }

    [TestCase("WaterSource", null, "map_marker_WaterSource")]
    [TestCase("Plant", null, "map_marker_Plant")]
    [TestCase("Plant", "berry", "map_marker_Plant")]
    [TestCase("Plant", "fruit", "map_marker_Plant_fruit")]
    [TestCase("Plant", "nut", "map_marker_Plant_nut")]
    [TestCase("Plant", "greens", "map_marker_Plant_greens")]
    [TestCase("Plant", "mushroom", "map_marker_Plant_mushroom")]
    [TestCase("Wildlife", null, "map_marker_Wildlife")]
    [TestCase("Fishing", null, "map_marker_Fishing")]
    [TestCase("PropertyFeature", null, "map_marker_PropertyFeature")]
    [TestCase("Wildlife", "no_such_icon", "map_marker_Wildlife")]
    public void EachMarkerUsesItsArtAtTheDrawnSize(string category, string icon, string texture)
    {
        Sprite sprite = Marker(category, icon);
        Assert.That(sprite, Is.Not.Null);
        Assert.That(sprite.texture.name, Is.EqualTo(texture));
        Assert.That(sprite.texture.width, Is.EqualTo(128));
        Assert.That(sprite.bounds.size.x, Is.EqualTo(0.64f).Within(0.001f), "the same size as the 64 px markers drawn at 100 pixels per unit");
    }
}
