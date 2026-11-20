using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Wave 7 "Ledger" look for the UI that lives in the scenes rather than in code: the HUD and Pause Menu in World, and the
// Main Menu. Menu: Homestead/UI/Apply Ledger Look (safe to run again; it finds objects by name, not by their old colours).
//  - The HUD stays light text over dark plates so it reads against the world: the plates are dark-brown paper cards, the
//    compass is a wood strip, bar tracks are rounded, and the minimap rim is wood-coloured.
//  - Discovery and Journal toasts are parchment notes with ink text, like the screens.
//  - Menu buttons are UiKit buttons (paper, ink, bold), titles are Alegreya Bold.
public static class LedgerUiApplier
{
    static readonly Color HudPlate = new Color(0.13f, 0.09f, 0.055f, 0.86f);
    static readonly Color TrackColor = new Color(0.06f, 0.04f, 0.02f, 0.85f);
    static readonly Color RimColor = new Color(0.66f, 0.48f, 0.28f, 1f);
    static readonly Color ToastPaper = new Color(1f, 0.97f, 0.88f, 0.97f);
    static readonly Color MenuBackgroundColor = new Color(0.10f, 0.075f, 0.05f, 1f);

    const string WorldScene = "Assets/Scenes/World.unity";
    const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
    const string LoadingScene = "Assets/Scenes/Loading.unity";

    // Wave 10 branding (2026-11-20).
    const string MenuBackground = "Assets/Resources/Art/menu_background.png";
    const string LogoLockup = "Assets/Resources/Art/logo_lockup.png";
    const string AppIcon = "Assets/Art/Branding/app_icon.png";

    [MenuItem("Homestead/UI/Apply Ledger Look")]
    public static void ApplyMenu()
    {
        string report = Apply();
        Debug.Log("Ledger look applied:\n" + report);
    }

