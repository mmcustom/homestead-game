using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

// The "no generated test prefab is left behind" check the art-swap tests share. Resources/Art can now hold real art (never committed from the playtest machine, but present on it), so the old
// "the folder does not exist" assertions failed for a reason that has nothing to do with the code. What the tests really protect is that the Homestead/Art/Create Test ... menu items clean up after
// themselves, and the generated test prefabs are recognisable: each carries an empty child named Test<Thing>_Marker, and its materials live in a folder named _Test<Thing>Materials. Real art has
// neither, so this stays true whatever is in the folder.
public static class ArtFolderGuard
{
    static readonly Regex Marker = new Regex(@"m_Name:\s*Test\w*_Marker");

    // Paths (relative to Assets) of generated test prefabs or test material folders still under Resources/Art; empty when there are none.
    public static List<string> LeftoverTestArt()
    {
        var left = new List<string>();
        string art = Path.Combine(Application.dataPath, "Resources", "Art");
        if (!Directory.Exists(art))
            return left;
        foreach (string dir in Directory.GetDirectories(art, "_Test*", SearchOption.AllDirectories))
            left.Add(dir.Substring(Application.dataPath.Length + 1).Replace('\\', '/'));
        foreach (string prefab in Directory.GetFiles(art, "*.prefab", SearchOption.AllDirectories))
            if (Marker.IsMatch(File.ReadAllText(prefab)))
                left.Add(prefab.Substring(Application.dataPath.Length + 1).Replace('\\', '/'));
        return left;
    }
}