    // Opens World and MainMenu (additively if they aren't open), restyles them and saves them. Returns a short report.
    public static string Apply()
    {
        var log = new System.Text.StringBuilder();
        ImportBranding(log);
        foreach (string path in new[] { WorldScene, MainMenuScene, LoadingScene })
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool opened = false;
            if (!scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                opened = true;
            }
            int changes = 0;
            if (path == LoadingScene)
                changes += EnsureLoadingScreen(scene);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas.name == "HUD") changes += StyleHud(canvas.transform);
                    else if (canvas.name == "Pause Menu") changes += StylePause(canvas.transform);
                    else if (path == MainMenuScene && canvas.transform.parent == null) changes += StyleMainMenu(canvas.transform);
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (opened)
                EditorSceneManager.CloseScene(scene, true);
            log.AppendLine($"{path}: {changes} objects styled");
        }
        return log.ToString();
    }

    // ---------------------------------------------------------------- HUD
    static int StyleHud(Transform hud)
    {
        int n = 0;
        foreach (Text text in hud.GetComponentsInChildren<Text>(true))
        {
            SetFont(text);
            n++;
        }

        // Survival meters: a plate per row, a rounded track, the label a touch bigger.
        Transform meters = hud.Find("Survival Meters");
        if (meters != null)
        {
            foreach (Transform row in meters)
            {
                Plate(row.GetComponent<Image>(), HudPlate);
                Text label = row.Find("Label") != null ? row.Find("Label").GetComponent<Text>() : null;
                if (label != null) label.fontSize = 22;
                Transform track = row.Find("Track");
                if (track != null) Track(track);
                n++;
            }
        }

        Transform compass = hud.Find("Compass");
        if (compass != null)
        {
            Image strip = compass.GetComponent<Image>();
            Slice(strip, UiKit.WoodSprite, 1f);
            strip.color = new Color(1f, 1f, 1f, 0.95f);
            // CompassHud draws its letters in UiKit's fonts unless a font is set on it, so clear any old one.
            Component compassHud = compass.GetComponent("CompassHud");
            if (compassHud != null)
            {
                var so = new SerializedObject(compassHud);
                SerializedProperty font = so.FindProperty("font");
                if (font != null) { font.objectReferenceValue = null; so.ApplyModifiedPropertiesWithoutUndo(); }
            }
            n++;
        }

        Transform minimap = hud.Find("Minimap");
        if (minimap != null)
        {
            Image backdrop = Child<Image>(minimap, "Backdrop");
            if (backdrop != null) backdrop.color = HudPlate; // its circle sprite is set by MinimapHud
            Image rim = Child<Image>(minimap, "Rim");
            if (rim != null) rim.color = RimColor;
            Image north = Child<Image>(minimap, "North");
            if (north != null) { Slice(north, UiKit.CardSprite, 2f); north.color = HudPlate; }
            n += 3;
        }

        Transform prompt = hud.Find("Interaction Prompt");
        if (prompt != null)
        {
            Plate(prompt.GetComponent<Image>(), HudPlate);
            Text label = Child<Text>(prompt, "Label");
            if (label != null) label.fontSize = 26;
            n++;
        }

        Transform tool = hud.Find("Tool HUD");
        if (tool != null)
        {
            Text status = Child<Text>(tool, "Status");
            if (status != null) status.fontSize = 24;
            Transform bar = tool.Find("Bar");
            if (bar != null) Track(bar);
            Transform flash = tool.Find("Flash");
            if (flash != null)
            {
                Plate(flash.GetComponent<Image>(), new Color(HudPlate.r, HudPlate.g, HudPlate.b, 0.94f));
                Text label = Child<Text>(flash, "Label");
                if (label != null) label.fontSize = 26;
            }
            n += 2;
        }

        // Toasts: parchment notes.
        foreach (string name in new[] { "Discovery Popup", "Journal Updated" })
        {
            Transform toast = hud.Find(name);
            if (toast == null) continue;
            Plate(toast.GetComponent<Image>(), ToastPaper);
            foreach (Text text in toast.GetComponentsInChildren<Text>(true))
            {
                if (text.name == "Heading")
                {
                    text.font = UiKit.TitleFont;
                    text.fontStyle = FontStyle.Normal;
                    text.color = UiKit.HeadingColor;
                    text.fontSize = 26;
                }
                else
                    text.color = UiKit.Ink;
                RemoveShadow(text);
            }
            n++;
        }
        return n;
    }

    // ---------------------------------------------------------------- Pause Menu
    static int StylePause(Transform pause)
    {
        int n = 0;
        foreach (Text text in pause.GetComponentsInChildren<Text>(true))
            SetFont(text);
        Text title = Find<Text>(pause, "Title");
        if (title != null) Title(title);
        n += StyleButtons(pause);
        return n + 1;
    }

    // ---------------------------------------------------------------- Main Menu
    static int StyleMainMenu(Transform canvas)
    {
        int n = 0;
        foreach (Text text in canvas.GetComponentsInChildren<Text>(true))
            SetFont(text);
        // The homestead at autumn dusk, darker on the left where the logo and buttons sit (Wave 10).
        Image background = Child<Image>(canvas, "Background");
        if (background != null)
        {
            Sprite picture = AssetDatabase.LoadAssetAtPath<Sprite>(MenuBackground);
            background.color = picture != null ? Color.white : MenuBackgroundColor;
            background.sprite = picture;
            background.type = Image.Type.Simple;
            var fitter = background.GetComponent<AspectRatioFitter>();
            if (picture != null && fitter == null) fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = 16f / 9f;
            }
            EditorUtility.SetDirty(background);
        }
        Text title = Child<Text>(canvas, "Title");
        if (title != null) Title(title);
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LogoLockup);
        if (logoSprite != null)
        {
            // The logo (emblem, HOMESTEAD, the motto) replaces the plain title; the title stays, hidden, as the fallback.
            Transform logoT = canvas.Find("Logo");
            Image logo = logoT != null ? logoT.GetComponent<Image>() : null;
            if (logo == null)
            {
                var go = new GameObject("Logo", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(canvas, false);
                go.transform.SetSiblingIndex(background != null ? background.transform.GetSiblingIndex() + 1 : 0);
                logo = go.GetComponent<Image>();
            }
            logo.sprite = logoSprite;
            logo.preserveAspect = true;
            logo.raycastTarget = false;
            var lrt = logo.rectTransform;
            lrt.anchorMin = lrt.anchorMax = new Vector2(0.27f, 0.7f);
            lrt.sizeDelta = new Vector2(820f, 820f * logoSprite.rect.height / logoSprite.rect.width);
            lrt.anchoredPosition = Vector2.zero;
            if (title != null) title.gameObject.SetActive(false);
            Transform buttons = canvas.Find("Buttons");
            if (buttons != null)
            {
                var brt = (RectTransform)buttons;
                brt.anchorMin = brt.anchorMax = new Vector2(0.27f, 0.32f);
                brt.anchoredPosition = Vector2.zero;
            }
            EditorUtility.SetDirty(logo);
        }

        Transform confirm = canvas.Find("Confirm New Game");
        if (confirm != null)
        {
            Image dialog = Child<Image>(confirm, "Dialog");
            if (dialog != null)
            {
                Slice(dialog, UiKit.PanelSprite, 1.4f);
                dialog.color = Color.white;
                Text message = Child<Text>(dialog.transform, "Message");
                if (message != null) message.color = UiKit.Ink;
            }
        }
        n += StyleButtons(canvas);
        return n + 2;
    }

    // ---------------------------------------------------------------- Loading scene
    static int EnsureLoadingScreen(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.GetComponent("LoadingScreen") != null)
                return 0;
        System.Type type = System.Type.GetType("LoadingScreen, Assembly-CSharp");
        if (type == null)
            return 0;
        var go = new GameObject("Loading Screen");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent(type);
        return 1;
    }

    // ---------------------------------------------------------------- branding (Wave 10)
    static void ImportBranding(System.Text.StringBuilder log)
    {
        foreach (string path in new[] { "Assets/Resources/Art/menu_background.png", "Assets/Resources/Art/title_background.png",
                                        "Assets/Resources/Art/loss_background.png", LogoLockup, AppIcon })
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { log.AppendLine("missing " + path); continue; }
            bool sprite = path == MenuBackground || path == LogoLockup;
            importer.textureType = sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (sprite) importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = path == LogoLockup || path == AppIcon;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ; // BC7: no banding in the dusk sky
            importer.SaveAndReimport();
        }
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIcon);
        if (icon != null)
        {
            PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            log.AppendLine("app icon set");
        }
    }

    // ---------------------------------------------------------------- helpers
    static int StyleButtons(Transform root)
    {
        int n = 0;
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            var image = button.targetGraphic as Image;
            if (image != null)
            {
                Slice(image, UiKit.ButtonSprite, 1f);
                image.color = Color.white;
            }
            ColorBlock colors = button.colors;
            colors.normalColor = UiKit.ButtonColor;
            colors.highlightedColor = UiKit.ButtonHover;
            colors.selectedColor = UiKit.ButtonHover; // keyboard focus shows as the lighter paper
            colors.pressedColor = UiKit.ButtonPressed;
            colors.disabledColor = new Color(UiKit.ButtonColor.r, UiKit.ButtonColor.g, UiKit.ButtonColor.b, 0.5f);
            button.colors = colors;
            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.font = UiKit.BoldFont;
                label.fontStyle = FontStyle.Normal;
                label.color = UiKit.Ink;
                RemoveShadow(label);
            }
            EditorUtility.SetDirty(button);
            n++;
        }
        return n;
    }

    static void Title(Text title)
    {
        title.font = UiKit.TitleFont;
        title.fontStyle = FontStyle.Normal;
        title.color = UiKit.Cream;
        Shadow shadow = title.GetComponent<Shadow>();
        if (shadow == null) shadow = title.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    static void SetFont(Text text)
    {
        bool bold = text.fontStyle == FontStyle.Bold || text.fontStyle == FontStyle.BoldAndItalic;
        text.font = bold ? UiKit.BoldFont : UiKit.Font;
        if (bold)
            text.fontStyle = text.fontStyle == FontStyle.BoldAndItalic ? FontStyle.Italic : FontStyle.Normal;
        EditorUtility.SetDirty(text);
    }

    static void Plate(Image image, Color color)
    {
        if (image == null) return;
        Slice(image, UiKit.CardSprite, 1.4f);
        image.color = color;
    }

    static void Track(Transform track)
    {
        Image image = track.GetComponent<Image>();
        if (image != null)
        {
            Slice(image, UiKit.TrackSprite, 1.6f);
            image.color = TrackColor;
        }
        Transform fill = track.Find("Fill");
        if (fill != null)
        {
            var rt = (RectTransform)fill;
            rt.offsetMin = new Vector2(2f, 2f);
            rt.offsetMax = new Vector2(-2f, -2f);
        }
    }

    static void Slice(Image image, Sprite sprite, float scale)
    {
        if (image == null || sprite == null) return;
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.fillCenter = true;
        image.pixelsPerUnitMultiplier = scale;
        EditorUtility.SetDirty(image);
    }

    static void RemoveShadow(Text text)
    {
        foreach (Shadow shadow in text.GetComponents<Shadow>())
            Object.DestroyImmediate(shadow);
    }

    static T Child<T>(Transform parent, string name) where T : Component
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<T>() : null;
    }

    static T Find<T>(Transform root, string name) where T : Component
    {
        foreach (T component in root.GetComponentsInChildren<T>(true))
            if (component.name == name) return component;
        return null;
    }
}
